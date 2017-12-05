// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PwDefs.cs" company="">
//   
// </copyright>
// <summary>
//   Contains KeePassLib-global definitions and enums.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    using KeePassLib.Delegates;
    using KeePassLib.Interfaces;
    using KeePassLib.Serialization;

    /// <summary>
    /// Contains KeePassLib-global definitions and enums.
    /// </summary>
    public static class PwDefs
    {
        #region Constants

        /// <summary>
        /// Prefix of a custom auto-type string field.
        /// </summary>
        public const string AutoTypeStringPrefix = "S:";

        /// <summary>
        /// The copyright.
        /// </summary>
        public const string Copyright = @"Copyright © 2003-2014 Dominik Reichl";

        /// <summary>
        /// Default auto-type keystroke sequence. If no custom sequence is
        /// specified, this sequence is used.
        /// </summary>
        public const string DefaultAutoTypeSequence = @"{USERNAME}{TAB}{PASSWORD}{ENTER}";

        /// <summary>
        /// Default auto-type keystroke sequence for TAN entries. If no custom
        /// sequence is specified, this sequence is used.
        /// </summary>
        public const string DefaultAutoTypeSequenceTan = @"{PASSWORD}";

        /// <summary>
        /// Default number of master key encryption/transformation rounds (making dictionary attacks harder).
        /// </summary>
        public const ulong DefaultKeyEncryptionRounds = 6000;

        /// <summary>
        /// Product donations URL.
        /// </summary>
        public const string DonationsUrl = "http://keepass.info/donate.html";

        /// <summary>
        /// Version, encoded as 64-bit unsigned integer
        /// (component-wise, 16 bits per component).
        /// </summary>
        public const ulong FileVersion64 = 0x0002001A00000000UL;

        /// <summary>
        /// URL to the root path of the online KeePass help. Terminated by
        /// a forward slash.
        /// </summary>
        public const string HelpUrl = "http://keepass.info/help/";

        /// <summary>
        /// Default string representing a hidden password.
        /// </summary>
        public const string HiddenPassword = "********";

        /// <summary>
        /// Product website URL. Terminated by a forward slash.
        /// </summary>
        public const string HomepageUrl = "http://keepass.info/";

        /// <summary>
        /// Default identifier string for the notes field. Should not contain
        /// spaces, tabs or other whitespace.
        /// </summary>
        public const string NotesField = "Notes";

        /// <summary>
        /// Default identifier string for the password field. Should not contain
        /// spaces, tabs or other whitespace.
        /// </summary>
        public const string PasswordField = "Password";

        /// <summary>
        /// URL to the online plugins page.
        /// </summary>
        public const string PluginsUrl = "http://keepass.info/plugins.html";

        /// <summary>
        /// The product name.
        /// </summary>
        public const string ProductName = "KeePass Password Safe";

        /// <summary>
        /// A short, simple string representing the product name. The string
        /// should contain no spaces, directory separator characters, etc.
        /// </summary>
        public const string ShortProductName = "KeePass";

        /// <summary>
        /// Default identifier string for the field which will contain TAN indices.
        /// </summary>
        public const string TanIndexField = UserNameField;

        /// <summary>
        /// Default title of an entry that is really a TAN entry.
        /// </summary>
        public const string TanTitle = @"<TAN>";

        /// <summary>
        /// Default identifier string for the title field. Should not contain
        /// spaces, tabs or other whitespace.
        /// </summary>
        public const string TitleField = "Title";

        /// <summary>
        /// URL to the online translations page.
        /// </summary>
        public const string TranslationsUrl = "http://keepass.info/translations.html";

        /// <summary>
        /// Default identifier string for the URL field. Should not contain
        /// spaces, tabs or other whitespace.
        /// </summary>
        public const string UrlField = "URL";

        /// <summary>
        /// Default identifier string for the user name field. Should not contain
        /// spaces, tabs or other whitespace.
        /// </summary>
        public const string UserNameField = "UserName";

        /// <summary>
        /// Version, encoded as 32-bit unsigned integer.
        /// 2.00 = 0x02000000, 2.01 = 0x02000100, ..., 2.18 = 0x02010800.
        /// As of 2.19, the version is encoded component-wise per byte,
        /// e.g. 2.19 = 0x02130000.
        /// It is highly recommended to use <c>FileVersion64</c> instead.
        /// </summary>
        public const uint Version32 = 0x021A0000;

        /// <summary>
        /// Version, encoded as string.
        /// </summary>
        public const string VersionString = "2.26";

        /// <summary>
        /// URL to a TXT file (eventually compressed) that contains information
        /// about the latest KeePass version available on the website.
        /// </summary>
        public const string VersionUrl = "http://keepass.info/update/version2x.txt.gz";

        /// <summary>
        /// The res class.
        /// </summary>
        internal const string ResClass = "KeePass2"; // With initial capital

        /// <summary>
        /// The unix name.
        /// </summary>
        internal const string UnixName = "keepass2";

        #endregion

        #region Static Fields

        /// <summary>
        /// A <c>DateTime</c> object that represents the time when the assembly
        /// was loaded.
        /// </summary>
        public static readonly DateTime DtDefaultNow = DateTime.Now;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get standard fields.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetStandardFields()
        {
            List<string> l = new List<string>();

            l.Add(TitleField);
            l.Add(UserNameField);
            l.Add(PasswordField);
            l.Add(UrlField);
            l.Add(NotesField);

            return l;
        }

        /// <summary>
        /// Check if a name is a standard field name.
        /// </summary>
        /// <param name="strFieldName">
        /// Input field name.
        /// </param>
        /// <returns>
        /// Returns <c>true</c>, if the field name is a standard
        /// field name (title, user name, password, ...), otherwise <c>false</c>.
        /// </returns>
        public static bool IsStandardField(string strFieldName)
        {
            Debug.Assert(strFieldName != null);
            if (strFieldName == null)
            {
                return false;
            }

            if (strFieldName.Equals(TitleField))
            {
                return true;
            }

            if (strFieldName.Equals(UserNameField))
            {
                return true;
            }

            if (strFieldName.Equals(PasswordField))
            {
                return true;
            }

            if (strFieldName.Equals(UrlField))
            {
                return true;
            }

            if (strFieldName.Equals(NotesField))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if an entry is a TAN.
        /// </summary>
        /// <param name="pe">
        /// Password entry.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the entry is a TAN.
        /// </returns>
        public static bool IsTanEntry(PwEntry pe)
        {
            Debug.Assert(pe != null);
            if (pe == null)
            {
                return false;
            }

            return pe.Strings.ReadSafe(PwDefs.TitleField) == TanTitle;
        }

        #endregion
    }

#pragma warning disable 1591 // Missing XML comments warning

    /// <summary>
    /// Search parameters for group and entry searches.
    /// </summary>
    public sealed class SearchParameters
    {
        /// <summary>
        /// The m_str text.
        /// </summary>
        private string m_strText = string.Empty;

        /// <summary>
        /// Gets or sets the search string.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        [DefaultValue("")]
        public string SearchString
        {
            get
            {
                return this.m_strText;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_strText = value;
            }
        }

        /// <summary>
        /// The m_b regex.
        /// </summary>
        private bool m_bRegex = false;

        /// <summary>
        /// Gets or sets a value indicating whether regular expression.
        /// </summary>
        [DefaultValue(false)]
        public bool RegularExpression
        {
            get
            {
                return this.m_bRegex;
            }

            set
            {
                this.m_bRegex = value;
            }
        }

        /// <summary>
        /// The m_b search in titles.
        /// </summary>
        private bool m_bSearchInTitles = true;

        /// <summary>
        /// Gets or sets a value indicating whether search in titles.
        /// </summary>
        [DefaultValue(true)]
        public bool SearchInTitles
        {
            get
            {
                return this.m_bSearchInTitles;
            }

            set
            {
                this.m_bSearchInTitles = value;
            }
        }

        /// <summary>
        /// The m_b search in user names.
        /// </summary>
        private bool m_bSearchInUserNames = true;

        /// <summary>
        /// Gets or sets a value indicating whether search in user names.
        /// </summary>
        [DefaultValue(true)]
        public bool SearchInUserNames
        {
            get
            {
                return this.m_bSearchInUserNames;
            }

            set
            {
                this.m_bSearchInUserNames = value;
            }
        }

        /// <summary>
        /// The m_b search in passwords.
        /// </summary>
        private bool m_bSearchInPasswords = false;

        /// <summary>
        /// Gets or sets a value indicating whether search in passwords.
        /// </summary>
        [DefaultValue(false)]
        public bool SearchInPasswords
        {
            get
            {
                return this.m_bSearchInPasswords;
            }

            set
            {
                this.m_bSearchInPasswords = value;
            }
        }

        /// <summary>
        /// The m_b search in urls.
        /// </summary>
        private bool m_bSearchInUrls = true;

        /// <summary>
        /// Gets or sets a value indicating whether search in urls.
        /// </summary>
        [DefaultValue(true)]
        public bool SearchInUrls
        {
            get
            {
                return this.m_bSearchInUrls;
            }

            set
            {
                this.m_bSearchInUrls = value;
            }
        }

        /// <summary>
        /// The m_b search in notes.
        /// </summary>
        private bool m_bSearchInNotes = true;

        /// <summary>
        /// Gets or sets a value indicating whether search in notes.
        /// </summary>
        [DefaultValue(true)]
        public bool SearchInNotes
        {
            get
            {
                return this.m_bSearchInNotes;
            }

            set
            {
                this.m_bSearchInNotes = value;
            }
        }

        /// <summary>
        /// The m_b search in other.
        /// </summary>
        private bool m_bSearchInOther = true;

        /// <summary>
        /// Gets or sets a value indicating whether search in other.
        /// </summary>
        [DefaultValue(true)]
        public bool SearchInOther
        {
            get
            {
                return this.m_bSearchInOther;
            }

            set
            {
                this.m_bSearchInOther = value;
            }
        }

        /// <summary>
        /// The m_b search in uuids.
        /// </summary>
        private bool m_bSearchInUuids = false;

        /// <summary>
        /// Gets or sets a value indicating whether search in uuids.
        /// </summary>
        [DefaultValue(false)]
        public bool SearchInUuids
        {
            get
            {
                return this.m_bSearchInUuids;
            }

            set
            {
                this.m_bSearchInUuids = value;
            }
        }

        /// <summary>
        /// The m_b search in group names.
        /// </summary>
        private bool m_bSearchInGroupNames = false;

        /// <summary>
        /// Gets or sets a value indicating whether search in group names.
        /// </summary>
        [DefaultValue(false)]
        public bool SearchInGroupNames
        {
            get
            {
                return this.m_bSearchInGroupNames;
            }

            set
            {
                this.m_bSearchInGroupNames = value;
            }
        }

        /// <summary>
        /// The m_b search in tags.
        /// </summary>
        private bool m_bSearchInTags = true;

        /// <summary>
        /// Gets or sets a value indicating whether search in tags.
        /// </summary>
        [DefaultValue(true)]
        public bool SearchInTags
        {
            get
            {
                return this.m_bSearchInTags;
            }

            set
            {
                this.m_bSearchInTags = value;
            }
        }

#if KeePassRT

        /// <summary>
        /// The m_sc type.
        /// </summary>
        private StringComparison m_scType = StringComparison.OrdinalIgnoreCase;
#else
		private StringComparison m_scType = StringComparison.InvariantCultureIgnoreCase;
#endif

        /// <summary>
        /// String comparison type. Specifies the condition when the specified
        /// text matches a group/entry string.
        /// </summary>
        public StringComparison ComparisonMode
        {
            get
            {
                return this.m_scType;
            }

            set
            {
                this.m_scType = value;
            }
        }

        /// <summary>
        /// The m_b exclude expired.
        /// </summary>
        private bool m_bExcludeExpired = false;

        /// <summary>
        /// Gets or sets a value indicating whether exclude expired.
        /// </summary>
        [DefaultValue(false)]
        public bool ExcludeExpired
        {
            get
            {
                return this.m_bExcludeExpired;
            }

            set
            {
                this.m_bExcludeExpired = value;
            }
        }

        /// <summary>
        /// The m_b respect entry searching disabled.
        /// </summary>
        private bool m_bRespectEntrySearchingDisabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether respect entry searching disabled.
        /// </summary>
        [DefaultValue(true)]
        public bool RespectEntrySearchingDisabled
        {
            get
            {
                return this.m_bRespectEntrySearchingDisabled;
            }

            set
            {
                this.m_bRespectEntrySearchingDisabled = value;
            }
        }

        /// <summary>
        /// The m_fn data trf.
        /// </summary>
        private StrPwEntryDelegate m_fnDataTrf = null;

        /// <summary>
        /// Gets or sets the data transformation fn.
        /// </summary>
        [XmlIgnore]
        public StrPwEntryDelegate DataTransformationFn
        {
            get
            {
                return this.m_fnDataTrf;
            }

            set
            {
                this.m_fnDataTrf = value;
            }
        }

        /// <summary>
        /// The m_str data trf.
        /// </summary>
        private string m_strDataTrf = string.Empty;

        /// <summary>
        /// Only for serialization.
        /// </summary>
        [DefaultValue("")]
        public string DataTransformation
        {
            get
            {
                return this.m_strDataTrf;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_strDataTrf = value;
            }
        }

        /// <summary>
        /// Gets the none.
        /// </summary>
        [XmlIgnore]
        public static SearchParameters None
        {
            get
            {
                SearchParameters sp = new SearchParameters();

                // sp.m_strText = string.Empty;
                // sp.m_bRegex = false;
                sp.m_bSearchInTitles = false;
                sp.m_bSearchInUserNames = false;

                // sp.m_bSearchInPasswords = false;
                sp.m_bSearchInUrls = false;
                sp.m_bSearchInNotes = false;
                sp.m_bSearchInOther = false;

                // sp.m_bSearchInUuids = false;
                // sp.SearchInGroupNames = false;
                sp.m_bSearchInTags = false;

                // sp.m_scType = StringComparison.InvariantCultureIgnoreCase;
                // sp.m_bExcludeExpired = false;
                // m_bRespectEntrySearchingDisabled = true;
                return sp;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchParameters"/> class. 
        /// Construct a new search parameters object.
        /// </summary>
        public SearchParameters()
        {
        }

        /// <summary>
        /// The clone.
        /// </summary>
        /// <returns>
        /// The <see cref="SearchParameters"/>.
        /// </returns>
        public SearchParameters Clone()
        {
            return (SearchParameters)this.MemberwiseClone();
        }
    }
#pragma warning restore 1591 // Missing XML comments warning

#pragma warning disable 1591 // Missing XML comments warning

    /// <summary>
    /// Memory protection configuration structure (for default fields).
    /// </summary>
    public sealed class MemoryProtectionConfig : IDeepCloneable<MemoryProtectionConfig>
    {
        #region Fields

        /// <summary>
        /// The protect notes.
        /// </summary>
        public bool ProtectNotes = false;

        /// <summary>
        /// The protect password.
        /// </summary>
        public bool ProtectPassword = true;

        /// <summary>
        /// The protect title.
        /// </summary>
        public bool ProtectTitle = false;

        /// <summary>
        /// The protect url.
        /// </summary>
        public bool ProtectUrl = false;

        /// <summary>
        /// The protect user name.
        /// </summary>
        public bool ProtectUserName = false;

        #endregion

        // public bool AutoEnableVisualHiding = false;
        #region Public Methods and Operators

        /// <summary>
        /// The clone deep.
        /// </summary>
        /// <returns>
        /// The <see cref="MemoryProtectionConfig"/>.
        /// </returns>
        public MemoryProtectionConfig CloneDeep()
        {
            return (MemoryProtectionConfig)this.MemberwiseClone();
        }

        /// <summary>
        /// The get protection.
        /// </summary>
        /// <param name="strField">
        /// The str field.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool GetProtection(string strField)
        {
            if (strField == PwDefs.TitleField)
            {
                return this.ProtectTitle;
            }

            if (strField == PwDefs.UserNameField)
            {
                return this.ProtectUserName;
            }

            if (strField == PwDefs.PasswordField)
            {
                return this.ProtectPassword;
            }

            if (strField == PwDefs.UrlField)
            {
                return this.ProtectUrl;
            }

            if (strField == PwDefs.NotesField)
            {
                return this.ProtectNotes;
            }

            return false;
        }

        #endregion
    }
#pragma warning restore 1591 // Missing XML comments warning

    /// <summary>
    /// The object touched event args.
    /// </summary>
    public sealed class ObjectTouchedEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        /// The m_b modified.
        /// </summary>
        private bool m_bModified;

        /// <summary>
        /// The m_b parents touched.
        /// </summary>
        private bool m_bParentsTouched;

        /// <summary>
        /// The m_o.
        /// </summary>
        private object m_o;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTouchedEventArgs"/> class.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <param name="bModified">
        /// The b modified.
        /// </param>
        /// <param name="bParentsTouched">
        /// The b parents touched.
        /// </param>
        public ObjectTouchedEventArgs(object o, bool bModified, bool bParentsTouched)
        {
            this.m_o = o;
            this.m_bModified = bModified;
            this.m_bParentsTouched = bParentsTouched;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether modified.
        /// </summary>
        public bool Modified
        {
            get
            {
                return this.m_bModified;
            }
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        public object Object
        {
            get
            {
                return this.m_o;
            }
        }

        /// <summary>
        /// Gets a value indicating whether parents touched.
        /// </summary>
        public bool ParentsTouched
        {
            get
            {
                return this.m_bParentsTouched;
            }
        }

        #endregion
    }

    /// <summary>
    /// The io access event args.
    /// </summary>
    public sealed class IOAccessEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        /// The m_ioc.
        /// </summary>
        private IOConnectionInfo m_ioc;

        /// <summary>
        /// The m_ioc 2.
        /// </summary>
        private IOConnectionInfo m_ioc2;

        /// <summary>
        /// The m_t.
        /// </summary>
        private IOAccessType m_t;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IOAccessEventArgs"/> class.
        /// </summary>
        /// <param name="ioc">
        /// The ioc.
        /// </param>
        /// <param name="ioc2">
        /// The ioc 2.
        /// </param>
        /// <param name="t">
        /// The t.
        /// </param>
        public IOAccessEventArgs(IOConnectionInfo ioc, IOConnectionInfo ioc2, IOAccessType t)
        {
            this.m_ioc = ioc;
            this.m_ioc2 = ioc2;
            this.m_t = t;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the io connection info.
        /// </summary>
        public IOConnectionInfo IOConnectionInfo
        {
            get
            {
                return this.m_ioc;
            }
        }

        /// <summary>
        /// Gets the io connection info 2.
        /// </summary>
        public IOConnectionInfo IOConnectionInfo2
        {
            get
            {
                return this.m_ioc2;
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public IOAccessType Type
        {
            get
            {
                return this.m_t;
            }
        }

        #endregion
    }
}