// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UrlUtil.cs" company="">
//   
// </copyright>
// <summary>
//   A class containing various static path utility helper methods (like
//   stripping extension from a file, etc.).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// A class containing various static path utility helper methods (like
    /// stripping extension from a file, etc.).
    /// </summary>
    public static class UrlUtil
    {
        #region Static Fields

        /// <summary>
        /// The m_v dir seps.
        /// </summary>
        private static readonly char[] m_vDirSeps = new[] { '\\', '/', UrlUtil.LocalDirSepChar };

        /// <summary>
        /// The m_v path trim chars ws.
        /// </summary>
        private static readonly char[] m_vPathTrimCharsWs = new[] { '\"', ' ', '\t', '\r', '\n' };

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the local dir sep char.
        /// </summary>
        public static char LocalDirSepChar
        {
#if KeePassRT
            get
            {
                return '\\';
            }

#else
			get { return Path.DirectorySeparatorChar; }
#endif
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The assembly equals.
        /// </summary>
        /// <param name="strExt">
        /// The str ext.
        /// </param>
        /// <param name="strShort">
        /// The str short.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool AssemblyEquals(string strExt, string strShort)
        {
            if ((strExt == null) || (strShort == null))
            {
                Debug.Assert(false);
                return false;
            }

            if (strExt.Equals(strShort, StrUtil.CaseIgnoreCmp) || strExt.StartsWith(strShort + ",", StrUtil.CaseIgnoreCmp))
            {
                return true;
            }

            if (!strShort.EndsWith(".dll", StrUtil.CaseIgnoreCmp))
            {
                if (strExt.Equals(strShort + ".dll", StrUtil.CaseIgnoreCmp) || strExt.StartsWith(strShort + ".dll,", StrUtil.CaseIgnoreCmp))
                {
                    return true;
                }
            }

            if (!strShort.EndsWith(".exe", StrUtil.CaseIgnoreCmp))
            {
                if (strExt.Equals(strShort + ".exe", StrUtil.CaseIgnoreCmp) || strExt.StartsWith(strShort + ".exe,", StrUtil.CaseIgnoreCmp))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The convert separators.
        /// </summary>
        /// <param name="strPath">
        /// The str path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ConvertSeparators(string strPath)
        {
            return ConvertSeparators(strPath, UrlUtil.LocalDirSepChar);
        }

        /// <summary>
        /// The convert separators.
        /// </summary>
        /// <param name="strPath">
        /// The str path.
        /// </param>
        /// <param name="chSeparator">
        /// The ch separator.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ConvertSeparators(string strPath, char chSeparator)
        {
            if (string.IsNullOrEmpty(strPath))
            {
                return string.Empty;
            }

            strPath = strPath.Replace('/', chSeparator);
            strPath = strPath.Replace('\\', chSeparator);

            return strPath;
        }

        /// <summary>
        /// Ensure that a path is terminated with a directory separator character.
        /// </summary>
        /// <param name="strPath">
        /// Input path.
        /// </param>
        /// <param name="bUrl">
        /// If <c>true</c>, a slash (<c>/</c>) is appended to
        /// the string if it's not terminated already. If <c>false</c>, the
        /// default system directory separator character is used.
        /// </param>
        /// <returns>
        /// Path having a directory separator as last character.
        /// </returns>
        public static string EnsureTerminatingSeparator(string strPath, bool bUrl)
        {
            Debug.Assert(strPath != null);
            if (strPath == null)
            {
                throw new ArgumentNullException("strPath");
            }

            int nLength = strPath.Length;
            if (nLength <= 0)
            {
                return string.Empty;
            }

            char chLast = strPath[nLength - 1];

            for (int i = 0; i < m_vDirSeps.Length; ++i)
            {
                if (chLast == m_vDirSeps[i])
                {
                    return strPath;
                }
            }

            if (bUrl)
            {
                return strPath + '/';
            }

            return strPath + UrlUtil.LocalDirSepChar;
        }

        /// <summary>
        /// The file url to path.
        /// </summary>
        /// <param name="strUrl">
        /// The str url.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static string FileUrlToPath(string strUrl)
        {
            Debug.Assert(strUrl != null);
            if (strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }

            string str = strUrl;
            if (str.StartsWith(@"file:///", StrUtil.CaseIgnoreCmp))
            {
                str = str.Substring(8, str.Length - 8);
            }

            str = str.Replace('/', UrlUtil.LocalDirSepChar);

            return str;
        }

        /// <summary>
        /// The filter file name.
        /// </summary>
        /// <param name="strName">
        /// The str name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string FilterFileName(string strName)
        {
            if (strName == null)
            {
                Debug.Assert(false);
                return string.Empty;
            }

            string str = strName;

            str = str.Replace('/', '-');
            str = str.Replace('\\', '-');
            str = str.Replace(":", string.Empty);
            str = str.Replace("*", string.Empty);
            str = str.Replace("?", string.Empty);
            str = str.Replace("\"", string.Empty);
            str = str.Replace(@"'", string.Empty);
            str = str.Replace('<', '(');
            str = str.Replace('>', ')');
            str = str.Replace('|', '-');

            return str;
        }

        /// <summary>
        /// Get the extension of a file.
        /// </summary>
        /// <param name="strPath">
        /// Full path of a file with extension.
        /// </param>
        /// <returns>
        /// Extension without prepending dot.
        /// </returns>
        public static string GetExtension(string strPath)
        {
            Debug.Assert(strPath != null);
            if (strPath == null)
            {
                throw new ArgumentNullException("strPath");
            }

            int nLastDirSep = strPath.LastIndexOfAny(m_vDirSeps);
            int nLastExtDot = strPath.LastIndexOf('.');

            if (nLastExtDot <= nLastDirSep)
            {
                return string.Empty;
            }

            if (nLastExtDot == (strPath.Length - 1))
            {
                return string.Empty;
            }

            return strPath.Substring(nLastExtDot + 1);
        }

        /// <summary>
        /// Get the directory (path) of a file name. The returned string is
        /// terminated by a directory separator character. Example:
        /// passing <c>C:\\My Documents\\My File.kdb</c> in <paramref name="strFile"/>
        /// would produce this string: <c>C:\\My Documents\\</c>.
        /// </summary>
        /// <param name="strFile">
        /// Full path of a file.
        /// </param>
        /// <param name="bAppendTerminatingChar">
        /// Append a terminating directory separator
        /// character to the returned path.
        /// </param>
        /// <param name="bEnsureValidDirSpec">
        /// If <c>true</c>, the returned path
        /// is guaranteed to be a valid directory path (for example <c>X:\\</c> instead
        /// of <c>X:</c>, overriding <paramref name="bAppendTerminatingChar"/>).
        /// This should only be set to <c>true</c>, if the returned path is directly
        /// passed to some directory API.
        /// </param>
        /// <returns>
        /// Directory of the file. The return value is an empty string
        /// (<c>""</c>) if the input parameter is <c>null</c>.
        /// </returns>
        public static string GetFileDirectory(string strFile, bool bAppendTerminatingChar, bool bEnsureValidDirSpec)
        {
            Debug.Assert(strFile != null);
            if (strFile == null)
            {
                throw new ArgumentNullException("strFile");
            }

            int nLastSep = strFile.LastIndexOfAny(m_vDirSeps);
            if (nLastSep < 0)
            {
                return strFile; // None
            }

            if (bEnsureValidDirSpec && (nLastSep == 2) && (strFile[1] == ':') && (strFile[2] == '\\'))
            {
                // Length >= 3 and Windows root directory
                bAppendTerminatingChar = true;
            }

            if (!bAppendTerminatingChar)
            {
                return strFile.Substring(0, nLastSep);
            }

            return EnsureTerminatingSeparator(strFile.Substring(0, nLastSep), false);
        }

        /// <summary>
        /// Gets the file name of the specified file (full path). Example:
        /// if <paramref name="strPath"/> is <c>C:\\My Documents\\My File.kdb</c>
        /// the returned string is <c>My File.kdb</c>.
        /// </summary>
        /// <param name="strPath">
        /// Full path of a file.
        /// </param>
        /// <returns>
        /// File name of the specified file. The return value is
        /// an empty string (<c>""</c>) if the input parameter is <c>null</c>.
        /// </returns>
        public static string GetFileName(string strPath)
        {
            Debug.Assert(strPath != null);
            if (strPath == null)
            {
                throw new ArgumentNullException("strPath");
            }

            int nLastSep = strPath.LastIndexOfAny(m_vDirSeps);

            if (nLastSep < 0)
            {
                return strPath;
            }

            if (nLastSep >= (strPath.Length - 1))
            {
                return string.Empty;
            }

            return strPath.Substring(nLastSep + 1);
        }

        /// <summary>
        /// Get the host component of an URL.
        /// This method is faster and more fault-tolerant than creating
        /// an 
        /// <code>
        /// Uri
        /// </code>
        /// object and querying its 
        /// <code>
        /// Host
        /// </code>
        /// property.
        /// </summary>
        /// <param name="strUrl">
        /// The str Url.
        /// </param>
        /// <example>
        /// For the input 
        /// <code>
        /// s://u:p@d.tld:p/p?q#f
        /// </code>
        /// the return
        /// value is 
        /// <code>
        /// d.tld
        /// </code>
        /// .
        /// </example>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetHost(string strUrl)
        {
            if (strUrl == null)
            {
                Debug.Assert(false);
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            bool bInExtHost = false;
            for (int i = 0; i < strUrl.Length; ++i)
            {
                char ch = strUrl[i];
                if (bInExtHost)
                {
                    if (ch == '/')
                    {
                        if (sb.Length == 0)
                        {
                        }
 // Ignore leading '/'s
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else
                {
                    // !bInExtHost
                    if (ch == ':')
                    {
                        bInExtHost = true;
                    }
                }
            }

            string str = sb.ToString();
            if (str.Length == 0)
            {
                str = strUrl;
            }

            // Remove the login part
            int nLoginLen = str.IndexOf('@');
            if (nLoginLen >= 0)
            {
                str = str.Substring(nLoginLen + 1);
            }

            // Remove the port
            int iPort = str.LastIndexOf(':');
            if (iPort >= 0)
            {
                str = str.Substring(0, iPort);
            }

            return str;
        }

        /* /// <summary>
		/// File access mode enumeration. Used by the <c>FileAccessible</c>
		/// method.
		/// </summary>
		public enum FileAccessMode
		{
			/// <summary>
			/// Opening a file in read mode. The specified file must exist.
			/// </summary>
			Read = 0,

			/// <summary>
			/// Opening a file in create mode. If the file exists already, it
			/// will be overwritten. If it doesn't exist, it will be created.
			/// The return value is <c>true</c>, if data can be written to the
			/// file.
			/// </summary>
			Create
		} */

        /* /// <summary>
		/// Test if a specified path is accessible, either in read or write mode.
		/// </summary>
		/// <param name="strFilePath">Path to test.</param>
		/// <param name="fMode">Requested file access mode.</param>
		/// <returns>Returns <c>true</c> if the specified path is accessible in
		/// the requested mode, otherwise the return value is <c>false</c>.</returns>
		public static bool FileAccessible(string strFilePath, FileAccessMode fMode)
		{
			Debug.Assert(strFilePath != null);
			if(strFilePath == null) throw new ArgumentNullException("strFilePath");

			if(fMode == FileAccessMode.Read)
			{
				FileStream fs;

				try { fs = File.OpenRead(strFilePath); }
				catch(Exception) { return false; }
				if(fs == null) return false;

				fs.Close();
				return true;
			}
			else if(fMode == FileAccessMode.Create)
			{
				FileStream fs;

				try { fs = File.Create(strFilePath); }
				catch(Exception) { return false; }
				if(fs == null) return false;

				fs.Close();
				return true;
			}

			return false;
		} */

        /// <summary>
        /// The get quoted app path.
        /// </summary>
        /// <param name="strPath">
        /// The str path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetQuotedAppPath(string strPath)
        {
            if (strPath == null)
            {
                Debug.Assert(false);
                return string.Empty;
            }

            // int nFirst = strPath.IndexOf('\"');
            // int nSecond = strPath.IndexOf('\"', nFirst + 1);
            // if((nFirst >= 0) && (nSecond >= 0))
            // 	return strPath.Substring(nFirst + 1, nSecond - nFirst - 1);
            // return strPath;
            string str = strPath.Trim();
            if (str.Length <= 1)
            {
                return str;
            }

            if (str[0] != '\"')
            {
                return str;
            }

            int iSecond = str.IndexOf('\"', 1);
            if (iSecond <= 0)
            {
                return str;
            }

            return str.Substring(1, iSecond - 1);
        }

        /// <summary>
        /// The get shortest absolute path.
        /// </summary>
        /// <param name="strPath">
        /// The str path.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static string GetShortestAbsolutePath(string strPath)
        {
            if (strPath == null)
            {
                throw new ArgumentNullException("strPath");
            }

            if (strPath.Length == 0)
            {
                return string.Empty;
            }

            // Path.GetFullPath is incompatible with UNC paths traversing over
            // different server shares (which are created by PathRelativePathTo);
            // we need to build the absolute path on our own...
            if (IsUncPath(strPath))
            {
                char chSep = strPath[0];
                Debug.Assert(Array.IndexOf<char>(m_vDirSeps, chSep) >= 0);

                List<string> l = new List<string>();
#if !KeePassLibSD
                string[] v = strPath.Split(m_vDirSeps, StringSplitOptions.None);
#else
				string[] v = strPath.Split(m_vDirSeps);
#endif
                Debug.Assert((v.Length >= 3) && (v[0].Length == 0) && (v[1].Length == 0));

                foreach (string strPart in v)
                {
                    if (strPart.Equals("."))
                    {
                        continue;
                    }
                    else if (strPart.Equals(".."))
                    {
                        if (l.Count > 0)
                        {
                            l.RemoveAt(l.Count - 1);
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                    }
                    else
                    {
                        l.Add(strPart); // Do not ignore zero length parts
                    }
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < l.Count; ++i)
                {
                    // Don't test length of sb, might be 0 due to initial UNC seps
                    if (i > 0)
                    {
                        sb.Append(chSep);
                    }

                    sb.Append(l[i]);
                }

                return sb.ToString();
            }

            string str;
            try
            {
#if KeePassRT
                var dirT = Windows.Storage.StorageFolder.GetFolderFromPathAsync(strPath).GetResults();
                str = dirT.Path;
#else
				str = Path.GetFullPath(strPath);
#endif
            }
            catch (Exception)
            {
                Debug.Assert(false);
                return strPath;
            }

            Debug.Assert(str.IndexOf("\\..\\") < 0);
            foreach (char ch in m_vDirSeps)
            {
                string strSep = new string(ch, 1);
                str = str.Replace(strSep + "." + strSep, strSep);
            }

            return str;
        }

        /// <summary>
        /// The get url length.
        /// </summary>
        /// <param name="strText">
        /// The str text.
        /// </param>
        /// <param name="nOffset">
        /// The n offset.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static int GetUrlLength(string strText, int nOffset)
        {
            if (strText == null)
            {
                throw new ArgumentNullException("strText");
            }

            if (nOffset > strText.Length)
            {
                throw new ArgumentException(); // Not >= (0 len)
            }

            int iPosition = nOffset, nLength = 0, nStrLen = strText.Length;

            while (iPosition < nStrLen)
            {
                char ch = strText[iPosition];
                ++iPosition;

                if ((ch == ' ') || (ch == '\t') || (ch == '\r') || (ch == '\n'))
                {
                    break;
                }

                ++nLength;
            }

            return nLength;
        }

        /// <summary>
        /// The hide file.
        /// </summary>
        /// <param name="strFile">
        /// The str file.
        /// </param>
        /// <param name="bHide">
        /// The b hide.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool HideFile(string strFile, bool bHide)
        {
#if (KeePassLibSD || KeePassRT)
            return false;
#else
			if(strFile == null) throw new ArgumentNullException("strFile");

			try
			{
				FileAttributes fa = File.GetAttributes(strFile);

				if(bHide) fa = ((fa & ~FileAttributes.Normal) | FileAttributes.Hidden);
				else				{

// Unhide
					fa &= ~FileAttributes.Hidden;
					if((long)fa == 0) fa |= FileAttributes.Normal;
				}

				File.SetAttributes(strFile, fa);
				return true;
			}
			catch(Exception) { }

			return false;
#endif
        }

        /// <summary>
        /// The is absolute path.
        /// </summary>
        /// <param name="strPath">
        /// The str path.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static bool IsAbsolutePath(string strPath)
        {
            if (strPath == null)
            {
                throw new ArgumentNullException("strPath");
            }

            if (strPath.Length == 0)
            {
                return false;
            }

            if (IsUncPath(strPath))
            {
                return true;
            }

            try
            {
                return Path.IsPathRooted(strPath);
            }
            catch (Exception)
            {
                Debug.Assert(false);
            }

            return true;
        }

        /// <summary>
        /// The is unc path.
        /// </summary>
        /// <param name="strPath">
        /// The str path.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static bool IsUncPath(string strPath)
        {
            if (strPath == null)
            {
                throw new ArgumentNullException("strPath");
            }

            return strPath.StartsWith("\\\\") || strPath.StartsWith("//");
        }

        /// <summary>
        /// The make absolute path.
        /// </summary>
        /// <param name="strBaseFile">
        /// The str base file.
        /// </param>
        /// <param name="strTargetFile">
        /// The str target file.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static string MakeAbsolutePath(string strBaseFile, string strTargetFile)
        {
            if (strBaseFile == null)
            {
                throw new ArgumentNullException("strBasePath");
            }

            if (strTargetFile == null)
            {
                throw new ArgumentNullException("strTargetPath");
            }

            if (strBaseFile.Length == 0)
            {
                return strTargetFile;
            }

            if (strTargetFile.Length == 0)
            {
                return string.Empty;
            }

            if (IsAbsolutePath(strTargetFile))
            {
                return strTargetFile;
            }

            string strBaseDir = GetFileDirectory(strBaseFile, true, false);
            return GetShortestAbsolutePath(strBaseDir + strTargetFile);
        }

        /// <summary>
        /// The make relative path.
        /// </summary>
        /// <param name="strBaseFile">
        /// The str base file.
        /// </param>
        /// <param name="strTargetFile">
        /// The str target file.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static string MakeRelativePath(string strBaseFile, string strTargetFile)
        {
            if (strBaseFile == null)
            {
                throw new ArgumentNullException("strBasePath");
            }

            if (strTargetFile == null)
            {
                throw new ArgumentNullException("strTargetPath");
            }

            if (strBaseFile.Length == 0)
            {
                return strTargetFile;
            }

            if (strTargetFile.Length == 0)
            {
                return string.Empty;
            }

            // Test whether on different Windows drives
            if ((strBaseFile.Length >= 3) && (strTargetFile.Length >= 3))
            {
                if ((strBaseFile[1] == ':') && (strTargetFile[1] == ':') && (strBaseFile[2] == '\\') && (strTargetFile[2] == '\\')
                    && (strBaseFile[0] != strTargetFile[0]))
                {
                    return strTargetFile;
                }
            }

#if (!KeePassLibSD && !KeePassRT)
			if(NativeLib.IsUnix())
#endif
            {
                bool bBaseUnc = IsUncPath(strBaseFile);
                bool bTargetUnc = IsUncPath(strTargetFile);
                if ((!bBaseUnc && bTargetUnc) || (bBaseUnc && !bTargetUnc))
                {
                    return strTargetFile;
                }

                string strBase = GetShortestAbsolutePath(strBaseFile);
                string strTarget = GetShortestAbsolutePath(strTargetFile);
                string[] vBase = strBase.Split(m_vDirSeps);
                string[] vTarget = strTarget.Split(m_vDirSeps);

                int i = 0;
                while ((i < (vBase.Length - 1)) && (i < (vTarget.Length - 1)) && (vBase[i] == vTarget[i]))
                {
                    ++i;
                }

                StringBuilder sbRel = new StringBuilder();
                for (int j = i; j < (vBase.Length - 1); ++j)
                {
                    if (sbRel.Length > 0)
                    {
                        sbRel.Append(UrlUtil.LocalDirSepChar);
                    }

                    sbRel.Append("..");
                }

                for (int k = i; k < vTarget.Length; ++k)
                {
                    if (sbRel.Length > 0)
                    {
                        sbRel.Append(UrlUtil.LocalDirSepChar);
                    }

                    sbRel.Append(vTarget[k]);
                }

                return sbRel.ToString();
            }

#if (!KeePassLibSD && !KeePassRT)
			try			{

// Windows
				const int nMaxPath = NativeMethods.MAX_PATH * 2;
				StringBuilder sb = new StringBuilder(nMaxPath + 2);
				if(NativeMethods.PathRelativePathTo(sb, strBaseFile, 0, 
					strTargetFile, 0) == false)
					return strTargetFile;

				string str = sb.ToString();
				while(str.StartsWith(".\\")) str = str.Substring(2, str.Length - 2);

				return str;
			}
			catch(Exception) { Debug.Assert(false); }
			return strTargetFile;
#endif
        }

        /// <summary>
        /// The remove scheme.
        /// </summary>
        /// <param name="strUrl">
        /// The str url.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string RemoveScheme(string strUrl)
        {
            if (string.IsNullOrEmpty(strUrl))
            {
                return string.Empty;
            }

            int nNetScheme = strUrl.IndexOf(@"://", StrUtil.CaseIgnoreCmp);
            int nShScheme = strUrl.IndexOf(@":/", StrUtil.CaseIgnoreCmp);
            int nSmpScheme = strUrl.IndexOf(@":", StrUtil.CaseIgnoreCmp);

            if ((nNetScheme < 0) && (nShScheme < 0) && (nSmpScheme < 0))
            {
                return strUrl; // No scheme
            }

            int nMin = Math.Min(
                Math.Min((nNetScheme >= 0) ? nNetScheme : int.MaxValue, (nShScheme >= 0) ? nShScheme : int.MaxValue), 
                (nSmpScheme >= 0) ? nSmpScheme : int.MaxValue);

            if (nMin == nNetScheme)
            {
                return strUrl.Substring(nMin + 3);
            }

            if (nMin == nShScheme)
            {
                return strUrl.Substring(nMin + 2);
            }

            return strUrl.Substring(nMin + 1);
        }

        /// <summary>
        /// Strip the extension of a file.
        /// </summary>
        /// <param name="strPath">
        /// Full path of a file with extension.
        /// </param>
        /// <returns>
        /// File name without extension.
        /// </returns>
        public static string StripExtension(string strPath)
        {
            Debug.Assert(strPath != null);
            if (strPath == null)
            {
                throw new ArgumentNullException("strPath");
            }

            int nLastDirSep = strPath.LastIndexOfAny(m_vDirSeps);
            int nLastExtDot = strPath.LastIndexOf('.');

            if (nLastExtDot <= nLastDirSep)
            {
                return strPath;
            }

            return strPath.Substring(0, nLastExtDot);
        }

        /// <summary>
        /// The unhide file.
        /// </summary>
        /// <param name="strFile">
        /// The str file.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool UnhideFile(string strFile)
        {
#if (KeePassLibSD || KeePassRT)
            return false;
#else
			if(strFile == null) throw new ArgumentNullException("strFile");

			try
			{
				FileAttributes fa = File.GetAttributes(strFile);
				if((long)(fa & FileAttributes.Hidden) == 0) return false;

				return HideFile(strFile, false);
			}
			catch(Exception) { }

			return false;
#endif
        }

        #endregion
    }
}