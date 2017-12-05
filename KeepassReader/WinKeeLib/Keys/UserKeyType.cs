// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserKeyType.cs" company="">
//   
// </copyright>
// <summary>
//   The user key type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WinKeeLib.Keys
{
    using System;

    /// <summary>
    /// The user key type.
    /// </summary>
    [Flags]
    public enum UserKeyType
    {
        /// <summary>
        /// The none.
        /// </summary>
        None = 0, 

        /// <summary>
        /// The other.
        /// </summary>
        Other = 1, 

        /// <summary>
        /// The password.
        /// </summary>
        Password = 2, 

        /// <summary>
        /// The key file.
        /// </summary>
        KeyFile = 4, 

        /// <summary>
        /// The user account.
        /// </summary>
        UserAccount = 8
    }
}