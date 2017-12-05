// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDeepCloneable.cs" company="">
//   
// </copyright>
// <summary>
//   Interface for objects that are deeply cloneable.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Interfaces
{
    /// <summary>
    /// Interface for objects that are deeply cloneable.
    /// </summary>
    /// <typeparam name="T">
    /// Reference type.
    /// </typeparam>
    public interface IDeepCloneable<T>
        where T : class
    {
        #region Public Methods and Operators

        /// <summary>
        /// Deeply clone the object.
        /// </summary>
        /// <returns>Cloned object.</returns>
        T CloneDeep();

        #endregion
    }
}