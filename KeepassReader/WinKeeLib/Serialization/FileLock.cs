// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileLock.cs" company="">
//   
// </copyright>
// <summary>
//   The file lock exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Serialization
{
    using System;
    using System.Diagnostics;
    using System.Text;

    using KeePassLib.Resources;
    using KeePassLib.Utility;

    /// <summary>
    /// The file lock exception.
    /// </summary>
    public sealed class FileLockException : Exception
    {
        #region Fields

        /// <summary>
        /// The m_str msg.
        /// </summary>
        private readonly string m_strMsg;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLockException"/> class.
        /// </summary>
        /// <param name="strBaseFile">
        /// The str base file.
        /// </param>
        /// <param name="strUser">
        /// The str user.
        /// </param>
        public FileLockException(string strBaseFile, string strUser)
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(strBaseFile))
            {
                sb.Append(strBaseFile);
                sb.Append(Environment.NewLine);
            }

            sb.Append(KLRes.FileLockedWrite);
            sb.Append(Environment.NewLine);

            if (!string.IsNullOrEmpty(strUser))
            {
                sb.Append(strUser);
            }
            else
            {
                sb.Append("?");
            }

            sb.Append(Environment.NewLine);
            sb.Append(KLRes.TryAgainSecs);

            this.m_strMsg = sb.ToString();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return this.m_strMsg;
            }
        }

        #endregion
    }

    /// <summary>
    /// The file lock.
    /// </summary>
    public sealed class FileLock : IDisposable
    {
        #region Constants

        /// <summary>
        /// The lock file ext.
        /// </summary>
        private const string LockFileExt = ".lock";

        /// <summary>
        /// The lock file header.
        /// </summary>
        private const string LockFileHeader = "KeePass Lock File";

        #endregion

        #region Fields

        /// <summary>
        /// The m_ioc lock file.
        /// </summary>
        private IOConnectionInfo m_iocLockFile;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Finalizes an instance of the <see cref="FileLock"/> class. 
        /// </summary>
        ~FileLock()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="bDisposing">
        /// The b disposing.
        /// </param>
        private void Dispose(bool bDisposing)
        {
            if (this.m_iocLockFile == null)
            {
                return;
            }

            bool bFileDeleted = false;
            for (int r = 0; r < 5; ++r)
            {
                // if(!OwnLockFile()) { bFileDeleted = true; break; }
                try
                {
                    bFileDeleted = true;
                }
                catch (Exception)
                {
                    Debug.Assert(false);
                }

                if (bFileDeleted)
                {
                    break;
                }
            }

            this.m_iocLockFile = null;
        }

        #endregion

        /// <summary>
        /// The lock file info.
        /// </summary>
        private sealed class LockFileInfo
        {
            #region Fields

            /// <summary>
            /// The domain.
            /// </summary>
            public readonly string Domain;

            /// <summary>
            /// The id.
            /// </summary>
            public readonly string ID;

            /// <summary>
            /// The machine.
            /// </summary>
            public readonly string Machine;

            /// <summary>
            /// The time.
            /// </summary>
            public readonly DateTime Time;

            /// <summary>
            /// The user name.
            /// </summary>
            public readonly string UserName;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="LockFileInfo"/> class.
            /// </summary>
            /// <param name="strID">
            /// The str id.
            /// </param>
            /// <param name="strTime">
            /// The str time.
            /// </param>
            /// <param name="strUserName">
            /// The str user name.
            /// </param>
            /// <param name="strMachine">
            /// The str machine.
            /// </param>
            /// <param name="strDomain">
            /// The str domain.
            /// </param>
            private LockFileInfo(string strID, string strTime, string strUserName, string strMachine, string strDomain)
            {
                this.ID = (strID ?? string.Empty).Trim();

                DateTime dt;
                if (TimeUtil.TryDeserializeUtc(strTime.Trim(), out dt))
                {
                    this.Time = dt;
                }
                else
                {
                    Debug.Assert(false);
                    this.Time = DateTime.UtcNow;
                }

                this.UserName = (strUserName ?? string.Empty).Trim();
                this.Machine = (strMachine ?? string.Empty).Trim();
                this.Domain = (strDomain ?? string.Empty).Trim();

                if (this.Domain.Equals(this.Machine, StrUtil.CaseIgnoreCmp))
                {
                    this.Domain = string.Empty;
                }
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The get owner.
            /// </summary>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public string GetOwner()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append((this.UserName.Length > 0) ? this.UserName : "?");

                bool bMachine = this.Machine.Length > 0;
                bool bDomain = this.Domain.Length > 0;
                if (bMachine || bDomain)
                {
                    sb.Append(" (");
                    sb.Append(this.Machine);
                    if (bMachine && bDomain)
                    {
                        sb.Append(" @ ");
                    }

                    sb.Append(this.Domain);
                    sb.Append(")");
                }

                return sb.ToString();
            }

            #endregion
        }

        // private bool OwnLockFile()
        // {
        // 	if(m_iocLockFile == null) { Debug.Assert(false); return false; }
        // 	if(m_strLockID == null) { Debug.Assert(false); return false; }
        // 	LockFileInfo lfi = LockFileInfo.Load(m_iocLockFile);
        // 	if(lfi == null) return false;
        // 	return m_strLockID.Equals(lfi.ID);
        // }
    }
}