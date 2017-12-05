// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HashedBlockStream.cs" company="">
//   
// </copyright>
// <summary>
//   The hashed block stream.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if KeePassLibSD
using KeePassLibSD;
#endif

namespace KeePassLib.Serialization
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;

    using KeePassLib.Utility;

    using Windows.Security.Cryptography.Core;

    /// <summary>
    /// The hashed block stream.
    /// </summary>
    public sealed class HashedBlockStream : Stream
    {
        #region Constants

        /// <summary>
        /// The m_n default buffer size.
        /// </summary>
        private const int m_nDefaultBufferSize = 1024 * 1024; // 1 MB

        #endregion

        #region Fields

        /// <summary>
        /// The m_b eos.
        /// </summary>
        private bool m_bEos = false;

        /// <summary>
        /// The m_b verify.
        /// </summary>
        private bool m_bVerify;

        /// <summary>
        /// The m_b writing.
        /// </summary>
        private bool m_bWriting;

        /// <summary>
        /// The m_br input.
        /// </summary>
        private BinaryReader m_brInput;

        /// <summary>
        /// The m_bw output.
        /// </summary>
        private BinaryWriter m_bwOutput;

        /// <summary>
        /// The m_n buffer pos.
        /// </summary>
        private int m_nBufferPos = 0;

        /// <summary>
        /// The m_pb buffer.
        /// </summary>
        private byte[] m_pbBuffer;

        /// <summary>
        /// The m_s base stream.
        /// </summary>
        private Stream m_sBaseStream;

        /// <summary>
        /// The m_u buffer index.
        /// </summary>
        private uint m_uBufferIndex = 0;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HashedBlockStream"/> class.
        /// </summary>
        /// <param name="sBaseStream">
        /// The s base stream.
        /// </param>
        /// <param name="bWriting">
        /// The b writing.
        /// </param>
        public HashedBlockStream(Stream sBaseStream, bool bWriting)
        {
            this.Initialize(sBaseStream, bWriting, 0, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashedBlockStream"/> class.
        /// </summary>
        /// <param name="sBaseStream">
        /// The s base stream.
        /// </param>
        /// <param name="bWriting">
        /// The b writing.
        /// </param>
        /// <param name="nBufferSize">
        /// The n buffer size.
        /// </param>
        public HashedBlockStream(Stream sBaseStream, bool bWriting, int nBufferSize)
        {
            this.Initialize(sBaseStream, bWriting, nBufferSize, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashedBlockStream"/> class.
        /// </summary>
        /// <param name="sBaseStream">
        /// The s base stream.
        /// </param>
        /// <param name="bWriting">
        /// The b writing.
        /// </param>
        /// <param name="nBufferSize">
        /// The n buffer size.
        /// </param>
        /// <param name="bVerify">
        /// The b verify.
        /// </param>
        public HashedBlockStream(Stream sBaseStream, bool bWriting, int nBufferSize, bool bVerify)
        {
            this.Initialize(sBaseStream, bWriting, nBufferSize, bVerify);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether can read.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return !this.m_bWriting;
            }
        }

        /// <summary>
        /// Gets a value indicating whether can seek.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether can write.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return this.m_bWriting;
            }
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// </exception>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// </exception>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The flush.
        /// </summary>
        public override void Flush()
        {
            if (this.m_bWriting)
            {
                this.m_bwOutput.Flush();
            }
        }

        /// <summary>
        /// The read.
        /// </summary>
        /// <param name="pbBuffer">
        /// The pb buffer.
        /// </param>
        /// <param name="nOffset">
        /// The n offset.
        /// </param>
        /// <param name="nCount">
        /// The n count.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public override int Read(byte[] pbBuffer, int nOffset, int nCount)
        {
            if (this.m_bWriting)
            {
                throw new InvalidOperationException();
            }

            int nRemaining = nCount;
            while (nRemaining > 0)
            {
                if (this.m_nBufferPos == this.m_pbBuffer.Length)
                {
                    if (this.ReadHashedBlock() == false)
                    {
                        return nCount - nRemaining; // Bytes actually read
                    }
                }

                int nCopy = Math.Min(this.m_pbBuffer.Length - this.m_nBufferPos, nRemaining);

                Array.Copy(this.m_pbBuffer, this.m_nBufferPos, pbBuffer, nOffset, nCopy);

                nOffset += nCopy;
                this.m_nBufferPos += nCopy;

                nRemaining -= nCopy;
            }

            return nCount;
        }

        /// <summary>
        /// The seek.
        /// </summary>
        /// <param name="lOffset">
        /// The l offset.
        /// </param>
        /// <param name="soOrigin">
        /// The so origin.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        public override long Seek(long lOffset, SeekOrigin soOrigin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The set length.
        /// </summary>
        /// <param name="lValue">
        /// The l value.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// </exception>
        public override void SetLength(long lValue)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="pbBuffer">
        /// The pb buffer.
        /// </param>
        /// <param name="nOffset">
        /// The n offset.
        /// </param>
        /// <param name="nCount">
        /// The n count.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public override void Write(byte[] pbBuffer, int nOffset, int nCount)
        {
            if (!this.m_bWriting)
            {
                throw new InvalidOperationException();
            }

            while (nCount > 0)
            {
                if (this.m_nBufferPos == this.m_pbBuffer.Length)
                {
                    this.WriteHashedBlock();
                }

                int nCopy = Math.Min(this.m_pbBuffer.Length - this.m_nBufferPos, nCount);

                Array.Copy(pbBuffer, nOffset, this.m_pbBuffer, this.m_nBufferPos, nCopy);

                nOffset += nCopy;
                this.m_nBufferPos += nCopy;

                nCount -= nCopy;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (this.m_sBaseStream == null)
            {
                return;
            }

            if (this.m_bWriting == false)
            {
                // Reading mode
                this.m_brInput.Dispose();
                this.m_brInput = null;
            }
            else
            {
                // Writing mode
                if (this.m_nBufferPos == 0)
                {
                    // No data left in buffer
                    this.WriteHashedBlock(); // Write terminating block
                }
                else
                {
                    this.WriteHashedBlock(); // Write remaining buffered data
                    this.WriteHashedBlock(); // Write terminating block
                }

                this.Flush();
                this.m_bwOutput.Dispose();
                this.m_bwOutput = null;
            }

            this.m_sBaseStream.Dispose();
            this.m_sBaseStream = null;
        }

        /// <summary>
        /// The initialize.
        /// </summary>
        /// <param name="sBaseStream">
        /// The s base stream.
        /// </param>
        /// <param name="bWriting">
        /// The b writing.
        /// </param>
        /// <param name="nBufferSize">
        /// The n buffer size.
        /// </param>
        /// <param name="bVerify">
        /// The b verify.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private void Initialize(Stream sBaseStream, bool bWriting, int nBufferSize, bool bVerify)
        {
            if (sBaseStream == null)
            {
                throw new ArgumentNullException("sBaseStream");
            }

            if (nBufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("nBufferSize");
            }

            if (nBufferSize == 0)
            {
                nBufferSize = m_nDefaultBufferSize;
            }

            this.m_sBaseStream = sBaseStream;
            this.m_bWriting = bWriting;
            this.m_bVerify = bVerify;

            UTF8Encoding utf8 = StrUtil.Utf8;
            if (this.m_bWriting == false)
            {
                // Reading mode
                if (this.m_sBaseStream.CanRead == false)
                {
                    throw new InvalidOperationException();
                }

                this.m_brInput = new BinaryReader(sBaseStream, utf8);

                this.m_pbBuffer = new byte[0];
            }
            else
            {
                // Writing mode
                if (this.m_sBaseStream.CanWrite == false)
                {
                    throw new InvalidOperationException();
                }

                this.m_bwOutput = new BinaryWriter(sBaseStream, utf8);

                this.m_pbBuffer = new byte[nBufferSize];
            }
        }

        /// <summary>
        /// The read hashed block.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="InvalidDataException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private bool ReadHashedBlock()
        {
            if (this.m_bEos)
            {
                return false; // End of stream reached already
            }

            this.m_nBufferPos = 0;

            if (this.m_brInput.ReadUInt32() != this.m_uBufferIndex)
            {
                throw new InvalidDataException();
            }

            ++this.m_uBufferIndex;

            byte[] pbStoredHash = this.m_brInput.ReadBytes(32);
            if ((pbStoredHash == null) || (pbStoredHash.Length != 32))
            {
                throw new InvalidDataException();
            }

            int nBufferSize = 0;
            nBufferSize = this.m_brInput.ReadInt32();

            if (nBufferSize < 0)
            {
                throw new InvalidDataException();
            }

            if (nBufferSize == 0)
            {
                for (int iHash = 0; iHash < 32; ++iHash)
                {
                    if (pbStoredHash[iHash] != 0)
                    {
                        throw new InvalidDataException();
                    }
                }

                this.m_bEos = true;
                this.m_pbBuffer = new byte[0];
                return false;
            }

            this.m_pbBuffer = this.m_brInput.ReadBytes(nBufferSize);
            if ((this.m_pbBuffer == null) || ((this.m_pbBuffer.Length != nBufferSize) && this.m_bVerify))
            {
                throw new InvalidDataException();
            }

            if (this.m_bVerify)
            {
                var sha256 = Windows.Security.Cryptography.Core.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                byte[] pbComputedHash = sha256.HashData(this.m_pbBuffer.AsBuffer()).ToArray();
                if ((pbComputedHash == null) || (pbComputedHash.Length != 32))
                {
                    throw new InvalidOperationException();
                }

                for (int iHashPos = 0; iHashPos < 32; ++iHashPos)
                {
                    if (pbStoredHash[iHashPos] != pbComputedHash[iHashPos])
                    {
                        throw new InvalidDataException();
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// The write hashed block.
        /// </summary>
        private void WriteHashedBlock()
        {
            this.m_bwOutput.Write(this.m_uBufferIndex);
            ++this.m_uBufferIndex;

            if (this.m_nBufferPos > 0)
            {
                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);

                byte[] pbHash = sha256.HashData(this.m_pbBuffer.AsBuffer(0, this.m_nBufferPos)).ToArray();

                this.m_bwOutput.Write(pbHash);
            }
            else
            {
                this.m_bwOutput.Write((ulong)0); // Zero hash
                this.m_bwOutput.Write((ulong)0);
                this.m_bwOutput.Write((ulong)0);
                this.m_bwOutput.Write((ulong)0);
            }

            this.m_bwOutput.Write(this.m_nBufferPos);

            if (this.m_nBufferPos > 0)
            {
                this.m_bwOutput.Write(this.m_pbBuffer, 0, this.m_nBufferPos);
            }

            this.m_nBufferPos = 0;
        }

        #endregion
    }
}