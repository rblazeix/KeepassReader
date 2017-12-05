// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KcpPassword.cs" company="">
//   
// </copyright>
// <summary>
//   Master password / passphrase as provided by the user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Keys
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices.WindowsRuntime;

    using KeePassLib.Security;
    using KeePassLib.Utility;

    using Windows.Security.Cryptography.Core;

    /// <summary>
    /// Master password / passphrase as provided by the user.
    /// </summary>
    public sealed class KcpPassword : IUserKey
    {
        #region Fields

        /// <summary>
        /// The m_pb key data.
        /// </summary>
        private ProtectedBinary m_pbKeyData;

        /// <summary>
        /// The m_ps password.
        /// </summary>
        private ProtectedString m_psPassword;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KcpPassword"/> class.
        /// </summary>
        /// <param name="pbPasswordUtf8">
        /// The pb password utf 8.
        /// </param>
        public KcpPassword(byte[] pbPasswordUtf8)
        {
            this.SetKey(pbPasswordUtf8);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KcpPassword"/> class.
        /// </summary>
        /// <param name="strPassword">
        /// The str password.
        /// </param>
        public KcpPassword(string strPassword)
        {
            this.SetKey(StrUtil.Utf8.GetBytes(strPassword));
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
        /// Get the password as protected string.
        /// </summary>
        public ProtectedString Password
        {
            get
            {
                return this.m_psPassword;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The set key.
        /// </summary>
        /// <param name="pbPasswordUtf8">
        /// The pb password utf 8.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        private void SetKey(byte[] pbPasswordUtf8)
        {
            Debug.Assert(pbPasswordUtf8 != null);
            if (pbPasswordUtf8 == null)
            {
                throw new ArgumentNullException("pbPasswordUtf8");
            }

            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            byte[] pbRaw = sha256.HashData(pbPasswordUtf8.AsBuffer()).ToArray();

            this.m_psPassword = new ProtectedString(true, pbPasswordUtf8);
            this.m_pbKeyData = new ProtectedBinary(true, pbRaw);
        }

        #endregion

        // public void Clear()
        // {
        // 	m_psPassword = null;
        // 	m_pbKeyData = null;
        // }
    }
}