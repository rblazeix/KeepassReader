// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTransactionEx.cs" company="">
//   
// </copyright>
// <summary>
//   The file transaction ex.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if (!KeePassLibSD && !KeePassRT)
using System.Security.AccessControl;
#endif

namespace KeePassLib.Serialization
{
    using System;

    using KeePassLib.Utility;

    /// <summary>
    /// The file transaction ex.
    /// </summary>
    public sealed class FileTransactionEx
    {
        #region Constants

        /// <summary>
        /// The str temp suffix.
        /// </summary>
        private const string StrTempSuffix = ".tmp";

        #endregion

        #region Fields

        /// <summary>
        /// The m_b made unhidden.
        /// </summary>
        private bool m_bMadeUnhidden = false;

        /// <summary>
        /// The m_b transacted.
        /// </summary>
        private bool m_bTransacted;

        /// <summary>
        /// The m_ioc base.
        /// </summary>
        private IOConnectionInfo m_iocBase;

        /// <summary>
        /// The m_ioc temp.
        /// </summary>
        private IOConnectionInfo m_iocTemp;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTransactionEx"/> class.
        /// </summary>
        /// <param name="iocBaseFile">
        /// The ioc base file.
        /// </param>
        public FileTransactionEx(IOConnectionInfo iocBaseFile)
        {
            this.Initialize(iocBaseFile, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTransactionEx"/> class.
        /// </summary>
        /// <param name="iocBaseFile">
        /// The ioc base file.
        /// </param>
        /// <param name="bTransacted">
        /// The b transacted.
        /// </param>
        public FileTransactionEx(IOConnectionInfo iocBaseFile, bool bTransacted)
        {
            this.Initialize(iocBaseFile, bTransacted);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The initialize.
        /// </summary>
        /// <param name="iocBaseFile">
        /// The ioc base file.
        /// </param>
        /// <param name="bTransacted">
        /// The b transacted.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        private void Initialize(IOConnectionInfo iocBaseFile, bool bTransacted)
        {
            if (iocBaseFile == null)
            {
                throw new ArgumentNullException("iocBaseFile");
            }

            this.m_bTransacted = bTransacted;
            this.m_iocBase = iocBaseFile.CloneDeep();

            // Prevent transactions for FTP URLs under .NET 4.0 in order to
            // avoid/workaround .NET bug 621450:
            // https://connect.microsoft.com/VisualStudio/feedback/details/621450/problem-renaming-file-on-ftp-server-using-ftpwebrequest-in-net-framework-4-0-vs2010-only
            if (this.m_iocBase.Path.StartsWith("ftp:", StrUtil.CaseIgnoreCmp))
            {
                this.m_bTransacted = false;
            }

            if (this.m_bTransacted)
            {
                this.m_iocTemp = this.m_iocBase.CloneDeep();
                this.m_iocTemp.Path += StrTempSuffix;
            }
            else
            {
                this.m_iocTemp = this.m_iocBase;
            }
        }

        #endregion
    }
}