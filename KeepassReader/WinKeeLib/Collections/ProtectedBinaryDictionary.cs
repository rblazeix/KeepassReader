// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProtectedBinaryDictionary.cs" company="">
//   
// </copyright>
// <summary>
//   A list of <c>ProtectedBinary</c> objects (dictionary).
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
    using System.Text;

    using KeePassLib.Interfaces;
    using KeePassLib.Security;

    /// <summary>
    /// A list of <c>ProtectedBinary</c> objects (dictionary).
    /// </summary>
    public sealed class ProtectedBinaryDictionary : IDeepCloneable<ProtectedBinaryDictionary>, IEnumerable<KeyValuePair<string, ProtectedBinary>>
    {
        #region Fields

        /// <summary>
        /// The m_v binaries.
        /// </summary>
        private SortedDictionary<string, ProtectedBinary> m_vBinaries = new SortedDictionary<string, ProtectedBinary>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedBinaryDictionary"/> class. 
        /// Construct a new list of protected binaries.
        /// </summary>
        public ProtectedBinaryDictionary()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the number of binaries in this entry.
        /// </summary>
        public uint UCount
        {
            get
            {
                return (uint)this.m_vBinaries.Count;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clear.
        /// </summary>
        public void Clear()
        {
            this.m_vBinaries.Clear();
        }

        /// <summary>
        /// Clone the current <c>ProtectedBinaryList</c> object, including all
        /// stored protected strings.
        /// </summary>
        /// <returns>New <c>ProtectedBinaryList</c> object.</returns>
        public ProtectedBinaryDictionary CloneDeep()
        {
            ProtectedBinaryDictionary plNew = new ProtectedBinaryDictionary();

            foreach (KeyValuePair<string, ProtectedBinary> kvpBin in this.m_vBinaries)
            {
                // ProtectedBinary objects are immutable
                plNew.Set(kvpBin.Key, kvpBin.Value);
            }

            return plNew;
        }

        /// <summary>
        /// The equals dictionary.
        /// </summary>
        /// <param name="dict">
        /// The dict.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool EqualsDictionary(ProtectedBinaryDictionary dict)
        {
            if (dict == null)
            {
                Debug.Assert(false);
                return false;
            }

            if (this.m_vBinaries.Count != dict.m_vBinaries.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, ProtectedBinary> kvp in this.m_vBinaries)
            {
                ProtectedBinary pb = dict.Get(kvp.Key);
                if (pb == null)
                {
                    return false;
                }

                if (!pb.Equals(kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get one of the stored binaries.
        /// </summary>
        /// <param name="strName">
        /// Binary identifier.
        /// </param>
        /// <returns>
        /// Protected binary. If the binary identified by
        /// <paramref name="strName"/> cannot be found, the function
        /// returns <c>null</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public ProtectedBinary Get(string strName)
        {
            Debug.Assert(strName != null);
            if (strName == null)
            {
                throw new ArgumentNullException("strName");
            }

            ProtectedBinary pb;
            if (this.m_vBinaries.TryGetValue(strName, out pb))
            {
                return pb;
            }

            return null;
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<KeyValuePair<string, ProtectedBinary>> GetEnumerator()
        {
            return this.m_vBinaries.GetEnumerator();
        }

        /// <summary>
        /// The keys to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string KeysToString()
        {
            if (this.m_vBinaries.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, ProtectedBinary> kvp in this.m_vBinaries)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(kvp.Key);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Remove a binary object.
        /// </summary>
        /// <param name="strField">
        /// Identifier of the binary field to remove.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the object has been successfully
        /// removed, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input parameter
        /// is <c>null</c>.
        /// </exception>
        public bool Remove(string strField)
        {
            Debug.Assert(strField != null);
            if (strField == null)
            {
                throw new ArgumentNullException("strField");
            }

            return this.m_vBinaries.Remove(strField);
        }

        /// <summary>
        /// Set a binary object.
        /// </summary>
        /// <param name="strField">
        /// Identifier of the binary field to modify.
        /// </param>
        /// <param name="pbNewValue">
        /// New value. This parameter must not be <c>null</c>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if any of the input
        /// parameters is <c>null</c>.
        /// </exception>
        public void Set(string strField, ProtectedBinary pbNewValue)
        {
            Debug.Assert(strField != null);
            if (strField == null)
            {
                throw new ArgumentNullException("strField");
            }

            Debug.Assert(pbNewValue != null);
            if (pbNewValue == null)
            {
                throw new ArgumentNullException("pbNewValue");
            }

            this.m_vBinaries[strField] = pbNewValue;
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
            return this.m_vBinaries.GetEnumerator();
        }

        #endregion
    }
}