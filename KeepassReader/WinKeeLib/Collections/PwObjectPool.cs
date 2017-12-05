// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PwObjectPool.cs" company="">
//   
// </copyright>
// <summary>
//   The pw object pool.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if KeePassLibSD
using KeePassLibSD;
#endif

namespace KeePassLib.Collections
{
    using System;
    using System.Collections.Generic;

    using KeePassLib.Delegates;
    using KeePassLib.Interfaces;

    /// <summary>
    /// The pw object pool.
    /// </summary>
    public sealed class PwObjectPool
    {
        #region Fields

        /// <summary>
        /// The m_dict.
        /// </summary>
        private SortedDictionary<PwUuid, IStructureItem> m_dict = new SortedDictionary<PwUuid, IStructureItem>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The from group recursive.
        /// </summary>
        /// <param name="pgRoot">
        /// The pg root.
        /// </param>
        /// <param name="bEntries">
        /// The b entries.
        /// </param>
        /// <returns>
        /// The <see cref="PwObjectPool"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static PwObjectPool FromGroupRecursive(PwGroup pgRoot, bool bEntries)
        {
            if (pgRoot == null)
            {
                throw new ArgumentNullException("pgRoot");
            }

            PwObjectPool p = new PwObjectPool();

            if (!bEntries)
            {
                p.m_dict[pgRoot.Uuid] = pgRoot;
            }

            GroupHandler gh = delegate(PwGroup pg)
                {
                    p.m_dict[pg.Uuid] = pg;
                    return true;
                };

            EntryHandler eh = delegate(PwEntry pe)
                {
                    p.m_dict[pe.Uuid] = pe;
                    return true;
                };

            pgRoot.TraverseTree(TraversalMethod.PreOrder, bEntries ? null : gh, bEntries ? eh : null);
            return p;
        }

        /// <summary>
        /// The contains only type.
        /// </summary>
        /// <param name="t">
        /// The t.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ContainsOnlyType(Type t)
        {
            foreach (KeyValuePair<PwUuid, IStructureItem> kvp in this.m_dict)
            {
                if (kvp.Value.GetType() != t)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="pwUuid">
        /// The pw uuid.
        /// </param>
        /// <returns>
        /// The <see cref="IStructureItem"/>.
        /// </returns>
        public IStructureItem Get(PwUuid pwUuid)
        {
            IStructureItem pItem;
            this.m_dict.TryGetValue(pwUuid, out pItem);
            return pItem;
        }

        #endregion
    }
}