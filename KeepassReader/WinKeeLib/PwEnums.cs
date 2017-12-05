// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PwEnums.cs" company="">
//   
// </copyright>
// <summary>
//   Compression algorithm specifiers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib
{
    using System;

    /// <summary>
    /// Compression algorithm specifiers.
    /// </summary>
    public enum PwCompressionAlgorithm
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0, 

        /// <summary>
        /// GZip compression.
        /// </summary>
        GZip = 1, 

        /// <summary>
        /// Virtual field: currently known number of algorithms. Should not be used
        /// by plugins or libraries -- it's used internally only.
        /// </summary>
        Count = 2
    }

    /// <summary>
    /// Tree traversal methods.
    /// </summary>
    public enum TraversalMethod
    {
        /// <summary>
        /// Don't traverse the tree.
        /// </summary>
        None = 0, 

        /// <summary>
        /// Traverse the tree in pre-order mode, i.e. first visit all items
        /// in the current node, then visit all subnodes.
        /// </summary>
        PreOrder = 1
    }

    /// <summary>
    /// Methods for merging password databases/entries.
    /// </summary>
    public enum PwMergeMethod
    {
        // Do not change the explicitly assigned values, otherwise
        // serialization (e.g. of Ecas triggers) breaks
        /// <summary>
        /// The none.
        /// </summary>
        None = 0, 

        /// <summary>
        /// The overwrite existing.
        /// </summary>
        OverwriteExisting = 1, 

        /// <summary>
        /// The keep existing.
        /// </summary>
        KeepExisting = 2, 

        /// <summary>
        /// The overwrite if newer.
        /// </summary>
        OverwriteIfNewer = 3, 

        /// <summary>
        /// The create new uuids.
        /// </summary>
        CreateNewUuids = 4, 

        /// <summary>
        /// The synchronize.
        /// </summary>
        Synchronize = 5
    }

    /// <summary>
    /// Icon identifiers for groups and password entries.
    /// </summary>
    public enum PwIcon
    {
        /// <summary>
        /// The key.
        /// </summary>
        Key = 0, 

        /// <summary>
        /// The world.
        /// </summary>
        World, 

        /// <summary>
        /// The warning.
        /// </summary>
        Warning, 

        /// <summary>
        /// The network server.
        /// </summary>
        NetworkServer, 

        /// <summary>
        /// The marked directory.
        /// </summary>
        MarkedDirectory, 

        /// <summary>
        /// The user communication.
        /// </summary>
        UserCommunication, 

        /// <summary>
        /// The parts.
        /// </summary>
        Parts, 

        /// <summary>
        /// The notepad.
        /// </summary>
        Notepad, 

        /// <summary>
        /// The world socket.
        /// </summary>
        WorldSocket, 

        /// <summary>
        /// The identity.
        /// </summary>
        Identity, 

        /// <summary>
        /// The paper ready.
        /// </summary>
        PaperReady, 

        /// <summary>
        /// The digicam.
        /// </summary>
        Digicam, 

        /// <summary>
        /// The ir communication.
        /// </summary>
        IRCommunication, 

        /// <summary>
        /// The multi keys.
        /// </summary>
        MultiKeys, 

        /// <summary>
        /// The energy.
        /// </summary>
        Energy, 

        /// <summary>
        /// The scanner.
        /// </summary>
        Scanner, 

        /// <summary>
        /// The world star.
        /// </summary>
        WorldStar, 

        /// <summary>
        /// The cd rom.
        /// </summary>
        CDRom, 

        /// <summary>
        /// The monitor.
        /// </summary>
        Monitor, 

        /// <summary>
        /// The e mail.
        /// </summary>
        EMail, 

        /// <summary>
        /// The configuration.
        /// </summary>
        Configuration, 

        /// <summary>
        /// The clipboard ready.
        /// </summary>
        ClipboardReady, 

        /// <summary>
        /// The paper new.
        /// </summary>
        PaperNew, 

        /// <summary>
        /// The screen.
        /// </summary>
        Screen, 

        /// <summary>
        /// The energy careful.
        /// </summary>
        EnergyCareful, 

        /// <summary>
        /// The e mail box.
        /// </summary>
        EMailBox, 

        /// <summary>
        /// The disk.
        /// </summary>
        Disk, 

        /// <summary>
        /// The drive.
        /// </summary>
        Drive, 

        /// <summary>
        /// The paper q.
        /// </summary>
        PaperQ, 

        /// <summary>
        /// The terminal encrypted.
        /// </summary>
        TerminalEncrypted, 

        /// <summary>
        /// The console.
        /// </summary>
        Console, 

        /// <summary>
        /// The printer.
        /// </summary>
        Printer, 

        /// <summary>
        /// The program icons.
        /// </summary>
        ProgramIcons, 

        /// <summary>
        /// The run.
        /// </summary>
        Run, 

        /// <summary>
        /// The settings.
        /// </summary>
        Settings, 

        /// <summary>
        /// The world computer.
        /// </summary>
        WorldComputer, 

        /// <summary>
        /// The archive.
        /// </summary>
        Archive, 

        /// <summary>
        /// The homebanking.
        /// </summary>
        Homebanking, 

        /// <summary>
        /// The drive windows.
        /// </summary>
        DriveWindows, 

        /// <summary>
        /// The clock.
        /// </summary>
        Clock, 

        /// <summary>
        /// The e mail search.
        /// </summary>
        EMailSearch, 

        /// <summary>
        /// The paper flag.
        /// </summary>
        PaperFlag, 

        /// <summary>
        /// The memory.
        /// </summary>
        Memory, 

        /// <summary>
        /// The trash bin.
        /// </summary>
        TrashBin, 

        /// <summary>
        /// The note.
        /// </summary>
        Note, 

        /// <summary>
        /// The expired.
        /// </summary>
        Expired, 

        /// <summary>
        /// The info.
        /// </summary>
        Info, 

        /// <summary>
        /// The package.
        /// </summary>
        Package, 

        /// <summary>
        /// The folder.
        /// </summary>
        Folder, 

        /// <summary>
        /// The folder open.
        /// </summary>
        FolderOpen, 

        /// <summary>
        /// The folder package.
        /// </summary>
        FolderPackage, 

        /// <summary>
        /// The lock open.
        /// </summary>
        LockOpen, 

        /// <summary>
        /// The paper locked.
        /// </summary>
        PaperLocked, 

        /// <summary>
        /// The checked.
        /// </summary>
        Checked, 

        /// <summary>
        /// The pen.
        /// </summary>
        Pen, 

        /// <summary>
        /// The thumbnail.
        /// </summary>
        Thumbnail, 

        /// <summary>
        /// The book.
        /// </summary>
        Book, 

        /// <summary>
        /// The list.
        /// </summary>
        List, 

        /// <summary>
        /// The user key.
        /// </summary>
        UserKey, 

        /// <summary>
        /// The tool.
        /// </summary>
        Tool, 

        /// <summary>
        /// The home.
        /// </summary>
        Home, 

        /// <summary>
        /// The star.
        /// </summary>
        Star, 

        /// <summary>
        /// The tux.
        /// </summary>
        Tux, 

        /// <summary>
        /// The feather.
        /// </summary>
        Feather, 

        /// <summary>
        /// The apple.
        /// </summary>
        Apple, 

        /// <summary>
        /// The wiki.
        /// </summary>
        Wiki, 

        /// <summary>
        /// The money.
        /// </summary>
        Money, 

        /// <summary>
        /// The certificate.
        /// </summary>
        Certificate, 

        /// <summary>
        /// The black berry.
        /// </summary>
        BlackBerry, 

        /// <summary>
        /// Virtual identifier -- represents the number of icons.
        /// </summary>
        Count
    }

    /// <summary>
    /// The proxy server type.
    /// </summary>
    public enum ProxyServerType
    {
        /// <summary>
        /// The none.
        /// </summary>
        None = 0, 

        /// <summary>
        /// The system.
        /// </summary>
        System = 1, 

        /// <summary>
        /// The manual.
        /// </summary>
        Manual = 2
    }

    /// <summary>
    /// Comparison modes for in-memory protected objects.
    /// </summary>
    public enum MemProtCmpMode
    {
        /// <summary>
        /// Ignore the in-memory protection states.
        /// </summary>
        None = 0, 

        /// <summary>
        /// Ignore the in-memory protection states of standard
        /// objects; do compare in-memory protection states of
        /// custom objects.
        /// </summary>
        CustomOnly, 

        /// <summary>
        /// Compare in-memory protection states.
        /// </summary>
        Full
    }

    /// <summary>
    /// The pw compare options.
    /// </summary>
    [Flags]
    public enum PwCompareOptions
    {
        /// <summary>
        /// The none.
        /// </summary>
        None = 0x0, 

        /// <summary>
        /// Empty standard string fields are considered to be the
        /// same as non-existing standard string fields.
        /// This doesn't affect custom string comparisons.
        /// </summary>
        NullEmptyEquivStd = 0x1, 

        /// <summary>
        /// The ignore parent group.
        /// </summary>
        IgnoreParentGroup = 0x2, 

        /// <summary>
        /// The ignore last access.
        /// </summary>
        IgnoreLastAccess = 0x4, 

        /// <summary>
        /// The ignore last mod.
        /// </summary>
        IgnoreLastMod = 0x8, 

        /// <summary>
        /// The ignore history.
        /// </summary>
        IgnoreHistory = 0x10, 

        /// <summary>
        /// The ignore last backup.
        /// </summary>
        IgnoreLastBackup = 0x20, 

        // For groups:
        /// <summary>
        /// The properties only.
        /// </summary>
        PropertiesOnly = 0x40, 

        /// <summary>
        /// The ignore times.
        /// </summary>
        IgnoreTimes = IgnoreLastAccess | IgnoreLastMod
    }

    /// <summary>
    /// The io access type.
    /// </summary>
    public enum IOAccessType
    {
        /// <summary>
        /// The none.
        /// </summary>
        None = 0, 

        /// <summary>
        /// The IO connection is being opened for reading.
        /// </summary>
        Read = 1, 

        /// <summary>
        /// The IO connection is being opened for writing.
        /// </summary>
        Write = 2, 

        /// <summary>
        /// The IO connection is being opened for testing
        /// whether a file/object exists.
        /// </summary>
        Exists = 3, 

        /// <summary>
        /// The IO connection is being opened for deleting a file/object.
        /// </summary>
        Delete = 4, 

        /// <summary>
        /// The IO connection is being opened for renaming/moving a file/object.
        /// </summary>
        Move = 5
    }

    // public enum PwLogicalOp
    // {
    // 	None = 0,
    // 	Or = 1,
    // 	And = 2,
    // 	NOr = 3,
    // 	NAnd = 4
    // }

    /// <summary>
    /// The app run flags.
    /// </summary>
    [Flags]
    public enum AppRunFlags
    {
        /// <summary>
        /// The none.
        /// </summary>
        None = 0, 

        /// <summary>
        /// The get std output.
        /// </summary>
        GetStdOutput = 1, 

        /// <summary>
        /// The wait for exit.
        /// </summary>
        WaitForExit = 2, 

        // This flag prevents any handles being garbage-collected
        // before the started process has terminated, without
        // blocking the current thread;
        // https://sourceforge.net/p/keepass/patches/84/
        /// <summary>
        /// The gc keep alive.
        /// </summary>
        GCKeepAlive = 4, 

        // https://sourceforge.net/p/keepass/patches/85/
        /// <summary>
        /// The do events.
        /// </summary>
        DoEvents = 8, 

        /// <summary>
        /// The disable forms.
        /// </summary>
        DisableForms = 16
    }
}