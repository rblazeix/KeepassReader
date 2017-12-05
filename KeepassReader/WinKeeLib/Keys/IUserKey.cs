// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUserKey.cs" company="">
//   
// </copyright>
// <summary>
//   Interface to a user key, like a password, key file data, etc.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Keys
{
    using KeePassLib.Security;

    /// <summary>
    /// Interface to a user key, like a password, key file data, etc.
    /// </summary>
    public interface IUserKey
    {
        #region Public Properties

        /// <summary>
        /// Get key data. Querying this property is fast (it returns a
        /// reference to a cached <c>ProtectedBinary</c> object).
        /// If no key data is available, <c>null</c> is returned.
        /// </summary>
        ProtectedBinary KeyData { get; }

        #endregion

        // /// <summary>
        // /// Clear the key and securely erase all security-critical information.
        // /// </summary>
        // void Clear();
    }
}