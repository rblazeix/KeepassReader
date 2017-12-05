// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PwDeletedObject.cs" company="">
//   
// </copyright>
// <summary>
//   Represents an object that has been deleted.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WinKeeLib
{
    using System;

    using KeePassLib;
    using KeePassLib.Interfaces;

    /// <summary>
    /// Represents an object that has been deleted.
    /// </summary>
    public sealed class PwDeletedObject : IDeepCloneable<PwDeletedObject>
    {
        #region Fields

        /// <summary>
        /// The m_dt deletion time.
        /// </summary>
        private DateTime m_dtDeletionTime = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_uuid.
        /// </summary>
        private PwUuid m_uuid = PwUuid.Zero;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PwDeletedObject"/> class. 
        /// Construct a new <c>PwDeletedObject</c> object.
        /// </summary>
        public PwDeletedObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PwDeletedObject"/> class.
        /// </summary>
        /// <param name="uuid">
        /// The uuid.
        /// </param>
        /// <param name="dtDeletionTime">
        /// The dt deletion time.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public PwDeletedObject(PwUuid uuid, DateTime dtDeletionTime)
        {
            if (uuid == null)
            {
                throw new ArgumentNullException("uuid");
            }

            this.m_uuid = uuid;
            this.m_dtDeletionTime = dtDeletionTime;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The date/time when the entry has been deleted.
        /// </summary>
        public DateTime DeletionTime
        {
            get
            {
                return this.m_dtDeletionTime;
            }

            set
            {
                this.m_dtDeletionTime = value;
            }
        }

        /// <summary>
        /// UUID of the entry that has been deleted.
        /// </summary>
        public PwUuid Uuid
        {
            get
            {
                return this.m_uuid;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_uuid = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Clone the object.
        /// </summary>
        /// <returns>Value copy of the current object.</returns>
        public PwDeletedObject CloneDeep()
        {
            PwDeletedObject pdo = new PwDeletedObject();

            pdo.m_uuid = this.m_uuid; // PwUuid objects are immutable
            pdo.m_dtDeletionTime = this.m_dtDeletionTime;

            return pdo;
        }

        #endregion
    }
}