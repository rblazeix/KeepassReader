// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HashingStreamEx.cs" company="">
//   
// </copyright>
// <summary>
//   The hashing stream ex.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Cryptography
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using KeePassLib.Utility;

    using Org.BouncyCastle.Crypto.Digests;

    /// <summary>
    /// The hashing stream ex.
    /// </summary>
    public sealed class HashingStreamEx : Stream
    {
        #region Fields

        /// <summary>
        /// The m_b writing.
        /// </summary>
        private bool m_bWriting;

        /// <summary>
        /// The m_hash.
        /// </summary>
        private GeneralDigest m_hash;

        /// <summary>
        /// The m_pb final hash.
        /// </summary>
        private byte[] m_pbFinalHash = null;

        /// <summary>
        /// The m_s base stream.
        /// </summary>
        private Stream m_sBaseStream;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HashingStreamEx"/> class.
        /// </summary>
        /// <param name="sBaseStream">
        /// The s base stream.
        /// </param>
        /// <param name="bWriting">
        /// The b writing.
        /// </param>
        /// <param name="hashAlgorithm">
        /// The hash algorithm.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public HashingStreamEx(Stream sBaseStream, bool bWriting, GeneralDigest hashAlgorithm)
        {
            if (sBaseStream == null)
            {
                throw new ArgumentNullException("sBaseStream");
            }

            this.m_sBaseStream = sBaseStream;
            this.m_bWriting = bWriting;

            this.m_hash = hashAlgorithm ?? new Sha256Digest();

            if (this.m_hash == null)
            {
                Debug.Assert(false);
            }
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
        /// Gets the hash.
        /// </summary>
        public byte[] Hash
        {
            get
            {
                return this.m_pbFinalHash;
            }
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        public override long Length
        {
            get
            {
                return this.m_sBaseStream.Length;
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
                return this.m_sBaseStream.Position;
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
            this.m_sBaseStream.Flush();
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

            int nRead = this.m_sBaseStream.Read(pbBuffer, nOffset, nCount);
            int nPartialRead = nRead;
            while ((nRead < nCount) && (nPartialRead != 0))
            {
                nPartialRead = this.m_sBaseStream.Read(pbBuffer, nOffset + nRead, nCount - nRead);
                nRead += nPartialRead;
            }

#if DEBUG
            byte[] pbOrg = new byte[pbBuffer.Length];
            Array.Copy(pbBuffer, pbOrg, pbBuffer.Length);
#endif

            if ((this.m_hash != null) && (nRead > 0))
            {
                this.m_hash.BlockUpdate(pbBuffer, nOffset, nRead);
            }

#if DEBUG
            Debug.Assert(MemUtil.ArraysEqual(pbBuffer, pbOrg));
#endif

            return nRead;
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
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void Write(byte[] pbBuffer, int nOffset, int nCount)
        {
            throw new NotImplementedException();
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

            if (this.m_hash != null)
            {
                try
                {
                    this.m_hash.DoFinal(this.m_pbFinalHash, 0);
                }
                catch (Exception e)
                {
                    //Debug.Assert(false);
                }

                this.m_hash = null;
            }

            this.m_sBaseStream.Dispose();
        }

        #endregion
    }
}