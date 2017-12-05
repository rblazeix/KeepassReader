// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KdbxFile.Read.Streamed.cs" company="">
//   
// </copyright>
// <summary>
//   Serialization to KeePass KDBX files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Xml;

    using KeePassLib.Collections;
    using KeePassLib.Interfaces;
    using KeePassLib.Resources;
    using KeePassLib.Security;
    using KeePassLib.Utility;

    using WinKeeLib;

    /// <summary>
    /// Serialization to KeePass KDBX files.
    /// </summary>
    public sealed partial class KdbxFile
    {
        #region Fields

        /// <summary>
        /// The m_b entry in history.
        /// </summary>
        private bool m_bEntryInHistory = false;

        /// <summary>
        /// The m_b read next node.
        /// </summary>
        private bool m_bReadNextNode = true;

        /// <summary>
        /// The m_ctx at name.
        /// </summary>
        private string m_ctxATName = null;

        /// <summary>
        /// The m_ctx at seq.
        /// </summary>
        private string m_ctxATSeq = null;

        /// <summary>
        /// The m_ctx binary name.
        /// </summary>
        private string m_ctxBinaryName = null;

        /// <summary>
        /// The m_ctx binary value.
        /// </summary>
        private ProtectedBinary m_ctxBinaryValue = null;

        /// <summary>
        /// The m_ctx deleted object.
        /// </summary>
        private PwDeletedObject m_ctxDeletedObject = null;

        /// <summary>
        /// The m_ctx entry.
        /// </summary>
        private PwEntry m_ctxEntry = null;

        /// <summary>
        /// The m_ctx group.
        /// </summary>
        private PwGroup m_ctxGroup = null;

        /// <summary>
        /// The m_ctx groups.
        /// </summary>
        private Stack<PwGroup> m_ctxGroups = new Stack<PwGroup>();

        /// <summary>
        /// The m_ctx history base.
        /// </summary>
        private PwEntry m_ctxHistoryBase = null;

        /// <summary>
        /// The m_ctx string name.
        /// </summary>
        private string m_ctxStringName = null;

        /// <summary>
        /// The m_ctx string value.
        /// </summary>
        private ProtectedString m_ctxStringValue = null;

        /// <summary>
        /// The m_pb custom icon data.
        /// </summary>
        private byte[] m_pbCustomIconData = null;

        /// <summary>
        /// The m_str custom data key.
        /// </summary>
        private string m_strCustomDataKey = null;

        /// <summary>
        /// The m_str custom data value.
        /// </summary>
        private string m_strCustomDataValue = null;

        /// <summary>
        /// The m_uuid custom icon id.
        /// </summary>
        private PwUuid m_uuidCustomIconID = PwUuid.Zero;

        #endregion

        #region Enums

        /// <summary>
        /// The kdb context.
        /// </summary>
        private enum KdbContext
        {
            /// <summary>
            /// The null.
            /// </summary>
            Null, 

            /// <summary>
            /// The kee pass file.
            /// </summary>
            KeePassFile, 

            /// <summary>
            /// The meta.
            /// </summary>
            Meta, 

            /// <summary>
            /// The root.
            /// </summary>
            Root, 

            /// <summary>
            /// The memory protection.
            /// </summary>
            MemoryProtection, 

            /// <summary>
            /// The custom icons.
            /// </summary>
            CustomIcons, 

            /// <summary>
            /// The custom icon.
            /// </summary>
            CustomIcon, 

            /// <summary>
            /// The binaries.
            /// </summary>
            Binaries, 

            /// <summary>
            /// The custom data.
            /// </summary>
            CustomData, 

            /// <summary>
            /// The custom data item.
            /// </summary>
            CustomDataItem, 

            /// <summary>
            /// The root deleted objects.
            /// </summary>
            RootDeletedObjects, 

            /// <summary>
            /// The deleted object.
            /// </summary>
            DeletedObject, 

            /// <summary>
            /// The group.
            /// </summary>
            Group, 

            /// <summary>
            /// The group times.
            /// </summary>
            GroupTimes, 

            /// <summary>
            /// The entry.
            /// </summary>
            Entry, 

            /// <summary>
            /// The entry times.
            /// </summary>
            EntryTimes, 

            /// <summary>
            /// The entry string.
            /// </summary>
            EntryString, 

            /// <summary>
            /// The entry binary.
            /// </summary>
            EntryBinary, 

            /// <summary>
            /// The entry auto type.
            /// </summary>
            EntryAutoType, 

            /// <summary>
            /// The entry auto type item.
            /// </summary>
            EntryAutoTypeItem, 

            /// <summary>
            /// The entry history.
            /// </summary>
            EntryHistory
        }

        #endregion

        #region Methods

        /// <summary>
        /// The create std xml reader settings.
        /// </summary>
        /// <returns>
        /// The <see cref="XmlReaderSettings"/>.
        /// </returns>
        internal static XmlReaderSettings CreateStdXmlReaderSettings()
        {
            XmlReaderSettings xrs = new XmlReaderSettings();

            xrs.CloseInput = true;
            xrs.IgnoreComments = true;
            xrs.IgnoreProcessingInstructions = true;
            xrs.IgnoreWhitespace = true;

            return xrs;
        }

        /// <summary>
        /// The create xml reader.
        /// </summary>
        /// <param name="readerStream">
        /// The reader stream.
        /// </param>
        /// <returns>
        /// The <see cref="XmlReader"/>.
        /// </returns>
        private static XmlReader CreateXmlReader(Stream readerStream)
        {
            XmlReaderSettings xrs = CreateStdXmlReaderSettings();
            return XmlReader.Create(readerStream, xrs);
        }

        /// <summary>
        /// The switch context.
        /// </summary>
        /// <param name="ctxCurrent">
        /// The ctx current.
        /// </param>
        /// <param name="ctxNew">
        /// The ctx new.
        /// </param>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="KdbContext"/>.
        /// </returns>
        private static KdbContext SwitchContext(KdbContext ctxCurrent, KdbContext ctxNew, XmlReader xr)
        {
            if (xr.IsEmptyElement)
            {
                return ctxCurrent;
            }

            return ctxNew;
        }

        /// <summary>
        /// The end xml element.
        /// </summary>
        /// <param name="ctx">
        /// The ctx.
        /// </param>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="KdbContext"/>.
        /// </returns>
        /// <exception cref="FormatException">
        /// </exception>
        private KdbContext EndXmlElement(KdbContext ctx, XmlReader xr)
        {
            Debug.Assert(xr.NodeType == XmlNodeType.EndElement);

            if ((ctx == KdbContext.KeePassFile) && (xr.Name == ElemDocNode))
            {
                return KdbContext.Null;
            }
            else if ((ctx == KdbContext.Meta) && (xr.Name == ElemMeta))
            {
                return KdbContext.KeePassFile;
            }
            else if ((ctx == KdbContext.Root) && (xr.Name == ElemRoot))
            {
                return KdbContext.KeePassFile;
            }
            else if ((ctx == KdbContext.MemoryProtection) && (xr.Name == ElemMemoryProt))
            {
                return KdbContext.Meta;
            }
            else if ((ctx == KdbContext.CustomIcons) && (xr.Name == ElemCustomIcons))
            {
                return KdbContext.Meta;
            }
            else if ((ctx == KdbContext.CustomIcon) && (xr.Name == ElemCustomIconItem))
            {
                if (!this.m_uuidCustomIconID.Equals(PwUuid.Zero) && (this.m_pbCustomIconData != null))
                {
                    this.m_pwDatabase.CustomIcons.Add(new PwCustomIcon(this.m_uuidCustomIconID, this.m_pbCustomIconData));
                }
                else
                {
                    Debug.Assert(false);
                }

                this.m_uuidCustomIconID = PwUuid.Zero;
                this.m_pbCustomIconData = null;

                return KdbContext.CustomIcons;
            }
            else if ((ctx == KdbContext.Binaries) && (xr.Name == ElemBinaries))
            {
                return KdbContext.Meta;
            }
            else if ((ctx == KdbContext.CustomData) && (xr.Name == ElemCustomData))
            {
                return KdbContext.Meta;
            }
            else if ((ctx == KdbContext.CustomDataItem) && (xr.Name == ElemStringDictExItem))
            {
                if ((this.m_strCustomDataKey != null) && (this.m_strCustomDataValue != null))
                {
                    this.m_pwDatabase.CustomData.Set(this.m_strCustomDataKey, this.m_strCustomDataValue);
                }
                else
                {
                    Debug.Assert(false);
                }

                this.m_strCustomDataKey = null;
                this.m_strCustomDataValue = null;

                return KdbContext.CustomData;
            }
            else if ((ctx == KdbContext.Group) && (xr.Name == ElemGroup))
            {
                if (PwUuid.Zero.Equals(this.m_ctxGroup.Uuid))
                {
                    this.m_ctxGroup.Uuid = new PwUuid(true); // No assert (import)
                }

                this.m_ctxGroups.Pop();

                if (this.m_ctxGroups.Count == 0)
                {
                    this.m_ctxGroup = null;
                    return KdbContext.Root;
                }
                else
                {
                    this.m_ctxGroup = this.m_ctxGroups.Peek();
                    return KdbContext.Group;
                }
            }
            else if ((ctx == KdbContext.GroupTimes) && (xr.Name == ElemTimes))
            {
                return KdbContext.Group;
            }
            else if ((ctx == KdbContext.Entry) && (xr.Name == ElemEntry))
            {
                // Create new UUID if absent
                if (PwUuid.Zero.Equals(this.m_ctxEntry.Uuid))
                {
                    this.m_ctxEntry.Uuid = new PwUuid(true); // No assert (import)
                }

                if (this.m_bEntryInHistory)
                {
                    this.m_ctxEntry = this.m_ctxHistoryBase;
                    return KdbContext.EntryHistory;
                }

                return KdbContext.Group;
            }
            else if ((ctx == KdbContext.EntryTimes) && (xr.Name == ElemTimes))
            {
                return KdbContext.Entry;
            }
            else if ((ctx == KdbContext.EntryString) && (xr.Name == ElemString))
            {
                this.m_ctxEntry.Strings.Set(this.m_ctxStringName, this.m_ctxStringValue);
                this.m_ctxStringName = null;
                this.m_ctxStringValue = null;
                return KdbContext.Entry;
            }
            else if ((ctx == KdbContext.EntryBinary) && (xr.Name == ElemBinary))
            {
                if (string.IsNullOrEmpty(this.m_strDetachBins))
                {
                    //this.m_ctxEntry.Binaries.Set(this.m_ctxBinaryName, this.m_ctxBinaryValue);
                }
                else
                {
                    // SaveBinary(m_ctxBinaryName, m_ctxBinaryValue, m_strDetachBins);
                    this.m_ctxBinaryValue = null;
                    GC.Collect();
                }

                this.m_ctxBinaryName = null;
                this.m_ctxBinaryValue = null;
                return KdbContext.Entry;
            }
            else if ((ctx == KdbContext.EntryAutoType) && (xr.Name == ElemAutoType))
            {
                return KdbContext.Entry;
            }
            else if ((ctx == KdbContext.EntryAutoTypeItem) && (xr.Name == ElemAutoTypeItem))
            {
                AutoTypeAssociation atAssoc = new AutoTypeAssociation(this.m_ctxATName, this.m_ctxATSeq);
                this.m_ctxEntry.AutoType.Add(atAssoc);
                this.m_ctxATName = null;
                this.m_ctxATSeq = null;
                return KdbContext.EntryAutoType;
            }
            else if ((ctx == KdbContext.EntryHistory) && (xr.Name == ElemHistory))
            {
                this.m_bEntryInHistory = false;
                return KdbContext.Entry;
            }
            else if ((ctx == KdbContext.RootDeletedObjects) && (xr.Name == ElemDeletedObjects))
            {
                return KdbContext.Root;
            }
            else if ((ctx == KdbContext.DeletedObject) && (xr.Name == ElemDeletedObject))
            {
                this.m_ctxDeletedObject = null;
                return KdbContext.RootDeletedObjects;
            }
            else
            {
                Debug.Assert(false);
                throw new FormatException();
            }
        }

        /// <summary>
        /// The process node.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="XorredBuffer"/>.
        /// </returns>
        private XorredBuffer ProcessNode(XmlReader xr)
        {
            // Debug.Assert(xr.NodeType == XmlNodeType.Element);
            XorredBuffer xb = null;
            if (xr.HasAttributes)
            {
                if (xr.MoveToAttribute(AttrProtected))
                {
                    if (xr.Value == ValTrue)
                    {
                        xr.MoveToElement();
                        string strEncrypted = this.ReadStringRaw(xr);

                        byte[] pbEncrypted;
                        if (strEncrypted.Length > 0)
                        {
                            pbEncrypted = Convert.FromBase64String(strEncrypted);
                        }
                        else
                        {
                            pbEncrypted = new byte[0];
                        }

                        byte[] pbPad = this.m_randomStream.GetRandomBytes((uint)pbEncrypted.Length);

                        xb = new XorredBuffer(pbEncrypted, pbPad);
                    }
                }
            }

            return xb;
        }

        /// <summary>
        /// The read bool.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <param name="bDefault">
        /// The b default.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ReadBool(XmlReader xr, bool bDefault)
        {
            string str = this.ReadString(xr);
            if (str == ValTrue)
            {
                return true;
            }
            else if (str == ValFalse)
            {
                return false;
            }

            Debug.Assert(false);
            return bDefault;
        }

        /// <summary>
        /// The read document streamed.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <param name="sParentStream">
        /// The s parent stream.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="FormatException">
        /// </exception>
        private void ReadDocumentStreamed(XmlReader xr, Stream sParentStream, CancellationToken token)
        {
            Debug.Assert(xr != null);
            if (xr == null)
            {
                throw new ArgumentNullException("xr");
            }

            this.m_ctxGroups.Clear();
            this.m_dictBinPool = new Dictionary<string, ProtectedBinary>();

            KdbContext ctx = KdbContext.Null;

            uint uTagCounter = 0;

            bool bSupportsStatus = this.m_slLogger != null;
            long lStreamLength = 1;
            try
            {
                sParentStream.Position.ToString(); // Test Position support
                lStreamLength = sParentStream.Length;
            }
            catch (Exception)
            {
                bSupportsStatus = false;
            }

            if (lStreamLength <= 0)
            {
                Debug.Assert(false);
                lStreamLength = 1;
            }

            this.m_bReadNextNode = true;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }

                if (this.m_bReadNextNode)
                {
                    if (!xr.Read())
                    {
                        break;
                    }
                }
                else
                {
                    this.m_bReadNextNode = true;
                }

                switch (xr.NodeType)
                {
                    case XmlNodeType.Element:
                        ctx = this.ReadXmlElement(ctx, xr);
                        break;

                    case XmlNodeType.EndElement:
                        ctx = this.EndXmlElement(ctx, xr);
                        break;

                    case XmlNodeType.XmlDeclaration:
                        break; // Ignore

                    default:
                        Debug.Assert(false);
                        break;
                }

                ++uTagCounter;
                if (((uTagCounter % 256) == 0) && bSupportsStatus)
                {
                    Debug.Assert(lStreamLength == sParentStream.Length);
                    uint uPct = (uint)((sParentStream.Position * 100) / lStreamLength);

                    // Clip percent value in case the stream reports incorrect
                    // position/length values (M120413)
                    if (uPct > 100)
                    {
                        Debug.Assert(false);
                        uPct = 100;
                    }

                    this.m_slLogger.SetProgress(uPct);
                }
            }

            Debug.Assert(ctx == KdbContext.Null);
            if (ctx != KdbContext.Null)
            {
                throw new FormatException();
            }

            Debug.Assert(this.m_ctxGroups.Count == 0);
            if (this.m_ctxGroups.Count != 0)
            {
                throw new FormatException();
            }
        }

        /// <summary>
        /// The read int.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <param name="nDefault">
        /// The n default.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        private int ReadInt(XmlReader xr, int nDefault)
        {
            string str = this.ReadString(xr);

            int n;
            if (StrUtil.TryParseIntInvariant(str, out n))
            {
                return n;
            }

            // Backward compatibility
            if (StrUtil.TryParseInt(str, out n))
            {
                return n;
            }

            Debug.Assert(false);
            return nDefault;
        }

        /// <summary>
        /// The read long.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <param name="lDefault">
        /// The l default.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        private long ReadLong(XmlReader xr, long lDefault)
        {
            string str = this.ReadString(xr);

            long l;
            if (StrUtil.TryParseLongInvariant(str, out l))
            {
                return l;
            }

            // Backward compatibility
            if (StrUtil.TryParseLong(str, out l))
            {
                return l;
            }

            Debug.Assert(false);
            return lDefault;
        }

        /// <summary>
        /// The read protected binary.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="ProtectedBinary"/>.
        /// </returns>
        /*private ProtectedBinary ReadProtectedBinary(XmlReader xr)
        {
            if (xr.MoveToAttribute(AttrRef))
            {
                string strRef = xr.Value;
                if (strRef != null)
                {
                    ProtectedBinary pb = this.BinPoolGet(strRef);
                    if (pb != null)
                    {
                        return pb;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            bool bCompressed = false;
            if (xr.MoveToAttribute(AttrCompressed))
            {
                bCompressed = xr.Value == ValTrue;
            }

            XorredBuffer xb = this.ProcessNode(xr);
            if (xb != null)
            {
                Debug.Assert(!bCompressed); // See SubWriteValue(ProtectedBinary value)
                return new ProtectedBinary(true, xb);
            }

            string strValue = this.ReadString(xr);
            if (strValue.Length == 0)
            {
                return new ProtectedBinary();
            }

            byte[] pbData = Convert.FromBase64String(strValue);
            if (bCompressed)
            {
                pbData = MemUtil.Decompress(pbData);
            }

            return new ProtectedBinary(false, pbData);
        }*/

        /// <summary>
        /// The read protected string.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="ProtectedString"/>.
        /// </returns>
        private ProtectedString ReadProtectedString(XmlReader xr)
        {
            XorredBuffer xb = this.ProcessNode(xr);
            if (xb != null)
            {
                return new ProtectedString(true, xb);
            }

            bool bProtect = false;
            if (this.m_format == KdbxFormat.PlainXml)
            {
                if (xr.MoveToAttribute(AttrProtectedInMemPlainXml))
                {
                    string strProtect = xr.Value;
                    bProtect = (strProtect != null) && (strProtect == ValTrue);
                }
            }

            ProtectedString ps = new ProtectedString(bProtect, this.ReadString(xr));
            return ps;
        }

        /// <summary>
        /// The read string.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ReadString(XmlReader xr)
        {
            XorredBuffer xb = this.ProcessNode(xr);
            if (xb != null)
            {
                byte[] pb = xb.ReadPlainText();
                if (pb.Length == 0)
                {
                    return string.Empty;
                }

                return StrUtil.Utf8.GetString(pb, 0, pb.Length);
            }

            this.m_bReadNextNode = false; // ReadElementString skips end tag
            return xr.ReadElementContentAsString();
        }

        /// <summary>
        /// The read string raw.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ReadStringRaw(XmlReader xr)
        {
            this.m_bReadNextNode = false; // ReadElementString skips end tag
            return xr.ReadElementContentAsString();
        }

        /// <summary>
        /// The read time.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        private DateTime ReadTime(XmlReader xr)
        {
            string str = this.ReadString(xr);

            DateTime dt;
            if (TimeUtil.TryDeserializeUtc(str, out dt))
            {
                return dt;
            }

            Debug.Assert(false);
            return this.m_dtNow;
        }

        /// <summary>
        /// The read u int.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <param name="uDefault">
        /// The u default.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        private uint ReadUInt(XmlReader xr, uint uDefault)
        {
            string str = this.ReadString(xr);

            uint u;
            if (StrUtil.TryParseUIntInvariant(str, out u))
            {
                return u;
            }

            // Backward compatibility
            if (StrUtil.TryParseUInt(str, out u))
            {
                return u;
            }

            Debug.Assert(false);
            return uDefault;
        }

        /// <summary>
        /// The read u long.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <param name="uDefault">
        /// The u default.
        /// </param>
        /// <returns>
        /// The <see cref="ulong"/>.
        /// </returns>
        private ulong ReadULong(XmlReader xr, ulong uDefault)
        {
            string str = this.ReadString(xr);

            ulong u;
            if (StrUtil.TryParseULongInvariant(str, out u))
            {
                return u;
            }

            // Backward compatibility
            if (StrUtil.TryParseULong(str, out u))
            {
                return u;
            }

            Debug.Assert(false);
            return uDefault;
        }

        /// <summary>
        /// The read unknown.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        private void ReadUnknown(XmlReader xr)
        {
            Debug.Assert(false); // Unknown node!

            if (xr.IsEmptyElement)
            {
                return;
            }

            string strUnknownName = xr.Name;
            this.ProcessNode(xr);

            while (xr.Read())
            {
                if (xr.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                if (xr.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                this.ReadUnknown(xr);
            }

            Debug.Assert(xr.Name == strUnknownName);
        }

        /// <summary>
        /// The read uuid.
        /// </summary>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="PwUuid"/>.
        /// </returns>
        private PwUuid ReadUuid(XmlReader xr)
        {
            string str = this.ReadString(xr);
            if (string.IsNullOrEmpty(str))
            {
                return PwUuid.Zero;
            }

            return new PwUuid(Convert.FromBase64String(str));
        }

        /// <summary>
        /// The read xml element.
        /// </summary>
        /// <param name="ctx">
        /// The ctx.
        /// </param>
        /// <param name="xr">
        /// The xr.
        /// </param>
        /// <returns>
        /// The <see cref="KdbContext"/>.
        /// </returns>
        /// <exception cref="IOException">
        /// </exception>
        /// <exception cref="FormatException">
        /// </exception>
        private KdbContext ReadXmlElement(KdbContext ctx, XmlReader xr)
        {
            Debug.Assert(xr.NodeType == XmlNodeType.Element);

            switch (ctx)
            {
                case KdbContext.Null:
                    if (xr.Name == ElemDocNode)
                    {
                        return SwitchContext(ctx, KdbContext.KeePassFile, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.KeePassFile:
                    if (xr.Name == ElemMeta)
                    {
                        return SwitchContext(ctx, KdbContext.Meta, xr);
                    }
                    else if (xr.Name == ElemRoot)
                    {
                        return SwitchContext(ctx, KdbContext.Root, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.Meta:
                    if (xr.Name == ElemGenerator)
                    {
                        this.ReadString(xr); // Ignore
                    }
                    else if (xr.Name == ElemHeaderHash)
                    {
                        string strHash = this.ReadString(xr);
                        if (!string.IsNullOrEmpty(strHash) && (this.m_pbHashOfHeader != null) && !this.m_bRepairMode)
                        {
                            byte[] pbHash = Convert.FromBase64String(strHash);
                            if (!MemUtil.ArraysEqual(pbHash, this.m_pbHashOfHeader))
                            {
                                throw new IOException(KLRes.FileCorrupted);
                            }
                        }
                    }
                    else if (xr.Name == ElemDbName)
                    {
                        this.m_pwDatabase.Name = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemDbNameChanged)
                    {
                        this.m_pwDatabase.NameChanged = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemDbDesc)
                    {
                        this.m_pwDatabase.Description = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemDbDescChanged)
                    {
                        this.m_pwDatabase.DescriptionChanged = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemDbDefaultUser)
                    {
                        this.m_pwDatabase.DefaultUserName = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemDbDefaultUserChanged)
                    {
                        this.m_pwDatabase.DefaultUserNameChanged = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemDbMntncHistoryDays)
                    {
                        this.m_pwDatabase.MaintenanceHistoryDays = this.ReadUInt(xr, 365);
                    }
                    else if (xr.Name == ElemDbColor)
                    {
                        string strColor = this.ReadString(xr);
                        if (!string.IsNullOrEmpty(strColor))
                        {
                            this.m_pwDatabase.Color = strColor;
                        }
                    }
                    else if (xr.Name == ElemDbKeyChanged)
                    {
                        this.m_pwDatabase.MasterKeyChanged = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemDbKeyChangeRec)
                    {
                        this.m_pwDatabase.MasterKeyChangeRec = this.ReadLong(xr, -1);
                    }
                    else if (xr.Name == ElemDbKeyChangeForce)
                    {
                        this.m_pwDatabase.MasterKeyChangeForce = this.ReadLong(xr, -1);
                    }
                    else if (xr.Name == ElemMemoryProt)
                    {
                        return SwitchContext(ctx, KdbContext.MemoryProtection, xr);
                    }
                    else if (xr.Name == ElemCustomIcons)
                    {
                        return SwitchContext(ctx, KdbContext.CustomIcons, xr);
                    }
                    else if (xr.Name == ElemRecycleBinEnabled)
                    {
                        this.m_pwDatabase.RecycleBinEnabled = this.ReadBool(xr, true);
                    }
                    else if (xr.Name == ElemRecycleBinUuid)
                    {
                        this.m_pwDatabase.RecycleBinUuid = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemRecycleBinChanged)
                    {
                        this.m_pwDatabase.RecycleBinChanged = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemEntryTemplatesGroup)
                    {
                        this.m_pwDatabase.EntryTemplatesGroup = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemEntryTemplatesGroupChanged)
                    {
                        this.m_pwDatabase.EntryTemplatesGroupChanged = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemHistoryMaxItems)
                    {
                        this.m_pwDatabase.HistoryMaxItems = this.ReadInt(xr, -1);
                    }
                    else if (xr.Name == ElemHistoryMaxSize)
                    {
                        this.m_pwDatabase.HistoryMaxSize = this.ReadLong(xr, -1);
                    }
                    else if (xr.Name == ElemLastSelectedGroup)
                    {
                        this.m_pwDatabase.LastSelectedGroup = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemLastTopVisibleGroup)
                    {
                        this.m_pwDatabase.LastTopVisibleGroup = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemBinaries)
                    {
                        return SwitchContext(ctx, KdbContext.Binaries, xr);
                    }
                    else if (xr.Name == ElemCustomData)
                    {
                        return SwitchContext(ctx, KdbContext.CustomData, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.MemoryProtection:
                    if (xr.Name == ElemProtTitle)
                    {
                        this.m_pwDatabase.MemoryProtection.ProtectTitle = this.ReadBool(xr, false);
                    }
                    else if (xr.Name == ElemProtUserName)
                    {
                        this.m_pwDatabase.MemoryProtection.ProtectUserName = this.ReadBool(xr, false);
                    }
                    else if (xr.Name == ElemProtPassword)
                    {
                        this.m_pwDatabase.MemoryProtection.ProtectPassword = this.ReadBool(xr, true);
                    }
                    else if (xr.Name == ElemProtUrl)
                    {
                        this.m_pwDatabase.MemoryProtection.ProtectUrl = this.ReadBool(xr, false);
                    }
                    else if (xr.Name == ElemProtNotes)
                    {
                        this.m_pwDatabase.MemoryProtection.ProtectNotes = this.ReadBool(xr, false);
                    }

                        // else if(xr.Name == ElemProtAutoHide)
                        // 	m_pwDatabase.MemoryProtection.AutoEnableVisualHiding = ReadBool(xr, true);
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.CustomIcons:
                    if (xr.Name == ElemCustomIconItem)
                    {
                        return SwitchContext(ctx, KdbContext.CustomIcon, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.CustomIcon:
                    if (xr.Name == ElemCustomIconItemID)
                    {
                        this.m_uuidCustomIconID = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemCustomIconItemData)
                    {
                        string strData = this.ReadString(xr);
                        if (!string.IsNullOrEmpty(strData))
                        {
                            this.m_pbCustomIconData = Convert.FromBase64String(strData);
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.Binaries:
                    if (xr.Name == ElemBinary)
                    {
                        // old code:
                        /*if (xr.MoveToAttribute(AttrId))
                        {
                            string strKey = xr.Value;
                            ProtectedBinary pbData = this.ReadProtectedBinary(xr);

                            this.m_dictBinPool[strKey ?? string.Empty] = pbData;
                        }
                        else
                        {
                            this.ReadUnknown(xr);
                        }*/

                        this.ReadUnknown(xr);
                        xr.ReadEndElement();
                        this.m_bReadNextNode = false;
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.CustomData:
                    if (xr.Name == ElemStringDictExItem)
                    {
                        return SwitchContext(ctx, KdbContext.CustomDataItem, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.CustomDataItem:
                    if (xr.Name == ElemKey)
                    {
                        this.m_strCustomDataKey = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemValue)
                    {
                        this.m_strCustomDataValue = this.ReadString(xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.Root:
                    if (xr.Name == ElemGroup)
                    {
                        Debug.Assert(this.m_ctxGroups.Count == 0);
                        if (this.m_ctxGroups.Count != 0)
                        {
                            throw new FormatException();
                        }

                        this.m_pwDatabase.RootGroup = new PwGroup(false, false);
                        this.m_ctxGroups.Push(this.m_pwDatabase.RootGroup);
                        this.m_ctxGroup = this.m_ctxGroups.Peek();

                        return SwitchContext(ctx, KdbContext.Group, xr);
                    }
                    else if (xr.Name == ElemDeletedObjects)
                    {
                        return SwitchContext(ctx, KdbContext.RootDeletedObjects, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.Group:
                    if (xr.Name == ElemUuid)
                    {
                        this.m_ctxGroup.Uuid = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemName)
                    {
                        this.m_ctxGroup.Name = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemNotes)
                    {
                        this.m_ctxGroup.Notes = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemIcon)
                    {
                        this.m_ctxGroup.IconId = (PwIcon)this.ReadInt(xr, (int)PwIcon.Folder);
                    }
                    else if (xr.Name == ElemCustomIconID)
                    {
                        this.m_ctxGroup.CustomIconUuid = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemTimes)
                    {
                        return SwitchContext(ctx, KdbContext.GroupTimes, xr);
                    }
                    else if (xr.Name == ElemIsExpanded)
                    {
                        this.m_ctxGroup.IsExpanded = this.ReadBool(xr, true);
                    }
                    else if (xr.Name == ElemGroupDefaultAutoTypeSeq)
                    {
                        this.m_ctxGroup.DefaultAutoTypeSequence = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemEnableAutoType)
                    {
                        this.m_ctxGroup.EnableAutoType = StrUtil.StringToBoolEx(this.ReadString(xr));
                    }
                    else if (xr.Name == ElemEnableSearching)
                    {
                        this.m_ctxGroup.EnableSearching = StrUtil.StringToBoolEx(this.ReadString(xr));
                    }
                    else if (xr.Name == ElemLastTopVisibleEntry)
                    {
                        this.m_ctxGroup.LastTopVisibleEntry = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemGroup)
                    {
                        this.m_ctxGroup = new PwGroup(false, false);
                        this.m_ctxGroups.Peek().AddGroup(this.m_ctxGroup, true);

                        this.m_ctxGroups.Push(this.m_ctxGroup);

                        return SwitchContext(ctx, KdbContext.Group, xr);
                    }
                    else if (xr.Name == ElemEntry)
                    {
                        this.m_ctxEntry = new PwEntry(false, false);
                        this.m_ctxGroup.AddEntry(this.m_ctxEntry, true);

                        this.m_bEntryInHistory = false;
                        return SwitchContext(ctx, KdbContext.Entry, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.Entry:
                    if (xr.Name == ElemUuid)
                    {
                        this.m_ctxEntry.Uuid = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemIcon)
                    {
                        this.m_ctxEntry.IconId = (PwIcon)this.ReadInt(xr, (int)PwIcon.Key);
                    }
                    else if (xr.Name == ElemCustomIconID)
                    {
                        this.m_ctxEntry.CustomIconUuid = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemFgColor)
                    {
                        string strColor = this.ReadString(xr);
                        if (!string.IsNullOrEmpty(strColor))
                        {
                            this.m_ctxEntry.ForegroundColor = strColor;
                        }
                    }
                    else if (xr.Name == ElemBgColor)
                    {
                        string strColor = this.ReadString(xr);
                        if (!string.IsNullOrEmpty(strColor))
                        {
                            this.m_ctxEntry.BackgroundColor = strColor;
                        }
                    }
                    else if (xr.Name == ElemOverrideUrl)
                    {
                        this.m_ctxEntry.OverrideUrl = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemTags)
                    {
                        this.m_ctxEntry.Tags = StrUtil.StringToTags(this.ReadString(xr));
                    }
                    else if (xr.Name == ElemTimes)
                    {
                        return SwitchContext(ctx, KdbContext.EntryTimes, xr);
                    }
                    else if (xr.Name == ElemString)
                    {
                        return SwitchContext(ctx, KdbContext.EntryString, xr);
                    }
                    else if (xr.Name == ElemBinary)
                    {
                        return SwitchContext(ctx, KdbContext.EntryBinary, xr);
                    }
                    else if (xr.Name == ElemAutoType)
                    {
                        return SwitchContext(ctx, KdbContext.EntryAutoType, xr);
                    }
                    else if (xr.Name == ElemHistory)
                    {
                        Debug.Assert(this.m_bEntryInHistory == false);

                        if (this.m_bEntryInHistory == false)
                        {
                            this.m_ctxHistoryBase = this.m_ctxEntry;
                            return SwitchContext(ctx, KdbContext.EntryHistory, xr);
                        }
                        else
                        {
                            this.ReadUnknown(xr);
                        }
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.GroupTimes:
                case KdbContext.EntryTimes:
                    ITimeLogger tl = (ctx == KdbContext.GroupTimes) ? (ITimeLogger)this.m_ctxGroup : (ITimeLogger)this.m_ctxEntry;
                    Debug.Assert(tl != null);

                    if (xr.Name == ElemCreationTime)
                    {
                        tl.CreationTime = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemLastModTime)
                    {
                        tl.LastModificationTime = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemLastAccessTime)
                    {
                        tl.LastAccessTime = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemExpiryTime)
                    {
                        tl.ExpiryTime = this.ReadTime(xr);
                    }
                    else if (xr.Name == ElemExpires)
                    {
                        tl.Expires = this.ReadBool(xr, false);
                    }
                    else if (xr.Name == ElemUsageCount)
                    {
                        tl.UsageCount = this.ReadULong(xr, 0);
                    }
                    else if (xr.Name == ElemLocationChanged)
                    {
                        tl.LocationChanged = this.ReadTime(xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.EntryString:
                    if (xr.Name == ElemKey)
                    {
                        this.m_ctxStringName = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemValue)
                    {
                            
                        this.m_ctxStringValue = this.ReadProtectedString(xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.EntryBinary:
                    if (xr.Name == ElemKey)
                    {
                        this.m_ctxBinaryName = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemValue)
                    {
                        // old code:
                        // this.m_ctxBinaryValue = this.ReadProtectedBinary(xr);

                        this.ReadUnknown(xr); 
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.EntryAutoType:
                    if (xr.Name == ElemAutoTypeEnabled)
                    {
                        this.m_ctxEntry.AutoType.Enabled = this.ReadBool(xr, true);
                    }
                    else if (xr.Name == ElemAutoTypeObfuscation)
                    {
                        this.m_ctxEntry.AutoType.ObfuscationOptions = (AutoTypeObfuscationOptions)this.ReadInt(xr, 0);
                    }
                    else if (xr.Name == ElemAutoTypeDefaultSeq)
                    {
                        this.m_ctxEntry.AutoType.DefaultSequence = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemAutoTypeItem)
                    {
                        return SwitchContext(ctx, KdbContext.EntryAutoTypeItem, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.EntryAutoTypeItem:
                    if (xr.Name == ElemWindow)
                    {
                        this.m_ctxATName = this.ReadString(xr);
                    }
                    else if (xr.Name == ElemKeystrokeSequence)
                    {
                        this.m_ctxATSeq = this.ReadString(xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.EntryHistory:
                    if (xr.Name == ElemEntry)
                    {
                        this.m_ctxEntry = new PwEntry(false, false);
                        this.m_ctxHistoryBase.History.Add(this.m_ctxEntry);

                        this.m_bEntryInHistory = true;
                        return SwitchContext(ctx, KdbContext.Entry, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.RootDeletedObjects:
                    if (xr.Name == ElemDeletedObject)
                    {
                        this.m_ctxDeletedObject = new PwDeletedObject();
                        this.m_pwDatabase.DeletedObjects.Add(this.m_ctxDeletedObject);

                        return SwitchContext(ctx, KdbContext.DeletedObject, xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                case KdbContext.DeletedObject:
                    if (xr.Name == ElemUuid)
                    {
                        this.m_ctxDeletedObject.Uuid = this.ReadUuid(xr);
                    }
                    else if (xr.Name == ElemDeletionTime)
                    {
                        this.m_ctxDeletedObject.DeletionTime = this.ReadTime(xr);
                    }
                    else
                    {
                        this.ReadUnknown(xr);
                    }

                    break;

                default:
                    this.ReadUnknown(xr);
                    break;
            }

            return ctx;
        }

        /// <summary>
        /// The read xml streamed.
        /// </summary>
        /// <param name="readerStream">
        /// The reader stream.
        /// </param>
        /// <param name="sParentStream">
        /// The s parent stream.
        /// </param>
        private void ReadXmlStreamed(Stream readerStream, Stream sParentStream, CancellationToken token)
        {
            this.ReadDocumentStreamed(CreateXmlReader(readerStream), sParentStream, token);
        }

        #endregion
    }
}