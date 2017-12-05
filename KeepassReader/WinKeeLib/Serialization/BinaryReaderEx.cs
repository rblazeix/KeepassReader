// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryReaderEx.cs" company="">
//   
// </copyright>
// <summary>
//   The binary reader ex.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Serialization
{
    using System;
    using System.IO;
    using System.Text;

    using KeePassLib.Utility;

    /// <summary>
    /// The binary reader ex.
    /// </summary>
    public sealed class BinaryReaderEx
    {
        #region Fields

        /// <summary>
        /// The m_s.
        /// </summary>
        private Stream m_s;

        // private Encoding m_enc; // See constructor

        /// <summary>
        /// The m_s copy to.
        /// </summary>
        private Stream m_sCopyTo = null;

        /// <summary>
        /// The m_str read excp.
        /// </summary>
        private string m_strReadExcp;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryReaderEx"/> class.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="encoding">
        /// The encoding.
        /// </param>
        /// <param name="strReadExceptionText">
        /// The str read exception text.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public BinaryReaderEx(Stream input, Encoding encoding, string strReadExceptionText)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            this.m_s = input;

            // m_enc = encoding; // Not used yet
            this.m_strReadExcp = strReadExceptionText;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// If this property is set to a non-null stream, all data that
        /// is read from the input stream is automatically written to
        /// the copy stream (before returning the read data).
        /// </summary>
        public Stream CopyDataTo
        {
            get
            {
                return this.m_sCopyTo;
            }

            set
            {
                this.m_sCopyTo = value;
            }
        }

        /// <summary>
        /// Gets or sets the read exception text.
        /// </summary>
        public string ReadExceptionText
        {
            get
            {
                return this.m_strReadExcp;
            }

            set
            {
                this.m_strReadExcp = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The read byte.
        /// </summary>
        /// <returns>
        /// The <see cref="byte"/>.
        /// </returns>
        public byte ReadByte()
        {
            byte[] pb = this.ReadBytes(1);
            return pb[0];
        }

        /// <summary>
        /// The read bytes.
        /// </summary>
        /// <param name="nCount">
        /// The n count.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        /// <exception cref="IOException">
        /// </exception>
        /// <exception cref="EndOfStreamException">
        /// </exception>
        public byte[] ReadBytes(int nCount)
        {
            try
            {
                byte[] pb = MemUtil.Read(this.m_s, nCount);
                if ((pb == null) || (pb.Length != nCount))
                {
                    if (this.m_strReadExcp != null)
                    {
                        throw new IOException(this.m_strReadExcp);
                    }
                    else
                    {
                        throw new EndOfStreamException();
                    }
                }

                if (this.m_sCopyTo != null)
                {
                    this.m_sCopyTo.Write(pb, 0, pb.Length);
                }

                return pb;
            }
            catch (Exception)
            {
                if (this.m_strReadExcp != null)
                {
                    throw new IOException(this.m_strReadExcp);
                }
                else
                {
                    throw;
                }
            }
        }

        #endregion
    }
}