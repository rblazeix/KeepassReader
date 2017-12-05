// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoTypeConfig.cs" company="">
//   
// </copyright>
// <summary>
//   The auto type obfuscation options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using KeePassLib.Interfaces;

    /// <summary>
    /// The auto type obfuscation options.
    /// </summary>
    [Flags]
    public enum AutoTypeObfuscationOptions
    {
        /// <summary>
        /// The none.
        /// </summary>
        None = 0, 

        /// <summary>
        /// The use clipboard.
        /// </summary>
        UseClipboard = 1
    }

    /// <summary>
    /// The auto type association.
    /// </summary>
    public sealed class AutoTypeAssociation : IEquatable<AutoTypeAssociation>, IDeepCloneable<AutoTypeAssociation>
    {
        #region Fields

        /// <summary>
        /// The m_str sequence.
        /// </summary>
        private string m_strSequence = string.Empty;

        /// <summary>
        /// The m_str window.
        /// </summary>
        private string m_strWindow = string.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTypeAssociation"/> class.
        /// </summary>
        public AutoTypeAssociation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTypeAssociation"/> class.
        /// </summary>
        /// <param name="strWindow">
        /// The str window.
        /// </param>
        /// <param name="strSeq">
        /// The str seq.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public AutoTypeAssociation(string strWindow, string strSeq)
        {
            if (strWindow == null)
            {
                throw new ArgumentNullException("strWindow");
            }

            if (strSeq == null)
            {
                throw new ArgumentNullException("strSeq");
            }

            this.m_strWindow = strWindow;
            this.m_strSequence = strSeq;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public string Sequence
        {
            get
            {
                return this.m_strSequence;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_strSequence = value;
            }
        }

        /// <summary>
        /// Gets or sets the window name.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public string WindowName
        {
            get
            {
                return this.m_strWindow;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_strWindow = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The clone deep.
        /// </summary>
        /// <returns>
        /// The <see cref="AutoTypeAssociation"/>.
        /// </returns>
        public AutoTypeAssociation CloneDeep()
        {
            return (AutoTypeAssociation)this.MemberwiseClone();
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Equals(AutoTypeAssociation other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.m_strWindow != other.m_strWindow)
            {
                return false;
            }

            if (this.m_strSequence != other.m_strSequence)
            {
                return false;
            }

            return true;
        }

        #endregion
    }

    /// <summary>
    /// A list of auto-type associations.
    /// </summary>
    public sealed class AutoTypeConfig : IEquatable<AutoTypeConfig>, IDeepCloneable<AutoTypeConfig>
    {
        #region Fields

        /// <summary>
        /// The m_atoo obfuscation.
        /// </summary>
        private AutoTypeObfuscationOptions m_atooObfuscation = AutoTypeObfuscationOptions.None;

        /// <summary>
        /// The m_b enabled.
        /// </summary>
        private bool m_bEnabled = true;

        /// <summary>
        /// The m_l window assocs.
        /// </summary>
        private List<AutoTypeAssociation> m_lWindowAssocs = new List<AutoTypeAssociation>();

        /// <summary>
        /// The m_str default sequence.
        /// </summary>
        private string m_strDefaultSequence = string.Empty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoTypeConfig"/> class. 
        /// Construct a new auto-type associations list.
        /// </summary>
        public AutoTypeConfig()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get all auto-type window/keystroke sequence pairs.
        /// </summary>
        public IEnumerable<AutoTypeAssociation> Associations
        {
            get
            {
                return this.m_lWindowAssocs;
            }
        }

        /// <summary>
        /// Gets the associations count.
        /// </summary>
        public int AssociationsCount
        {
            get
            {
                return this.m_lWindowAssocs.Count;
            }
        }

        /// <summary>
        /// The default keystroke sequence that is auto-typed if
        /// no matching window is found in the <c>Associations</c>
        /// container.
        /// </summary>
        public string DefaultSequence
        {
            get
            {
                return this.m_strDefaultSequence;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.m_strDefaultSequence = value;
            }
        }

        /// <summary>
        /// Specify whether auto-type is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return this.m_bEnabled;
            }

            set
            {
                this.m_bEnabled = value;
            }
        }

        /// <summary>
        /// Specify whether the typing should be obfuscated.
        /// </summary>
        public AutoTypeObfuscationOptions ObfuscationOptions
        {
            get
            {
                return this.m_atooObfuscation;
            }

            set
            {
                this.m_atooObfuscation = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="a">
        /// The a.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public void Add(AutoTypeAssociation a)
        {
            Debug.Assert(a != null);
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            this.m_lWindowAssocs.Add(a);
        }

        /// <summary>
        /// Remove all associations.
        /// </summary>
        public void Clear()
        {
            this.m_lWindowAssocs.Clear();
        }

        /// <summary>
        /// Clone the auto-type associations list.
        /// </summary>
        /// <returns>New, cloned object.</returns>
        public AutoTypeConfig CloneDeep()
        {
            AutoTypeConfig newCfg = new AutoTypeConfig();

            newCfg.m_bEnabled = this.m_bEnabled;
            newCfg.m_atooObfuscation = this.m_atooObfuscation;
            newCfg.m_strDefaultSequence = this.m_strDefaultSequence;

            foreach (AutoTypeAssociation a in this.m_lWindowAssocs)
            {
                newCfg.Add(a.CloneDeep());
            }

            return newCfg;
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Equals(AutoTypeConfig other)
        {
            if (other == null)
            {
                Debug.Assert(false);
                return false;
            }

            if (this.m_bEnabled != other.m_bEnabled)
            {
                return false;
            }

            if (this.m_atooObfuscation != other.m_atooObfuscation)
            {
                return false;
            }

            if (this.m_strDefaultSequence != other.m_strDefaultSequence)
            {
                return false;
            }

            if (this.m_lWindowAssocs.Count != other.m_lWindowAssocs.Count)
            {
                return false;
            }

            for (int i = 0; i < this.m_lWindowAssocs.Count; ++i)
            {
                if (!this.m_lWindowAssocs[i].Equals(other.m_lWindowAssocs[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The get at.
        /// </summary>
        /// <param name="iIndex">
        /// The i index.
        /// </param>
        /// <returns>
        /// The <see cref="AutoTypeAssociation"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public AutoTypeAssociation GetAt(int iIndex)
        {
            if ((iIndex < 0) || (iIndex >= this.m_lWindowAssocs.Count))
            {
                throw new ArgumentOutOfRangeException("iIndex");
            }

            return this.m_lWindowAssocs[iIndex];
        }

        /// <summary>
        /// The remove at.
        /// </summary>
        /// <param name="iIndex">
        /// The i index.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public void RemoveAt(int iIndex)
        {
            if ((iIndex < 0) || (iIndex >= this.m_lWindowAssocs.Count))
            {
                throw new ArgumentOutOfRangeException("iIndex");
            }

            this.m_lWindowAssocs.RemoveAt(iIndex);
        }

        #endregion
    }
}