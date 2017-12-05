// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KcpCustomKey.cs" company="">
//   
// </copyright>
// <summary>
//   The kcp custom key.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Keys
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices.WindowsRuntime;

    using KeePassLib.Security;

    using Windows.Security.Cryptography.Core;

    /// <summary>
    /// The kcp custom key.
    /// </summary>
    public sealed class KcpCustomKey : IUserKey
    {
        #region Fields

        /// <summary>
        /// The m_str name.
        /// </summary>
        private readonly string m_strName;

        /// <summary>
        /// The m_pb key.
        /// </summary>
        private ProtectedBinary m_pbKey;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KcpCustomKey"/> class.
        /// </summary>
        /// <param name="strName">
        /// The str name.
        /// </param>
        /// <param name="pbKeyData">
        /// The pb key data.
        /// </param>
        /// <param name="bPerformHash">
        /// The b perform hash.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public KcpCustomKey(string strName, byte[] pbKeyData, bool bPerformHash)
        {
            Debug.Assert(strName != null);
            if (strName == null)
            {
                throw new ArgumentNullException("strName");
            }

            Debug.Assert(pbKeyData != null);
            if (pbKeyData == null)
            {
                throw new ArgumentNullException("pbKeyData");
            }

            this.m_strName = strName;

            if (bPerformHash)
            {
                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                byte[] pbRaw = sha256.HashData(pbKeyData.AsBuffer()).ToArray();
                this.m_pbKey = new ProtectedBinary(true, pbRaw);
            }
            else
            {
                this.m_pbKey = new ProtectedBinary(true, pbKeyData);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the key data.
        /// </summary>
        public ProtectedBinary KeyData
        {
            get
            {
                return this.m_pbKey;
            }
        }

        /// <summary>
        /// Name of the provider that generated the custom key.
        /// </summary>
        public string Name
        {
            get
            {
                return this.m_strName;
            }
        }

        #endregion

        // public void Clear()
        // {
        // 	m_pbKey = null;
        // }
    }
}