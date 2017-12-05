// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CryptoRandomStream.cs" company="">
//   
// </copyright>
// <summary>
//   Algorithms supported by <c>CryptoRandomStream</c>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Cryptography
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices.WindowsRuntime;

    using KeePassLib.Cryptography.Cipher;

    using Windows.Security.Cryptography.Core;

    /// <summary>
    /// Algorithms supported by <c>CryptoRandomStream</c>.
    /// </summary>
    public enum CrsAlgorithm
    {
        /// <summary>
        /// Not supported.
        /// </summary>
        Null = 0, 

        /// <summary>
        /// A variant of the ARCFour algorithm (RC4 incompatible).
        /// </summary>
        ArcFourVariant = 1, 

        /// <summary>
        /// Salsa20 stream cipher algorithm.
        /// </summary>
        Salsa20 = 2, 

        /// <summary>
        /// The count.
        /// </summary>
        Count = 3
    }

    /// <summary>
    /// A random stream class. The class is initialized using random
    /// bytes provided by the caller. The produced stream has random
    /// properties, but for the same seed always the same stream
    /// is produced, i.e. this class can be used as stream cipher.
    /// </summary>
    public sealed class CryptoRandomStream
    {
        /// <summary>
        /// The m_crs algorithm.
        /// </summary>
        private CrsAlgorithm m_crsAlgorithm;

        /// <summary>
        /// The m_pb state.
        /// </summary>
        private byte[] m_pbState = null;

        /// <summary>
        /// The m_i.
        /// </summary>
        private byte m_i = 0;

        /// <summary>
        /// The m_j.
        /// </summary>
        private byte m_j = 0;

        /// <summary>
        /// The m_salsa 20.
        /// </summary>
        private Salsa20Cipher m_salsa20 = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoRandomStream"/> class. 
        /// Construct a new cryptographically secure random stream object.
        /// </summary>
        /// <param name="genAlgorithm">
        /// Algorithm to use.
        /// </param>
        /// <param name="pbKey">
        /// Initialization key. Must not be <c>null</c> and
        /// must contain at least 1 byte.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the
        /// <paramref name="pbKey"/> parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the
        /// <paramref name="pbKey"/> parameter contains no bytes or the
        /// algorithm is unknown.
        /// </exception>
        public CryptoRandomStream(CrsAlgorithm genAlgorithm, byte[] pbKey)
        {
            this.m_crsAlgorithm = genAlgorithm;

            Debug.Assert(pbKey != null);
            if (pbKey == null)
            {
                throw new ArgumentNullException("pbKey");
            }

            uint uKeyLen = (uint)pbKey.Length;
            Debug.Assert(uKeyLen != 0);
            if (uKeyLen == 0)
            {
                throw new ArgumentException();
            }

            if (genAlgorithm == CrsAlgorithm.ArcFourVariant)
            {
                // Fill the state linearly
                this.m_pbState = new byte[256];
                for (uint w = 0; w < 256; ++w)
                {
                    this.m_pbState[w] = (byte)w;
                }

                unchecked
                {
                    byte j = 0, t;
                    uint inxKey = 0;
                    for (uint w = 0; w < 256; ++w)
                    {
                        // Key setup
                        j += (byte)(this.m_pbState[w] + pbKey[inxKey]);

                        t = this.m_pbState[0]; // Swap entries
                        this.m_pbState[0] = this.m_pbState[j];
                        this.m_pbState[j] = t;

                        ++inxKey;
                        if (inxKey >= uKeyLen)
                        {
                            inxKey = 0;
                        }
                    }
                }

                this.GetRandomBytes(512); // Increases security, see cryptanalysis
            }
            else if (genAlgorithm == CrsAlgorithm.Salsa20)
            {
                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);

                byte[] pbKey32 = sha256.HashData(pbKey.AsBuffer()).ToArray();
                byte[] pbIV = new byte[] { 0xE8, 0x30, 0x09, 0x4B, 0x97, 0x20, 0x5D, 0x2A }; // Unique constant

                this.m_salsa20 = new Salsa20Cipher(pbKey32, pbIV);
            }
            else
            {
                // Unknown algorithm
                Debug.Assert(false);
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Get <paramref name="uRequestedCount"/> random bytes.
        /// </summary>
        /// <param name="uRequestedCount">
        /// Number of random bytes to retrieve.
        /// </param>
        /// <returns>
        /// Returns <paramref name="uRequestedCount"/> random bytes.
        /// </returns>
        public byte[] GetRandomBytes(uint uRequestedCount)
        {
            if (uRequestedCount == 0)
            {
                return new byte[0];
            }

            byte[] pbRet = new byte[uRequestedCount];

            if (this.m_crsAlgorithm == CrsAlgorithm.ArcFourVariant)
            {
                unchecked
                {
                    for (uint w = 0; w < uRequestedCount; ++w)
                    {
                        ++this.m_i;
                        this.m_j += this.m_pbState[this.m_i];

                        byte t = this.m_pbState[this.m_i]; // Swap entries
                        this.m_pbState[this.m_i] = this.m_pbState[this.m_j];
                        this.m_pbState[this.m_j] = t;

                        t = (byte)(this.m_pbState[this.m_i] + this.m_pbState[this.m_j]);
                        pbRet[w] = this.m_pbState[t];
                    }
                }
            }
            else if (this.m_crsAlgorithm == CrsAlgorithm.Salsa20)
            {
                this.m_salsa20.Encrypt(pbRet, pbRet.Length, false);
            }
            else
            {
                Debug.Assert(false);
            }

            return pbRet;
        }

        /// <summary>
        /// The get random u int 64.
        /// </summary>
        /// <returns>
        /// The <see cref="ulong"/>.
        /// </returns>
        public ulong GetRandomUInt64()
        {
            byte[] pb = this.GetRandomBytes(8);

            unchecked
            {
                return ((ulong)pb[0]) | ((ulong)pb[1] << 8) | ((ulong)pb[2] << 16) | ((ulong)pb[3] << 24) | ((ulong)pb[4] << 32)
                       | ((ulong)pb[5] << 40) | ((ulong)pb[6] << 48) | ((ulong)pb[7] << 56);
            }
        }

#if CRSBENCHMARK
		public static string Benchmark()
		{
			int nRounds = 2000000;
			
			string str = "ArcFour small: " + BenchTime(CrsAlgorithm.ArcFourVariant, 
				nRounds, 16).ToString() + "\r\n";
			str += "ArcFour big: " + BenchTime(CrsAlgorithm.ArcFourVariant, 
				32, 2 * 1024 * 1024).ToString() + "\r\n";
			str += "Salsa20 small: " + BenchTime(CrsAlgorithm.Salsa20, 
				nRounds, 16).ToString() + "\r\n";
			str += "Salsa20 big: " + BenchTime(CrsAlgorithm.Salsa20, 
				32, 2 * 1024 * 1024).ToString();
			return str;
		}

		private static int BenchTime(CrsAlgorithm cra, int nRounds, int nDataSize)
		{
			byte[] pbKey = new byte[4] { 0x00, 0x01, 0x02, 0x03 };

			int nStart = Environment.TickCount;
			for(int i = 0; i < nRounds; ++i)
			{
				CryptoRandomStream c = new CryptoRandomStream(cra, pbKey);
				c.GetRandomBytes((uint)nDataSize);
			}
			int nEnd = Environment.TickCount;

			return (nEnd - nStart);
		}
#endif
    }
}