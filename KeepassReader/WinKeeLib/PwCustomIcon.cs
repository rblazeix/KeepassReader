// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PwCustomIcon.cs" company="Mar3ek">
//   This code is licensed under the Mozilla Public License 2.0.
//   You can find the full license text at the following address:
//   https://winkee.codeplex.com/license
// </copyright>
// <summary>
//   Custom icon. <c>PwCustomIcon</c> objects are immutable.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KeePassLib
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Custom icon. <c>PwCustomIcon</c> objects are immutable.
    /// </summary>
    public sealed class PwCustomIcon
    {
        #region Fields

        /// <summary>
        /// The m_pb image data png.
        /// </summary>
        private byte[] m_pbImageDataPng;

        /// <summary>
        /// The m_pw uuid.
        /// </summary>
        private PwUuid m_pwUuid;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PwCustomIcon"/> class.
        /// </summary>
        /// <param name="pwUuid">
        /// The pw uuid.
        /// </param>
        /// <param name="pbImageDataPng">
        /// The pb image data png.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentException">
        /// </exception>
        public PwCustomIcon(PwUuid pwUuid, byte[] pbImageDataPng)
        {
            Debug.Assert(pwUuid != null);
            if (pwUuid == null)
            {
                throw new ArgumentNullException("pwUuid");
            }

            Debug.Assert(!pwUuid.Equals(PwUuid.Zero));
            if (pwUuid.Equals(PwUuid.Zero))
            {
                throw new ArgumentException("pwUuid == 0");
            }

            Debug.Assert(pbImageDataPng != null);
            if (pbImageDataPng == null)
            {
                throw new ArgumentNullException("pbImageDataPng");
            }

            this.m_pwUuid = pwUuid;
            this.m_pbImageDataPng = pbImageDataPng;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the image data png.
        /// </summary>
        public byte[] ImageDataPng
        {
            get
            {
                return this.m_pbImageDataPng;
            }
        }

        /// <summary>
        /// Gets the uuid.
        /// </summary>
        public PwUuid Uuid
        {
            get
            {
                return this.m_pwUuid;
            }
        }

        #endregion
    }
}