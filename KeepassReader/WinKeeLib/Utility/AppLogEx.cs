// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppLogEx.cs" company="">
//   
// </copyright>
// <summary>
//   Application-wide logging services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if !KeePassLibSD

#endif

namespace KeePassLib.Utility
{
    using System;
    using System.IO;

    /// <summary>
    /// Application-wide logging services.
    /// </summary>
    public static class AppLogEx
    {
        #region Static Fields

        /// <summary>
        /// The m_sw out.
        /// </summary>
        private static StreamWriter m_swOut = null;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The close.
        /// </summary>
        public static void Close()
        {
            if (m_swOut == null)
            {
                return;
            }

            m_swOut.Dispose();
            m_swOut = null;
        }

        /// <summary>
        /// The log.
        /// </summary>
        /// <param name="strText">
        /// The str text.
        /// </param>
        public static void Log(string strText)
        {
            if (m_swOut == null)
            {
                return;
            }

            if (strText == null)
            {
                m_swOut.WriteLine();
            }
            else
            {
                m_swOut.WriteLine(strText);
            }
        }

        /// <summary>
        /// The log.
        /// </summary>
        /// <param name="ex">
        /// The ex.
        /// </param>
        public static void Log(Exception ex)
        {
            if (m_swOut == null)
            {
                return;
            }

            if (ex == null)
            {
                m_swOut.WriteLine();
            }
            else
            {
                m_swOut.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// The open.
        /// </summary>
        /// <param name="strPrefix">
        /// The str prefix.
        /// </param>
        public static void Open(string strPrefix)
        {
            return; // Logging is not enabled in normal builds of KeePass!

            /*
			AppLogEx.Close();

			Debug.Assert(strPrefix != null);
			if(strPrefix == null) strPrefix = "Log";

			try
			{
				string strDirSep = string.Empty;
				strDirSep += UrlUtil.LocalDirSepChar;

				string strTemp = UrlUtil.GetTempPath();
				if(!strTemp.EndsWith(strDirSep))
					strTemp += strDirSep;

				string strPath = strTemp + strPrefix + "-";
				Debug.Assert(strPath.IndexOf('/') < 0);

				DateTime dtNow = DateTime.Now;
				string strTime = dtNow.ToString("s");
				strTime = strTime.Replace('T', '-');
				strTime = strTime.Replace(':', '-');

				strPath += strTime + "-" + Environment.TickCount.ToString(
					NumberFormatInfo.InvariantInfo) + ".log.gz";

				FileStream fsOut = new FileStream(strPath, FileMode.Create,
					FileAccess.Write, FileShare.None);
				GZipStream gz = new GZipStream(fsOut, CompressionMode.Compress);
				m_swOut = new StreamWriter(gz);

				AppLogEx.Log("Started logging on " + dtNow.ToString("s") + ".");
			}
			catch(Exception) { Debug.Assert(false); }
			*/
        }

        #endregion
    }
}