// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStructureItem.cs" company="">
//   
// </copyright>
// <summary>
//   The StructureItem interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Interfaces
{
    /// <summary>
    /// The StructureItem interface.
    /// </summary>
    public interface IStructureItem : ITimeLogger
    {
        // Provides LocationChanged
        #region Public Properties

        /// <summary>
        /// Gets the parent group.
        /// </summary>
        PwGroup ParentGroup { get; }

        /// <summary>
        /// Gets or sets the uuid.
        /// </summary>
        PwUuid Uuid { get; set; }

        #endregion
    }
}