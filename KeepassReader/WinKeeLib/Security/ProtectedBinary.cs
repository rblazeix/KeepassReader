// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProtectedBinary.cs" company="">
//   
// </copyright>
// <summary>
//   Represents a protected binary, i.e. a byte array that is encrypted
//   in memory. A <c>ProtectedBinary</c> object is immutable and
//   thread-safe.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Security
{
    using System;
    using System.Diagnostics;

    using KeePassLib.Cryptography;
    using KeePassLib.Utility;

    /// <summary>
    /// Represents a protected binary, i.e. a byte array that is encrypted
    /// in memory. A <c>ProtectedBinary</c> object is immutable and
    /// thread-safe.
    /// </summary>
    public sealed class ProtectedBinary : IEquatable<ProtectedBinary>
    {
        #region Constants

        /// <summary>
        /// The pm block size.
        /// </summary>
        private const int PmBlockSize = 16;

        #endregion

        // In-memory protection is supported only on Windows 2000 SP3 and
        // higher.
        #region Static Fields

        /// <summary>
        /// The m_b protection supported.
        /// </summary>
        private static bool m_bProtectionSupported;

        #endregion

        #region Fields

        /// <summary>
        /// The m_b protected.
        /// </summary>
        private bool m_bProtected;

        /// <summary>
        /// The m_obj sync.
        /// </summary>
        private object m_objSync = new object();

        /// <summary>
        /// The m_pb data.
        /// </summary>
        private byte[] m_pbData; // Never null

        // The real length of the data. This value can be different than
        // m_pbData.Length, as the length of m_pbData always is a multiple
        // of PmBlockSize (required for fast in-memory protection).
        /// <summary>
        /// The m_u data len.
        /// </summary>
        private uint m_uDataLen;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="ProtectedBinary"/> class.
        /// </summary>
        static ProtectedBinary()
        {
            m_bProtectionSupported = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedBinary"/> class. 
        /// Construct a new, empty protected binary data object. Protection
        /// is disabled.
        /// </summary>
        public ProtectedBinary()
        {
            this.Init(false, new byte[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedBinary"/> class. 
        /// Construct a new protected binary data object.
        /// </summary>
        /// <param name="bEnableProtection">
        /// If this paremeter is <c>true</c>,
        /// the data will be encrypted in memory. If it is <c>false</c>, the
        /// data is stored in plain-text in the process memory.
        /// </param>
        /// <param name="pbData">
        /// Value of the protected object.
        /// The input parameter is not modified and
        /// <c>ProtectedBinary</c> doesn't take ownership of the data,
        /// i.e. the caller is responsible for clearing it.
        /// </param>
        public ProtectedBinary(bool bEnableProtection, byte[] pbData)
        {
            this.Init(bEnableProtection, pbData);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedBinary"/> class. 
        /// Construct a new protected binary data object. Copy the data from
        /// a <c>XorredBuffer</c> object.
        /// </summary>
        /// <param name="bEnableProtection">
        /// Enable protection or not.
        /// </param>
        /// <param name="xbProtected">
        /// <c>XorredBuffer</c> object used to
        /// initialize the <c>ProtectedBinary</c> object.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public ProtectedBinary(bool bEnableProtection, XorredBuffer xbProtected)
        {
            Debug.Assert(xbProtected != null);
            if (xbProtected == null)
            {
                throw new ArgumentNullException("xbProtected");
            }

            byte[] pb = xbProtected.ReadPlainText();
            this.Init(bEnableProtection, pb);
            MemUtil.ZeroByteArray(pb);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A flag specifying whether the <c>ProtectedBinary</c> object has
        /// turned on in-memory protection or not.
        /// </summary>
        public bool IsProtected
        {
            get
            {
                return this.m_bProtected;
            }
        }

        /// <summary>
        /// Length of the stored data.
        /// </summary>
        public uint Length
        {
            get
            {
                return this.m_uDataLen;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ProtectedBinary);
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Equals(ProtectedBinary other)
        {
            if (other == null)
            {
                return false; // No assert
            }

            if (this.m_bProtected != other.m_bProtected)
            {
                return false;
            }

            if (this.m_uDataLen != other.m_uDataLen)
            {
                return false;
            }

            byte[] pbL = this.ReadData();
            byte[] pbR = other.ReadData();
            bool bEq = MemUtil.ArraysEqual(pbL, pbR);
            MemUtil.ZeroByteArray(pbL);
            MemUtil.ZeroByteArray(pbR);

#if DEBUG
            if (bEq)
            {
                Debug.Assert(this.GetHashCode() == other.GetHashCode());
            }

#endif

            return bEq;
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            int h = this.m_bProtected ? 0x7B11D289 : 0;

            byte[] pb = this.ReadData();
            unchecked
            {
                for (int i = 0; i < pb.Length; ++i)
                {
                    h = (h << 3) + h + (int)pb[i];
                }
            }

            MemUtil.ZeroByteArray(pb);

            return h;
        }

        /// <summary>
        /// Get a copy of the protected data as a byte array.
        /// Please note that the returned byte array is not protected and
        /// can therefore been read by any other application.
        /// Make sure that your clear it properly after usage.
        /// </summary>
        /// <returns>Unprotected byte array. This is always a copy of the internal
        /// protected data and can therefore be cleared safely.</returns>
        public byte[] ReadData()
        {
            if (this.m_uDataLen == 0)
            {
                return new byte[0];
            }

            byte[] pbReturn = new byte[this.m_uDataLen];

            Array.Copy(this.m_pbData, pbReturn, (int)this.m_uDataLen);

            return pbReturn;
        }

        /// <summary>
        /// Read the protected data and return it protected with a sequence
        /// of bytes generated by a random stream.
        /// </summary>
        /// <param name="crsRandomSource">
        /// Random number source.
        /// </param>
        /// <returns>
        /// Protected data.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public byte[] ReadXorredData(CryptoRandomStream crsRandomSource)
        {
            Debug.Assert(crsRandomSource != null);
            if (crsRandomSource == null)
            {
                throw new ArgumentNullException("crsRandomSource");
            }

            byte[] pbData = this.ReadData();
            uint uLen = (uint)pbData.Length;

            byte[] randomPad = crsRandomSource.GetRandomBytes(uLen);
            Debug.Assert(randomPad.Length == uLen);

            for (uint i = 0; i < uLen; ++i)
            {
                pbData[i] ^= randomPad[i];
            }

            return pbData;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The init.
        /// </summary>
        /// <param name="bEnableProtection">
        /// The b enable protection.
        /// </param>
        /// <param name="pbData">
        /// The pb data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        private void Init(bool bEnableProtection, byte[] pbData)
        {
            if (pbData == null)
            {
                throw new ArgumentNullException("pbData");
            }

            this.m_bProtected = bEnableProtection;
            this.m_uDataLen = (uint)pbData.Length;

            int nBlocks = (int)this.m_uDataLen / PmBlockSize;
            if ((nBlocks * PmBlockSize) < (int)this.m_uDataLen)
            {
                ++nBlocks;
            }

            Debug.Assert((nBlocks * PmBlockSize) >= (int)this.m_uDataLen);

            this.m_pbData = new byte[nBlocks * PmBlockSize];
            Array.Copy(pbData, this.m_pbData, (int)this.m_uDataLen);
        }

        #endregion
    }
}