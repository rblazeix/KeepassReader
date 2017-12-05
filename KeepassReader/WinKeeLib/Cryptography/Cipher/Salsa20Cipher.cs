// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Salsa20Cipher.cs" company="">
//   
// </copyright>
// <summary>
//   The salsa 20 cipher.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Cryptography.Cipher
{
    using System;
    using System.Diagnostics;

    using KeePassLib.Utility;

    /// <summary>
    /// The salsa 20 cipher.
    /// </summary>
    public sealed class Salsa20Cipher
    {
        #region Static Fields

        /// <summary>
        /// The m_sigma.
        /// </summary>
        private static readonly uint[] m_sigma = new uint[4] { 0x61707865, 0x3320646E, 0x79622D32, 0x6B206574 };

        #endregion

        #region Fields

        /// <summary>
        /// The m_output.
        /// </summary>
        private byte[] m_output = new byte[64];

        /// <summary>
        /// The m_output pos.
        /// </summary>
        private int m_outputPos = 64;

        /// <summary>
        /// The m_state.
        /// </summary>
        private uint[] m_state = new uint[16];

        /// <summary>
        /// The m_x.
        /// </summary>
        private uint[] m_x = new uint[16]; // Working buffer

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Salsa20Cipher"/> class.
        /// </summary>
        /// <param name="pbKey32">
        /// The pb key 32.
        /// </param>
        /// <param name="pbIV8">
        /// The pb i v 8.
        /// </param>
        public Salsa20Cipher(byte[] pbKey32, byte[] pbIV8)
        {
            this.KeySetup(pbKey32);
            this.IvSetup(pbIV8);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Salsa20Cipher"/> class. 
        /// </summary>
        ~Salsa20Cipher()
        {
            // Clear sensitive data
            Array.Clear(this.m_state, 0, this.m_state.Length);
            Array.Clear(this.m_x, 0, this.m_x.Length);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The encrypt.
        /// </summary>
        /// <param name="m">
        /// The m.
        /// </param>
        /// <param name="nByteCount">
        /// The n byte count.
        /// </param>
        /// <param name="bXor">
        /// The b xor.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void Encrypt(byte[] m, int nByteCount, bool bXor)
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }

            if (nByteCount > m.Length)
            {
                throw new ArgumentException();
            }

            int nBytesRem = nByteCount, nOffset = 0;
            while (nBytesRem > 0)
            {
                Debug.Assert((this.m_outputPos >= 0) && (this.m_outputPos <= 64));
                if (this.m_outputPos == 64)
                {
                    this.NextOutput();
                }

                Debug.Assert(this.m_outputPos < 64);

                int nCopy = Math.Min(64 - this.m_outputPos, nBytesRem);

                if (bXor)
                {
                    MemUtil.XorArray(this.m_output, this.m_outputPos, m, nOffset, nCopy);
                }
                else
                {
                    Array.Copy(this.m_output, this.m_outputPos, m, nOffset, nCopy);
                }

                this.m_outputPos += nCopy;
                nBytesRem -= nCopy;
                nOffset += nCopy;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The rotl 32.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        private static uint Rotl32(uint x, int b)
        {
            unchecked
            {
                return (x << b) | (x >> (32 - b));
            }
        }

        /// <summary>
        /// The u 8 to 32 little.
        /// </summary>
        /// <param name="pb">
        /// The pb.
        /// </param>
        /// <param name="iOffset">
        /// The i offset.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        private static uint U8To32Little(byte[] pb, int iOffset)
        {
            unchecked
            {
                return (uint)pb[iOffset] | ((uint)pb[iOffset + 1] << 8) | ((uint)pb[iOffset + 2] << 16) | ((uint)pb[iOffset + 3] << 24);
            }
        }

        /// <summary>
        /// The iv setup.
        /// </summary>
        /// <param name="pbIV">
        /// The pb iv.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentException">
        /// </exception>
        private void IvSetup(byte[] pbIV)
        {
            if (pbIV == null)
            {
                throw new ArgumentNullException("pbIV");
            }

            if (pbIV.Length != 8)
            {
                throw new ArgumentException();
            }

            this.m_state[6] = U8To32Little(pbIV, 0);
            this.m_state[7] = U8To32Little(pbIV, 4);
            this.m_state[8] = 0;
            this.m_state[9] = 0;
        }

        /// <summary>
        /// The key setup.
        /// </summary>
        /// <param name="k">
        /// The k.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentException">
        /// </exception>
        private void KeySetup(byte[] k)
        {
            if (k == null)
            {
                throw new ArgumentNullException("k");
            }

            if (k.Length != 32)
            {
                throw new ArgumentException();
            }

            this.m_state[1] = U8To32Little(k, 0);
            this.m_state[2] = U8To32Little(k, 4);
            this.m_state[3] = U8To32Little(k, 8);
            this.m_state[4] = U8To32Little(k, 12);
            this.m_state[11] = U8To32Little(k, 16);
            this.m_state[12] = U8To32Little(k, 20);
            this.m_state[13] = U8To32Little(k, 24);
            this.m_state[14] = U8To32Little(k, 28);
            this.m_state[0] = m_sigma[0];
            this.m_state[5] = m_sigma[1];
            this.m_state[10] = m_sigma[2];
            this.m_state[15] = m_sigma[3];
        }

        /// <summary>
        /// The next output.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private void NextOutput()
        {
            uint[] x = this.m_x; // Local alias for working buffer

            // Compiler/runtime might remove array bound checks after this
            if (x.Length < 16)
            {
                throw new InvalidOperationException();
            }

            Array.Copy(this.m_state, x, 16);

            unchecked
            {
                for (int i = 0; i < 10; ++i)
                {
                    // (int i = 20; i > 0; i -= 2)
                    x[4] ^= Rotl32(x[0] + x[12], 7);
                    x[8] ^= Rotl32(x[4] + x[0], 9);
                    x[12] ^= Rotl32(x[8] + x[4], 13);
                    x[0] ^= Rotl32(x[12] + x[8], 18);
                    x[9] ^= Rotl32(x[5] + x[1], 7);
                    x[13] ^= Rotl32(x[9] + x[5], 9);
                    x[1] ^= Rotl32(x[13] + x[9], 13);
                    x[5] ^= Rotl32(x[1] + x[13], 18);
                    x[14] ^= Rotl32(x[10] + x[6], 7);
                    x[2] ^= Rotl32(x[14] + x[10], 9);
                    x[6] ^= Rotl32(x[2] + x[14], 13);
                    x[10] ^= Rotl32(x[6] + x[2], 18);
                    x[3] ^= Rotl32(x[15] + x[11], 7);
                    x[7] ^= Rotl32(x[3] + x[15], 9);
                    x[11] ^= Rotl32(x[7] + x[3], 13);
                    x[15] ^= Rotl32(x[11] + x[7], 18);
                    x[1] ^= Rotl32(x[0] + x[3], 7);
                    x[2] ^= Rotl32(x[1] + x[0], 9);
                    x[3] ^= Rotl32(x[2] + x[1], 13);
                    x[0] ^= Rotl32(x[3] + x[2], 18);
                    x[6] ^= Rotl32(x[5] + x[4], 7);
                    x[7] ^= Rotl32(x[6] + x[5], 9);
                    x[4] ^= Rotl32(x[7] + x[6], 13);
                    x[5] ^= Rotl32(x[4] + x[7], 18);
                    x[11] ^= Rotl32(x[10] + x[9], 7);
                    x[8] ^= Rotl32(x[11] + x[10], 9);
                    x[9] ^= Rotl32(x[8] + x[11], 13);
                    x[10] ^= Rotl32(x[9] + x[8], 18);
                    x[12] ^= Rotl32(x[15] + x[14], 7);
                    x[13] ^= Rotl32(x[12] + x[15], 9);
                    x[14] ^= Rotl32(x[13] + x[12], 13);
                    x[15] ^= Rotl32(x[14] + x[13], 18);
                }

                for (int i = 0; i < 16; ++i)
                {
                    x[i] += this.m_state[i];
                }

                for (int i = 0; i < 16; ++i)
                {
                    this.m_output[i << 2] = (byte)x[i];
                    this.m_output[(i << 2) + 1] = (byte)(x[i] >> 8);
                    this.m_output[(i << 2) + 2] = (byte)(x[i] >> 16);
                    this.m_output[(i << 2) + 3] = (byte)(x[i] >> 24);
                }

                this.m_outputPos = 0;
                ++this.m_state[8];
                if (this.m_state[8] == 0)
                {
                    ++this.m_state[9];
                }
            }
        }

        #endregion
    }
}