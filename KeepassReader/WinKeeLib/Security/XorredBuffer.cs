// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XorredBuffer.cs" company="">
//   
// </copyright>
// <summary>
//   Represents an object that is encrypted using a XOR pad until
//   it is read. <c>XorredBuffer</c> objects are immutable and
//   thread-safe.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Security
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents an object that is encrypted using a XOR pad until
    /// it is read. <c>XorredBuffer</c> objects are immutable and
    /// thread-safe.
    /// </summary>
    public sealed class XorredBuffer
    {
        #region Fields

        /// <summary>
        /// The m_pb data.
        /// </summary>
        private byte[] m_pbData; // Never null

        /// <summary>
        /// The m_pb xor pad.
        /// </summary>
        private byte[] m_pbXorPad; // Always valid for m_pbData

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XorredBuffer"/> class. 
        /// Construct a new XOR-protected object using a protected byte array
        /// and a XOR pad that decrypts the protected data. The
        /// <paramref name="pbProtectedData"/> byte array must have the same size
        /// as the <paramref name="pbXorPad"/> byte array.
        /// The <c>XorredBuffer</c> object takes ownership of the two byte
        /// arrays, i.e. the caller must not use or modify them afterwards.
        /// </summary>
        /// <param name="pbProtectedData">
        /// Protected data (XOR pad applied).
        /// </param>
        /// <param name="pbXorPad">
        /// XOR pad that can be used to decrypt the
        /// <paramref name="pbProtectedData"/> parameter.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if one of the input
        /// parameters is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the byte arrays are
        /// of different size.
        /// </exception>
        public XorredBuffer(byte[] pbProtectedData, byte[] pbXorPad)
        {
            if (pbProtectedData == null)
            {
                Debug.Assert(false);
                throw new ArgumentNullException("pbProtectedData");
            }

            if (pbXorPad == null)
            {
                Debug.Assert(false);
                throw new ArgumentNullException("pbXorPad");
            }

            Debug.Assert(pbProtectedData.Length == pbXorPad.Length);
            if (pbProtectedData.Length != pbXorPad.Length)
            {
                throw new ArgumentException();
            }

            this.m_pbData = pbProtectedData;
            this.m_pbXorPad = pbXorPad;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Length of the protected data in bytes.
        /// </summary>
        public uint Length
        {
            get
            {
                return (uint)this.m_pbData.Length;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Get a copy of the plain-text. The caller is responsible
        /// for clearing the byte array safely after using it.
        /// </summary>
        /// <returns>Unprotected plain-text byte array.</returns>
        public byte[] ReadPlainText()
        {
            byte[] pbPlain = new byte[this.m_pbData.Length];

            for (int i = 0; i < pbPlain.Length; ++i)
            {
                pbPlain[i] = (byte)(this.m_pbData[i] ^ this.m_pbXorPad[i]);
            }

            return pbPlain;
        }

        #endregion

        /* public bool EqualsValue(XorredBuffer xb)
		{
			if(xb == null) { Debug.Assert(false); throw new ArgumentNullException("xb"); }

			if(xb.m_pbData.Length != m_pbData.Length) return false;

			for(int i = 0; i < m_pbData.Length; ++i)
			{
				byte bt1 = (byte)(m_pbData[i] ^ m_pbXorPad[i]);
				byte bt2 = (byte)(xb.m_pbData[i] ^ xb.m_pbXorPad[i]);

				if(bt1 != bt2) return false;
			}

			return true;
		}

		public bool EqualsValue(byte[] pb)
		{
			if(pb == null) { Debug.Assert(false); throw new ArgumentNullException("pb"); }

			if(pb.Length != m_pbData.Length) return false;

			for(int i = 0; i < m_pbData.Length; ++i)
			{
				if((byte)(m_pbData[i] ^ m_pbXorPad[i]) != pb[i]) return false;
			}

			return true;
		} */
    }
}