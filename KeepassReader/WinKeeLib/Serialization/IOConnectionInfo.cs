// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOConnectionInfo.cs" company="">
//   
// </copyright>
// <summary>
//   The io cred save mode.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Serialization
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    using KeePassLib.Interfaces;
    using KeePassLib.Utility;

    /// <summary>
    /// The io cred save mode.
    /// </summary>
    public enum IOCredSaveMode
    {
        /// <summary>
        /// Do not remember user name or password.
        /// </summary>
        NoSave = 0, 

        /// <summary>
        /// Remember the user name only, not the password.
        /// </summary>
        UserNameOnly, 

        /// <summary>
        /// Save both user name and password.
        /// </summary>
        SaveCred
    }

    /// <summary>
    /// The io cred prot mode.
    /// </summary>
    public enum IOCredProtMode
    {
        /// <summary>
        /// The none.
        /// </summary>
        None = 0, 

        /// <summary>
        /// The obf.
        /// </summary>
        Obf
    }

    /* public enum IOFileFormatHint
	{
		None = 0,
		Deprecated
	} */

    /// <summary>
    /// The io connection info.
    /// </summary>
    public sealed class IOConnectionInfo : IDeepCloneable<IOConnectionInfo>
    {
        // private IOFileFormatHint m_ioHint = IOFileFormatHint.None;
        #region Fields

        /// <summary>
        /// The m_b complete.
        /// </summary>
        private bool m_bComplete = false;

        /// <summary>
        /// The m_io cred prot mode.
        /// </summary>
        private IOCredProtMode m_ioCredProtMode = IOCredProtMode.None;

        /// <summary>
        /// The m_io cred save mode.
        /// </summary>
        private IOCredSaveMode m_ioCredSaveMode = IOCredSaveMode.NoSave;

        /// <summary>
        /// The m_str password.
        /// </summary>
        private string m_strPassword = string.Empty;

        /// <summary>
        /// The m_str url.
        /// </summary>
        private string m_strUrl = string.Empty;

        /// <summary>
        /// The m_str user.
        /// </summary>
        private string m_strUser = string.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the cred prot mode.
        /// </summary>
        public IOCredProtMode CredProtMode
        {
            get
            {
                return this.m_ioCredProtMode;
            }

            set
            {
                this.m_ioCredProtMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the cred save mode.
        /// </summary>
        public IOCredSaveMode CredSaveMode
        {
            get
            {
                return this.m_ioCredSaveMode;
            }

            set
            {
                this.m_ioCredSaveMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        [DefaultValue("")]
        public string Password
        {
            get
            {
                return this.m_strPassword;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_strPassword = value;
            }
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public string Path
        {
            get
            {
                return this.m_strUrl;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_strUrl = value;
            }
        }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        [DefaultValue("")]
        public string UserName
        {
            get
            {
                return this.m_strUser;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_strUser = value;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether is complete.
        /// </summary>
        [XmlIgnore]
        internal bool IsComplete
        {
            // Credentials etc. fully specified
            get
            {
                return this.m_bComplete;
            }

            set
            {
                this.m_bComplete = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The from path.
        /// </summary>
        /// <param name="strPath">
        /// The str path.
        /// </param>
        /// <returns>
        /// The <see cref="IOConnectionInfo"/>.
        /// </returns>
        public static IOConnectionInfo FromPath(string strPath)
        {
            IOConnectionInfo ioc = new IOConnectionInfo();

            ioc.Path = strPath;
            ioc.CredSaveMode = IOCredSaveMode.NoSave;

            return ioc;
        }

        /// <summary>
        /// The can probably access.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool CanProbablyAccess()
        {
            return true;
        }

        /// <summary>
        /// The clear credentials.
        /// </summary>
        /// <param name="bDependingOnRememberMode">
        /// The b depending on remember mode.
        /// </param>
        public void ClearCredentials(bool bDependingOnRememberMode)
        {
            if ((bDependingOnRememberMode == false) || (this.m_ioCredSaveMode == IOCredSaveMode.NoSave))
            {
                this.m_strUser = string.Empty;
            }

            if ((bDependingOnRememberMode == false) || (this.m_ioCredSaveMode == IOCredSaveMode.NoSave)
                || (this.m_ioCredSaveMode == IOCredSaveMode.UserNameOnly))
            {
                this.m_strPassword = string.Empty;
            }
        }

        /* public IOFileFormatHint FileFormatHint
		{
			get { return m_ioHint; }
			set { m_ioHint = value; }
		} */

        /// <summary>
        /// The clone deep.
        /// </summary>
        /// <returns>
        /// The <see cref="IOConnectionInfo"/>.
        /// </returns>
        public IOConnectionInfo CloneDeep()
        {
            return (IOConnectionInfo)this.MemberwiseClone();
        }

        /*
		/// <summary>
		/// Serialize the current connection info to a string. Credentials
		/// are only serialized if the <c>SaveCredentials</c> property
		/// is <c>true</c>.
		/// </summary>
		/// <param name="iocToCompile">Input object to be serialized.</param>
		/// <returns>Serialized object as string.</returns>
		public static string SerializeToString(IOConnectionInfo iocToCompile)
		{
			Debug.Assert(iocToCompile != null);
			if(iocToCompile == null) throw new ArgumentNullException("iocToCompile");

			string strUrl = iocToCompile.Path;
			string strUser = TransformUnreadable(iocToCompile.UserName, true);
			string strPassword = TransformUnreadable(iocToCompile.Password, true);

			string strAll = strUrl + strUser + strPassword + "CUN";
			char chSep = StrUtil.GetUnusedChar(strAll);
			if(chSep == char.MinValue) throw new FormatException();

			StringBuilder sb = new StringBuilder();
			sb.Append(chSep);
			sb.Append(strUrl);
			sb.Append(chSep);

			if(iocToCompile.CredSaveMode == IOCredSaveMode.SaveCred)
			{
				sb.Append('C');
				sb.Append(chSep);
				sb.Append(strUser);
				sb.Append(chSep);
				sb.Append(strPassword);
			}
			else if(iocToCompile.CredSaveMode == IOCredSaveMode.UserNameOnly)
			{
				sb.Append('U');
				sb.Append(chSep);
				sb.Append(strUser);
				sb.Append(chSep);
			}
			else // Don't remember credentials
			{
				sb.Append('N');
				sb.Append(chSep);
				sb.Append(chSep);
			}

			return sb.ToString();
		}

		public static IOConnectionInfo UnserializeFromString(string strToDecompile)
		{
			Debug.Assert(strToDecompile != null);
			if(strToDecompile == null) throw new ArgumentNullException("strToDecompile");
			if(strToDecompile.Length <= 1) throw new ArgumentException();

			char chSep = strToDecompile[0];
			string[] vParts = strToDecompile.Substring(1, strToDecompile.Length -
				1).Split(new char[]{ chSep });
			if(vParts.Length < 4) throw new ArgumentException();

			IOConnectionInfo s = new IOConnectionInfo();
			s.Path = vParts[0];

			if(vParts[1] == "C")
				s.CredSaveMode = IOCredSaveMode.SaveCred;
			else if(vParts[1] == "U")
				s.CredSaveMode = IOCredSaveMode.UserNameOnly;
			else
				s.CredSaveMode = IOCredSaveMode.NoSave;

			s.UserName = TransformUnreadable(vParts[2], false);
			s.Password = TransformUnreadable(vParts[3], false);
			return s;
		}
		*/

        /*
		/// <summary>
		/// Very simple string protection. Doesn't really encrypt the input
		/// string, only encodes it that it's not readable on the first glance.
		/// </summary>
		/// <param name="strToEncode">The string to encode/decode.</param>
		/// <param name="bEncode">If <c>true</c>, the string will be encoded,
		/// otherwise it'll be decoded.</param>
		/// <returns>Encoded/decoded string.</returns>
		private static string TransformUnreadable(string strToEncode, bool bEncode)
		{
			Debug.Assert(strToEncode != null);
			if(strToEncode == null) throw new ArgumentNullException("strToEncode");

			if(bEncode)
			{
				byte[] pbUtf8 = StrUtil.Utf8.GetBytes(strToEncode);

				unchecked
				{
					for(int iPos = 0; iPos < pbUtf8.Length; ++iPos)
						pbUtf8[iPos] += (byte)(iPos * 11);
				}

				return Convert.ToBase64String(pbUtf8);
			}
			else // Decode
			{
				byte[] pbBase = Convert.FromBase64String(strToEncode);

				unchecked
				{
					for(int iPos = 0; iPos < pbBase.Length; ++iPos)
						pbBase[iPos] -= (byte)(iPos * 11);
				}

				return StrUtil.Utf8.GetString(pbBase, 0, pbBase.Length);
			}
		}
		*/

        /// <summary>
        /// The get display name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetDisplayName()
        {
            string str = this.m_strUrl;

            if (this.m_strUser.Length > 0)
            {
                str += " (" + this.m_strUser + ")";
            }

            return str;
        }

        /// <summary>
        /// The is empty.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsEmpty()
        {
            return this.m_strUrl.Length > 0;
        }

        /// <summary>
        /// The is local file.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsLocalFile()
        {
            // Not just ":/", see e.g. AppConfigEx.ChangePathRelAbs
            return this.m_strUrl.IndexOf(@"://") < 0;
        }

        /// <summary>
        /// The obfuscate.
        /// </summary>
        /// <param name="bObf">
        /// The b obf.
        /// </param>
        public void Obfuscate(bool bObf)
        {
            if (bObf && (this.m_ioCredProtMode == IOCredProtMode.None))
            {
                this.m_strPassword = StrUtil.Obfuscate(this.m_strPassword);
                this.m_ioCredProtMode = IOCredProtMode.Obf;
            }
            else if (!bObf && (this.m_ioCredProtMode == IOCredProtMode.Obf))
            {
                this.m_strPassword = StrUtil.Deobfuscate(this.m_strPassword);
                this.m_ioCredProtMode = IOCredProtMode.None;
            }
        }

        #endregion
    }
}