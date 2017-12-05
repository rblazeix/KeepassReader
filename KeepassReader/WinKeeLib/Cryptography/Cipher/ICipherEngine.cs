// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICipherEngine.cs" company="">
//   
// </copyright>
// <summary>
//   Interface of an encryption/decryption class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Cryptography.Cipher
{
    using System.IO;

    /// <summary>
    /// Interface of an encryption/decryption class.
    /// </summary>
    public interface ICipherEngine
    {
        #region Public Properties

        /// <summary>
        /// UUID of the engine. If you want to write an engine/plugin,
        /// please contact the KeePass team to obtain a new UUID.
        /// </summary>
        PwUuid CipherUuid { get; }

        /// <summary>
        /// String displayed in the list of available encryption/decryption
        /// engines in the GUI.
        /// </summary>
        string DisplayName { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Decrypt a stream.
        /// </summary>
        /// <param name="sEncrypted">
        /// Stream to read the encrypted data from.
        /// </param>
        /// <param name="pbKey">
        /// Key to use.
        /// </param>
        /// <param name="pbIV">
        /// Initialization vector.
        /// </param>
        /// <returns>
        /// Stream, from which the decrypted data can be read.
        /// </returns>
        Stream DecryptStream(Stream sEncrypted, byte[] pbKey, byte[] pbIV);

        /// <summary>
        /// Encrypt a stream.
        /// </summary>
        /// <param name="sPlainText">
        /// Stream to read the plain-text from.
        /// </param>
        /// <param name="pbKey">
        /// Key to use.
        /// </param>
        /// <param name="pbIV">
        /// Initialization vector.
        /// </param>
        /// <returns>
        /// Stream, from which the encrypted data can be read.
        /// </returns>
        Stream EncryptStream(Stream sPlainText, byte[] pbKey, byte[] pbIV);

        #endregion
    }
}