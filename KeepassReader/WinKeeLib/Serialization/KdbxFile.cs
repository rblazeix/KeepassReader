// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KdbxFile.cs" company="">
//   
// </copyright>
// <summary>
//   The <c>KdbxFile</c> class supports saving the data to various
//   formats.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if !KeePassLibSD
#endif

namespace KeePassLib.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    using KeePassLib.Collections;
    using KeePassLib.Cryptography;
    using KeePassLib.Interfaces;
    using KeePassLib.Security;

    /// <summary>
    /// The <c>KdbxFile</c> class supports saving the data to various
    /// formats.
    /// </summary>
    public enum KdbxFormat
    {
        /// <summary>
        /// The default, encrypted file format.
        /// </summary>
        Default = 0, 

        /// <summary>
        /// Use this flag when exporting data to a plain-text XML file.
        /// </summary>
        PlainXml
    }

    /// <summary>
    /// Serialization to KeePass KDBX files.
    /// </summary>
    public sealed partial class KdbxFile
    {
        #region Constants

        /// <summary>
        /// File identifier, first 32-bit value.
        /// </summary>
        internal const uint FileSignature1 = 0x9AA2D903;

        /// <summary>
        /// File identifier, second 32-bit value.
        /// </summary>
        internal const uint FileSignature2 = 0xB54BFB67;

        // KeePass 1.x signature
        /// <summary>
        /// The file signature old 1.
        /// </summary>
        internal const uint FileSignatureOld1 = 0x9AA2D903;

        /// <summary>
        /// The file signature old 2.
        /// </summary>
        internal const uint FileSignatureOld2 = 0xB54BFB65;

        // KeePass 2.x pre-release (alpha and beta) signature
        /// <summary>
        /// The file signature pre release 1.
        /// </summary>
        internal const uint FileSignaturePreRelease1 = 0x9AA2D903;

        /// <summary>
        /// The file signature pre release 2.
        /// </summary>
        internal const uint FileSignaturePreRelease2 = 0xB54BFB66;

        /// <summary>
        /// The attr compressed.
        /// </summary>
        private const string AttrCompressed = "Compressed";

        /// <summary>
        /// The attr id.
        /// </summary>
        private const string AttrId = "ID";

        /// <summary>
        /// The attr protected.
        /// </summary>
        private const string AttrProtected = "Protected";

        /// <summary>
        /// The attr protected in mem plain xml.
        /// </summary>
        private const string AttrProtectedInMemPlainXml = "ProtectInMemory";

        /// <summary>
        /// The attr ref.
        /// </summary>
        private const string AttrRef = "Ref";

        /// <summary>
        /// The elem auto type.
        /// </summary>
        private const string ElemAutoType = "AutoType";

        /// <summary>
        /// The elem auto type default seq.
        /// </summary>
        private const string ElemAutoTypeDefaultSeq = "DefaultSequence";

        /// <summary>
        /// The elem auto type enabled.
        /// </summary>
        private const string ElemAutoTypeEnabled = "Enabled";

        /// <summary>
        /// The elem auto type item.
        /// </summary>
        private const string ElemAutoTypeItem = "Association";

        /// <summary>
        /// The elem auto type obfuscation.
        /// </summary>
        private const string ElemAutoTypeObfuscation = "DataTransferObfuscation";

        /// <summary>
        /// The elem bg color.
        /// </summary>
        private const string ElemBgColor = "BackgroundColor";

        /// <summary>
        /// The elem binaries.
        /// </summary>
        private const string ElemBinaries = "Binaries";

        /// <summary>
        /// The elem binary.
        /// </summary>
        private const string ElemBinary = "Binary";

        /// <summary>
        /// The elem creation time.
        /// </summary>
        private const string ElemCreationTime = "CreationTime";

        /// <summary>
        /// The elem custom data.
        /// </summary>
        private const string ElemCustomData = "CustomData";

        /// <summary>
        /// The elem custom icon id.
        /// </summary>
        private const string ElemCustomIconID = "CustomIconUUID";

        /// <summary>
        /// The elem custom icon item.
        /// </summary>
        private const string ElemCustomIconItem = "Icon";

        /// <summary>
        /// The elem custom icon item data.
        /// </summary>
        private const string ElemCustomIconItemData = "Data";

        /// <summary>
        /// The elem custom icon item id.
        /// </summary>
        private const string ElemCustomIconItemID = "UUID";

        /// <summary>
        /// The elem custom icons.
        /// </summary>
        private const string ElemCustomIcons = "CustomIcons";

        /// <summary>
        /// The elem db color.
        /// </summary>
        private const string ElemDbColor = "Color";

        /// <summary>
        /// The elem db default user.
        /// </summary>
        private const string ElemDbDefaultUser = "DefaultUserName";

        /// <summary>
        /// The elem db default user changed.
        /// </summary>
        private const string ElemDbDefaultUserChanged = "DefaultUserNameChanged";

        /// <summary>
        /// The elem db desc.
        /// </summary>
        private const string ElemDbDesc = "DatabaseDescription";

        /// <summary>
        /// The elem db desc changed.
        /// </summary>
        private const string ElemDbDescChanged = "DatabaseDescriptionChanged";

        /// <summary>
        /// The elem db key change force.
        /// </summary>
        private const string ElemDbKeyChangeForce = "MasterKeyChangeForce";

        /// <summary>
        /// The elem db key change rec.
        /// </summary>
        private const string ElemDbKeyChangeRec = "MasterKeyChangeRec";

        /// <summary>
        /// The elem db key changed.
        /// </summary>
        private const string ElemDbKeyChanged = "MasterKeyChanged";

        /// <summary>
        /// The elem db mntnc history days.
        /// </summary>
        private const string ElemDbMntncHistoryDays = "MaintenanceHistoryDays";

        /// <summary>
        /// The elem db name.
        /// </summary>
        private const string ElemDbName = "DatabaseName";

        /// <summary>
        /// The elem db name changed.
        /// </summary>
        private const string ElemDbNameChanged = "DatabaseNameChanged";

        /// <summary>
        /// The elem deleted object.
        /// </summary>
        private const string ElemDeletedObject = "DeletedObject";

        /// <summary>
        /// The elem deleted objects.
        /// </summary>
        private const string ElemDeletedObjects = "DeletedObjects";

        /// <summary>
        /// The elem deletion time.
        /// </summary>
        private const string ElemDeletionTime = "DeletionTime";

        /// <summary>
        /// The elem doc node.
        /// </summary>
        private const string ElemDocNode = "KeePassFile";

        /// <summary>
        /// The elem enable auto type.
        /// </summary>
        private const string ElemEnableAutoType = "EnableAutoType";

        /// <summary>
        /// The elem enable searching.
        /// </summary>
        private const string ElemEnableSearching = "EnableSearching";

        /// <summary>
        /// The elem entry.
        /// </summary>
        private const string ElemEntry = "Entry";

        /// <summary>
        /// The elem entry templates group.
        /// </summary>
        private const string ElemEntryTemplatesGroup = "EntryTemplatesGroup";

        /// <summary>
        /// The elem entry templates group changed.
        /// </summary>
        private const string ElemEntryTemplatesGroupChanged = "EntryTemplatesGroupChanged";

        /// <summary>
        /// The elem expires.
        /// </summary>
        private const string ElemExpires = "Expires";

        /// <summary>
        /// The elem expiry time.
        /// </summary>
        private const string ElemExpiryTime = "ExpiryTime";

        /// <summary>
        /// The elem fg color.
        /// </summary>
        private const string ElemFgColor = "ForegroundColor";

        /// <summary>
        /// The elem generator.
        /// </summary>
        private const string ElemGenerator = "Generator";

        /// <summary>
        /// The elem group.
        /// </summary>
        private const string ElemGroup = "Group";

        /// <summary>
        /// The elem group default auto type seq.
        /// </summary>
        private const string ElemGroupDefaultAutoTypeSeq = "DefaultAutoTypeSequence";

        /// <summary>
        /// The elem header hash.
        /// </summary>
        private const string ElemHeaderHash = "HeaderHash";

        /// <summary>
        /// The elem history.
        /// </summary>
        private const string ElemHistory = "History";

        /// <summary>
        /// The elem history max items.
        /// </summary>
        private const string ElemHistoryMaxItems = "HistoryMaxItems";

        /// <summary>
        /// The elem history max size.
        /// </summary>
        private const string ElemHistoryMaxSize = "HistoryMaxSize";

        /// <summary>
        /// The elem icon.
        /// </summary>
        private const string ElemIcon = "IconID";

        /// <summary>
        /// The elem is expanded.
        /// </summary>
        private const string ElemIsExpanded = "IsExpanded";

        /// <summary>
        /// The elem key.
        /// </summary>
        private const string ElemKey = "Key";

        /// <summary>
        /// The elem keystroke sequence.
        /// </summary>
        private const string ElemKeystrokeSequence = "KeystrokeSequence";

        /// <summary>
        /// The elem last access time.
        /// </summary>
        private const string ElemLastAccessTime = "LastAccessTime";

        /// <summary>
        /// The elem last mod time.
        /// </summary>
        private const string ElemLastModTime = "LastModificationTime";

        /// <summary>
        /// The elem last selected group.
        /// </summary>
        private const string ElemLastSelectedGroup = "LastSelectedGroup";

        /// <summary>
        /// The elem last top visible entry.
        /// </summary>
        private const string ElemLastTopVisibleEntry = "LastTopVisibleEntry";

        /// <summary>
        /// The elem last top visible group.
        /// </summary>
        private const string ElemLastTopVisibleGroup = "LastTopVisibleGroup";

        /// <summary>
        /// The elem location changed.
        /// </summary>
        private const string ElemLocationChanged = "LocationChanged";

        /// <summary>
        /// The elem memory prot.
        /// </summary>
        private const string ElemMemoryProt = "MemoryProtection";

        /// <summary>
        /// The elem meta.
        /// </summary>
        private const string ElemMeta = "Meta";

        /// <summary>
        /// The elem name.
        /// </summary>
        private const string ElemName = "Name";

        /// <summary>
        /// The elem notes.
        /// </summary>
        private const string ElemNotes = "Notes";

        /// <summary>
        /// The elem override url.
        /// </summary>
        private const string ElemOverrideUrl = "OverrideURL";

        /// <summary>
        /// The elem prot notes.
        /// </summary>
        private const string ElemProtNotes = "ProtectNotes";

        /// <summary>
        /// The elem prot password.
        /// </summary>
        private const string ElemProtPassword = "ProtectPassword";

        /// <summary>
        /// The elem prot title.
        /// </summary>
        private const string ElemProtTitle = "ProtectTitle";

        /// <summary>
        /// The elem prot url.
        /// </summary>
        private const string ElemProtUrl = "ProtectURL";

        /// <summary>
        /// The elem prot user name.
        /// </summary>
        private const string ElemProtUserName = "ProtectUserName";

        /// <summary>
        /// The elem recycle bin changed.
        /// </summary>
        private const string ElemRecycleBinChanged = "RecycleBinChanged";

        /// <summary>
        /// The elem recycle bin enabled.
        /// </summary>
        private const string ElemRecycleBinEnabled = "RecycleBinEnabled";

        /// <summary>
        /// The elem recycle bin uuid.
        /// </summary>
        private const string ElemRecycleBinUuid = "RecycleBinUUID";

        /// <summary>
        /// The elem root.
        /// </summary>
        private const string ElemRoot = "Root";

        /// <summary>
        /// The elem string.
        /// </summary>
        private const string ElemString = "String";

        /// <summary>
        /// The elem string dict ex item.
        /// </summary>
        private const string ElemStringDictExItem = "Item";

        /// <summary>
        /// The elem tags.
        /// </summary>
        private const string ElemTags = "Tags";

        /// <summary>
        /// The elem times.
        /// </summary>
        private const string ElemTimes = "Times";

        /// <summary>
        /// The elem usage count.
        /// </summary>
        private const string ElemUsageCount = "UsageCount";

        /// <summary>
        /// The elem uuid.
        /// </summary>
        private const string ElemUuid = "UUID";

        /// <summary>
        /// The elem value.
        /// </summary>
        private const string ElemValue = "Value";

        /// <summary>
        /// The elem window.
        /// </summary>
        private const string ElemWindow = "Window";

        /// <summary>
        /// File version of files saved by the current <c>KdbxFile</c> class.
        /// KeePass 2.07 has version 1.01, 2.08 has 1.02, 2.09 has 2.00,
        /// 2.10 has 2.02, 2.11 has 2.04, 2.15 has 3.00, 2.20 has 3.01.
        /// The first 2 bytes are critical (i.e. loading will fail, if the
        /// file version is too high), the last 2 bytes are informational.
        /// </summary>
        private const uint FileVersion32 = 0x00030001;

        /// <summary>
        /// The file version critical mask.
        /// </summary>
        private const uint FileVersionCriticalMask = 0xFFFF0000;

        /// <summary>
        /// The neutral language id.
        /// </summary>
        private const uint NeutralLanguageID = NeutralLanguageOffset + NeutralLanguageIDSec;

        /// <summary>
        /// The neutral language id sec.
        /// </summary>
        private const uint NeutralLanguageIDSec = 0x7DC5C; // See 32-bit Unicode specs

        /// <summary>
        /// The neutral language offset.
        /// </summary>
        private const uint NeutralLanguageOffset = 0x100000; // 2^20, see 32-bit Unicode specs

        /// <summary>
        /// The val false.
        /// </summary>
        private const string ValFalse = "False";

        /// <summary>
        /// The val true.
        /// </summary>
        private const string ValTrue = "True";

        #endregion

        #region Static Fields

        /// <summary>
        /// The m_b localized names.
        /// </summary>
        private static bool m_bLocalizedNames = false;

        #endregion

        #region Fields

        /// <summary>
        /// The m_dt now.
        /// </summary>
        private readonly DateTime m_dtNow = DateTime.Now; // Cache current time

        /// <summary>
        /// The m_b repair mode.
        /// </summary>
        private bool m_bRepairMode = false;

        /// <summary>
        /// The m_cra inner random stream.
        /// </summary>
        private CrsAlgorithm m_craInnerRandomStream = CrsAlgorithm.ArcFourVariant;

        /// <summary>
        /// The m_dict bin pool.
        /// </summary>
        private Dictionary<string, ProtectedBinary> m_dictBinPool = new Dictionary<string, ProtectedBinary>();

        /// <summary>
        /// The m_format.
        /// </summary>
        private KdbxFormat m_format = KdbxFormat.Default;

        /// <summary>
        /// The m_pb encryption iv.
        /// </summary>
        private byte[] m_pbEncryptionIV = null;

        /// <summary>
        /// The m_pb hash of file on disk.
        /// </summary>
        private byte[] m_pbHashOfFileOnDisk = null;

        /// <summary>
        /// The m_pb hash of header.
        /// </summary>
        private byte[] m_pbHashOfHeader = null;

        /// <summary>
        /// The m_pb master seed.
        /// </summary>
        private byte[] m_pbMasterSeed = null;

        /// <summary>
        /// The m_pb protected stream key.
        /// </summary>
        private byte[] m_pbProtectedStreamKey = null;

        /// <summary>
        /// The m_pb stream start bytes.
        /// </summary>
        private byte[] m_pbStreamStartBytes = null;

        /// <summary>
        /// The m_pb transform seed.
        /// </summary>
        private byte[] m_pbTransformSeed = null;

        /// <summary>
        /// The m_pw database.
        /// </summary>
        private PwDatabase m_pwDatabase; // Not null, see constructor

        /// <summary>
        /// The m_random stream.
        /// </summary>
        private CryptoRandomStream m_randomStream = null;

        /// <summary>
        /// The m_sl logger.
        /// </summary>
        private IStatusLogger m_slLogger = null;

        /// <summary>
        /// The m_str detach bins.
        /// </summary>
        private string m_strDetachBins = null;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KdbxFile"/> class. 
        /// Default constructor.
        /// </summary>
        /// <param name="pwDataStore">
        /// The <c>PwDatabase</c> instance that the
        /// class will load file data into or use to create a KDBX file.
        /// </param>
        public KdbxFile(PwDatabase pwDataStore)
        {
            Debug.Assert(pwDataStore != null);
            if (pwDataStore == null)
            {
                throw new ArgumentNullException("pwDataStore");
            }

            this.m_pwDatabase = pwDataStore;
        }

        #endregion

        // ArcFourVariant only for compatibility; KeePass will default to a
        // different (more secure) algorithm when *writing* databases
        #region Enums

        /// <summary>
        /// The kdbx header field id.
        /// </summary>
        private enum KdbxHeaderFieldID : byte
        {
            /// <summary>
            /// The end of header.
            /// </summary>
            EndOfHeader = 0, 

            /// <summary>
            /// The comment.
            /// </summary>
            Comment = 1, 

            /// <summary>
            /// The cipher id.
            /// </summary>
            CipherID = 2, 

            /// <summary>
            /// The compression flags.
            /// </summary>
            CompressionFlags = 3, 

            /// <summary>
            /// The master seed.
            /// </summary>
            MasterSeed = 4, 

            /// <summary>
            /// The transform seed.
            /// </summary>
            TransformSeed = 5, 

            /// <summary>
            /// The transform rounds.
            /// </summary>
            TransformRounds = 6, 

            /// <summary>
            /// The encryption iv.
            /// </summary>
            EncryptionIV = 7, 

            /// <summary>
            /// The protected stream key.
            /// </summary>
            ProtectedStreamKey = 8, 

            /// <summary>
            /// The stream start bytes.
            /// </summary>
            StreamStartBytes = 9, 

            /// <summary>
            /// The inner random stream id.
            /// </summary>
            InnerRandomStreamID = 10
        }

        #endregion

        #region Public Properties

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
        /// Gets the hash of file on disk.
        /// </summary>
        public byte[] HashOfFileOnDisk
        {
            get
            {
                return this.m_pbHashOfFileOnDisk;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether repair mode.
        /// </summary>
        public bool RepairMode
        {
            get
            {
                return this.m_bRepairMode;
            }

            set
            {
                this.m_bRepairMode = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Call this once to determine the current localization settings.
        /// </summary>
        public static void DetermineLanguageId()
        {
            // Test if localized names should be used. If localized names are used,
            // the m_bLocalizedNames value must be set to true. By default, localized
            // names should be used! (Otherwise characters could be corrupted
            // because of different code pages).
            unchecked
            {
                uint uTest = 0;
                foreach (char ch in PwDatabase.LocalizedAppName)
                {
                    uTest = uTest * 5 + ch;
                }

                m_bLocalizedNames = uTest != NeutralLanguageID;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The bin pool add.
        /// </summary>
        /// <param name="dict">
        /// The dict.
        /// </param>
        private void BinPoolAdd(ProtectedBinaryDictionary dict)
        {
            foreach (KeyValuePair<string, ProtectedBinary> kvp in dict)
            {
                BinPoolAdd(kvp.Value);
            }
        }

        /// <summary>
        /// The bin pool add.
        /// </summary>
        /// <param name="pb">
        /// The pb.
        /// </param>
        private void BinPoolAdd(ProtectedBinary pb)
        {
            if (pb == null)
            {
                Debug.Assert(false);
                return;
            }

            if (this.BinPoolFind(pb) != null)
            {
                return; // Exists already
            }

            this.m_dictBinPool.Add(this.m_dictBinPool.Count.ToString(NumberFormatInfo.InvariantInfo), pb);
        }

        /// <summary>
        /// The bin pool find.
        /// </summary>
        /// <param name="pb">
        /// The pb.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string BinPoolFind(ProtectedBinary pb)
        {
            if (pb == null)
            {
                Debug.Assert(false);
                return null;
            }

            foreach (KeyValuePair<string, ProtectedBinary> kvp in this.m_dictBinPool)
            {
                if (pb.Equals(kvp.Value))
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        /// <summary>
        /// The bin pool get.
        /// </summary>
        /// <param name="strKey">
        /// The str key.
        /// </param>
        /// <returns>
        /// The <see cref="ProtectedBinary"/>.
        /// </returns>
        private ProtectedBinary BinPoolGet(string strKey)
        {
            if (strKey == null)
            {
                Debug.Assert(false);
                return null;
            }

            ProtectedBinary pb;
            if (this.m_dictBinPool.TryGetValue(strKey, out pb))
            {
                return pb;
            }

            return null;
        }

        #endregion
    }
}