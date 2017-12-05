// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyValidatorPool.cs" company="">
//   
// </copyright>
// <summary>
//   The key validator pool.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WinKeeLib.Keys
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    using KeePassLib.Keys;
    using KeePassLib.Utility;

    /// <summary>
    /// The key validator pool.
    /// </summary>
    public sealed class KeyValidatorPool : IEnumerable<KeyValidator>
    {
        #region Fields

        /// <summary>
        /// The m_v validators.
        /// </summary>
        private List<KeyValidator> m_vValidators = new List<KeyValidator>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get
            {
                return this.m_vValidators.Count;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="v">
        /// The v.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public void Add(KeyValidator v)
        {
            Debug.Assert(v != null);
            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            this.m_vValidators.Add(v);
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<KeyValidator> GetEnumerator()
        {
            return this.m_vValidators.GetEnumerator();
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="v">
        /// The v.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public bool Remove(KeyValidator v)
        {
            Debug.Assert(v != null);
            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            return this.m_vValidators.Remove(v);
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="strKey">
        /// The str key.
        /// </param>
        /// <param name="t">
        /// The t.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public string Validate(string strKey, KeyValidationType t)
        {
            Debug.Assert(strKey != null);
            if (strKey == null)
            {
                throw new ArgumentNullException("strKey");
            }

            foreach (KeyValidator v in this.m_vValidators)
            {
                string strResult = v.Validate(strKey, t);
                if (strResult != null)
                {
                    return strResult;
                }
            }

            return null;
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="pbKeyUtf8">
        /// The pb key utf 8.
        /// </param>
        /// <param name="t">
        /// The t.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public string Validate(byte[] pbKeyUtf8, KeyValidationType t)
        {
            Debug.Assert(pbKeyUtf8 != null);
            if (pbKeyUtf8 == null)
            {
                throw new ArgumentNullException("pbKeyUtf8");
            }

            if (this.m_vValidators.Count == 0)
            {
                return null;
            }

            string strKey = StrUtil.Utf8.GetString(pbKeyUtf8, 0, pbKeyUtf8.Length);
            return this.Validate(strKey, t);
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_vValidators.GetEnumerator();
        }

        #endregion
    }
}