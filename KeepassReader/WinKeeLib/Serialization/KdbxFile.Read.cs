// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KdbxFile.Read.cs" company="">
//   
// </copyright>
// <summary>
//   Serialization to KeePass KDBX files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System.IO.Compression;
#if !KeePassLibSD
#else
using KeePassLibSD;
#endif

namespace KeePassLib.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Security;
    using System.Text;
    using System.Threading;

    using KeePassLib.Cryptography;
    using KeePassLib.Cryptography.Cipher;
    using KeePassLib.Interfaces;
    using KeePassLib.Keys;
    using KeePassLib.Resources;
    using KeePassLib.Utility;

    using Windows.Security.Cryptography.Core;

    using WinKeeLib;
    using WinKeeLib.Keys;

    /// <summary>
    /// Serialization to KeePass KDBX files.
    /// </summary>
    public sealed partial class KdbxFile
    {
        #region Public Methods and Operators

        /// <summary>
        /// The read entries.
        /// </summary>
        /// <param name="pwDatabase">
        /// The pw database.
        /// </param>
        /// <param name="msData">
        /// The ms data.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        [Obsolete]
        public static List<PwEntry> ReadEntries(PwDatabase pwDatabase, Stream msData)
        {
            return ReadEntries(msData);
        }

        /// <summary>
        /// Read entries from a stream.
        /// </summary>
        /// <param name="msData">
        /// Input stream to read the entries from.
        /// </param>
        /// <returns>
        /// Extracted entries.
        /// </returns>
        public static List<PwEntry> ReadEntries(Stream msData)
        {
            /* KdbxFile f = new KdbxFile(pwDatabase);
			f.m_format = KdbxFormat.PlainXml;

			XmlDocument doc = new XmlDocument();
			doc.Load(msData);

			XmlElement el = doc.DocumentElement;
			if(el.Name != ElemRoot) throw new FormatException();

			List<PwEntry> vEntries = new List<PwEntry>();

			foreach(XmlNode xmlChild in el.ChildNodes)
			{
				if(xmlChild.Name == ElemEntry)
				{
					PwEntry pe = f.ReadEntry(xmlChild);
					pe.Uuid = new PwUuid(true);

					foreach(PwEntry peHistory in pe.History)
						peHistory.Uuid = pe.Uuid;

					vEntries.Add(pe);
				}
				else { Debug.Assert(false); }
			}

			return vEntries; */
            var pd = new PwDatabase();
            var f = new KdbxFile(pd);
            f.Load(msData, KdbxFormat.PlainXml, null, CancellationToken.None);

            List<PwEntry> vEntries = new List<PwEntry>();
            foreach (PwEntry pe in pd.RootGroup.Entries)
            {
                pe.SetUuid(new PwUuid(true), true);
                vEntries.Add(pe);
            }

            return vEntries;
        }

        /// <summary>
        /// Load a KDB file from a stream.
        /// </summary>
        /// <param name="sSource">
        /// Stream to read the data from. Must contain
        /// a KDBX stream.
        /// </param>
        /// <param name="kdbFormat">
        /// Format specifier.
        /// </param>
        /// <param name="slLogger">
        /// Status logger (optional).
        /// </param>
        public void Load(Stream sSource, KdbxFormat kdbFormat, IStatusLogger slLogger, CancellationToken token)
        {
            Debug.Assert(sSource != null);
            if (sSource == null)
            {
                throw new ArgumentNullException("sSource");
            }

            this.m_format = kdbFormat;
            this.m_slLogger = slLogger;

            var hashedStream = new HashingStreamEx(sSource, false, null);

            var encNoBom = StrUtil.Utf8;
            try
            {
                BinaryReaderEx br = null;
                BinaryReaderEx brDecrypted = null;
                Stream readerStream = null;

                if (kdbFormat == KdbxFormat.Default)
                {
                    br = new BinaryReaderEx(hashedStream, encNoBom, KLRes.FileCorrupted);
                    this.ReadHeader(br);

                    Stream sDecrypted = this.AttachStreamDecryptor(hashedStream, token);
                    if ((sDecrypted == null) || (sDecrypted == hashedStream))
                    {
                        throw new SecurityException(KLRes.CryptoStreamFailed);
                    }

                    brDecrypted = new BinaryReaderEx(sDecrypted, encNoBom, KLRes.FileCorrupted);
                    byte[] pbStoredStartBytes = brDecrypted.ReadBytes(32);

                    if ((this.m_pbStreamStartBytes == null) || (this.m_pbStreamStartBytes.Length != 32))
                    {
                        throw new InvalidDataException();
                    }

                    for (int iStart = 0; iStart < 32; ++iStart)
                    {
                        if (pbStoredStartBytes[iStart] != this.m_pbStreamStartBytes[iStart])
                        {
                            throw new InvalidCompositeKeyException();
                        }
                    }

                    Stream sHashed = new HashedBlockStream(sDecrypted, false, 0, !this.m_bRepairMode);

                    if (this.m_pwDatabase.Compression == PwCompressionAlgorithm.GZip)
                    {
                        readerStream = new GZipStream(sHashed, CompressionMode.Decompress);
                    }
                    else
                    {
                        readerStream = sHashed;
                    }
                }
                else if (kdbFormat == KdbxFormat.PlainXml)
                {
                    readerStream = hashedStream;
                }
                else
                {
                    Debug.Assert(false);
                    throw new FormatException("KdbFormat");
                }

                if (kdbFormat != KdbxFormat.PlainXml)
                {
                    // Is an encrypted format
                    if (this.m_pbProtectedStreamKey == null)
                    {
                        Debug.Assert(false);
                        throw new SecurityException("Invalid protected stream key!");
                    }

                    this.m_randomStream = new CryptoRandomStream(
                        this.m_craInnerRandomStream,
                        this.m_pbProtectedStreamKey);
                }
                else
                {
                    this.m_randomStream = null; // No random stream for plain-text files
                }

                this.ReadXmlStreamed(readerStream, hashedStream, token);

                readerStream.Dispose();
            }
            finally
            {
                this.CommonCleanUpRead(sSource, hashedStream);
            }
        }

        public void LoadHeader(Stream sSource, KdbxFormat kdbFormat)
        {
            Debug.Assert(sSource != null);
            if (sSource == null)
            {
                throw new ArgumentNullException("sSource");
            }

            this.m_format = kdbFormat;

            var hashedStream = new HashingStreamEx(sSource, false, null);

            var encNoBom = StrUtil.Utf8;

            try
            {
                if (kdbFormat != KdbxFormat.Default)
                {
                    return;
                }

                var br = new BinaryReaderEx(hashedStream, encNoBom, KLRes.FileCorrupted);
                this.ReadHeader(br);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                this.CommonCleanUpRead(sSource, hashedStream);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The attach stream decryptor.
        /// </summary>
        /// <param name="s">
        /// The s.
        /// </param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        /// <exception cref="FormatException">
        /// </exception>
        /// <exception cref="SecurityException">
        /// </exception>
        private Stream AttachStreamDecryptor(Stream s, CancellationToken token)
        {
            MemoryStream ms = new MemoryStream();

            Debug.Assert(this.m_pbMasterSeed.Length == 32);
            if (this.m_pbMasterSeed.Length != 32)
            {
                throw new FormatException(KLRes.MasterSeedLengthInvalid);
            }

            ms.Write(this.m_pbMasterSeed, 0, 32);

            byte[] pKey32 = this.m_pwDatabase.MasterKey.GenerateKey32(this.m_pbTransformSeed, this.m_pwDatabase.KeyEncryptionRounds, token).ReadData();
            if ((pKey32 == null) || (pKey32.Length != 32))
            {
                throw new SecurityException(KLRes.InvalidCompositeKey);
            }

            ms.Write(pKey32, 0, 32);

            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            byte[] aesKey = sha256.HashData(ms.ToArray().AsBuffer()).ToArray();

            ms.Dispose();
            Array.Clear(pKey32, 0, 32);

            if ((aesKey == null) || (aesKey.Length != 32))
            {
                throw new SecurityException(KLRes.FinalKeyCreationFailed);
            }

            ICipherEngine iEngine = CipherPool.GlobalPool.GetCipher(this.m_pwDatabase.DataCipherUuid);
            if (iEngine == null)
            {
                throw new SecurityException(KLRes.FileUnknownCipher);
            }

            return iEngine.DecryptStream(s, aesKey, this.m_pbEncryptionIV);
        }

        /// <summary>
        /// The common clean up read.
        /// </summary>
        /// <param name="sSource">
        /// The s source.
        /// </param>
        /// <param name="hashedStream">
        /// The hashed stream.
        /// </param>
        private void CommonCleanUpRead(Stream sSource, HashingStreamEx hashedStream)
        {
            hashedStream.Dispose();
            this.m_pbHashOfFileOnDisk = hashedStream.Hash;

            sSource.Dispose();

            // Reset memory protection settings (to always use reasonable
            // defaults)
            this.m_pwDatabase.MemoryProtection = new MemoryProtectionConfig();

            // Remove old backups (this call is required here in order to apply
            // the default history maintenance settings for people upgrading from
            // KeePass <= 2.14 to >= 2.15; also it ensures history integrity in
            // case a different application has created the KDBX file and ignored
            // the history maintenance settings)
            this.m_pwDatabase.MaintainBackups(); // Don't mark database as modified

            this.m_pbHashOfHeader = null;
        }

        /// <summary>
        /// The read header.
        /// </summary>
        /// <param name="br">
        /// The br.
        /// </param>
        /// <exception cref="OldFormatException">
        /// </exception>
        /// <exception cref="FormatException">
        /// </exception>
        private void ReadHeader(BinaryReaderEx br)
        {
            MemoryStream msHeader = new MemoryStream();
            Debug.Assert(br.CopyDataTo == null);
            br.CopyDataTo = msHeader;

            byte[] pbSig1 = br.ReadBytes(4);
            uint uSig1 = MemUtil.BytesToUInt32(pbSig1);
            byte[] pbSig2 = br.ReadBytes(4);
            uint uSig2 = MemUtil.BytesToUInt32(pbSig2);

            if ((uSig1 == FileSignatureOld1) && (uSig2 == FileSignatureOld2))
            {
                throw new OldFormatException(PwDefs.ShortProductName + @" 1.x", OldFormatException.OldFormatType.KeePass1x);
            }

            if ((uSig1 == FileSignature1) && (uSig2 == FileSignature2))
            {
            }
            else if ((uSig1 == FileSignaturePreRelease1) && (uSig2 == FileSignaturePreRelease2))
            {
            }
            else
            {
                throw new FormatException(KLRes.FileSigInvalid);
            }

            byte[] pb = br.ReadBytes(4);
            uint uVersion = MemUtil.BytesToUInt32(pb);
            if ((uVersion & FileVersionCriticalMask) > (FileVersion32 & FileVersionCriticalMask))
            {
                throw new FormatException(KLRes.FileVersionUnsupported + Environment.NewLine + KLRes.FileNewVerReq);
            }

            while (true)
            {
                if (this.ReadHeaderField(br) == false)
                {
                    break;
                }
            }

            br.CopyDataTo = null;
            byte[] pbHeader = msHeader.ToArray();
            msHeader.Dispose();
            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            this.m_pbHashOfHeader = sha256.HashData(pbHeader.AsBuffer()).ToArray();
        }

        /// <summary>
        /// The read header field.
        /// </summary>
        /// <param name="brSource">
        /// The br source.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        private bool ReadHeaderField(BinaryReaderEx brSource)
        {
            Debug.Assert(brSource != null);
            if (brSource == null)
            {
                throw new ArgumentNullException("brSource");
            }

            byte btFieldID = brSource.ReadByte();
            ushort uSize = MemUtil.BytesToUInt16(brSource.ReadBytes(2));

            byte[] pbData = null;
            if (uSize > 0)
            {
                string strPrevExcpText = brSource.ReadExceptionText;
                brSource.ReadExceptionText = KLRes.FileHeaderEndEarly;

                pbData = brSource.ReadBytes(uSize);

                brSource.ReadExceptionText = strPrevExcpText;
            }

            bool bResult = true;
            KdbxHeaderFieldID kdbID = (KdbxHeaderFieldID)btFieldID;
            switch (kdbID)
            {
                case KdbxHeaderFieldID.EndOfHeader:
                    bResult = false; // Returning false indicates end of header
                    break;

                case KdbxHeaderFieldID.CipherID:
                    this.SetCipher(pbData);
                    break;

                case KdbxHeaderFieldID.CompressionFlags:
                    this.SetCompressionFlags(pbData);
                    break;

                case KdbxHeaderFieldID.MasterSeed:
                    this.m_pbMasterSeed = pbData;
                    CryptoRandom.Instance.AddEntropy(pbData);
                    break;

                case KdbxHeaderFieldID.TransformSeed:
                    this.m_pbTransformSeed = pbData;
                    CryptoRandom.Instance.AddEntropy(pbData);
                    break;

                case KdbxHeaderFieldID.TransformRounds:
                    this.m_pwDatabase.KeyEncryptionRounds = MemUtil.BytesToUInt64(pbData);
                    break;

                case KdbxHeaderFieldID.EncryptionIV:
                    this.m_pbEncryptionIV = pbData;
                    break;

                case KdbxHeaderFieldID.ProtectedStreamKey:
                    this.m_pbProtectedStreamKey = pbData;
                    CryptoRandom.Instance.AddEntropy(pbData);
                    break;

                case KdbxHeaderFieldID.StreamStartBytes:
                    this.m_pbStreamStartBytes = pbData;
                    break;

                case KdbxHeaderFieldID.InnerRandomStreamID:
                    this.SetInnerRandomStreamID(pbData);
                    break;

                default:
                    Debug.Assert(false);
                    if (this.m_slLogger != null)
                    {
                        this.m_slLogger.SetText(KLRes.UnknownHeaderId + @": " + kdbID.ToString() + "!", LogStatusType.Warning);
                    }

                    break;
            }

            return bResult;
        }

        /// <summary>
        /// The set cipher.
        /// </summary>
        /// <param name="pbID">
        /// The pb id.
        /// </param>
        /// <exception cref="FormatException">
        /// </exception>
        private void SetCipher(byte[] pbID)
        {
            if ((pbID == null) || (pbID.Length != 16))
            {
                throw new FormatException(KLRes.FileUnknownCipher);
            }

            this.m_pwDatabase.DataCipherUuid = new PwUuid(pbID);
        }

        /// <summary>
        /// The set compression flags.
        /// </summary>
        /// <param name="pbFlags">
        /// The pb flags.
        /// </param>
        /// <exception cref="FormatException">
        /// </exception>
        private void SetCompressionFlags(byte[] pbFlags)
        {
            int nID = (int)MemUtil.BytesToUInt32(pbFlags);
            if ((nID < 0) || (nID >= (int)PwCompressionAlgorithm.Count))
            {
                throw new FormatException(KLRes.FileUnknownCompression);
            }

            this.m_pwDatabase.Compression = (PwCompressionAlgorithm)nID;
        }

        /// <summary>
        /// The set inner random stream id.
        /// </summary>
        /// <param name="pbID">
        /// The pb id.
        /// </param>
        /// <exception cref="FormatException">
        /// </exception>
        private void SetInnerRandomStreamID(byte[] pbID)
        {
            uint uID = MemUtil.BytesToUInt32(pbID);
            if (uID >= (uint)CrsAlgorithm.Count)
            {
                throw new FormatException(KLRes.FileUnknownCipher);
            }

            this.m_craInnerRandomStream = (CrsAlgorithm)uID;
        }

        #endregion
    }
}