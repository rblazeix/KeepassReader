// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProtectedString.cs" company="">
//   
// </copyright>
// <summary>
//   Represents an in-memory encrypted string.
//   <c>ProtectedString</c> objects are immutable and thread-safe.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if KeePassLibSD
using KeePassLibSD;
#endif

// SecureString objects are limited to 65536 characters, don't use
namespace KeePassLib.Security
{
    using System;
    using System.Diagnostics;

    using KeePassLib.Cryptography;
    using KeePassLib.Utility;

    /// <summary>
    /// Represents an in-memory encrypted string.
    /// <c>ProtectedString</c> objects are immutable and thread-safe.
    /// </summary>
#if (DEBUG && !KeePassLibSD)
    [DebuggerDisplay(@"{ReadString()}")]
#endif
    public sealed class ProtectedString
    {
        // Exactly one of the following will be non-null
        /// <summary>
        /// The m_pb utf 8.
        /// </summary>
        private ProtectedBinary m_pbUtf8 = null;

        /// <summary>
        /// The m_str plain text.
        /// </summary>
        private string m_strPlainText = null;

        /// <summary>
        /// The m_b is protected.
        /// </summary>
        private bool m_bIsProtected;

        /// <summary>
        /// The m_ps empty.
        /// </summary>
        private static ProtectedString m_psEmpty = new ProtectedString();

        /// <summary>
        /// Gets the empty.
        /// </summary>
        public static ProtectedString Empty
        {
            get
            {
                return m_psEmpty;
            }
        }

        /// <summary>
        /// A flag specifying whether the <c>ProtectedString</c> object
        /// has turned on in-memory protection or not.
        /// </summary>
        public bool IsProtected
        {
            get
            {
                return this.m_bIsProtected;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                ProtectedBinary pBin = this.m_pbUtf8; // Local ref for thread-safety
                if (pBin != null)
                {
                    return pBin.Length == 0;
                }

                Debug.Assert(this.m_strPlainText != null);
                return this.m_strPlainText.Length == 0;
            }
        }

        /// <summary>
        /// The m_n cached length.
        /// </summary>
        private int m_nCachedLength = -1;

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length
        {
            get
            {
                if (this.m_nCachedLength >= 0)
                {
                    return this.m_nCachedLength;
                }

                ProtectedBinary pBin = this.m_pbUtf8; // Local ref for thread-safety
                if (pBin != null)
                {
                    byte[] pbPlain = pBin.ReadData();
                    this.m_nCachedLength = StrUtil.Utf8.GetCharCount(pbPlain);
                    MemUtil.ZeroByteArray(pbPlain);
                }
                else
                {
                    Debug.Assert(this.m_strPlainText != null);
                    this.m_nCachedLength = this.m_strPlainText.Length;
                }

                return this.m_nCachedLength;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedString"/> class. 
        /// Construct a new protected string object. Protection is
        /// disabled.
        /// </summary>
        public ProtectedString()
        {
            this.Init(false, string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedString"/> class. 
        /// Construct a new protected string. The string is initialized
        /// to the value supplied in the parameters.
        /// </summary>
        /// <param name="bEnableProtection">
        /// If this parameter is <c>true</c>,
        /// the string will be protected in-memory (encrypted). If it
        /// is <c>false</c>, the string will be stored as plain-text.
        /// </param>
        /// <param name="strValue">
        /// The initial string value. This
        /// parameter won't be modified.
        /// </param>
        public ProtectedString(bool bEnableProtection, string strValue)
        {
            Init(bEnableProtection, strValue);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedString"/> class. 
        /// Construct a new protected string. The string is initialized
        /// to the value supplied in the parameters (UTF-8 encoded string).
        /// </summary>
        /// <param name="bEnableProtection">
        /// If this parameter is <c>true</c>,
        /// the string will be protected in-memory (encrypted). If it
        /// is <c>false</c>, the string will be stored as plain-text.
        /// </param>
        /// <param name="vUtf8Value">
        /// The initial string value, encoded as
        /// UTF-8 byte array. This parameter won't be modified; the caller
        /// is responsible for clearing it.
        /// </param>
        public ProtectedString(bool bEnableProtection, byte[] vUtf8Value)
        {
            Init(bEnableProtection, vUtf8Value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedString"/> class. 
        /// Construct a new protected string. The string is initialized
        /// to the value passed in the <c>XorredBuffer</c> object.
        /// </summary>
        /// <param name="bEnableProtection">
        /// Enable protection or not.
        /// </param>
        /// <param name="xbProtected">
        /// <c>XorredBuffer</c> object containing the
        /// string in UTF-8 representation. The UTF-8 string must not
        /// be <c>null</c>-terminated.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public ProtectedString(bool bEnableProtection, XorredBuffer xbProtected)
        {
            if (xbProtected == null)
            {
                throw new ArgumentNullException("xbProtected");
            }

            byte[] pb = xbProtected.ReadPlainText();
            Init(bEnableProtection, pb);
            MemUtil.ZeroByteArray(pb);
        }

        /// <summary>
        /// The init.
        /// </summary>
        /// <param name="bEnableProtection">
        /// The b enable protection.
        /// </param>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        private void Init(bool bEnableProtection, string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            this.m_bIsProtected = bEnableProtection;

            // The string already is in memory and immutable,
            // protection would be useless
            this.m_strPlainText = str;
        }

        /// <summary>
        /// The init.
        /// </summary>
        /// <param name="bEnableProtection">
        /// The b enable protection.
        /// </param>
        /// <param name="pbUtf8">
        /// The pb utf 8.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        private void Init(bool bEnableProtection, byte[] pbUtf8)
        {
            if (pbUtf8 == null)
            {
                throw new ArgumentNullException("pbUtf8");
            }

            this.m_bIsProtected = bEnableProtection;

            if (bEnableProtection)
            {
                this.m_pbUtf8 = new ProtectedBinary(true, pbUtf8);
            }
            else
            {
                this.m_strPlainText = StrUtil.Utf8.GetString(pbUtf8, 0, pbUtf8.Length);
            }
        }

        /// <summary>
        /// Convert the protected string to a normal string object.
        /// Be careful with this function, the returned string object
        /// isn't protected anymore and stored in plain-text in the
        /// process memory.
        /// </summary>
        /// <returns>Plain-text string. Is never <c>null</c>.</returns>
        public string ReadString()
        {
            if (this.m_strPlainText != null)
            {
                return this.m_strPlainText;
            }

            byte[] pb = this.ReadUtf8();
            string str = (pb.Length == 0) ? string.Empty : StrUtil.Utf8.GetString(pb, 0, pb.Length);

            // No need to clear pb

            // As the text is now visible in process memory anyway,
            // there's no need to protect it anymore
            this.m_strPlainText = str;
            this.m_pbUtf8 = null; // Thread-safe order

            return str;
        }

        /// <summary>
        /// Read out the string and return a byte array that contains the
        /// string encoded using UTF-8. The returned string is not protected
        /// anymore!
        /// </summary>
        /// <returns>Plain-text UTF-8 byte array.</returns>
        public byte[] ReadUtf8()
        {
            ProtectedBinary pBin = this.m_pbUtf8; // Local ref for thread-safety
            if (pBin != null)
            {
                return pBin.ReadData();
            }

            return StrUtil.Utf8.GetBytes(this.m_strPlainText);
        }

        /// <summary>
        /// Read the protected string and return it protected with a sequence
        /// of bytes generated by a random stream.
        /// </summary>
        /// <param name="crsRandomSource">
        /// Random number source.
        /// </param>
        /// <returns>
        /// Protected string.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public byte[] ReadXorredString(CryptoRandomStream crsRandomSource)
        {
            Debug.Assert(crsRandomSource != null);
            if (crsRandomSource == null)
            {
                throw new ArgumentNullException("crsRandomSource");
            }

            byte[] pbData = this.ReadUtf8();
            uint uLen = (uint)pbData.Length;

            byte[] randomPad = crsRandomSource.GetRandomBytes(uLen);
            Debug.Assert(randomPad.Length == uLen);

            for (uint i = 0; i < uLen; ++i)
            {
                pbData[i] ^= randomPad[i];
            }

            return pbData;
        }

        /// <summary>
        /// The with protection.
        /// </summary>
        /// <param name="bProtect">
        /// The b protect.
        /// </param>
        /// <returns>
        /// The <see cref="ProtectedString"/>.
        /// </returns>
        public ProtectedString WithProtection(bool bProtect)
        {
            if (bProtect == this.m_bIsProtected)
            {
                return this;
            }

            byte[] pb = this.ReadUtf8();
            ProtectedString ps = new ProtectedString(bProtect, pb);
            MemUtil.ZeroByteArray(pb);
            return ps;
        }
    }
}