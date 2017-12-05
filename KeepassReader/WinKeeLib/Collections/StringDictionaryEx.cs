// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringDictionaryEx.cs" company="">
//   
// </copyright>
// <summary>
//   The string dictionary ex.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if KeePassLibSD
using KeePassLibSD;
#endif

namespace KeePassLib.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    using KeePassLib.Interfaces;

    /// <summary>
    /// The string dictionary ex.
    /// </summary>
    public sealed class StringDictionaryEx : IDeepCloneable<StringDictionaryEx>, IEnumerable<KeyValuePair<string, string>>
    {
        #region Fields

        /// <summary>
        /// The m_v dict.
        /// </summary>
        private SortedDictionary<string, string> m_vDict = new SortedDictionary<string, string>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StringDictionaryEx"/> class.
        /// </summary>
        public StringDictionaryEx()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get
            {
                return this.m_vDict.Count;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clone deep.
        /// </summary>
        /// <returns>
        /// The <see cref="StringDictionaryEx"/>.
        /// </returns>
        public StringDictionaryEx CloneDeep()
        {
            StringDictionaryEx plNew = new StringDictionaryEx();

            foreach (KeyValuePair<string, string> kvpStr in this.m_vDict)
            {
                plNew.Set(kvpStr.Key, kvpStr.Value);
            }

            return plNew;
        }

        /// <summary>
        /// The exists.
        /// </summary>
        /// <param name="strName">
        /// The str name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public bool Exists(string strName)
        {
            Debug.Assert(strName != null);
            if (strName == null)
            {
                throw new ArgumentNullException("strName");
            }

            return this.m_vDict.ContainsKey(strName);
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="strName">
        /// The str name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public string Get(string strName)
        {
            Debug.Assert(strName != null);
            if (strName == null)
            {
                throw new ArgumentNullException("strName");
            }

            string s;
            if (this.m_vDict.TryGetValue(strName, out s))
            {
                return s;
            }

            return null;
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.m_vDict.GetEnumerator();
        }

        /// <summary>
        /// Delete a string.
        /// </summary>
        /// <param name="strField">
        /// Name of the string field to delete.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the field has been successfully
        /// removed, otherwise the return value is <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public bool Remove(string strField)
        {
            Debug.Assert(strField != null);
            if (strField == null)
            {
                throw new ArgumentNullException("strField");
            }

            return this.m_vDict.Remove(strField);
        }

        /// <summary>
        /// Set a string.
        /// </summary>
        /// <param name="strField">
        /// Identifier of the string field to modify.
        /// </param>
        /// <param name="strNewValue">
        /// New value. This parameter must not be <c>null</c>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if one of the input
        /// parameters is <c>null</c>.
        /// </exception>
        public void Set(string strField, string strNewValue)
        {
            Debug.Assert(strField != null);
            if (strField == null)
            {
                throw new ArgumentNullException("strField");
            }

            Debug.Assert(strNewValue != null);
            if (strNewValue == null)
            {
                throw new ArgumentNullException("strNewValue");
            }

            this.m_vDict[strField] = strNewValue;
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
            return this.m_vDict.GetEnumerator();
        }

        #endregion
    }
}