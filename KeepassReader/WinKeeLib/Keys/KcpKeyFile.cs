// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KcpKeyFile.cs" company="">
//   
// </copyright>
// <summary>
//   Key files as provided by the user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Keys
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;

    using KeePassLib.Resources;
    using KeePassLib.Security;
    using KeePassLib.Serialization;
    using KeePassLib.Utility;

    using Windows.Data.Xml.Dom;
    using Windows.Security.Cryptography.Core;

    /// <summary>
    /// Key files as provided by the user.
    /// </summary>
    public sealed class KcpKeyFile : IUserKey
    {
        #region Constants

        /// <summary>
        /// The key data element name.
        /// </summary>
        private const string KeyDataElementName = "Data";

        /// <summary>
        /// The key element name.
        /// </summary>
        private const string KeyElementName = "Key";

        /// <summary>
        /// The meta element name.
        /// </summary>
        private const string MetaElementName = "Meta";

        /// <summary>
        /// The root element name.
        /// </summary>
        private const string RootElementName = "KeyFile";

        /// <summary>
        /// The version element name.
        /// </summary>
        private const string VersionElementName = "Version";

        #endregion

        #region Fields

        /// <summary>
        /// The m_pb key data.
        /// </summary>
        private ProtectedBinary m_pbKeyData;

        /// <summary>
        /// The m_str path.
        /// </summary>
        private string m_strPath;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KcpKeyFile"/> class.
        /// </summary>
        /// <param name="strKeyFile">
        /// The str key file.
        /// </param>
        public KcpKeyFile(Stream fileStream)
        {
            this.Construct(fileStream, false);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get key data. Querying this property is fast (it returns a
        /// reference to a cached <c>ProtectedBinary</c> object).
        /// If no key data is available, <c>null</c> is returned.
        /// </summary>
        public ProtectedBinary KeyData
        {
            get
            {
                return this.m_pbKeyData;
            }
        }

        /// <summary>
        /// Path to the key file.
        /// </summary>
        public string Path
        {
            get
            {
                return this.m_strPath;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The load binary key 32.
        /// </summary>
        /// <param name="pbFileData">
        /// The pb file data.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        private static byte[] LoadBinaryKey32(byte[] pbFileData)
        {
            if (pbFileData == null)
            {
                Debug.Assert(false);
                return null;
            }

            if (pbFileData.Length != 32)
            {
                Debug.Assert(false);
                return null;
            }

            return pbFileData;
        }

        /// <summary>
        /// The load hex key 32.
        /// </summary>
        /// <param name="pbFileData">
        /// The pb file data.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        private static byte[] LoadHexKey32(byte[] pbFileData)
        {
            if (pbFileData == null)
            {
                Debug.Assert(false);
                return null;
            }

            if (pbFileData.Length != 64)
            {
                Debug.Assert(false);
                return null;
            }

            try
            {
                string strHex = StrUtil.Utf8.GetString(pbFileData, 0, 64);
                if (!StrUtil.IsHexString(strHex, true))
                {
                    return null;
                }

                byte[] pbKey = MemUtil.HexStringToByteArray(strHex);
                if ((pbKey == null) || (pbKey.Length != 32))
                {
                    return null;
                }

                return pbKey;
            }
            catch (Exception)
            {
                Debug.Assert(false);
            }

            return null;
        }

        /// <summary>
        /// The load key file.
        /// </summary>
        /// <param name="pbFileData">
        /// The pb file data.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        private static byte[] LoadKeyFile(byte[] pbFileData)
        {
            if (pbFileData == null)
            {
                Debug.Assert(false);
                return null;
            }

            int iLength = pbFileData.Length;

            byte[] pbKey = null;
            if (iLength == 32)
            {
                pbKey = LoadBinaryKey32(pbFileData);
            }
            else if (iLength == 64)
            {
                pbKey = LoadHexKey32(pbFileData);
            }

            if (pbKey == null)
            {
                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                pbKey = sha256.HashData(pbFileData.AsBuffer()).ToArray();
            }

            return pbKey;
        }

        // ================================================================
        // XML Key Files
        // ================================================================

        // Sample XML file:
        // <?xml version="1.0" encoding="utf-8"?>
        // <KeyFile>
        // <Meta>
        // <Version>1.00</Version>
        // </Meta>
        // <Key>
        // <Data>ySFoKuCcJblw8ie6RkMBdVCnAf4EedSch7ItujK6bmI=</Data>
        // </Key>
        // </KeyFile>

        /// <summary>
        /// The load xml key file.
        /// </summary>
        /// <param name="pbFileData">
        /// The pb file data.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        private static byte[] LoadXmlKeyFile(byte[] pbFileData)
        {
            if (pbFileData == null)
            {
                Debug.Assert(false);
                return null;
            }

            MemoryStream ms = new MemoryStream(pbFileData, false);
            byte[] pbKeyData = null;

            try
            {
                XmlDocument doc = new XmlDocument();
                var array = ms.ToArray();
                doc.LoadXml(Encoding.UTF8.GetString(array, 0, array.Length));

                XmlElement el = doc.DocumentElement;
                if ((el == null) || !el.NodeName.Equals(RootElementName))
                {
                    return null;
                }

                if (el.ChildNodes.Count < 2)
                {
                    return null;
                }

                foreach (var xmlChild in el.ChildNodes)
                {
                    if (xmlChild.NodeName.Equals(MetaElementName))
                    {
                    }
 // Ignore Meta
                    else if (xmlChild.NodeName == KeyElementName)
                    {
                        foreach (var xmlKeyChild in xmlChild.ChildNodes)
                        {
                            if (xmlKeyChild.NodeName == KeyDataElementName)
                            {
                                if (pbKeyData == null)
                                {
                                    pbKeyData = Convert.FromBase64String(xmlKeyChild.InnerText);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                pbKeyData = null;
            }
            finally
            {
                ms.Dispose();
            }

            return pbKeyData;
        }

        /// <summary>
        /// The construct.
        /// </summary>
        /// <param name="iocFile">
        /// The ioc file.
        /// </param>
        /// <param name="bThrowIfDbFile">
        /// The b throw if db file.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        private void Construct(Stream fileStream, bool bThrowIfDbFile)
        {
            var data = new byte[fileStream.Length];
            fileStream.Read(data, 0, (int)fileStream.Length);

            if (bThrowIfDbFile && (data.Length >= 8))
            {
                uint uSig1 = MemUtil.BytesToUInt32(MemUtil.Mid(data, 0, 4));
                uint uSig2 = MemUtil.BytesToUInt32(MemUtil.Mid(data, 4, 4));

                if (((uSig1 == KdbxFile.FileSignature1) &&
                    (uSig2 == KdbxFile.FileSignature2)) ||
                    ((uSig1 == KdbxFile.FileSignaturePreRelease1) &&
                    (uSig2 == KdbxFile.FileSignaturePreRelease2)) ||
                    ((uSig1 == KdbxFile.FileSignatureOld1) &&
                    (uSig2 == KdbxFile.FileSignatureOld2)))
#if KeePassLibSD
					throw new Exception(KLRes.KeyFileDbSel);
#else
                    throw new InvalidDataException(KLRes.KeyFileDbSel);
#endif
            }

            var pbKey = LoadXmlKeyFile(data) ?? LoadKeyFile(data);

            if (pbKey == null)
            {
                throw new InvalidOperationException();
            }

            m_pbKeyData = new ProtectedBinary(true, pbKey);

            MemUtil.ZeroByteArray(pbKey);
        }

        #endregion
    }
}