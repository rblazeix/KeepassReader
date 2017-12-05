// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyValidator.cs" company="">
//   
// </copyright>
// <summary>
//   The key validation type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Keys
{
    /// <summary>
    /// The key validation type.
    /// </summary>
    public enum KeyValidationType
    {
        /// <summary>
        /// The master password.
        /// </summary>
        MasterPassword = 0
    }

    /// <summary>
    /// The key validator.
    /// </summary>
    public abstract class KeyValidator
    {
        #region Public Properties

        /// <summary>
        /// Name of your key validator (should be unique).
        /// </summary>
        public abstract string Name { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Validate a key.
        /// </summary>
        /// <param name="strKey">
        /// Key to validate.
        /// </param>
        /// <param name="t">
        /// Type of the validation to perform.
        /// </param>
        /// <returns>
        /// Returns <c>null</c>, if the validation is successful.
        /// If there's a problem with the key, the returned string describes
        /// the problem.
        /// </returns>
        public abstract string Validate(string strKey, KeyValidationType t);

        #endregion
    }
}