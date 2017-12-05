// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PwDatabase.cs" company="">
//   
// </copyright>
// <summary>
//   The core password manager class. It contains a number of groups, which
//   contain the actual entries.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using KeePassLib.Collections;
    using KeePassLib.Cryptography.Cipher;
    using KeePassLib.Delegates;
    using KeePassLib.Interfaces;
    using KeePassLib.Keys;
    using KeePassLib.Security;
    using KeePassLib.Serialization;
    using KeePassLib.Utility;

    using WinKeeLib;
    using WinKeeLib.Keys;

    /// <summary>
    /// The core password manager class. It contains a number of groups, which
    /// contain the actual entries.
    /// </summary>
    public sealed class PwDatabase
    {
        #region Constants

        /// <summary>
        /// The default history max items.
        /// </summary>
        internal const int DefaultHistoryMaxItems = 10; // -1 = unlimited

        /// <summary>
        /// The default history max size.
        /// </summary>
        internal const long DefaultHistoryMaxSize = 6 * 1024 * 1024; // -1 = unlimited

        #endregion

        #region Static Fields

        /// <summary>
        /// The m_b primary created.
        /// </summary>
        private static bool m_bPrimaryCreated = false;

        /// <summary>
        /// The m_l std fields.
        /// </summary>
        private static List<string> m_lStdFields = null;

        /// <summary>
        /// The m_str localized app name.
        /// </summary>
        private static string m_strLocalizedAppName = string.Empty;

        #endregion

        // Initializations see Clear()
        #region Fields

        /// <summary>
        /// The m_b database opened.
        /// </summary>
        private bool m_bDatabaseOpened = false;

        /// <summary>
        /// The m_b modified.
        /// </summary>
        private bool m_bModified = false;

        /// <summary>
        /// The m_b ui needs icon update.
        /// </summary>
        private bool m_bUINeedsIconUpdate = true;

        /// <summary>
        /// The m_b use file locks.
        /// </summary>
        private bool m_bUseFileLocks = false;

        /// <summary>
        /// The m_b use file transactions.
        /// </summary>
        private bool m_bUseFileTransactions = false;

        /// <summary>
        /// The m_b use recycle bin.
        /// </summary>
        private bool m_bUseRecycleBin = true;

        /// <summary>
        /// The m_ca compression.
        /// </summary>
        private PwCompressionAlgorithm m_caCompression = PwCompressionAlgorithm.GZip;

        /// <summary>
        /// The m_clr.
        /// </summary>
        private string m_clr = string.Empty;

        /// <summary>
        /// The m_dt default user changed.
        /// </summary>
        private DateTime m_dtDefaultUserChanged = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_dt desc changed.
        /// </summary>
        private DateTime m_dtDescChanged = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_dt entry templates changed.
        /// </summary>
        private DateTime m_dtEntryTemplatesChanged = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_dt key last changed.
        /// </summary>
        private DateTime m_dtKeyLastChanged = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_dt name changed.
        /// </summary>
        private DateTime m_dtNameChanged = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_dt recycle bin changed.
        /// </summary>
        private DateTime m_dtRecycleBinChanged = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_l history max size.
        /// </summary>
        private long m_lHistoryMaxSize = DefaultHistoryMaxSize; // In bytes

        /// <summary>
        /// The m_l key change force days.
        /// </summary>
        private long m_lKeyChangeForceDays = -1;

        /// <summary>
        /// The m_l key change rec days.
        /// </summary>
        private long m_lKeyChangeRecDays = -1;

        /// <summary>
        /// The m_mem prot config.
        /// </summary>
        private MemoryProtectionConfig m_memProtConfig = new MemoryProtectionConfig();

        /// <summary>
        /// The m_n history max items.
        /// </summary>
        private int m_nHistoryMaxItems = DefaultHistoryMaxItems;

        /// <summary>
        /// The m_pb hash of file on disk.
        /// </summary>
        private byte[] m_pbHashOfFileOnDisk = null;

        /// <summary>
        /// The m_pb hash of last io.
        /// </summary>
        private byte[] m_pbHashOfLastIO = null;

        /// <summary>
        /// The m_pg root group.
        /// </summary>
        private PwGroup m_pgRootGroup = null;

        /// <summary>
        /// The m_pw entry templates group.
        /// </summary>
        private PwUuid m_pwEntryTemplatesGroup = PwUuid.Zero;

        /// <summary>
        /// The m_pw last selected group.
        /// </summary>
        private PwUuid m_pwLastSelectedGroup = PwUuid.Zero;

        /// <summary>
        /// The m_pw last top visible group.
        /// </summary>
        private PwUuid m_pwLastTopVisibleGroup = PwUuid.Zero;

        /// <summary>
        /// The m_pw recycle bin.
        /// </summary>
        private PwUuid m_pwRecycleBin = PwUuid.Zero;

        /// <summary>
        /// The m_pw user key.
        /// </summary>
        private CompositeKey m_pwUserKey = null;

        /// <summary>
        /// The m_sl status.
        /// </summary>
        private IStatusLogger m_slStatus = null;

        /// <summary>
        /// The m_str default user name.
        /// </summary>
        private string m_strDefaultUserName = string.Empty;

        /// <summary>
        /// The m_str desc.
        /// </summary>
        private string m_strDesc = string.Empty;

        /// <summary>
        /// The m_str detach bins.
        /// </summary>
        private string m_strDetachBins = null;

        /// <summary>
        /// The m_str name.
        /// </summary>
        private string m_strName = string.Empty;

        /// <summary>
        /// The m_u key encryption rounds.
        /// </summary>
        private ulong m_uKeyEncryptionRounds = PwDefs.DefaultKeyEncryptionRounds;

        /// <summary>
        /// The m_u mntnc history days.
        /// </summary>
        private uint m_uMntncHistoryDays = 365;

        /// <summary>
        /// The m_uuid data cipher.
        /// </summary>
        private PwUuid m_uuidDataCipher = StandardAesEngine.AesUuid;

        /// <summary>
        /// The m_v custom data.
        /// </summary>
        private StringDictionaryEx m_vCustomData = new StringDictionaryEx();

        /// <summary>
        /// The m_v custom icons.
        /// </summary>
        private List<PwCustomIcon> m_vCustomIcons = new List<PwCustomIcon>();

        /// <summary>
        /// The m_v deleted objects.
        /// </summary>
        private PwObjectList<PwDeletedObject> m_vDeletedObjects = new PwObjectList<PwDeletedObject>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PwDatabase"/> class. 
        /// Constructs an empty password manager object.
        /// </summary>
        public PwDatabase()
        {
            if (m_bPrimaryCreated == false)
            {
                m_bPrimaryCreated = true;
            }

            this.Clear();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Localized application name.
        /// </summary>
        public static string LocalizedAppName
        {
            get
            {
                return m_strLocalizedAppName;
            }

            set
            {
                Debug.Assert(value != null);
                m_strLocalizedAppName = value;
            }
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public string Color
        {
            get
            {
                return this.m_clr;
            }

            set
            {
                this.m_clr = value;
            }
        }

        /// <summary>
        /// Compression algorithm used to encrypt the data part of the database.
        /// </summary>
        public PwCompressionAlgorithm Compression
        {
            get
            {
                return this.m_caCompression;
            }

            set
            {
                this.m_caCompression = value;
            }
        }

        /// <summary>
        /// Custom data container that can be used by plugins to store
        /// own data in KeePass databases.
        /// </summary>
        public StringDictionaryEx CustomData
        {
            get
            {
                return this.m_vCustomData;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_vCustomData = value;
            }
        }

        /// <summary>
        /// Get all custom icons stored in this database.
        /// </summary>
        public List<PwCustomIcon> CustomIcons
        {
            get
            {
                return this.m_vCustomIcons;
            }
        }

        /// <summary>
        /// The encryption algorithm used to encrypt the data part of the database.
        /// </summary>
        public PwUuid DataCipherUuid
        {
            get
            {
                return this.m_uuidDataCipher;
            }

            set
            {
                Debug.Assert(value != null);
                if (value != null)
                {
                    this.m_uuidDataCipher = value;
                }
            }
        }

        /// <summary>
        /// Default user name used for new entries.
        /// </summary>
        public string DefaultUserName
        {
            get
            {
                return this.m_strDefaultUserName;
            }

            set
            {
                Debug.Assert(value != null);
                if (value != null)
                {
                    this.m_strDefaultUserName = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default user name changed.
        /// </summary>
        public DateTime DefaultUserNameChanged
        {
            get
            {
                return this.m_dtDefaultUserChanged;
            }

            set
            {
                this.m_dtDefaultUserChanged = value;
            }
        }

        /// <summary>
        /// Get a list of all deleted objects.
        /// </summary>
        public PwObjectList<PwDeletedObject> DeletedObjects
        {
            get
            {
                return this.m_vDeletedObjects;
            }
        }

        /// <summary>
        /// Database description.
        /// </summary>
        public string Description
        {
            get
            {
                return this.m_strDesc;
            }

            set
            {
                Debug.Assert(value != null);
                if (value != null)
                {
                    this.m_strDesc = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the description changed.
        /// </summary>
        public DateTime DescriptionChanged
        {
            get
            {
                return this.m_dtDescChanged;
            }

            set
            {
                this.m_dtDescChanged = value;
            }
        }

        /// <summary>
        /// Detach binaries when opening a file. If this isn't <c>null</c>,
        /// all binaries are saved to the specified path and are removed
        /// from the database.
        /// </summary>
        public string DetachBinaries
        {
            get
            {
                return this.m_strDetachBins;
            }

            set
            {
                this.m_strDetachBins = value;
            }
        }

        /// <summary>
        /// UUID of the group containing template entries. May be
        /// <c>PwUuid.Zero</c>, if no entry templates group has been specified.
        /// </summary>
        public PwUuid EntryTemplatesGroup
        {
            get
            {
                return this.m_pwEntryTemplatesGroup;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_pwEntryTemplatesGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets the entry templates group changed.
        /// </summary>
        public DateTime EntryTemplatesGroupChanged
        {
            get
            {
                return this.m_dtEntryTemplatesChanged;
            }

            set
            {
                this.m_dtEntryTemplatesChanged = value;
            }
        }

        /// <summary>
        /// Hash value of the primary file on disk (last read or last write).
        /// A call to <c>SaveAs</c> without making the saved file primary will
        /// not change this hash. May be <c>null</c>.
        /// </summary>
        public byte[] HashOfFileOnDisk
        {
            get
            {
                return this.m_pbHashOfFileOnDisk;
            }
        }

        /// <summary>
        /// Gets the hash of last io.
        /// </summary>
        public byte[] HashOfLastIO
        {
            get
            {
                return this.m_pbHashOfLastIO;
            }
        }

        /// <summary>
        /// Gets or sets the history max items.
        /// </summary>
        public int HistoryMaxItems
        {
            get
            {
                return this.m_nHistoryMaxItems;
            }

            set
            {
                this.m_nHistoryMaxItems = value;
            }
        }

        /// <summary>
        /// Gets or sets the history max size.
        /// </summary>
        public long HistoryMaxSize
        {
            get
            {
                return this.m_lHistoryMaxSize;
            }

            set
            {
                this.m_lHistoryMaxSize = value;
            }
        }

        /// <summary>
        /// If this is <c>true</c>, a database is currently open.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this.m_bDatabaseOpened;
            }
        }

        /// <summary>
        /// Number of key transformation rounds (in order to make dictionary
        /// attacks harder).
        /// </summary>
        public ulong KeyEncryptionRounds
        {
            get
            {
                return this.m_uKeyEncryptionRounds;
            }

            set
            {
                this.m_uKeyEncryptionRounds = value;
            }
        }

        /// <summary>
        /// Gets or sets the last selected group.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public PwUuid LastSelectedGroup
        {
            get
            {
                return this.m_pwLastSelectedGroup;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_pwLastSelectedGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets the last top visible group.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public PwUuid LastTopVisibleGroup
        {
            get
            {
                return this.m_pwLastTopVisibleGroup;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_pwLastTopVisibleGroup = value;
            }
        }

        /// <summary>
        /// Number of days until history entries are being deleted
        /// in a database maintenance operation.
        /// </summary>
        public uint MaintenanceHistoryDays
        {
            get
            {
                return this.m_uMntncHistoryDays;
            }

            set
            {
                this.m_uMntncHistoryDays = value;
            }
        }

        /// <summary>
        /// The user key used for database encryption. This key must be created
        /// and set before using any of the database load/save functions.
        /// </summary>
        public CompositeKey MasterKey
        {
            get
            {
                return this.m_pwUserKey;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_pwUserKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the master key change force.
        /// </summary>
        public long MasterKeyChangeForce
        {
            get
            {
                return this.m_lKeyChangeForceDays;
            }

            set
            {
                this.m_lKeyChangeForceDays = value;
            }
        }

        /// <summary>
        /// Gets or sets the master key change rec.
        /// </summary>
        public long MasterKeyChangeRec
        {
            get
            {
                return this.m_lKeyChangeRecDays;
            }

            set
            {
                this.m_lKeyChangeRecDays = value;
            }
        }

        /// <summary>
        /// Gets or sets the master key changed.
        /// </summary>
        public DateTime MasterKeyChanged
        {
            get
            {
                return this.m_dtKeyLastChanged;
            }

            set
            {
                this.m_dtKeyLastChanged = value;
            }
        }

        /// <summary>
        /// Memory protection configuration (for default fields).
        /// </summary>
        public MemoryProtectionConfig MemoryProtection
        {
            get
            {
                return this.m_memProtConfig;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_memProtConfig = value;
            }
        }

        /// <summary>
        /// Modification flag. If true, the class has been modified and the
        /// user interface should prompt the user to save the changes before
        /// closing the database for example.
        /// </summary>
        public bool Modified
        {
            get
            {
                return this.m_bModified;
            }

            set
            {
                this.m_bModified = value;
            }
        }

        /// <summary>
        /// Name of the database.
        /// </summary>
        public string Name
        {
            get
            {
                return this.m_strName;
            }

            set
            {
                Debug.Assert(value != null);
                if (value != null)
                {
                    this.m_strName = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name changed.
        /// </summary>
        public DateTime NameChanged
        {
            get
            {
                return this.m_dtNameChanged;
            }

            set
            {
                this.m_dtNameChanged = value;
            }
        }

        /// <summary>
        /// Gets or sets the recycle bin changed.
        /// </summary>
        public DateTime RecycleBinChanged
        {
            get
            {
                return this.m_dtRecycleBinChanged;
            }

            set
            {
                this.m_dtRecycleBinChanged = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether recycle bin enabled.
        /// </summary>
        public bool RecycleBinEnabled
        {
            get
            {
                return this.m_bUseRecycleBin;
            }

            set
            {
                this.m_bUseRecycleBin = value;
            }
        }

        /// <summary>
        /// Gets or sets the recycle bin uuid.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public PwUuid RecycleBinUuid
        {
            get
            {
                return this.m_pwRecycleBin;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_pwRecycleBin = value;
            }
        }

        /// <summary>
        /// Get the root group that contains all groups and entries stored in the
        /// database.
        /// </summary>
        /// <returns>Root group. The return value is <c>null</c>, if no database
        /// has been opened.</returns>
        public PwGroup RootGroup
        {
            get
            {
                return this.m_pgRootGroup;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_pgRootGroup = value;
            }
        }

        /// <summary>
        /// This is a dirty-flag for the UI. It is used to indicate when an
        /// icon list update is required.
        /// </summary>
        public bool UINeedsIconUpdate
        {
            get
            {
                return this.m_bUINeedsIconUpdate;
            }

            set
            {
                this.m_bUINeedsIconUpdate = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use file locks.
        /// </summary>
        public bool UseFileLocks
        {
            get
            {
                return this.m_bUseFileLocks;
            }

            set
            {
                this.m_bUseFileLocks = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use file transactions.
        /// </summary>
        public bool UseFileTransactions
        {
            get
            {
                return this.m_bUseFileTransactions;
            }

            set
            {
                this.m_bUseFileTransactions = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Closes the currently opened database. No confirmation message is shown
        /// before closing. Unsaved changes will be lost.
        /// </summary>
        public void Close()
        {
            this.Clear();
        }

        /// <summary>
        /// The delete custom icons.
        /// </summary>
        /// <param name="vUuidsToDelete">
        /// The v uuids to delete.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public bool DeleteCustomIcons(List<PwUuid> vUuidsToDelete)
        {
            Debug.Assert(vUuidsToDelete != null);
            if (vUuidsToDelete == null)
            {
                throw new ArgumentNullException("vUuidsToDelete");
            }

            if (vUuidsToDelete.Count <= 0)
            {
                return true;
            }

            GroupHandler gh = delegate(PwGroup pg)
                {
                    PwUuid uuidThis = pg.CustomIconUuid;
                    if (uuidThis.Equals(PwUuid.Zero))
                    {
                        return true;
                    }

                    foreach (PwUuid uuidDelete in vUuidsToDelete)
                    {
                        if (uuidThis.Equals(uuidDelete))
                        {
                            pg.CustomIconUuid = PwUuid.Zero;
                            break;
                        }
                    }

                    return true;
                };

            EntryHandler eh = delegate(PwEntry pe)
                {
                    RemoveCustomIconUuid(pe, vUuidsToDelete);
                    return true;
                };

            gh(this.m_pgRootGroup);
            if (!this.m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh))
            {
                Debug.Assert(false);
                return false;
            }

            foreach (PwUuid pwUuid in vUuidsToDelete)
            {
                int nIndex = GetCustomIconIndex(pwUuid);
                if (nIndex >= 0)
                {
                    this.m_vCustomIcons.RemoveAt(nIndex);
                }
            }

            return true;
        }

        /// <summary>
        /// The delete duplicate entries.
        /// </summary>
        /// <param name="sl">
        /// The sl.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        public uint DeleteDuplicateEntries(IStatusLogger sl)
        {
            uint uDeleted = 0;

            PwGroup pgRecycleBin = null;
            if (this.m_bUseRecycleBin)
            {
                pgRecycleBin = this.m_pgRootGroup.FindGroup(this.m_pwRecycleBin, true);
            }

            DateTime dtNow = DateTime.Now;
            PwObjectList<PwEntry> l = this.m_pgRootGroup.GetEntries(true);
            int i = 0;
            while (true)
            {
                if (i >= ((int)l.UCount - 1))
                {
                    break;
                }

                if (sl != null)
                {
                    long lCnt = (long)l.UCount, li = (long)i;
                    long nArTotal = (lCnt * lCnt) / 2L;
                    long nArCur = li * lCnt - ((li * li) / 2L);
                    long nArPct = (nArCur * 100L) / nArTotal;
                    if (nArPct < 0)
                    {
                        nArPct = 0;
                    }

                    if (nArPct > 100)
                    {
                        nArPct = 100;
                    }

                    if (!sl.SetProgress((uint)nArPct))
                    {
                        break;
                    }
                }

                PwEntry peA = l.GetAt((uint)i);

                for (uint j = (uint)i + 1; j < l.UCount; ++j)
                {
                    PwEntry peB = l.GetAt(j);
                    if (!DupEntriesEqual(peA, peB))
                    {
                        continue;
                    }

                    bool bDeleteA = TimeUtil.CompareLastMod(peA, peB, true) <= 0;
                    if (pgRecycleBin != null)
                    {
                        bool bAInBin = peA.IsContainedIn(pgRecycleBin);
                        bool bBInBin = peB.IsContainedIn(pgRecycleBin);

                        if (bAInBin && !bBInBin)
                        {
                            bDeleteA = true;
                        }
                        else if (bBInBin && !bAInBin)
                        {
                            bDeleteA = false;
                        }
                    }

                    if (bDeleteA)
                    {
                        peA.ParentGroup.Entries.Remove(peA);
                        this.m_vDeletedObjects.Add(new PwDeletedObject(peA.Uuid, dtNow));

                        l.RemoveAt((uint)i);
                        --i;
                    }
                    else
                    {
                        peB.ParentGroup.Entries.Remove(peB);
                        this.m_vDeletedObjects.Add(new PwDeletedObject(peB.Uuid, dtNow));

                        l.RemoveAt(j);
                    }

                    ++uDeleted;
                    break;
                }

                ++i;
            }

            return uDeleted;
        }

        /// <summary>
        /// The delete empty groups.
        /// </summary>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        public uint DeleteEmptyGroups()
        {
            uint uDeleted = 0;

            PwObjectList<PwGroup> l = this.m_pgRootGroup.GetGroups(true);
            int iStart = (int)l.UCount - 1;
            for (int i = iStart; i >= 0; --i)
            {
                PwGroup pg = l.GetAt((uint)i);
                if ((pg.Groups.UCount > 0) || (pg.Entries.UCount > 0))
                {
                    continue;
                }

                pg.ParentGroup.Groups.Remove(pg);
                this.m_vDeletedObjects.Add(new PwDeletedObject(pg.Uuid, DateTime.Now));

                ++uDeleted;
            }

            return uDeleted;
        }

        /// <summary>
        /// The delete unused custom icons.
        /// </summary>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        public uint DeleteUnusedCustomIcons()
        {
            List<PwUuid> lToDelete = new List<PwUuid>();
            foreach (PwCustomIcon pwci in this.m_vCustomIcons)
            {
                lToDelete.Add(pwci.Uuid);
            }

            GroupHandler gh = delegate(PwGroup pg)
                {
                    PwUuid pwUuid = pg.CustomIconUuid;
                    if ((pwUuid == null) || pwUuid.Equals(PwUuid.Zero))
                    {
                        return true;
                    }

                    for (int i = 0; i < lToDelete.Count; ++i)
                    {
                        if (lToDelete[i].Equals(pwUuid))
                        {
                            lToDelete.RemoveAt(i);
                            break;
                        }
                    }

                    return true;
                };

            EntryHandler eh = delegate(PwEntry pe)
                {
                    PwUuid pwUuid = pe.CustomIconUuid;
                    if ((pwUuid == null) || pwUuid.Equals(PwUuid.Zero))
                    {
                        return true;
                    }

                    for (int i = 0; i < lToDelete.Count; ++i)
                    {
                        if (lToDelete[i].Equals(pwUuid))
                        {
                            lToDelete.RemoveAt(i);
                            break;
                        }
                    }

                    return true;
                };

            gh(this.m_pgRootGroup);
            this.m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh);

            uint uDeleted = 0;
            foreach (PwUuid pwDel in lToDelete)
            {
                int nIndex = GetCustomIconIndex(pwDel);
                if (nIndex < 0)
                {
                    Debug.Assert(false);
                    continue;
                }

                this.m_vCustomIcons.RemoveAt(nIndex);
                ++uDeleted;
            }

            if (uDeleted > 0)
            {
                this.m_bUINeedsIconUpdate = true;
            }

            return uDeleted;
        }

        /// <summary>
        /// Get the index of a custom icon.
        /// </summary>
        /// <param name="pwIconId">
        /// ID of the icon.
        /// </param>
        /// <returns>
        /// Index of the icon.
        /// </returns>
        public int GetCustomIconIndex(PwUuid pwIconId)
        {
            for (int i = 0; i < this.m_vCustomIcons.Count; ++i)
            {
                PwCustomIcon pwci = this.m_vCustomIcons[i];
                if (pwci.Uuid.Equals(pwIconId))
                {
                    return i;
                }
            }

            // Debug.Assert(false); // Do not assert
            return -1;
        }

        /// <summary>
        /// The get custom icon index.
        /// </summary>
        /// <param name="pbPngData">
        /// The pb png data.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int GetCustomIconIndex(byte[] pbPngData)
        {
            if (pbPngData == null)
            {
                Debug.Assert(false);
                return -1;
            }

            for (int i = 0; i < this.m_vCustomIcons.Count; ++i)
            {
                PwCustomIcon pwci = this.m_vCustomIcons[i];
                byte[] pbEx = pwci.ImageDataPng;
                if (pbEx == null)
                {
                    Debug.Assert(false);
                    continue;
                }

                if (MemUtil.ArraysEqual(pbEx, pbPngData))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// The maintain backups.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool MaintainBackups()
        {
            if (this.m_pgRootGroup == null)
            {
                Debug.Assert(false);
                return false;
            }

            bool bDeleted = false;
            EntryHandler eh = delegate(PwEntry pe)
                {
                    if (pe.MaintainBackups(this))
                    {
                        bDeleted = true;
                    }

                    return true;
                };

            this.m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);
            return bDeleted;
        }

        /// <summary>
        /// The merge in.
        /// </summary>
        /// <param name="pwSource">
        /// The pw source.
        /// </param>
        /// <param name="mm">
        /// The mm.
        /// </param>
        public void MergeIn(PwDatabase pwSource, PwMergeMethod mm)
        {
            this.MergeIn(pwSource, mm, null);
        }

        /// <summary>
        /// Synchronize the current database with another one.
        /// </summary>
        /// <param name="pwSource">
        /// Input database to synchronize with. This input
        /// database is used to update the current one, but is not modified! You
        /// must copy the current object if you want a second instance of the
        /// synchronized database. The input database must not be seen as valid
        /// database any more after calling <c>Synchronize</c>.
        /// </param>
        /// <param name="mm">
        /// Merge method.
        /// </param>
        /// <param name="slStatus">
        /// Logger to report status messages to.
        /// May be <c>null</c>.
        /// </param>
        public void MergeIn(PwDatabase pwSource, PwMergeMethod mm, IStatusLogger slStatus)
        {
            if (pwSource == null)
            {
                throw new ArgumentNullException("pwSource");
            }

            PwGroup pgOrgStructure = this.m_pgRootGroup.CloneStructure();
            PwGroup pgSrcStructure = pwSource.m_pgRootGroup.CloneStructure();

            if (mm == PwMergeMethod.CreateNewUuids)
            {
                pwSource.RootGroup.CreateNewItemUuids(true, true, true);
            }

            GroupHandler gh = delegate(PwGroup pg)
                {
                    if (pg == pwSource.m_pgRootGroup)
                    {
                        return true;
                    }

                    PwGroup pgLocal = this.m_pgRootGroup.FindGroup(pg.Uuid, true);
                    if (pgLocal == null)
                    {
                        PwGroup pgSourceParent = pg.ParentGroup;
                        PwGroup pgLocalContainer;
                        if (pgSourceParent == pwSource.m_pgRootGroup)
                        {
                            pgLocalContainer = this.m_pgRootGroup;
                        }
                        else
                        {
                            pgLocalContainer = this.m_pgRootGroup.FindGroup(pgSourceParent.Uuid, true);
                        }

                        Debug.Assert(pgLocalContainer != null);
                        if (pgLocalContainer == null)
                        {
                            pgLocalContainer = this.m_pgRootGroup;
                        }

                        PwGroup pgNew = new PwGroup(false, false);
                        pgNew.Uuid = pg.Uuid;
                        pgNew.AssignProperties(pg, false, true);
                        pgLocalContainer.AddGroup(pgNew, true);
                    }
                    else
                    {
                        // pgLocal != null
                        Debug.Assert(mm != PwMergeMethod.CreateNewUuids);

                        if (mm == PwMergeMethod.OverwriteExisting)
                        {
                            pgLocal.AssignProperties(pg, false, false);
                        }
                        else if ((mm == PwMergeMethod.OverwriteIfNewer) || (mm == PwMergeMethod.Synchronize))
                        {
                            pgLocal.AssignProperties(pg, true, false);
                        }

                        // else if(mm == PwMergeMethod.KeepExisting) ...
                    }

                    return (slStatus != null) ? slStatus.ContinueWork() : true;
                };

            EntryHandler eh = delegate(PwEntry pe)
                {
                    PwEntry peLocal = this.m_pgRootGroup.FindEntry(pe.Uuid, true);
                    if (peLocal == null)
                    {
                        PwGroup pgSourceParent = pe.ParentGroup;
                        PwGroup pgLocalContainer;
                        if (pgSourceParent == pwSource.m_pgRootGroup)
                        {
                            pgLocalContainer = this.m_pgRootGroup;
                        }
                        else
                        {
                            pgLocalContainer = this.m_pgRootGroup.FindGroup(pgSourceParent.Uuid, true);
                        }

                        Debug.Assert(pgLocalContainer != null);
                        if (pgLocalContainer == null)
                        {
                            pgLocalContainer = this.m_pgRootGroup;
                        }

                        PwEntry peNew = new PwEntry(false, false);
                        peNew.Uuid = pe.Uuid;
                        peNew.AssignProperties(pe, false, true, true);
                        pgLocalContainer.AddEntry(peNew, true);
                    }
                    else
                    {
                        // peLocal != null
                        Debug.Assert(mm != PwMergeMethod.CreateNewUuids);

                        const PwCompareOptions cmpOpt =
                            PwCompareOptions.IgnoreParentGroup | PwCompareOptions.IgnoreLastAccess | PwCompareOptions.IgnoreHistory
                            | PwCompareOptions.NullEmptyEquivStd;
                        bool bEquals = peLocal.EqualsEntry(pe, cmpOpt, MemProtCmpMode.None);

                        bool bOrgBackup = !bEquals;
                        if (mm != PwMergeMethod.OverwriteExisting)
                        {
                            bOrgBackup &= TimeUtil.CompareLastMod(pe, peLocal, true) > 0;
                        }

                        bOrgBackup &= !pe.HasBackupOfData(peLocal, false, true);
                        if (bOrgBackup)
                        {
                            peLocal.CreateBackup(null); // Maintain at end
                        }

                        bool bSrcBackup = !bEquals && (mm != PwMergeMethod.OverwriteExisting);
                        bSrcBackup &= TimeUtil.CompareLastMod(peLocal, pe, true) > 0;
                        bSrcBackup &= !peLocal.HasBackupOfData(pe, false, true);
                        if (bSrcBackup)
                        {
                            pe.CreateBackup(null); // Maintain at end
                        }

                        if (mm == PwMergeMethod.OverwriteExisting)
                        {
                            peLocal.AssignProperties(pe, false, false, false);
                        }
                        else if ((mm == PwMergeMethod.OverwriteIfNewer) || (mm == PwMergeMethod.Synchronize))
                        {
                            peLocal.AssignProperties(pe, true, false, false);
                        }

                        // else if(mm == PwMergeMethod.KeepExisting) ...
                        this.MergeEntryHistory(peLocal, pe, mm);
                    }

                    return (slStatus != null) ? slStatus.ContinueWork() : true;
                };

            if (!pwSource.RootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh))
            {
                throw new InvalidOperationException();
            }

            IStatusLogger slPrevStatus = this.m_slStatus;
            this.m_slStatus = slStatus;

            if (mm == PwMergeMethod.Synchronize)
            {
                this.ApplyDeletions(pwSource.m_vDeletedObjects, true);
                this.ApplyDeletions(this.m_vDeletedObjects, false);

                PwObjectPool ppOrgGroups = PwObjectPool.FromGroupRecursive(pgOrgStructure, false);
                PwObjectPool ppSrcGroups = PwObjectPool.FromGroupRecursive(pgSrcStructure, false);
                PwObjectPool ppOrgEntries = PwObjectPool.FromGroupRecursive(pgOrgStructure, true);
                PwObjectPool ppSrcEntries = PwObjectPool.FromGroupRecursive(pgSrcStructure, true);

                this.RelocateGroups(ppOrgGroups, ppSrcGroups);
                this.ReorderGroups(ppOrgGroups, ppSrcGroups);
                this.RelocateEntries(ppOrgEntries, ppSrcEntries);
                this.ReorderEntries(ppOrgEntries, ppSrcEntries);
                Debug.Assert(this.ValidateUuidUniqueness());
            }

            // Must be called *after* merging groups, because group UUIDs
            // are required for recycle bin and entry template UUIDs
            this.MergeInDbProperties(pwSource, mm);

            this.MergeInCustomIcons(pwSource);

            this.MaintainBackups();

            this.m_slStatus = slPrevStatus;
        }

        /// <summary>
        /// Initialize the class for managing a new database. Previously loaded
        /// data is deleted.
        /// </summary>
        /// <param name="ioConnection">
        /// IO connection of the new database.
        /// </param>
        /// <param name="pwKey">
        /// Key to open the database.
        /// </param>
        public void New(IOConnectionInfo ioConnection, CompositeKey pwKey)
        {
            Debug.Assert(ioConnection != null);
            if (ioConnection == null)
            {
                throw new ArgumentNullException("ioConnection");
            }

            Debug.Assert(pwKey != null);
            if (pwKey == null)
            {
                throw new ArgumentNullException("pwKey");
            }

            this.Close();

            this.m_pwUserKey = pwKey;

            this.m_bDatabaseOpened = true;
            this.m_bModified = true;

            this.m_pgRootGroup = new PwGroup(true, true, UrlUtil.StripExtension(UrlUtil.GetFileName(ioConnection.Path)), PwIcon.FolderOpen);
            this.m_pgRootGroup.IsExpanded = true;
        }

        /// <summary>
        /// Open a database. The URL may point to any supported data source.
        /// </summary>
        /// <param name="ioSource">
        /// IO connection to load the database from.
        /// </param>
        /// <param name="pwKey">
        /// Key used to open the specified database.
        /// </param>
        /// <param name="slLogger">
        /// Logger, which gets all status messages.
        /// </param>
        public void Open(Stream ioSource, CompositeKey pwKey, IStatusLogger slLogger, CancellationToken token)
        {
            Debug.Assert(ioSource != null);
            if (ioSource == null)
            {
                throw new ArgumentNullException("ioSource");
            }

            Debug.Assert(pwKey != null);
            if (pwKey == null)
            {
                throw new ArgumentNullException("pwKey");
            }

            this.Close();

            try
            {
                this.m_pgRootGroup = new PwGroup(true, true);
                this.m_pgRootGroup.IsExpanded = true;

                this.m_pwUserKey = pwKey;

                this.m_bModified = false;

                var kdbx = new KdbxFile(this) { DetachBinaries = this.m_strDetachBins };

                kdbx.Load(ioSource, KdbxFormat.Default, slLogger, token);

                this.m_pbHashOfLastIO = kdbx.HashOfFileOnDisk;
                this.m_pbHashOfFileOnDisk = kdbx.HashOfFileOnDisk;
                // Debug.Assert(this.m_pbHashOfFileOnDisk != null);

                this.m_bDatabaseOpened = true;
            }
            catch (Exception)
            {
                this.Clear();
                throw;
            }
        }

        public void ReadHeader(Stream ioSource)
        {
            if (ioSource == null)
            {
                throw new ArgumentNullException("ioSource");
            }

            this.Close();

            try
            {
                this.m_pgRootGroup = new PwGroup(true, true) { IsExpanded = true };

                this.m_bModified = false;

                var kdbx = new KdbxFile(this) { DetachBinaries = this.m_strDetachBins };

                kdbx.LoadHeader(ioSource, KdbxFormat.Default);

                this.m_pbHashOfLastIO = kdbx.HashOfFileOnDisk;
                this.m_pbHashOfFileOnDisk = kdbx.HashOfFileOnDisk;
                //Debug.Assert(this.m_pbHashOfFileOnDisk != null);

                this.m_bDatabaseOpened = true;
            }
            catch (Exception)
            {
                this.Clear();
                throw;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dup entries equal.
        /// </summary>
        /// <param name="a">
        /// The a.
        /// </param>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool DupEntriesEqual(PwEntry a, PwEntry b)
        {
            if (m_lStdFields == null)
            {
                m_lStdFields = PwDefs.GetStandardFields();
            }

            foreach (string strStdKey in m_lStdFields)
            {
                string strA = a.Strings.ReadSafe(strStdKey);
                string strB = b.Strings.ReadSafe(strStdKey);
                if (!strA.Equals(strB))
                {
                    return false;
                }
            }

            foreach (KeyValuePair<string, ProtectedString> kvpA in a.Strings)
            {
                if (PwDefs.IsStandardField(kvpA.Key))
                {
                    continue;
                }

                ProtectedString psB = b.Strings.Get(kvpA.Key);
                if (psB == null)
                {
                    return false;
                }

                // Ignore protection setting, compare values only
                if (!kvpA.Value.ReadString().Equals(psB.ReadString()))
                {
                    return false;
                }
            }

            foreach (KeyValuePair<string, ProtectedString> kvpB in b.Strings)
            {
                if (PwDefs.IsStandardField(kvpB.Key))
                {
                    continue;
                }

                ProtectedString psA = a.Strings.Get(kvpB.Key);
                if (psA == null)
                {
                    return false;
                }

                // Must be equal by logic
                Debug.Assert(kvpB.Value.ReadString().Equals(psA.ReadString()));
            }

            if (a.Binaries.UCount != b.Binaries.UCount)
            {
                return false;
            }

            foreach (KeyValuePair<string, ProtectedBinary> kvpBin in a.Binaries)
            {
                ProtectedBinary pbB = b.Binaries.Get(kvpBin.Key);
                if (pbB == null)
                {
                    return false;
                }

                // Ignore protection setting, compare values only
                byte[] pbDataA = kvpBin.Value.ReadData();
                byte[] pbDataB = pbB.ReadData();
                bool bBinEq = MemUtil.ArraysEqual(pbDataA, pbDataB);
                MemUtil.ZeroByteArray(pbDataA);
                MemUtil.ZeroByteArray(pbDataB);
                if (!bBinEq)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The find location changed pivot.
        /// </summary>
        /// <param name="vItems">
        /// The v items.
        /// </param>
        /// <param name="kvpRange">
        /// The kvp range.
        /// </param>
        /// <param name="ppOrgStructure">
        /// The pp org structure.
        /// </param>
        /// <param name="ppSrcStructure">
        /// The pp src structure.
        /// </param>
        /// <param name="qBefore">
        /// The q before.
        /// </param>
        /// <param name="qAfter">
        /// The q after.
        /// </param>
        /// <param name="bEntries">
        /// The b entries.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        private static uint FindLocationChangedPivot<T>(
            PwObjectList<T> vItems, 
            KeyValuePair<uint, uint> kvpRange, 
            PwObjectPool ppOrgStructure, 
            PwObjectPool ppSrcStructure, 
            Queue<PwUuid> qBefore, 
            Queue<PwUuid> qAfter, 
            bool bEntries) where T : class, IStructureItem, IDeepCloneable<T>
        {
            uint uPosMax = kvpRange.Key;
            DateTime dtMax = DateTime.MinValue;
            List<IStructureItem> vNeighborSrc = null;

            for (uint u = kvpRange.Key; u <= kvpRange.Value; ++u)
            {
                T pt = vItems.GetAt(u);

                // IStructureItem ptOrg = pgOrgStructure.FindObject(pt.Uuid, true, bEntries);
                IStructureItem ptOrg = ppOrgStructure.Get(pt.Uuid);
                if ((ptOrg != null) && (ptOrg.LocationChanged > dtMax))
                {
                    uPosMax = u;
                    dtMax = ptOrg.LocationChanged; // No 'continue'
                    vNeighborSrc = ptOrg.ParentGroup.GetObjects(false, bEntries);
                }

                // IStructureItem ptSrc = pgSrcStructure.FindObject(pt.Uuid, true, bEntries);
                IStructureItem ptSrc = ppSrcStructure.Get(pt.Uuid);
                if ((ptSrc != null) && (ptSrc.LocationChanged > dtMax))
                {
                    uPosMax = u;
                    dtMax = ptSrc.LocationChanged; // No 'continue'
                    vNeighborSrc = ptSrc.ParentGroup.GetObjects(false, bEntries);
                }
            }

            GetNeighborItems(vNeighborSrc, vItems.GetAt(uPosMax).Uuid, qBefore, qAfter);
            return uPosMax;
        }

        /// <summary>
        /// The get neighbor items.
        /// </summary>
        /// <param name="vItems">
        /// The v items.
        /// </param>
        /// <param name="pwPivot">
        /// The pw pivot.
        /// </param>
        /// <param name="qBefore">
        /// The q before.
        /// </param>
        /// <param name="qAfter">
        /// The q after.
        /// </param>
        private static void GetNeighborItems(List<IStructureItem> vItems, PwUuid pwPivot, Queue<PwUuid> qBefore, Queue<PwUuid> qAfter)
        {
            qBefore.Clear();
            qAfter.Clear();

            // Checks after clearing the queues
            if (vItems == null)
            {
                Debug.Assert(false);
                return;
            }
 // No throw

            bool bBefore = true;
            for (int i = 0; i < vItems.Count; ++i)
            {
                PwUuid pw = vItems[i].Uuid;

                if (pw.Equals(pwPivot))
                {
                    bBefore = false;
                }
                else if (bBefore)
                {
                    qBefore.Enqueue(pw);
                }
                else
                {
                    qAfter.Enqueue(pw);
                }
            }

            Debug.Assert(bBefore == false);
        }

        /// <summary>
        /// The remove custom icon uuid.
        /// </summary>
        /// <param name="pe">
        /// The pe.
        /// </param>
        /// <param name="vToDelete">
        /// The v to delete.
        /// </param>
        private static void RemoveCustomIconUuid(PwEntry pe, List<PwUuid> vToDelete)
        {
            PwUuid uuidThis = pe.CustomIconUuid;
            if (uuidThis.Equals(PwUuid.Zero))
            {
                return;
            }

            foreach (PwUuid uuidDelete in vToDelete)
            {
                if (uuidThis.Equals(uuidDelete))
                {
                    pe.CustomIconUuid = PwUuid.Zero;
                    break;
                }
            }

            foreach (PwEntry peHistory in pe.History)
            {
                RemoveCustomIconUuid(peHistory, vToDelete);
            }
        }

        /// <summary>
        /// The apply deletions.
        /// </summary>
        /// <param name="listDelObjects">
        /// The list del objects.
        /// </param>
        /// <param name="bCopyDeletionInfoToLocal">
        /// The b copy deletion info to local.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        private void ApplyDeletions(PwObjectList<PwDeletedObject> listDelObjects, bool bCopyDeletionInfoToLocal)
        {
            Debug.Assert(listDelObjects != null);
            if (listDelObjects == null)
            {
                throw new ArgumentNullException("listDelObjects");
            }

            LinkedList<PwGroup> listGroupsToDelete = new LinkedList<PwGroup>();
            LinkedList<PwEntry> listEntriesToDelete = new LinkedList<PwEntry>();

            GroupHandler gh = delegate(PwGroup pg)
                {
                    if (pg == this.m_pgRootGroup)
                    {
                        return true;
                    }

                    foreach (PwDeletedObject pdo in listDelObjects)
                    {
                        if (pg.Uuid.Equals(pdo.Uuid))
                        {
                            if (TimeUtil.Compare(pg.LastModificationTime, pdo.DeletionTime, true) < 0)
                            {
                                listGroupsToDelete.AddLast(pg);
                            }
                        }
                    }

                    return (this.m_slStatus != null) ? this.m_slStatus.ContinueWork() : true;
                };

            EntryHandler eh = delegate(PwEntry pe)
                {
                    foreach (PwDeletedObject pdo in listDelObjects)
                    {
                        if (pe.Uuid.Equals(pdo.Uuid))
                        {
                            if (TimeUtil.Compare(pe.LastModificationTime, pdo.DeletionTime, true) < 0)
                            {
                                listEntriesToDelete.AddLast(pe);
                            }
                        }
                    }

                    return (this.m_slStatus != null) ? this.m_slStatus.ContinueWork() : true;
                };

            this.m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh);

            foreach (PwGroup pg in listGroupsToDelete)
            {
                pg.ParentGroup.Groups.Remove(pg);
            }

            foreach (PwEntry pe in listEntriesToDelete)
            {
                pe.ParentGroup.Entries.Remove(pe);
            }

            if (bCopyDeletionInfoToLocal)
            {
                foreach (PwDeletedObject pdoNew in listDelObjects)
                {
                    bool bCopy = true;

                    foreach (PwDeletedObject pdoLocal in this.m_vDeletedObjects)
                    {
                        if (pdoNew.Uuid.Equals(pdoLocal.Uuid))
                        {
                            bCopy = false;

                            if (pdoNew.DeletionTime > pdoLocal.DeletionTime)
                            {
                                pdoLocal.DeletionTime = pdoNew.DeletionTime;
                            }

                            break;
                        }
                    }

                    if (bCopy)
                    {
                        this.m_vDeletedObjects.Add(pdoNew);
                    }
                }
            }
        }

        /// <summary>
        /// The clear.
        /// </summary>
        private void Clear()
        {
            this.m_pgRootGroup = null;
            this.m_vDeletedObjects = new PwObjectList<PwDeletedObject>();

            this.m_uuidDataCipher = StandardAesEngine.AesUuid;
            this.m_caCompression = PwCompressionAlgorithm.GZip;
            this.m_uKeyEncryptionRounds = PwDefs.DefaultKeyEncryptionRounds;

            this.m_pwUserKey = null;
            this.m_memProtConfig = new MemoryProtectionConfig();

            this.m_vCustomIcons = new List<PwCustomIcon>();
            this.m_bUINeedsIconUpdate = true;

            DateTime dtNow = DateTime.Now;

            this.m_strName = string.Empty;
            this.m_dtNameChanged = dtNow;
            this.m_strDesc = string.Empty;
            this.m_dtDescChanged = dtNow;
            this.m_strDefaultUserName = string.Empty;
            this.m_dtDefaultUserChanged = dtNow;
            this.m_uMntncHistoryDays = 365;
            this.m_clr = string.Empty;

            this.m_dtKeyLastChanged = dtNow;
            this.m_lKeyChangeRecDays = -1;
            this.m_lKeyChangeForceDays = -1;

            this.m_bDatabaseOpened = false;
            this.m_bModified = false;

            this.m_pwLastSelectedGroup = PwUuid.Zero;
            this.m_pwLastTopVisibleGroup = PwUuid.Zero;

            this.m_bUseRecycleBin = true;
            this.m_pwRecycleBin = PwUuid.Zero;
            this.m_dtRecycleBinChanged = dtNow;
            this.m_pwEntryTemplatesGroup = PwUuid.Zero;
            this.m_dtEntryTemplatesChanged = dtNow;

            this.m_nHistoryMaxItems = DefaultHistoryMaxItems;
            this.m_lHistoryMaxSize = DefaultHistoryMaxSize;

            this.m_vCustomData = new StringDictionaryEx();

            this.m_pbHashOfFileOnDisk = null;
            this.m_pbHashOfLastIO = null;

            this.m_bUseFileTransactions = false;
            this.m_bUseFileLocks = false;
        }

        /// <summary>
        /// The merge entry history.
        /// </summary>
        /// <param name="pe">
        /// The pe.
        /// </param>
        /// <param name="peSource">
        /// The pe source.
        /// </param>
        /// <param name="mm">
        /// The mm.
        /// </param>
        private void MergeEntryHistory(PwEntry pe, PwEntry peSource, PwMergeMethod mm)
        {
            if (!pe.Uuid.Equals(peSource.Uuid))
            {
                Debug.Assert(false);
                return;
            }

            if (pe.History.UCount == peSource.History.UCount)
            {
                bool bEqual = true;
                for (uint uEnum = 0; uEnum < pe.History.UCount; ++uEnum)
                {
                    if (pe.History.GetAt(uEnum).LastModificationTime != peSource.History.GetAt(uEnum).LastModificationTime)
                    {
                        bEqual = false;
                        break;
                    }
                }

                if (bEqual)
                {
                    return;
                }
            }

            if ((this.m_slStatus != null) && !this.m_slStatus.ContinueWork())
            {
                return;
            }

            IDictionary<DateTime, PwEntry> dict =
#if KeePassLibSD
				new SortedList<DateTime, PwEntry>();
#else
                new SortedDictionary<DateTime, PwEntry>();
#endif
            foreach (PwEntry peOrg in pe.History)
            {
                dict[peOrg.LastModificationTime] = peOrg;
            }

            foreach (PwEntry peSrc in peSource.History)
            {
                DateTime dt = peSrc.LastModificationTime;
                if (dict.ContainsKey(dt))
                {
                    if (mm == PwMergeMethod.OverwriteExisting)
                    {
                        dict[dt] = peSrc.CloneDeep();
                    }
                }
                else
                {
                    dict[dt] = peSrc.CloneDeep();
                }
            }

            pe.History.Clear();
            foreach (KeyValuePair<DateTime, PwEntry> kvpCur in dict)
            {
                Debug.Assert(kvpCur.Value.Uuid.Equals(pe.Uuid));
                Debug.Assert(kvpCur.Value.History.UCount == 0);
                pe.History.Add(kvpCur.Value);
            }
        }

        /// <summary>
        /// The merge in custom icons.
        /// </summary>
        /// <param name="pwSource">
        /// The pw source.
        /// </param>
        private void MergeInCustomIcons(PwDatabase pwSource)
        {
            foreach (PwCustomIcon pwci in pwSource.CustomIcons)
            {
                if (GetCustomIconIndex(pwci.Uuid) >= 0)
                {
                    continue;
                }

                this.m_vCustomIcons.Add(pwci); // PwCustomIcon is immutable
                this.m_bUINeedsIconUpdate = true;
            }
        }

        /// <summary>
        /// The merge in db properties.
        /// </summary>
        /// <param name="pwSource">
        /// The pw source.
        /// </param>
        /// <param name="mm">
        /// The mm.
        /// </param>
        private void MergeInDbProperties(PwDatabase pwSource, PwMergeMethod mm)
        {
            if (pwSource == null)
            {
                Debug.Assert(false);
                return;
            }

            if ((mm == PwMergeMethod.KeepExisting) || (mm == PwMergeMethod.None))
            {
                return;
            }

            bool bForce = mm == PwMergeMethod.OverwriteExisting;

            if (bForce || (pwSource.m_dtNameChanged > this.m_dtNameChanged))
            {
                this.m_strName = pwSource.m_strName;
                this.m_dtNameChanged = pwSource.m_dtNameChanged;
            }

            if (bForce || (pwSource.m_dtDescChanged > this.m_dtDescChanged))
            {
                this.m_strDesc = pwSource.m_strDesc;
                this.m_dtDescChanged = pwSource.m_dtDescChanged;
            }

            if (bForce || (pwSource.m_dtDefaultUserChanged > this.m_dtDefaultUserChanged))
            {
                this.m_strDefaultUserName = pwSource.m_strDefaultUserName;
                this.m_dtDefaultUserChanged = pwSource.m_dtDefaultUserChanged;
            }

            if (bForce)
            {
                this.m_clr = pwSource.m_clr;
            }

            PwUuid pwPrefBin = this.m_pwRecycleBin, pwAltBin = pwSource.m_pwRecycleBin;
            if (bForce || (pwSource.m_dtRecycleBinChanged > this.m_dtRecycleBinChanged))
            {
                pwPrefBin = pwSource.m_pwRecycleBin;
                pwAltBin = this.m_pwRecycleBin;
                this.m_bUseRecycleBin = pwSource.m_bUseRecycleBin;
                this.m_dtRecycleBinChanged = pwSource.m_dtRecycleBinChanged;
            }

            if (this.m_pgRootGroup.FindGroup(pwPrefBin, true) != null)
            {
                this.m_pwRecycleBin = pwPrefBin;
            }
            else if (this.m_pgRootGroup.FindGroup(pwAltBin, true) != null)
            {
                this.m_pwRecycleBin = pwAltBin;
            }
            else
            {
                this.m_pwRecycleBin = PwUuid.Zero; // Debug.Assert(false);
            }

            PwUuid pwPrefTmp = this.m_pwEntryTemplatesGroup, pwAltTmp = pwSource.m_pwEntryTemplatesGroup;
            if (bForce || (pwSource.m_dtEntryTemplatesChanged > this.m_dtEntryTemplatesChanged))
            {
                pwPrefTmp = pwSource.m_pwEntryTemplatesGroup;
                pwAltTmp = this.m_pwEntryTemplatesGroup;
                this.m_dtEntryTemplatesChanged = pwSource.m_dtEntryTemplatesChanged;
            }

            if (this.m_pgRootGroup.FindGroup(pwPrefTmp, true) != null)
            {
                this.m_pwEntryTemplatesGroup = pwPrefTmp;
            }
            else if (this.m_pgRootGroup.FindGroup(pwAltTmp, true) != null)
            {
                this.m_pwEntryTemplatesGroup = pwAltTmp;
            }
            else
            {
                this.m_pwEntryTemplatesGroup = PwUuid.Zero; // Debug.Assert(false);
            }
        }

        /// <summary>
        /// Method to check whether a reordering is required. This fast test
        /// allows to skip the reordering routine, resulting in a large
        /// performance increase.
        /// </summary>
        /// <param name="vItems">
        /// The v Items.
        /// </param>
        /// <param name="ppOrgStructure">
        /// The pp Org Structure.
        /// </param>
        /// <param name="ppSrcStructure">
        /// The pp Src Structure.
        /// </param>
        /// <param name="bEntries">
        /// The b Entries.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ObjectListRequiresReorder<T>(PwObjectList<T> vItems, PwObjectPool ppOrgStructure, PwObjectPool ppSrcStructure, bool bEntries)
            where T : class, IStructureItem, IDeepCloneable<T>
        {
            Debug.Assert(ppOrgStructure.ContainsOnlyType(bEntries ? typeof(PwEntry) : typeof(PwGroup)));
            Debug.Assert(ppSrcStructure.ContainsOnlyType(bEntries ? typeof(PwEntry) : typeof(PwGroup)));
            if (vItems.UCount <= 1)
            {
                return false;
            }

            if ((this.m_slStatus != null) && !this.m_slStatus.ContinueWork())
            {
                return false;
            }

            T ptFirst = vItems.GetAt(0);

            // IStructureItem ptOrg = pgOrgStructure.FindObject(ptFirst.Uuid, true, bEntries);
            IStructureItem ptOrg = ppOrgStructure.Get(ptFirst.Uuid);
            if (ptOrg == null)
            {
                return true;
            }

            // IStructureItem ptSrc = pgSrcStructure.FindObject(ptFirst.Uuid, true, bEntries);
            IStructureItem ptSrc = ppSrcStructure.Get(ptFirst.Uuid);
            if (ptSrc == null)
            {
                return true;
            }

            if (ptFirst.ParentGroup == null)
            {
                Debug.Assert(false);
                return true;
            }

            PwGroup pgOrgParent = ptOrg.ParentGroup;
            if (pgOrgParent == null)
            {
                return true; // Root might be in tree
            }

            PwGroup pgSrcParent = ptSrc.ParentGroup;
            if (pgSrcParent == null)
            {
                return true; // Root might be in tree
            }

            if (!ptFirst.ParentGroup.Uuid.Equals(pgOrgParent.Uuid))
            {
                return true;
            }

            if (!pgOrgParent.Uuid.Equals(pgSrcParent.Uuid))
            {
                return true;
            }

            List<IStructureItem> lOrg = pgOrgParent.GetObjects(false, bEntries);
            List<IStructureItem> lSrc = pgSrcParent.GetObjects(false, bEntries);
            if (vItems.UCount != (uint)lOrg.Count)
            {
                return true;
            }

            if (lOrg.Count != lSrc.Count)
            {
                return true;
            }

            for (uint u = 0; u < vItems.UCount; ++u)
            {
                IStructureItem pt = vItems.GetAt(u);
                Debug.Assert(pt.ParentGroup == ptFirst.ParentGroup);

                if (!pt.Uuid.Equals(lOrg[(int)u].Uuid))
                {
                    return true;
                }

                if (!pt.Uuid.Equals(lSrc[(int)u].Uuid))
                {
                    return true;
                }

                if (pt.LocationChanged != lOrg[(int)u].LocationChanged)
                {
                    return true;
                }

                if (pt.LocationChanged != lSrc[(int)u].LocationChanged)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The relocate entries.
        /// </summary>
        /// <param name="ppOrgStructure">
        /// The pp org structure.
        /// </param>
        /// <param name="ppSrcStructure">
        /// The pp src structure.
        /// </param>
        private void RelocateEntries(PwObjectPool ppOrgStructure, PwObjectPool ppSrcStructure)
        {
            PwObjectList<PwEntry> vEntries = this.m_pgRootGroup.GetEntries(true);

            foreach (PwEntry pe in vEntries)
            {
                if ((this.m_slStatus != null) && !this.m_slStatus.ContinueWork())
                {
                    break;
                }

                // PwEntry peOrg = pgOrgStructure.FindEntry(pe.Uuid, true);
                IStructureItem ptOrg = ppOrgStructure.Get(pe.Uuid);
                if (ptOrg == null)
                {
                    continue;
                }

                // PwEntry peSrc = pgSrcStructure.FindEntry(pe.Uuid, true);
                IStructureItem ptSrc = ppSrcStructure.Get(pe.Uuid);
                if (ptSrc == null)
                {
                    continue;
                }

                PwGroup pgOrg = ptOrg.ParentGroup;
                PwGroup pgSrc = ptSrc.ParentGroup;
                if (pgOrg.Uuid.Equals(pgSrc.Uuid))
                {
                    pe.LocationChanged = (ptSrc.LocationChanged > ptOrg.LocationChanged) ? ptSrc.LocationChanged : ptOrg.LocationChanged;
                    continue;
                }

                if (ptSrc.LocationChanged > ptOrg.LocationChanged)
                {
                    PwGroup pgLocal = this.m_pgRootGroup.FindGroup(pgSrc.Uuid, true);
                    if (pgLocal == null)
                    {
                        Debug.Assert(false);
                        continue;
                    }

                    pe.ParentGroup.Entries.Remove(pe);
                    pgLocal.AddEntry(pe, true);
                    pe.LocationChanged = ptSrc.LocationChanged;
                }
                else
                {
                    Debug.Assert(pe.ParentGroup.Uuid.Equals(pgOrg.Uuid));
                    Debug.Assert(pe.LocationChanged == ptOrg.LocationChanged);
                }
            }

            Debug.Assert(this.m_pgRootGroup.GetEntries(true).UCount == vEntries.UCount);
        }

        /// <summary>
        /// The relocate groups.
        /// </summary>
        /// <param name="ppOrgStructure">
        /// The pp org structure.
        /// </param>
        /// <param name="ppSrcStructure">
        /// The pp src structure.
        /// </param>
        private void RelocateGroups(PwObjectPool ppOrgStructure, PwObjectPool ppSrcStructure)
        {
            PwObjectList<PwGroup> vGroups = this.m_pgRootGroup.GetGroups(true);

            foreach (PwGroup pg in vGroups)
            {
                if ((this.m_slStatus != null) && !this.m_slStatus.ContinueWork())
                {
                    break;
                }

                // PwGroup pgOrg = pgOrgStructure.FindGroup(pg.Uuid, true);
                IStructureItem ptOrg = ppOrgStructure.Get(pg.Uuid);
                if (ptOrg == null)
                {
                    continue;
                }

                // PwGroup pgSrc = pgSrcStructure.FindGroup(pg.Uuid, true);
                IStructureItem ptSrc = ppSrcStructure.Get(pg.Uuid);
                if (ptSrc == null)
                {
                    continue;
                }

                PwGroup pgOrgParent = ptOrg.ParentGroup;
                PwGroup pgSrcParent = ptSrc.ParentGroup;
                if (pgOrgParent.Uuid.Equals(pgSrcParent.Uuid))
                {
                    pg.LocationChanged = (ptSrc.LocationChanged > ptOrg.LocationChanged) ? ptSrc.LocationChanged : ptOrg.LocationChanged;
                    continue;
                }

                if (ptSrc.LocationChanged > ptOrg.LocationChanged)
                {
                    PwGroup pgLocal = this.m_pgRootGroup.FindGroup(pgSrcParent.Uuid, true);
                    if (pgLocal == null)
                    {
                        Debug.Assert(false);
                        continue;
                    }

                    if (pgLocal.IsContainedIn(pg))
                    {
                        continue;
                    }

                    pg.ParentGroup.Groups.Remove(pg);
                    pgLocal.AddGroup(pg, true);
                    pg.LocationChanged = ptSrc.LocationChanged;
                }
                else
                {
                    Debug.Assert(pg.ParentGroup.Uuid.Equals(pgOrgParent.Uuid));
                    Debug.Assert(pg.LocationChanged == ptOrg.LocationChanged);
                }
            }

            Debug.Assert(this.m_pgRootGroup.GetGroups(true).UCount == vGroups.UCount);
        }

        /// <summary>
        /// The reorder entries.
        /// </summary>
        /// <param name="ppOrgStructure">
        /// The pp org structure.
        /// </param>
        /// <param name="ppSrcStructure">
        /// The pp src structure.
        /// </param>
        private void ReorderEntries(PwObjectPool ppOrgStructure, PwObjectPool ppSrcStructure)
        {
            GroupHandler gh = delegate(PwGroup pg)
                {
                    this.ReorderObjectList<PwEntry>(pg.Entries, ppOrgStructure, ppSrcStructure, true);
                    return true;
                };

            this.ReorderObjectList<PwEntry>(this.m_pgRootGroup.Entries, ppOrgStructure, ppSrcStructure, true);
            this.m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, gh, null);
        }

        /// <summary>
        /// The reorder groups.
        /// </summary>
        /// <param name="ppOrgStructure">
        /// The pp org structure.
        /// </param>
        /// <param name="ppSrcStructure">
        /// The pp src structure.
        /// </param>
        private void ReorderGroups(PwObjectPool ppOrgStructure, PwObjectPool ppSrcStructure)
        {
            GroupHandler gh = delegate(PwGroup pg)
                {
                    this.ReorderObjectList<PwGroup>(pg.Groups, ppOrgStructure, ppSrcStructure, false);
                    return true;
                };

            this.ReorderObjectList<PwGroup>(this.m_pgRootGroup.Groups, ppOrgStructure, ppSrcStructure, false);
            this.m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, gh, null);
        }

        /// <summary>
        /// The reorder object list.
        /// </summary>
        /// <param name="vItems">
        /// The v items.
        /// </param>
        /// <param name="ppOrgStructure">
        /// The pp org structure.
        /// </param>
        /// <param name="ppSrcStructure">
        /// The pp src structure.
        /// </param>
        /// <param name="bEntries">
        /// The b entries.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        private void ReorderObjectList<T>(PwObjectList<T> vItems, PwObjectPool ppOrgStructure, PwObjectPool ppSrcStructure, bool bEntries)
            where T : class, IStructureItem, IDeepCloneable<T>
        {
            if (!this.ObjectListRequiresReorder<T>(vItems, ppOrgStructure, ppSrcStructure, bEntries))
            {
                return;
            }

#if DEBUG
            PwObjectList<T> vOrgListItems = vItems.CloneShallow();
#endif

            Queue<KeyValuePair<uint, uint>> qToDo = new Queue<KeyValuePair<uint, uint>>();
            qToDo.Enqueue(new KeyValuePair<uint, uint>(0, vItems.UCount - 1));

            while (qToDo.Count > 0)
            {
                if ((this.m_slStatus != null) && !this.m_slStatus.ContinueWork())
                {
                    break;
                }

                KeyValuePair<uint, uint> kvp = qToDo.Dequeue();
                if (kvp.Value <= kvp.Key)
                {
                    Debug.Assert(false);
                    continue;
                }

                Queue<PwUuid> qRelBefore = new Queue<PwUuid>();
                Queue<PwUuid> qRelAfter = new Queue<PwUuid>();
                uint uPivot = FindLocationChangedPivot<T>(vItems, kvp, ppOrgStructure, ppSrcStructure, qRelBefore, qRelAfter, bEntries);
                T ptPivot = vItems.GetAt(uPivot);

                List<T> vToSort = vItems.GetRange(kvp.Key, kvp.Value);
                Queue<T> qBefore = new Queue<T>();
                Queue<T> qAfter = new Queue<T>();
                bool bBefore = true;

                foreach (T pt in vToSort)
                {
                    if (pt == ptPivot)
                    {
                        bBefore = false;
                        continue;
                    }

                    bool bAdded = false;
                    foreach (PwUuid puBefore in qRelBefore)
                    {
                        if (puBefore.Equals(pt.Uuid))
                        {
                            qBefore.Enqueue(pt);
                            bAdded = true;
                            break;
                        }
                    }

                    if (bAdded)
                    {
                        continue;
                    }

                    foreach (PwUuid puAfter in qRelAfter)
                    {
                        if (puAfter.Equals(pt.Uuid))
                        {
                            qAfter.Enqueue(pt);
                            bAdded = true;
                            break;
                        }
                    }

                    if (bAdded)
                    {
                        continue;
                    }

                    if (bBefore)
                    {
                        qBefore.Enqueue(pt);
                    }
                    else
                    {
                        qAfter.Enqueue(pt);
                    }
                }

                Debug.Assert(bBefore == false);

                uint uPos = kvp.Key;
                while (qBefore.Count > 0)
                {
                    vItems.SetAt(uPos++, qBefore.Dequeue());
                }

                vItems.SetAt(uPos++, ptPivot);
                while (qAfter.Count > 0)
                {
                    vItems.SetAt(uPos++, qAfter.Dequeue());
                }

                Debug.Assert(uPos == (kvp.Value + 1));

                int iNewPivot = vItems.IndexOf(ptPivot);
                if ((iNewPivot < (int)kvp.Key) || (iNewPivot > (int)kvp.Value))
                {
                    Debug.Assert(false);
                    continue;
                }

                if ((iNewPivot - 1) > (int)kvp.Key)
                {
                    qToDo.Enqueue(new KeyValuePair<uint, uint>(kvp.Key, (uint)(iNewPivot - 1)));
                }

                if ((iNewPivot + 1) < (int)kvp.Value)
                {
                    qToDo.Enqueue(new KeyValuePair<uint, uint>((uint)(iNewPivot + 1), kvp.Value));
                }
            }

#if DEBUG
            foreach (T ptItem in vOrgListItems)
            {
                Debug.Assert(vItems.IndexOf(ptItem) >= 0);
            }

#endif
        }

        /// <summary>
        /// The validate uuid uniqueness.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ValidateUuidUniqueness()
        {
#if DEBUG
            List<PwUuid> l = new List<PwUuid>();
            bool bAllUnique = true;

            GroupHandler gh = delegate(PwGroup pg)
                {
                    foreach (PwUuid u in l)
                    {
                        bAllUnique &= !pg.Uuid.Equals(u);
                    }

                    l.Add(pg.Uuid);
                    return bAllUnique;
                };

            EntryHandler eh = delegate(PwEntry pe)
                {
                    foreach (PwUuid u in l)
                    {
                        bAllUnique &= !pe.Uuid.Equals(u);
                    }

                    l.Add(pe.Uuid);
                    return bAllUnique;
                };

            this.m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh);
            return bAllUnique;
#else
			return true;
#endif
        }

        #endregion

        /* public void CreateBackupFile(IStatusLogger sl)
		{
			if(sl != null) sl.SetText(KLRes.CreatingBackupFile, LogStatusType.Info);

			IOConnectionInfo iocBk = m_ioSource.CloneDeep();
			iocBk.Path += StrBackupExtension;

			bool bMadeUnhidden = UrlUtil.UnhideFile(iocBk.Path);

			bool bFastCopySuccess = false;
			if(m_ioSource.IsLocalFile() && (m_ioSource.UserName.Length == 0) &&
				(m_ioSource.Password.Length == 0))
			{
				try
				{
					string strFile = m_ioSource.Path + StrBackupExtension;
					File.Copy(m_ioSource.Path, strFile, true);
					bFastCopySuccess = true;
				}
				catch(Exception) { Debug.Assert(false); }
			}

			if(bFastCopySuccess == false)
			{
				using(Stream sIn = IOConnection.OpenRead(m_ioSource))
				{
					using(Stream sOut = IOConnection.OpenWrite(iocBk))
					{
						MemUtil.CopyStream(sIn, sOut);
						sOut.Close();
					}

					sIn.Close();
				}
			}

			if(bMadeUnhidden) UrlUtil.HideFile(iocBk.Path, true); // Hide again
		} */

        /* private static void RemoveData(PwGroup pg)
		{
			EntryHandler eh = delegate(PwEntry pe)
			{
				pe.AutoType.Clear();
				pe.Binaries.Clear();
				pe.History.Clear();
				pe.Strings.Clear();
				return true;
			};

			pg.TraverseTree(TraversalMethod.PreOrder, null, eh);
		} */
    }
}