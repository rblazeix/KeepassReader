// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITimeLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Interface for objects that support various times (creation time, last
//   access time, last modification time and expiry time). Offers
//   several helper functions (for example a function to touch the current
//   object).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Interfaces
{
    using System;

    /// <summary>
    /// Interface for objects that support various times (creation time, last
    /// access time, last modification time and expiry time). Offers
    /// several helper functions (for example a function to touch the current
    /// object).
    /// </summary>
    public interface ITimeLogger
    {
        #region Public Properties

        /// <summary>
        /// The date/time when the object was created.
        /// </summary>
        DateTime CreationTime { get; set; }

        /// <summary>
        /// Flag that determines if the object does expire.
        /// </summary>
        bool Expires { get; set; }

        /// <summary>
        /// The date/time when the object expires.
        /// </summary>
        DateTime ExpiryTime { get; set; }

        /// <summary>
        /// The date/time when the object was last accessed.
        /// </summary>
        DateTime LastAccessTime { get; set; }

        /// <summary>
        /// The date/time when the object was last modified.
        /// </summary>
        DateTime LastModificationTime { get; set; }

        /// <summary>
        /// The date/time when the location of the object was last changed.
        /// </summary>
        DateTime LocationChanged { get; set; }

        /// <summary>
        /// Get or set the usage count of the object. To increase the usage
        /// count by one, use the <c>Touch</c> function.
        /// </summary>
        ulong UsageCount { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Touch the object. This function updates the internal last access
        /// time. If the <paramref name="bModified"/> parameter is <c>true</c>,
        /// the last modification time gets updated, too. Each time you call
        /// <c>Touch</c>, the usage count of the object is increased by one.
        /// </summary>
        /// <param name="bModified">
        /// Update last modification time.
        /// </param>
        void Touch(bool bModified);

        #endregion
    }
}