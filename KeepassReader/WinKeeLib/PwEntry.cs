// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PwEntry.cs" company="">
//   
// </copyright>
// <summary>
//   A class representing a password entry. A password entry consists of several
//   fields like title, user name, password, etc. Each password entry has a
//   unique ID (UUID).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using KeePassLib.Collections;
    using KeePassLib.Interfaces;
    using KeePassLib.Security;

    using KeePassLib.Utility;

    /// <summary>
    /// A class representing a password entry. A password entry consists of several
    /// fields like title, user name, password, etc. Each password entry has a
    /// unique ID (UUID).
    /// </summary>
    public sealed class PwEntry : ITimeLogger, IStructureItem, IDeepCloneable<PwEntry>
    {
        #region Static Fields

        /// <summary>
        /// The entry touched.
        /// </summary>
        public static EventHandler<ObjectTouchedEventArgs> EntryTouched;

        #endregion

        #region Fields

        /// <summary>
        /// The touched.
        /// </summary>
        public EventHandler<ObjectTouchedEventArgs> Touched;

        /// <summary>
        /// The m_b expires.
        /// </summary>
        private bool m_bExpires = false;

        /// <summary>
        /// The m_clr background.
        /// </summary>
        private string m_clrBackground = string.Empty;

        /// <summary>
        /// The m_clr foreground.
        /// </summary>
        private string m_clrForeground = string.Empty;

        /// <summary>
        /// The m_list auto type.
        /// </summary>
        private AutoTypeConfig m_listAutoType = new AutoTypeConfig();

        /// <summary>
        /// The m_list binaries.
        /// </summary>
        private ProtectedBinaryDictionary m_listBinaries = new ProtectedBinaryDictionary();

        /// <summary>
        /// The m_list history.
        /// </summary>
        private PwObjectList<PwEntry> m_listHistory = new PwObjectList<PwEntry>();

        /// <summary>
        /// The m_list strings.
        /// </summary>
        private ProtectedStringDictionary m_listStrings = new ProtectedStringDictionary();

        /// <summary>
        /// The m_p parent group.
        /// </summary>
        private PwGroup m_pParentGroup = null;

        /// <summary>
        /// The m_pw custom icon id.
        /// </summary>
        private PwUuid m_pwCustomIconID = PwUuid.Zero;

        /// <summary>
        /// The m_pw icon.
        /// </summary>
        private PwIcon m_pwIcon = PwIcon.Key;

        /// <summary>
        /// The m_str override url.
        /// </summary>
        private string m_strOverrideUrl = string.Empty;

        /// <summary>
        /// The m_t creation.
        /// </summary>
        private DateTime m_tCreation = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_t expire.
        /// </summary>
        private DateTime m_tExpire = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_t last access.
        /// </summary>
        private DateTime m_tLastAccess = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_t last mod.
        /// </summary>
        private DateTime m_tLastMod = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_t parent group last mod.
        /// </summary>
        private DateTime m_tParentGroupLastMod = PwDefs.DtDefaultNow;

        /// <summary>
        /// The m_u usage count.
        /// </summary>
        private ulong m_uUsageCount = 0;

        /// <summary>
        /// The m_uuid.
        /// </summary>
        private PwUuid m_uuid = PwUuid.Zero;

        /// <summary>
        /// The m_v tags.
        /// </summary>
        private List<string> m_vTags = new List<string>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PwEntry"/> class. 
        /// Construct a new, empty password entry. Member variables will be initialized
        /// to their default values.
        /// </summary>
        /// <param name="bCreateNewUuid">
        /// If <c>true</c>, a new UUID will be created
        /// for this entry. If <c>false</c>, the UUID is zero and you must set it
        /// manually later.
        /// </param>
        /// <param name="bSetTimes">
        /// If <c>true</c>, the creation, last modification
        /// and last access times will be set to the current system time.
        /// </param>
        public PwEntry(bool bCreateNewUuid, bool bSetTimes)
        {
            if (bCreateNewUuid)
            {
                this.m_uuid = new PwUuid(true);
            }

            if (bSetTimes)
            {
                this.m_tCreation = this.m_tLastMod = this.m_tLastAccess = this.m_tParentGroupLastMod = DateTime.Now;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PwEntry"/> class. 
        /// Construct a new, empty password entry. Member variables will be initialized
        /// to their default values.
        /// </summary>
        /// <param name="pwParentGroup">
        /// Reference to the containing group, this
        /// parameter may be <c>null</c> and set later manually.
        /// </param>
        /// <param name="bCreateNewUuid">
        /// If <c>true</c>, a new UUID will be created
        /// for this entry. If <c>false</c>, the UUID is zero and you must set it
        /// manually later.
        /// </param>
        /// <param name="bSetTimes">
        /// If <c>true</c>, the creation, last modification
        /// and last access times will be set to the current system time.
        /// </param>
        [Obsolete("Use a different constructor. To add an entry to a group, use AddEntry of PwGroup.")]
        public PwEntry(PwGroup pwParentGroup, bool bCreateNewUuid, bool bSetTimes)
        {
            this.m_pParentGroup = pwParentGroup;

            if (bCreateNewUuid)
            {
                this.m_uuid = new PwUuid(true);
            }

            if (bSetTimes)
            {
                this.m_tCreation = this.m_tLastMod = this.m_tLastAccess = this.m_tParentGroupLastMod = DateTime.Now;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get or set all auto-type window/keystroke sequence associations.
        /// </summary>
        public AutoTypeConfig AutoType
        {
            get
            {
                return this.m_listAutoType;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_listAutoType = value;
            }
        }

        /// <summary>
        /// Get or set the background color of this entry.
        /// </summary>
        public string BackgroundColor
        {
            get
            {
                return this.m_clrBackground;
            }

            set
            {
                this.m_clrBackground = value;
            }
        }

        /// <summary>
        /// Get or set all entry binaries.
        /// </summary>
        public ProtectedBinaryDictionary Binaries
        {
            get
            {
                return this.m_listBinaries;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_listBinaries = value;
            }
        }

        /// <summary>
        /// The date/time when this entry was created.
        /// </summary>
        public DateTime CreationTime
        {
            get
            {
                return this.m_tCreation;
            }

            set
            {
                this.m_tCreation = value;
            }
        }

        /// <summary>
        /// Get the custom icon ID. This value is 0, if no custom icon is
        /// being used (i.e. the icon specified by the <c>IconID</c> property
        /// should be displayed).
        /// </summary>
        public PwUuid CustomIconUuid
        {
            get
            {
                return this.m_pwCustomIconID;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_pwCustomIconID = value;
            }
        }

        /// <summary>
        /// Specifies whether the entry expires or not.
        /// </summary>
        public bool Expires
        {
            get
            {
                return this.m_bExpires;
            }

            set
            {
                this.m_bExpires = value;
            }
        }

        /// <summary>
        /// The date/time when this entry expires. Use the <c>Expires</c> property
        /// to specify if the entry does actually expire or not.
        /// </summary>
        public DateTime ExpiryTime
        {
            get
            {
                return this.m_tExpire;
            }

            set
            {
                this.m_tExpire = value;
            }
        }

        /// <summary>
        /// Get or set the foreground color of this entry.
        /// </summary>
        public string ForegroundColor
        {
            get
            {
                return this.m_clrForeground;
            }

            set
            {
                this.m_clrForeground = value;
            }
        }

        /// <summary>
        /// Get all previous versions of this entry (backups).
        /// </summary>
        public PwObjectList<PwEntry> History
        {
            get
            {
                return this.m_listHistory;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_listHistory = value;
            }
        }

        /// <summary>
        /// Image ID specifying the icon that will be used for this entry.
        /// </summary>
        public PwIcon IconId
        {
            get
            {
                return this.m_pwIcon;
            }

            set
            {
                this.m_pwIcon = value;
            }
        }

        /// <summary>
        /// The date/time when this entry was last accessed (read).
        /// </summary>
        public DateTime LastAccessTime
        {
            get
            {
                return this.m_tLastAccess;
            }

            set
            {
                this.m_tLastAccess = value;
            }
        }

        /// <summary>
        /// The date/time when this entry was last modified.
        /// </summary>
        public DateTime LastModificationTime
        {
            get
            {
                return this.m_tLastMod;
            }

            set
            {
                this.m_tLastMod = value;
            }
        }

        /// <summary>
        /// The date/time when the location of the object was last changed.
        /// </summary>
        public DateTime LocationChanged
        {
            get
            {
                return this.m_tParentGroupLastMod;
            }

            set
            {
                this.m_tParentGroupLastMod = value;
            }
        }

        /// <summary>
        /// Entry-specific override URL. If this string is non-empty,
        /// </summary>
        public string OverrideUrl
        {
            get
            {
                return this.m_strOverrideUrl;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_strOverrideUrl = value;
            }
        }

        /// <summary>
        /// Reference to a group which contains the current entry.
        /// </summary>
        public PwGroup ParentGroup
        {
            get
            {
                return this.m_pParentGroup;
            }

            // Plugins: use <c>PwGroup.AddEntry</c> instead.
            internal set
            {
                this.m_pParentGroup = value;
            }
        }

        /// <summary>
        /// Get or set all entry strings.
        /// </summary>
        public ProtectedStringDictionary Strings
        {
            get
            {
                return this.m_listStrings;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_listStrings = value;
            }
        }

        /// <summary>
        /// List of tags associated with this entry.
        /// </summary>
        public List<string> Tags
        {
            get
            {
                return this.m_vTags;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_vTags = value;
            }
        }

        /// <summary>
        /// Get or set the usage count of the entry. To increase the usage
        /// count by one, use the <c>Touch</c> function.
        /// </summary>
        public ulong UsageCount
        {
            get
            {
                return this.m_uUsageCount;
            }

            set
            {
                this.m_uUsageCount = value;
            }
        }

        /// <summary>
        /// UUID of this entry.
        /// </summary>
        public PwUuid Uuid
        {
            get
            {
                return this.m_uuid;
            }

            set
            {
                Debug.Assert(value != null);
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
        /// The add tag.
        /// </summary>
        /// <param name="strTag">
        /// The str tag.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool AddTag(string strTag)
        {
            if (string.IsNullOrEmpty(strTag))
            {
                Debug.Assert(false);
                return false;
            }

            for (int i = 0; i < this.m_vTags.Count; ++i)
            {
                if (this.m_vTags[i].Equals(strTag, StrUtil.CaseIgnoreCmp))
                {
                    return false;
                }
            }

            this.m_vTags.Add(strTag);
            return true;
        }

        /// <summary>
        /// Assign properties to the current entry based on a template entry.
        /// </summary>
        /// <param name="peTemplate">
        /// Template entry. Must not be <c>null</c>.
        /// </param>
        /// <param name="bOnlyIfNewer">
        /// Only set the properties of the template entry
        /// if it is newer than the current one.
        /// </param>
        /// <param name="bIncludeHistory">
        /// If <c>true</c>, the history will be
        /// copied, too.
        /// </param>
        /// <param name="bAssignLocationChanged">
        /// If <c>true</c>, the
        /// <c>LocationChanged</c> property is copied, otherwise not.
        /// </param>
        public void AssignProperties(PwEntry peTemplate, bool bOnlyIfNewer, bool bIncludeHistory, bool bAssignLocationChanged)
        {
            Debug.Assert(peTemplate != null);
            if (peTemplate == null)
            {
                throw new ArgumentNullException("peTemplate");
            }

            if (bOnlyIfNewer && (TimeUtil.Compare(peTemplate.m_tLastMod, this.m_tLastMod, true) < 0))
            {
                return;
            }

            // Template UUID should be the same as the current one
            Debug.Assert(this.m_uuid.Equals(peTemplate.m_uuid));
            this.m_uuid = peTemplate.m_uuid;

            if (bAssignLocationChanged)
            {
                this.m_tParentGroupLastMod = peTemplate.m_tParentGroupLastMod;
            }

            this.m_listStrings = peTemplate.m_listStrings;
            this.m_listBinaries = peTemplate.m_listBinaries;
            this.m_listAutoType = peTemplate.m_listAutoType;
            if (bIncludeHistory)
            {
                this.m_listHistory = peTemplate.m_listHistory;
            }

            this.m_pwIcon = peTemplate.m_pwIcon;
            this.m_pwCustomIconID = peTemplate.m_pwCustomIconID; // Immutable

            this.m_clrForeground = peTemplate.m_clrForeground;
            this.m_clrBackground = peTemplate.m_clrBackground;

            this.m_tCreation = peTemplate.m_tCreation;
            this.m_tLastMod = peTemplate.m_tLastMod;
            this.m_tLastAccess = peTemplate.m_tLastAccess;
            this.m_tExpire = peTemplate.m_tExpire;
            this.m_bExpires = peTemplate.m_bExpires;
            this.m_uUsageCount = peTemplate.m_uUsageCount;

            this.m_strOverrideUrl = peTemplate.m_strOverrideUrl;

            this.m_vTags = new List<string>(peTemplate.m_vTags);
        }

        /// <summary>
        /// Clone the current entry. The returned entry is an exact value copy
        /// of the current entry (including UUID and parent group reference).
        /// All mutable members are cloned.
        /// </summary>
        /// <returns>Exact value clone. All references to mutable values changed.</returns>
        public PwEntry CloneDeep()
        {
            PwEntry peNew = new PwEntry(false, false);

            peNew.m_uuid = this.m_uuid; // PwUuid is immutable
            peNew.m_pParentGroup = this.m_pParentGroup;
            peNew.m_tParentGroupLastMod = this.m_tParentGroupLastMod;

            peNew.m_listStrings = this.m_listStrings.CloneDeep();
            peNew.m_listBinaries = this.m_listBinaries.CloneDeep();
            peNew.m_listAutoType = this.m_listAutoType.CloneDeep();
            peNew.m_listHistory = this.m_listHistory.CloneDeep();

            peNew.m_pwIcon = this.m_pwIcon;
            peNew.m_pwCustomIconID = this.m_pwCustomIconID;

            peNew.m_clrForeground = this.m_clrForeground;
            peNew.m_clrBackground = this.m_clrBackground;

            peNew.m_tCreation = this.m_tCreation;
            peNew.m_tLastMod = this.m_tLastMod;
            peNew.m_tLastAccess = this.m_tLastAccess;
            peNew.m_tExpire = this.m_tExpire;
            peNew.m_bExpires = this.m_bExpires;
            peNew.m_uUsageCount = this.m_uUsageCount;

            peNew.m_strOverrideUrl = this.m_strOverrideUrl;

            peNew.m_vTags = new List<string>(this.m_vTags);

            return peNew;
        }

        /// <summary>
        /// The clone structure.
        /// </summary>
        /// <returns>
        /// The <see cref="PwEntry"/>.
        /// </returns>
        public PwEntry CloneStructure()
        {
            PwEntry peNew = new PwEntry(false, false);

            peNew.m_uuid = this.m_uuid; // PwUuid is immutable
            peNew.m_tParentGroupLastMod = this.m_tParentGroupLastMod;

            // Do not assign m_pParentGroup
            return peNew;
        }

        /// <summary>
        /// Create a backup of this entry. The backup item doesn't contain any
        /// history items.
        /// </summary>
        [Obsolete]
        public void CreateBackup()
        {
            this.CreateBackup(null);
        }

        /// <summary>
        /// Create a backup of this entry. The backup item doesn't contain any
        /// history items.
        /// </summary>
        /// <param name="pwHistMntcSettings">
        /// If this parameter isn't <c>null</c>,
        /// the history list is maintained automatically (i.e. old backups are
        /// deleted if there are too many or the history size is too large).
        /// This parameter may be <c>null</c> (no maintenance then).
        /// </param>
        public void CreateBackup(PwDatabase pwHistMntcSettings)
        {
            PwEntry peCopy = this.CloneDeep();
            peCopy.History = new PwObjectList<PwEntry>(); // Remove history

            this.m_listHistory.Add(peCopy); // Must be added at end, see EqualsEntry

            if (pwHistMntcSettings != null)
            {
                this.MaintainBackups(pwHistMntcSettings);
            }
        }

        /// <summary>
        /// The equals entry.
        /// </summary>
        /// <param name="pe">
        /// The pe.
        /// </param>
        /// <param name="bIgnoreParentGroup">
        /// The b ignore parent group.
        /// </param>
        /// <param name="bIgnoreLastMod">
        /// The b ignore last mod.
        /// </param>
        /// <param name="bIgnoreLastAccess">
        /// The b ignore last access.
        /// </param>
        /// <param name="bIgnoreHistory">
        /// The b ignore history.
        /// </param>
        /// <param name="bIgnoreThisLastBackup">
        /// The b ignore this last backup.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [Obsolete]
        public bool EqualsEntry(
            PwEntry pe, 
            bool bIgnoreParentGroup, 
            bool bIgnoreLastMod, 
            bool bIgnoreLastAccess, 
            bool bIgnoreHistory, 
            bool bIgnoreThisLastBackup)
        {
            return this.EqualsEntry(
                pe, 
                BuildCmpOpt(bIgnoreParentGroup, bIgnoreLastMod, bIgnoreLastAccess, bIgnoreHistory, bIgnoreThisLastBackup), 
                MemProtCmpMode.None);
        }

        /// <summary>
        /// The equals entry.
        /// </summary>
        /// <param name="pe">
        /// The pe.
        /// </param>
        /// <param name="bIgnoreParentGroup">
        /// The b ignore parent group.
        /// </param>
        /// <param name="bIgnoreLastMod">
        /// The b ignore last mod.
        /// </param>
        /// <param name="bIgnoreLastAccess">
        /// The b ignore last access.
        /// </param>
        /// <param name="bIgnoreHistory">
        /// The b ignore history.
        /// </param>
        /// <param name="bIgnoreThisLastBackup">
        /// The b ignore this last backup.
        /// </param>
        /// <param name="mpCmpStr">
        /// The mp cmp str.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [Obsolete]
        public bool EqualsEntry(
            PwEntry pe, 
            bool bIgnoreParentGroup, 
            bool bIgnoreLastMod, 
            bool bIgnoreLastAccess, 
            bool bIgnoreHistory, 
            bool bIgnoreThisLastBackup, 
            MemProtCmpMode mpCmpStr)
        {
            return this.EqualsEntry(
                pe, 
                BuildCmpOpt(bIgnoreParentGroup, bIgnoreLastMod, bIgnoreLastAccess, bIgnoreHistory, bIgnoreThisLastBackup), 
                mpCmpStr);
        }

        /// <summary>
        /// The equals entry.
        /// </summary>
        /// <param name="pe">
        /// The pe.
        /// </param>
        /// <param name="pwOpt">
        /// The pw opt.
        /// </param>
        /// <param name="mpCmpStr">
        /// The mp cmp str.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool EqualsEntry(PwEntry pe, PwCompareOptions pwOpt, MemProtCmpMode mpCmpStr)
        {
            if (pe == null)
            {
                Debug.Assert(false);
                return false;
            }

            bool bNeEqStd = (pwOpt & PwCompareOptions.NullEmptyEquivStd) != PwCompareOptions.None;
            bool bIgnoreLastAccess = (pwOpt & PwCompareOptions.IgnoreLastAccess) != PwCompareOptions.None;
            bool bIgnoreLastMod = (pwOpt & PwCompareOptions.IgnoreLastMod) != PwCompareOptions.None;

            if (!this.m_uuid.Equals(pe.m_uuid))
            {
                return false;
            }

            if ((pwOpt & PwCompareOptions.IgnoreParentGroup) == PwCompareOptions.None)
            {
                if (this.m_pParentGroup != pe.m_pParentGroup)
                {
                    return false;
                }

                if (!bIgnoreLastMod && (this.m_tParentGroupLastMod != pe.m_tParentGroupLastMod))
                {
                    return false;
                }
            }

            if (!this.m_listStrings.EqualsDictionary(pe.m_listStrings, pwOpt, mpCmpStr))
            {
                return false;
            }

            if (!this.m_listBinaries.EqualsDictionary(pe.m_listBinaries))
            {
                return false;
            }

            if (!this.m_listAutoType.Equals(pe.m_listAutoType))
            {
                return false;
            }

            if ((pwOpt & PwCompareOptions.IgnoreHistory) == PwCompareOptions.None)
            {
                bool bIgnoreLastBackup = (pwOpt & PwCompareOptions.IgnoreLastBackup) != PwCompareOptions.None;

                if (!bIgnoreLastBackup && (this.m_listHistory.UCount != pe.m_listHistory.UCount))
                {
                    return false;
                }

                if (bIgnoreLastBackup && (this.m_listHistory.UCount == 0))
                {
                    Debug.Assert(false);
                    return false;
                }

                if (bIgnoreLastBackup && ((this.m_listHistory.UCount - 1) != pe.m_listHistory.UCount))
                {
                    return false;
                }

                PwCompareOptions cmpSub = PwCompareOptions.IgnoreParentGroup;
                if (bNeEqStd)
                {
                    cmpSub |= PwCompareOptions.NullEmptyEquivStd;
                }

                if (bIgnoreLastMod)
                {
                    cmpSub |= PwCompareOptions.IgnoreLastMod;
                }

                if (bIgnoreLastAccess)
                {
                    cmpSub |= PwCompareOptions.IgnoreLastAccess;
                }

                for (uint uHist = 0; uHist < pe.m_listHistory.UCount; ++uHist)
                {
                    if (!this.m_listHistory.GetAt(uHist).EqualsEntry(pe.m_listHistory.GetAt(uHist), cmpSub, MemProtCmpMode.None))
                    {
                        return false;
                    }
                }
            }

            if (this.m_pwIcon != pe.m_pwIcon)
            {
                return false;
            }

            if (!this.m_pwCustomIconID.Equals(pe.m_pwCustomIconID))
            {
                return false;
            }

            if (this.m_clrForeground != pe.m_clrForeground)
            {
                return false;
            }

            if (this.m_clrBackground != pe.m_clrBackground)
            {
                return false;
            }

            if (this.m_tCreation != pe.m_tCreation)
            {
                return false;
            }

            if (!bIgnoreLastMod && (this.m_tLastMod != pe.m_tLastMod))
            {
                return false;
            }

            if (!bIgnoreLastAccess && (this.m_tLastAccess != pe.m_tLastAccess))
            {
                return false;
            }

            if (this.m_tExpire != pe.m_tExpire)
            {
                return false;
            }

            if (this.m_bExpires != pe.m_bExpires)
            {
                return false;
            }

            if (!bIgnoreLastAccess && (this.m_uUsageCount != pe.m_uUsageCount))
            {
                return false;
            }

            if (this.m_strOverrideUrl != pe.m_strOverrideUrl)
            {
                return false;
            }

            if (this.m_vTags.Count != pe.m_vTags.Count)
            {
                return false;
            }

            for (int iTag = 0; iTag < this.m_vTags.Count; ++iTag)
            {
                if (this.m_vTags[iTag] != pe.m_vTags[iTag])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The get auto type enabled.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool GetAutoTypeEnabled()
        {
            if (!this.m_listAutoType.Enabled)
            {
                return false;
            }

            if (this.m_pParentGroup != null)
            {
                return this.m_pParentGroup.GetAutoTypeEnabledInherited();
            }

            return PwGroup.DefaultAutoTypeEnabled;
        }

        /// <summary>
        /// The get auto type sequence.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetAutoTypeSequence()
        {
            string strSeq = this.m_listAutoType.DefaultSequence;

            PwGroup pg = this.m_pParentGroup;
            while (pg != null)
            {
                if (strSeq.Length != 0)
                {
                    break;
                }

                strSeq = pg.DefaultAutoTypeSequence;
                pg = pg.ParentGroup;
            }

            if (strSeq.Length != 0)
            {
                return strSeq;
            }

            if (PwDefs.IsTanEntry(this))
            {
                return PwDefs.DefaultAutoTypeSequenceTan;
            }

            return PwDefs.DefaultAutoTypeSequence;
        }

        /// <summary>
        /// The get searching enabled.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool GetSearchingEnabled()
        {
            if (this.m_pParentGroup != null)
            {
                return this.m_pParentGroup.GetSearchingEnabledInherited();
            }

            return PwGroup.DefaultSearchingEnabled;
        }

        /// <summary>
        /// Approximate the total size of this entry in bytes (including
        /// strings, binaries and history entries).
        /// </summary>
        /// <returns>Size in bytes.</returns>
        public ulong GetSize()
        {
            ulong uSize = 128; // Approx fixed length data

            foreach (KeyValuePair<string, ProtectedString> kvpStr in this.m_listStrings)
            {
                uSize += (ulong)kvpStr.Key.Length;
                uSize += (ulong)kvpStr.Value.Length;
            }

            foreach (KeyValuePair<string, ProtectedBinary> kvpBin in this.m_listBinaries)
            {
                uSize += (ulong)kvpBin.Key.Length;
                uSize += kvpBin.Value.Length;
            }

            uSize += (ulong)this.m_listAutoType.DefaultSequence.Length;
            foreach (AutoTypeAssociation a in this.m_listAutoType.Associations)
            {
                uSize += (ulong)a.WindowName.Length;
                uSize += (ulong)a.Sequence.Length;
            }

            foreach (PwEntry peHistory in this.m_listHistory)
            {
                uSize += peHistory.GetSize();
            }

            uSize += (ulong)this.m_strOverrideUrl.Length;

            foreach (string strTag in this.m_vTags)
            {
                uSize += (ulong)strTag.Length;
            }

            return uSize;
        }

        /// <summary>
        /// The has backup of data.
        /// </summary>
        /// <param name="peData">
        /// The pe data.
        /// </param>
        /// <param name="bIgnoreLastMod">
        /// The b ignore last mod.
        /// </param>
        /// <param name="bIgnoreLastAccess">
        /// The b ignore last access.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool HasBackupOfData(PwEntry peData, bool bIgnoreLastMod, bool bIgnoreLastAccess)
        {
            if (peData == null)
            {
                Debug.Assert(false);
                return false;
            }

            PwCompareOptions cmpOpt = PwCompareOptions.IgnoreParentGroup | PwCompareOptions.IgnoreHistory | PwCompareOptions.NullEmptyEquivStd;
            if (bIgnoreLastMod)
            {
                cmpOpt |= PwCompareOptions.IgnoreLastMod;
            }

            if (bIgnoreLastAccess)
            {
                cmpOpt |= PwCompareOptions.IgnoreLastAccess;
            }

            foreach (PwEntry pe in this.m_listHistory)
            {
                if (pe.EqualsEntry(peData, cmpOpt, MemProtCmpMode.None))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The has tag.
        /// </summary>
        /// <param name="strTag">
        /// The str tag.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool HasTag(string strTag)
        {
            if (string.IsNullOrEmpty(strTag))
            {
                Debug.Assert(false);
                return false;
            }

            for (int i = 0; i < this.m_vTags.Count; ++i)
            {
                if (this.m_vTags[i].Equals(strTag, StrUtil.CaseIgnoreCmp))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The is contained in.
        /// </summary>
        /// <param name="pgContainer">
        /// The pg container.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsContainedIn(PwGroup pgContainer)
        {
            PwGroup pgCur = this.m_pParentGroup;
            while (pgCur != null)
            {
                if (pgCur == pgContainer)
                {
                    return true;
                }

                pgCur = pgCur.ParentGroup;
            }

            return false;
        }

        /// <summary>
        /// Delete old history items if there are too many or the history
        /// size is too large.
        /// <returns>
        /// If one or more history items have been deleted, <c>true</c>
        /// is returned. Otherwise <c>false</c>.
        /// </returns>
        /// </summary>
        /// <param name="pwSettings">
        /// The pw Settings.
        /// </param>
        public bool MaintainBackups(PwDatabase pwSettings)
        {
            if (pwSettings == null)
            {
                Debug.Assert(false);
                return false;
            }

            bool bDeleted = false;

            int nMaxItems = pwSettings.HistoryMaxItems;
            if (nMaxItems >= 0)
            {
                while (this.m_listHistory.UCount > (uint)nMaxItems)
                {
                    this.RemoveOldestBackup();
                    bDeleted = true;
                }
            }

            long lMaxSize = pwSettings.HistoryMaxSize;
            if (lMaxSize >= 0)
            {
                while (true)
                {
                    ulong uHistSize = 0;
                    foreach (PwEntry pe in this.m_listHistory)
                    {
                        uHistSize += pe.GetSize();
                    }

                    if (uHistSize > (ulong)lMaxSize)
                    {
                        this.RemoveOldestBackup();
                        bDeleted = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return bDeleted;
        }

        /// <summary>
        /// The remove tag.
        /// </summary>
        /// <param name="strTag">
        /// The str tag.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool RemoveTag(string strTag)
        {
            if (string.IsNullOrEmpty(strTag))
            {
                Debug.Assert(false);
                return false;
            }

            for (int i = 0; i < this.m_vTags.Count; ++i)
            {
                if (this.m_vTags[i].Equals(strTag, StrUtil.CaseIgnoreCmp))
                {
                    this.m_vTags.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Restore an entry snapshot from backups.
        /// </summary>
        /// <param name="uBackupIndex">
        /// Index of the backup item, to which
        /// should be reverted.
        /// </param>
        [Obsolete]
        public void RestoreFromBackup(uint uBackupIndex)
        {
            this.RestoreFromBackup(uBackupIndex, null);
        }

        /// <summary>
        /// Restore an entry snapshot from backups.
        /// </summary>
        /// <param name="uBackupIndex">
        /// Index of the backup item, to which
        /// should be reverted.
        /// </param>
        /// <param name="pwHistMntcSettings">
        /// If this parameter isn't <c>null</c>,
        /// the history list is maintained automatically (i.e. old backups are
        /// deleted if there are too many or the history size is too large).
        /// This parameter may be <c>null</c> (no maintenance then).
        /// </param>
        public void RestoreFromBackup(uint uBackupIndex, PwDatabase pwHistMntcSettings)
        {
            Debug.Assert(uBackupIndex < this.m_listHistory.UCount);
            if (uBackupIndex >= this.m_listHistory.UCount)
            {
                throw new ArgumentOutOfRangeException("uBackupIndex");
            }

            PwEntry pe = this.m_listHistory.GetAt(uBackupIndex);
            Debug.Assert(pe != null);
            if (pe == null)
            {
                throw new InvalidOperationException();
            }

            this.CreateBackup(pwHistMntcSettings); // Backup current data before restoring
            this.AssignProperties(pe, false, false, false);
        }

        /// <summary>
        /// The set uuid.
        /// </summary>
        /// <param name="pwNewUuid">
        /// The pw new uuid.
        /// </param>
        /// <param name="bAlsoChangeHistoryUuids">
        /// The b also change history uuids.
        /// </param>
        public void SetUuid(PwUuid pwNewUuid, bool bAlsoChangeHistoryUuids)
        {
            this.Uuid = pwNewUuid;

            if (bAlsoChangeHistoryUuids)
            {
                foreach (PwEntry peHist in this.m_listHistory)
                {
                    peHist.Uuid = pwNewUuid;
                }
            }
        }

        /// <summary>
        /// Touch the entry. This function updates the internal last access
        /// time. If the <paramref name="bModified"/> parameter is <c>true</c>,
        /// the last modification time gets updated, too.
        /// </summary>
        /// <param name="bModified">
        /// Modify last modification time.
        /// </param>
        public void Touch(bool bModified)
        {
            this.Touch(bModified, true);
        }

        /// <summary>
        /// Touch the entry. This function updates the internal last access
        /// time. If the <paramref name="bModified"/> parameter is <c>true</c>,
        /// the last modification time gets updated, too.
        /// </summary>
        /// <param name="bModified">
        /// Modify last modification time.
        /// </param>
        /// <param name="bTouchParents">
        /// If <c>true</c>, all parent objects
        /// get touched, too.
        /// </param>
        public void Touch(bool bModified, bool bTouchParents)
        {
            this.m_tLastAccess = DateTime.Now;
            ++this.m_uUsageCount;

            if (bModified)
            {
                this.m_tLastMod = this.m_tLastAccess;
            }

            if (this.Touched != null)
            {
                this.Touched(this, new ObjectTouchedEventArgs(this, bModified, bTouchParents));
            }

            if (PwEntry.EntryTouched != null)
            {
                PwEntry.EntryTouched(this, new ObjectTouchedEventArgs(this, bModified, bTouchParents));
            }

            if (bTouchParents && (this.m_pParentGroup != null))
            {
                this.m_pParentGroup.Touch(bModified, true);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The build cmp opt.
        /// </summary>
        /// <param name="bIgnoreParentGroup">
        /// The b ignore parent group.
        /// </param>
        /// <param name="bIgnoreLastMod">
        /// The b ignore last mod.
        /// </param>
        /// <param name="bIgnoreLastAccess">
        /// The b ignore last access.
        /// </param>
        /// <param name="bIgnoreHistory">
        /// The b ignore history.
        /// </param>
        /// <param name="bIgnoreThisLastBackup">
        /// The b ignore this last backup.
        /// </param>
        /// <returns>
        /// The <see cref="PwCompareOptions"/>.
        /// </returns>
        private static PwCompareOptions BuildCmpOpt(
            bool bIgnoreParentGroup, 
            bool bIgnoreLastMod, 
            bool bIgnoreLastAccess, 
            bool bIgnoreHistory, 
            bool bIgnoreThisLastBackup)
        {
            PwCompareOptions pwOpt = PwCompareOptions.None;
            if (bIgnoreParentGroup)
            {
                pwOpt |= PwCompareOptions.IgnoreParentGroup;
            }

            if (bIgnoreLastMod)
            {
                pwOpt |= PwCompareOptions.IgnoreLastMod;
            }

            if (bIgnoreLastAccess)
            {
                pwOpt |= PwCompareOptions.IgnoreLastAccess;
            }

            if (bIgnoreHistory)
            {
                pwOpt |= PwCompareOptions.IgnoreHistory;
            }

            if (bIgnoreThisLastBackup)
            {
                pwOpt |= PwCompareOptions.IgnoreLastBackup;
            }

            return pwOpt;
        }

        /// <summary>
        /// The remove oldest backup.
        /// </summary>
        private void RemoveOldestBackup()
        {
            DateTime dtMin = DateTime.MaxValue;
            uint idxRemove = uint.MaxValue;

            for (uint u = 0; u < this.m_listHistory.UCount; ++u)
            {
                PwEntry pe = this.m_listHistory.GetAt(u);
                if (TimeUtil.Compare(pe.LastModificationTime, dtMin, true) < 0)
                {
                    idxRemove = u;
                    dtMin = pe.LastModificationTime;
                }
            }

            if (idxRemove != uint.MaxValue)
            {
                this.m_listHistory.RemoveAt(idxRemove);
            }
        }

        #endregion
    }

    /// <summary>
    /// The pw entry comparer.
    /// </summary>
    public sealed class PwEntryComparer : IComparer<PwEntry>
    {
        #region Fields

        /// <summary>
        /// The m_b case insensitive.
        /// </summary>
        private bool m_bCaseInsensitive;

        /// <summary>
        /// The m_b compare naturally.
        /// </summary>
        private bool m_bCompareNaturally;

        /// <summary>
        /// The m_str field name.
        /// </summary>
        private string m_strFieldName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PwEntryComparer"/> class.
        /// </summary>
        /// <param name="strFieldName">
        /// The str field name.
        /// </param>
        /// <param name="bCaseInsensitive">
        /// The b case insensitive.
        /// </param>
        /// <param name="bCompareNaturally">
        /// The b compare naturally.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public PwEntryComparer(string strFieldName, bool bCaseInsensitive, bool bCompareNaturally)
        {
            if (strFieldName == null)
            {
                throw new ArgumentNullException("strFieldName");
            }

            this.m_strFieldName = strFieldName;
            this.m_bCaseInsensitive = bCaseInsensitive;
            this.m_bCompareNaturally = bCompareNaturally;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The compare.
        /// </summary>
        /// <param name="a">
        /// The a.
        /// </param>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Compare(PwEntry a, PwEntry b)
        {
            string strA = a.Strings.ReadSafe(this.m_strFieldName);
            string strB = b.Strings.ReadSafe(this.m_strFieldName);

            if (this.m_bCompareNaturally)
            {
                return StrUtil.CompareNaturally(strA, strB);
            }

            return string.Compare(strA, strB, this.m_bCaseInsensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
        }

        #endregion
    }
}