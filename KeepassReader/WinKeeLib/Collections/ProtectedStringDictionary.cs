// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProtectedStringDictionary.cs" company="">
//   
// </copyright>
// <summary>
//   A list of <c>ProtectedString</c> objects (dictionary).
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
    using KeePassLib.Security;
    using KeePassLib.Utility;

    using WinKeeLib;

    /// <summary>
    /// A list of <c>ProtectedString</c> objects (dictionary).
    /// </summary>
    public sealed class ProtectedStringDictionary : IDeepCloneable<ProtectedStringDictionary>, IEnumerable<KeyValuePair<string, ProtectedString>>
    {
        #region Fields

        /// <summary>
        /// The m_v strings.
        /// </summary>
        private SortedDictionary<string, ProtectedString> m_vStrings = new SortedDictionary<string, ProtectedString>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedStringDictionary"/> class. 
        /// Construct a new list of protected strings.
        /// </summary>
        public ProtectedStringDictionary()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the number of strings in this entry.
        /// </summary>
        public uint UCount
        {
            get
            {
                return (uint)this.m_vStrings.Count;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clear.
        /// </summary>
        public void Clear()
        {
            this.m_vStrings.Clear();
        }

        /// <summary>
        /// Clone the current <c>ProtectedStringList</c> object, including all
        /// stored protected strings.
        /// </summary>
        /// <returns>New <c>ProtectedStringList</c> object.</returns>
        public ProtectedStringDictionary CloneDeep()
        {
            ProtectedStringDictionary plNew = new ProtectedStringDictionary();

            foreach (KeyValuePair<string, ProtectedString> kvpStr in this.m_vStrings)
            {
                // ProtectedString objects are immutable
                plNew.Set(kvpStr.Key, kvpStr.Value);
            }

            return plNew;
        }

        /// <summary>
        /// The enable protection.
        /// </summary>
        /// <param name="strField">
        /// The str field.
        /// </param>
        /// <param name="bProtect">
        /// The b protect.
        /// </param>
        public void EnableProtection(string strField, bool bProtect)
        {
            ProtectedString ps = this.Get(strField);
            if (ps == null)
            {
                return; // Nothing to do, no assert
            }

            if (ps.IsProtected != bProtect)
            {
                byte[] pbData = ps.ReadUtf8();
                this.Set(strField, new ProtectedString(bProtect, pbData));
                MemUtil.ZeroByteArray(pbData);
            }
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
        [Obsolete]
        public bool EqualsDictionary(ProtectedStringDictionary dict)
        {
            return this.EqualsDictionary(dict, PwCompareOptions.None, MemProtCmpMode.None);
        }

        /// <summary>
        /// The equals dictionary.
        /// </summary>
        /// <param name="dict">
        /// The dict.
        /// </param>
        /// <param name="mpCompare">
        /// The mp compare.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [Obsolete]
        public bool EqualsDictionary(ProtectedStringDictionary dict, MemProtCmpMode mpCompare)
        {
            return this.EqualsDictionary(dict, PwCompareOptions.None, mpCompare);
        }

        /// <summary>
        /// The equals dictionary.
        /// </summary>
        /// <param name="dict">
        /// The dict.
        /// </param>
        /// <param name="pwOpt">
        /// The pw opt.
        /// </param>
        /// <param name="mpCompare">
        /// The mp compare.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool EqualsDictionary(ProtectedStringDictionary dict, PwCompareOptions pwOpt, MemProtCmpMode mpCompare)
        {
            if (dict == null)
            {
                Debug.Assert(false);
                return false;
            }

            bool bNeEqStd = (pwOpt & PwCompareOptions.NullEmptyEquivStd) != PwCompareOptions.None;
            if (!bNeEqStd)
            {
                if (this.m_vStrings.Count != dict.m_vStrings.Count)
                {
                    return false;
                }
            }

            foreach (KeyValuePair<string, ProtectedString> kvp in this.m_vStrings)
            {
                bool bStdField = PwDefs.IsStandardField(kvp.Key);
                ProtectedString ps = dict.Get(kvp.Key);

                if (bNeEqStd && (ps == null) && bStdField)
                {
                    ps = ProtectedString.Empty;
                }

                if (ps == null)
                {
                    return false;
                }

                if (mpCompare == MemProtCmpMode.Full)
                {
                    if (ps.IsProtected != kvp.Value.IsProtected)
                    {
                        return false;
                    }
                }
                else if (mpCompare == MemProtCmpMode.CustomOnly)
                {
                    if (!bStdField && (ps.IsProtected != kvp.Value.IsProtected))
                    {
                        return false;
                    }
                }

                if (ps.ReadString() != kvp.Value.ReadString())
                {
                    return false;
                }
            }

            if (bNeEqStd)
            {
                foreach (KeyValuePair<string, ProtectedString> kvp in dict.m_vStrings)
                {
                    ProtectedString ps = this.Get(kvp.Key);

                    if (ps != null)
                    {
                        continue; // Compared previously
                    }

                    if (!PwDefs.IsStandardField(kvp.Key))
                    {
                        return false;
                    }

                    if (!kvp.Value.IsEmpty)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Test if a named string exists.
        /// </summary>
        /// <param name="strName">
        /// Name of the string to try.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the string exists, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if
        /// <paramref name="strName"/> is <c>null</c>.
        /// </exception>
        public bool Exists(string strName)
        {
            Debug.Assert(strName != null);
            if (strName == null)
            {
                throw new ArgumentNullException("strName");
            }

            return this.m_vStrings.ContainsKey(strName);
        }

        /// <summary>
        /// Get one of the protected strings.
        /// </summary>
        /// <param name="strName">
        /// String identifier.
        /// </param>
        /// <returns>
        /// Protected string. If the string identified by
        /// <paramref name="strName"/> cannot be found, the function
        /// returns <c>null</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input parameter
        /// is <c>null</c>.
        /// </exception>
        public ProtectedString Get(string strName)
        {
            Debug.Assert(strName != null);
            if (strName == null)
            {
                throw new ArgumentNullException("strName");
            }

            ProtectedString ps;
            if (this.m_vStrings.TryGetValue(strName, out ps))
            {
                return ps;
            }

            return null;
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<KeyValuePair<string, ProtectedString>> GetEnumerator()
        {
            return this.m_vStrings.GetEnumerator();
        }

        /// <summary>
        /// The get keys.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<string> GetKeys()
        {
            List<string> v = new List<string>();

            foreach (string strKey in this.m_vStrings.Keys)
            {
                v.Add(strKey);
            }

            return v;
        }

        /// <summary>
        /// Get one of the protected strings. The return value is never <c>null</c>.
        /// If the requested string cannot be found, an empty protected string
        /// object is returned.
        /// </summary>
        /// <param name="strName">
        /// String identifier.
        /// </param>
        /// <returns>
        /// Returns a protected string object. If the standard string
        /// has not been set yet, the return value is an empty string (<c>""</c>).
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public ProtectedString GetSafe(string strName)
        {
            Debug.Assert(strName != null);
            if (strName == null)
            {
                throw new ArgumentNullException("strName");
            }

            ProtectedString ps;
            if (this.m_vStrings.TryGetValue(strName, out ps))
            {
                return ps;
            }

            return ProtectedString.Empty;
        }

        /// <summary>
        /// Get one of the protected strings. If the string doesn't exist, the
        /// return value is an empty string (<c>""</c>).
        /// </summary>
        /// <param name="strName">
        /// Name of the requested string.
        /// </param>
        /// <returns>
        /// Requested string value or an empty string, if the named
        /// string doesn't exist.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public string ReadSafe(string strName)
        {
            Debug.Assert(strName != null);
            if (strName == null)
            {
                throw new ArgumentNullException("strName");
            }

            ProtectedString ps;
            if (this.m_vStrings.TryGetValue(strName, out ps))
            {
                return ps.ReadString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Get one of the entry strings. If the string doesn't exist, the
        /// return value is an empty string (<c>""</c>). If the string is
        /// in-memory protected, the return value is <c>PwDefs.HiddenPassword</c>.
        /// </summary>
        /// <param name="strName">
        /// Name of the requested string.
        /// </param>
        /// <returns>
        /// Returns the requested string in plain-text or
        /// <c>PwDefs.HiddenPassword</c> if the string cannot be found.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public string ReadSafeEx(string strName)
        {
            Debug.Assert(strName != null);
            if (strName == null)
            {
                throw new ArgumentNullException("strName");
            }

            ProtectedString ps;
            if (this.m_vStrings.TryGetValue(strName, out ps))
            {
                if (ps.IsProtected)
                {
                    return PwDefs.HiddenPassword;
                }

                return ps.ReadString();
            }

            return string.Empty;
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

            return this.m_vStrings.Remove(strField);
        }

        /// <summary>
        /// Set a string.
        /// </summary>
        /// <param name="strField">
        /// Identifier of the string field to modify.
        /// </param>
        /// <param name="psNewValue">
        /// New value. This parameter must not be <c>null</c>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if one of the input
        /// parameters is <c>null</c>.
        /// </exception>
        public void Set(string strField, ProtectedString psNewValue)
        {
            Debug.Assert(strField != null);
            if (strField == null)
            {
                throw new ArgumentNullException("strField");
            }

            Debug.Assert(psNewValue != null);
            if (psNewValue == null)
            {
                throw new ArgumentNullException("psNewValue");
            }

            this.m_vStrings[strField] = psNewValue;
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
            return this.m_vStrings.GetEnumerator();
        }

        #endregion
    }
}