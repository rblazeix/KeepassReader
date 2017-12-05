// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OldFormatException.cs" company="">
//   
// </copyright>
// <summary>
//   The old format exception.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Serialization
{
    using System;

    using KeePassLib.Resources;

    /// <summary>
    /// The old format exception.
    /// </summary>
    public sealed class OldFormatException : Exception
    {
        #region Fields

        /// <summary>
        /// The m_str format.
        /// </summary>
        private string m_strFormat = string.Empty;

        /// <summary>
        /// The m_type.
        /// </summary>
        private OldFormatType m_type = OldFormatType.Unknown;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OldFormatException"/> class.
        /// </summary>
        /// <param name="strFormatName">
        /// The str format name.
        /// </param>
        public OldFormatException(string strFormatName)
        {
            if (strFormatName != null)
            {
                this.m_strFormat = strFormatName;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OldFormatException"/> class.
        /// </summary>
        /// <param name="strFormatName">
        /// The str format name.
        /// </param>
        /// <param name="t">
        /// The t.
        /// </param>
        public OldFormatException(string strFormatName, OldFormatType t)
        {
            if (strFormatName != null)
            {
                this.m_strFormat = strFormatName;
            }

            this.m_type = t;
        }

        #endregion

        #region Enums

        /// <summary>
        /// The old format type.
        /// </summary>
        public enum OldFormatType
        {
            /// <summary>
            /// The unknown.
            /// </summary>
            Unknown = 0, 

            /// <summary>
            /// The kee pass 1 x.
            /// </summary>
            KeePass1x = 1
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
                string str = KLRes.OldFormat + ((this.m_strFormat.Length > 0) ? (@" (" + this.m_strFormat + @")") : string.Empty) + ".";

                if (this.m_type == OldFormatType.KeePass1x)
                {
                    str += Environment.NewLine + KLRes.KeePass1xHint;
                }

                return str;
            }
        }

        #endregion
    }
}