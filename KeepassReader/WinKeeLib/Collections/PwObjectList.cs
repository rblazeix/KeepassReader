// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PwObjectList.cs" company="">
//   
// </copyright>
// <summary>
//   List of objects that implement <c>IDeepCloneable</c>,
//   and cannot be <c>null</c>.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    using KeePassLib.Interfaces;

    /// <summary>
    /// List of objects that implement <c>IDeepCloneable</c>,
    /// and cannot be <c>null</c>.
    /// </summary>
    /// <typeparam name="T">
    /// Type specifier.
    /// </typeparam>
    public sealed class PwObjectList<T> : IEnumerable<T>
        where T : class, IDeepCloneable<T>
    {
        #region Fields

        /// <summary>
        /// The m_v objects.
        /// </summary>
        private List<T> m_vObjects = new List<T>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PwObjectList{T}"/> class. 
        /// Construct a new list of objects.
        /// </summary>
        public PwObjectList()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get number of objects in this list.
        /// </summary>
        public uint UCount
        {
            get
            {
                return (uint)this.m_vObjects.Count;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The from array.
        /// </summary>
        /// <param name="tArray">
        /// The t array.
        /// </param>
        /// <returns>
        /// The <see cref="PwObjectList"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static PwObjectList<T> FromArray(T[] tArray)
        {
            if (tArray == null)
            {
                throw new ArgumentNullException("tArray");
            }

            PwObjectList<T> l = new PwObjectList<T>();
            foreach (T t in tArray)
            {
                l.Add(t);
            }

            return l;
        }

        /// <summary>
        /// The from list.
        /// </summary>
        /// <param name="tList">
        /// The t list.
        /// </param>
        /// <returns>
        /// The <see cref="PwObjectList"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static PwObjectList<T> FromList(List<T> tList)
        {
            if (tList == null)
            {
                throw new ArgumentNullException("tList");
            }

            PwObjectList<T> l = new PwObjectList<T>();
            l.Add(tList);
            return l;
        }

        /// <summary>
        /// Add an object to this list.
        /// </summary>
        /// <param name="pwObject">
        /// Object to be added.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public void Add(T pwObject)
        {
            Debug.Assert(pwObject != null);
            if (pwObject == null)
            {
                throw new ArgumentNullException("pwObject");
            }

            this.m_vObjects.Add(pwObject);
        }

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="vObjects">
        /// The v objects.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public void Add(PwObjectList<T> vObjects)
        {
            Debug.Assert(vObjects != null);
            if (vObjects == null)
            {
                throw new ArgumentNullException("vObjects");
            }

            foreach (T po in vObjects)
            {
                this.m_vObjects.Add(po);
            }
        }

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="vObjects">
        /// The v objects.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public void Add(List<T> vObjects)
        {
            Debug.Assert(vObjects != null);
            if (vObjects == null)
            {
                throw new ArgumentNullException("vObjects");
            }

            foreach (T po in vObjects)
            {
                this.m_vObjects.Add(po);
            }
        }

        /// <summary>
        /// The clear.
        /// </summary>
        public void Clear()
        {
            // Do not destroy contained objects!
            this.m_vObjects.Clear();
        }

        /// <summary>
        /// Clone the current <c>PwObjectList</c>, including all
        /// stored objects (deep copy).
        /// </summary>
        /// <returns>New <c>PwObjectList</c>.</returns>
        public PwObjectList<T> CloneDeep()
        {
            PwObjectList<T> pl = new PwObjectList<T>();

            foreach (T po in this.m_vObjects)
            {
                pl.Add(po.CloneDeep());
            }

            return pl;
        }

        /// <summary>
        /// The clone shallow.
        /// </summary>
        /// <returns>
        /// The <see cref="PwObjectList"/>.
        /// </returns>
        public PwObjectList<T> CloneShallow()
        {
            PwObjectList<T> tNew = new PwObjectList<T>();

            foreach (T po in this.m_vObjects)
            {
                tNew.Add(po);
            }

            return tNew;
        }

        /// <summary>
        /// The clone shallow to list.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<T> CloneShallowToList()
        {
            PwObjectList<T> tNew = this.CloneShallow();
            return tNew.m_vObjects;
        }

        /// <summary>
        /// Get an object of the list.
        /// </summary>
        /// <param name="uIndex">
        /// Index of the object to get. Must be valid, otherwise an
        /// exception is thrown.
        /// </param>
        /// <returns>
        /// Reference to an existing <c>T</c> object. Is never <c>null</c>.
        /// </returns>
        public T GetAt(uint uIndex)
        {
            Debug.Assert(uIndex < this.m_vObjects.Count);
            if (uIndex >= this.m_vObjects.Count)
            {
                throw new ArgumentOutOfRangeException("uIndex");
            }

            return this.m_vObjects[(int)uIndex];
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.m_vObjects.GetEnumerator();
        }

        /// <summary>
        /// Get a range of objects.
        /// </summary>
        /// <param name="uStartIndexIncl">
        /// Index of the first object to be
        /// returned (inclusive).
        /// </param>
        /// <param name="uEndIndexIncl">
        /// Index of the last object to be
        /// returned (inclusive).
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<T> GetRange(uint uStartIndexIncl, uint uEndIndexIncl)
        {
            if (uStartIndexIncl >= (uint)this.m_vObjects.Count)
            {
                throw new ArgumentOutOfRangeException("uStartIndexIncl");
            }

            if (uEndIndexIncl >= (uint)this.m_vObjects.Count)
            {
                throw new ArgumentOutOfRangeException("uEndIndexIncl");
            }

            if (uStartIndexIncl > uEndIndexIncl)
            {
                throw new ArgumentException();
            }

            List<T> list = new List<T>((int)(uEndIndexIncl - uStartIndexIncl) + 1);
            for (uint u = uStartIndexIncl; u <= uEndIndexIncl; ++u)
            {
                list.Add(this.m_vObjects[(int)u]);
            }

            return list;
        }

        /// <summary>
        /// The index of.
        /// </summary>
        /// <param name="pwReference">
        /// The pw reference.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public int IndexOf(T pwReference)
        {
            Debug.Assert(pwReference != null);
            if (pwReference == null)
            {
                throw new ArgumentNullException("pwReference");
            }

            return this.m_vObjects.IndexOf(pwReference);
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="uIndex">
        /// The u index.
        /// </param>
        /// <param name="pwObject">
        /// The pw object.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public void Insert(uint uIndex, T pwObject)
        {
            Debug.Assert(pwObject != null);
            if (pwObject == null)
            {
                throw new ArgumentNullException("pwObject");
            }

            this.m_vObjects.Insert((int)uIndex, pwObject);
        }

        /// <summary>
        /// Move an object up or down.
        /// </summary>
        /// <param name="tObject">
        /// The object to be moved.
        /// </param>
        /// <param name="bUp">
        /// Move one up. If <c>false</c>, move one down.
        /// </param>
        public void MoveOne(T tObject, bool bUp)
        {
            Debug.Assert(tObject != null);
            if (tObject == null)
            {
                throw new ArgumentNullException("tObject");
            }

            int nCount = this.m_vObjects.Count;
            if (nCount <= 1)
            {
                return;
            }

            int nIndex = this.m_vObjects.IndexOf(tObject);
            Debug.Assert(nIndex >= 0);

            if (bUp && (nIndex > 0))
            {
                // No assert for top item
                T tTemp = this.m_vObjects[nIndex - 1];
                this.m_vObjects[nIndex - 1] = this.m_vObjects[nIndex];
                this.m_vObjects[nIndex] = tTemp;
            }
            else if (!bUp && (nIndex != (nCount - 1)))
            {
                // No assert for bottom item
                T tTemp = this.m_vObjects[nIndex + 1];
                this.m_vObjects[nIndex + 1] = this.m_vObjects[nIndex];
                this.m_vObjects[nIndex] = tTemp;
            }
        }

        /// <summary>
        /// Move some of the objects in this list to the top/bottom.
        /// </summary>
        /// <param name="vObjects">
        /// List of objects to be moved.
        /// </param>
        /// <param name="bTop">
        /// Move to top. If <c>false</c>, move to bottom.
        /// </param>
        public void MoveTopBottom(T[] vObjects, bool bTop)
        {
            Debug.Assert(vObjects != null);
            if (vObjects == null)
            {
                throw new ArgumentNullException("vObjects");
            }

            if (vObjects.Length == 0)
            {
                return;
            }

            int nCount = this.m_vObjects.Count;
            foreach (T t in vObjects)
            {
                this.m_vObjects.Remove(t);
            }

            if (bTop)
            {
                int nPos = 0;
                foreach (T t in vObjects)
                {
                    this.m_vObjects.Insert(nPos, t);
                    ++nPos;
                }
            }
            else
            {
                // Move to bottom
                foreach (T t in vObjects)
                {
                    this.m_vObjects.Add(t);
                }
            }

            Debug.Assert(nCount == this.m_vObjects.Count);
            if (nCount != this.m_vObjects.Count)
            {
                throw new ArgumentException("At least one of the T objects in the vObjects list doesn't exist!");
            }
        }

        /// <summary>
        /// Delete an object of this list. The object to be deleted is identified
        /// by a reference handle.
        /// </summary>
        /// <param name="pwReference">
        /// Reference of the object to be deleted.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the object was deleted, <c>false</c> if
        /// the object wasn't found in this list.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if the input
        /// parameter is <c>null</c>.
        /// </exception>
        public bool Remove(T pwReference)
        {
            Debug.Assert(pwReference != null);
            if (pwReference == null)
            {
                throw new ArgumentNullException("pwReference");
            }

            return this.m_vObjects.Remove(pwReference);
        }

        /// <summary>
        /// The remove at.
        /// </summary>
        /// <param name="uIndex">
        /// The u index.
        /// </param>
        public void RemoveAt(uint uIndex)
        {
            this.m_vObjects.RemoveAt((int)uIndex);
        }

        /// <summary>
        /// The set at.
        /// </summary>
        /// <param name="uIndex">
        /// The u index.
        /// </param>
        /// <param name="pwObject">
        /// The pw object.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public void SetAt(uint uIndex, T pwObject)
        {
            Debug.Assert(pwObject != null);
            if (pwObject == null)
            {
                throw new ArgumentNullException("pwObject");
            }

            if (uIndex >= (uint)this.m_vObjects.Count)
            {
                throw new ArgumentOutOfRangeException("uIndex");
            }

            this.m_vObjects[(int)uIndex] = pwObject;
        }

        /// <summary>
        /// The sort.
        /// </summary>
        /// <param name="tComparer">
        /// The t comparer.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public void Sort(IComparer<T> tComparer)
        {
            if (tComparer == null)
            {
                throw new ArgumentNullException("tComparer");
            }

            this.m_vObjects.Sort(tComparer);
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
            return this.m_vObjects.GetEnumerator();
        }

        #endregion
    }
}