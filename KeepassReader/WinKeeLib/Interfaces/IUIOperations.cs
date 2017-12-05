// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUIOperations.cs" company="">
//   
// </copyright>
// <summary>
//   The UIOperations interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Interfaces
{
    /// <summary>
    /// The UIOperations interface.
    /// </summary>
    public interface IUIOperations
    {
        #region Public Methods and Operators

        /// <summary>
        /// Let the user interface save the current database.
        /// </summary>
        /// <param name="bForceSave">
        /// If <c>true</c>, the UI will not ask for
        /// whether to synchronize or overwrite, it'll simply overwrite the
        /// file.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the file has been saved.
        /// </returns>
        bool UIFileSave(bool bForceSave);

        #endregion
    }
}