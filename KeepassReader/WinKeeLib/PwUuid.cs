// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PwUuid.cs" company="">
//   
// </copyright>
// <summary>
//   Represents an UUID of a password entry or group. Once created,
//   <c>PwUuid</c> objects aren't modifyable anymore (immutable).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using KeePassLib.Utility;

    // [ImmutableObject(true)]
    /// <summary>
    /// Represents an UUID of a password entry or group. Once created,
    /// <c>PwUuid</c> objects aren't modifyable anymore (immutable).
    /// </summary>
    public sealed class PwUuid : IComparable<PwUuid>, IEquatable<PwUuid>
    {
        /// <summary>
        /// Standard size in bytes of a UUID.
        /// </summary>
        public const uint UuidSize = 16;

        /// <summary>
        /// Zero UUID (all bytes are zero).
        /// </summary>
        public static readonly PwUuid Zero = new PwUuid(false);

        /// <summary>
        /// The m_pb uuid.
        /// </summary>
        private byte[] m_pbUuid = null; // Never null after constructor

        /// <summary>
        /// Get the 16 UUID bytes.
        /// </summary>
        public byte[] UuidBytes
        {
            get
            {
                return this.m_pbUuid;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PwUuid"/> class. 
        /// Construct a new UUID object.
        /// </summary>
        /// <param name="bCreateNew">
        /// If this parameter is <c>true</c>, a new
        /// UUID is generated. If it is <c>false</c>, the UUID is initialized
        /// to zero.
        /// </param>
        public PwUuid(bool bCreateNew)
        {
            if (bCreateNew)
            {
                this.CreateNew();
            }
            else
            {
                this.SetZero();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PwUuid"/> class. 
        /// Construct a new UUID object.
        /// </summary>
        /// <param name="uuidBytes">
        /// Initial value of the <c>PwUuid</c> object.
        /// </param>
        public PwUuid(byte[] uuidBytes)
        {
            this.SetValue(uuidBytes);
        }

        /// <summary>
        /// Create a new, random UUID.
        /// </summary>
        private void CreateNew()
        {
            Debug.Assert(this.m_pbUuid == null); // Only call from constructor
            while (true)
            {
                this.m_pbUuid = Guid.NewGuid().ToByteArray();

                if ((this.m_pbUuid == null) || (this.m_pbUuid.Length != (int)UuidSize))
                {
                    Debug.Assert(false);
                    throw new InvalidOperationException();
                }

                // Zero is a reserved value -- do not generate Zero
                if (!this.Equals(PwUuid.Zero))
                {
                    break;
                }

                Debug.Assert(false);
            }
        }

        /// <summary>
        /// The set value.
        /// </summary>
        /// <param name="uuidBytes">
        /// The uuid bytes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentException">
        /// </exception>
        private void SetValue(byte[] uuidBytes)
        {
            Debug.Assert((uuidBytes != null) && (uuidBytes.Length == (int)UuidSize));
            if (uuidBytes == null)
            {
                throw new ArgumentNullException("uuidBytes");
            }

            if (uuidBytes.Length != (int)UuidSize)
            {
                throw new ArgumentException();
            }

            Debug.Assert(this.m_pbUuid == null); // Only call from constructor
            this.m_pbUuid = new byte[UuidSize];

            Array.Copy(uuidBytes, this.m_pbUuid, (int)UuidSize);
        }

        /// <summary>
        /// The set zero.
        /// </summary>
        private void SetZero()
        {
            Debug.Assert(this.m_pbUuid == null); // Only call from constructor
            this.m_pbUuid = new byte[UuidSize];

            // Array.Clear(m_pbUuid, 0, (int)UuidSize);
#if DEBUG
            List<byte> l = new List<byte>(this.m_pbUuid);
            Debug.Assert(l.TrueForAll(bt => (bt == 0)));
#endif
        }

        /// <summary>
        /// The equals value.
        /// </summary>
        /// <param name="uuid">
        /// The uuid.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [Obsolete]
        public bool EqualsValue(PwUuid uuid)
        {
            return this.Equals(uuid);
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as PwUuid);
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
        public bool Equals(PwUuid other)
        {
            if (other == null)
            {
                Debug.Assert(false);
                return false;
            }

            for (int i = 0; i < (int)UuidSize; ++i)
            {
                if (this.m_pbUuid[i] != other.m_pbUuid[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// The m_h.
        /// </summary>
        private int m_h = 0;

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            if (this.m_h == 0)
            {
                this.m_h = (int)MemUtil.Hash32(this.m_pbUuid, 0, this.m_pbUuid.Length);
            }

            return this.m_h;
        }

        /// <summary>
        /// The compare to.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public int CompareTo(PwUuid other)
        {
            if (other == null)
            {
                Debug.Assert(false);
                throw new ArgumentNullException("other");
            }

            for (int i = 0; i < (int)UuidSize; ++i)
            {
                if (this.m_pbUuid[i] < other.m_pbUuid[i])
                {
                    return -1;
                }

                if (this.m_pbUuid[i] > other.m_pbUuid[i])
                {
                    return 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Convert the UUID to its string representation.
        /// </summary>
        /// <returns>String containing the UUID value.</returns>
        public string ToHexString()
        {
            return MemUtil.ByteArrayToHexString(this.m_pbUuid);
        }

#if DEBUG

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return this.ToHexString();
        }

#endif
    }

    /// <summary>
    /// The pw uuid comparable.
    /// </summary>
    [Obsolete]
    public sealed class PwUuidComparable : IComparable<PwUuidComparable>
    {
        #region Fields

        /// <summary>
        /// The m_pb uuid.
        /// </summary>
        private byte[] m_pbUuid = new byte[PwUuid.UuidSize];

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PwUuidComparable"/> class.
        /// </summary>
        /// <param name="pwUuid">
        /// The pw uuid.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public PwUuidComparable(PwUuid pwUuid)
        {
            if (pwUuid == null)
            {
                throw new ArgumentNullException("pwUuid");
            }

            Array.Copy(pwUuid.UuidBytes, this.m_pbUuid, (int)PwUuid.UuidSize);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The compare to.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public int CompareTo(PwUuidComparable other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            for (int i = 0; i < (int)PwUuid.UuidSize; ++i)
            {
                if (this.m_pbUuid[i] < other.m_pbUuid[i])
                {
                    return -1;
                }

                if (this.m_pbUuid[i] > other.m_pbUuid[i])
                {
                    return 1;
                }
            }

            return 0;
        }

        #endregion
    }
}