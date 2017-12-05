// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStatusLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Status message types.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Interfaces
{
    /// <summary>
    /// Status message types.
    /// </summary>
    public enum LogStatusType
    {
        /// <summary>
        /// Default type: simple information type.
        /// </summary>
        Info = 0, 

        /// <summary>
        /// Warning message.
        /// </summary>
        Warning, 

        /// <summary>
        /// Error message.
        /// </summary>
        Error, 

        /// <summary>
        /// Additional information. Depends on lines above.
        /// </summary>
        AdditionalInfo
    }

    /// <summary>
    /// Status logging interface.
    /// </summary>
    public interface IStatusLogger
    {
        #region Public Methods and Operators

        /// <summary>
        /// Check if the user cancelled the current work.
        /// </summary>
        /// <returns>Returns <c>true</c> if the caller should continue
        /// the current work.</returns>
        bool ContinueWork();

        /// <summary>
        /// Function which needs to be called when logging is ended
        /// (i.e. when no more messages will be logged and when the
        /// percent value won't change any more).
        /// </summary>
        void EndLogging();

        /// <summary>
        /// Set the current progress in percent.
        /// </summary>
        /// <param name="uPercent">
        /// Percent of work finished.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the caller should continue
        /// the current work.
        /// </returns>
        bool SetProgress(uint uPercent);

        /// <summary>
        /// Set the current status text.
        /// </summary>
        /// <param name="strNewText">
        /// Status text.
        /// </param>
        /// <param name="lsType">
        /// Type of the message.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the caller should continue
        /// the current work.
        /// </returns>
        bool SetText(string strNewText, LogStatusType lsType);

        /// <summary>
        /// Function which needs to be called when logging is started.
        /// </summary>
        /// <param name="strOperation">
        /// This string should roughly describe
        /// the operation, of which the status is logged.
        /// </param>
        /// <param name="bWriteOperationToLog">
        /// Specifies whether the
        /// operation is written to the log or not.
        /// </param>
        void StartLogging(string strOperation, bool bWriteOperationToLog);

        #endregion
    }

    /// <summary>
    /// The null status logger.
    /// </summary>
    public sealed class NullStatusLogger : IStatusLogger
    {
        #region Public Methods and Operators

        /// <summary>
        /// The continue work.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ContinueWork()
        {
            return true;
        }

        /// <summary>
        /// The end logging.
        /// </summary>
        public void EndLogging()
        {
        }

        /// <summary>
        /// The set progress.
        /// </summary>
        /// <param name="uPercent">
        /// The u percent.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool SetProgress(uint uPercent)
        {
            return true;
        }

        /// <summary>
        /// The set text.
        /// </summary>
        /// <param name="strNewText">
        /// The str new text.
        /// </param>
        /// <param name="lsType">
        /// The ls type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool SetText(string strNewText, LogStatusType lsType)
        {
            return true;
        }

        /// <summary>
        /// The start logging.
        /// </summary>
        /// <param name="strOperation">
        /// The str operation.
        /// </param>
        /// <param name="bWriteOperationToLog">
        /// The b write operation to log.
        /// </param>
        public void StartLogging(string strOperation, bool bWriteOperationToLog)
        {
        }

        #endregion
    }
}