// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompositeKey.cs" company="">
//   
// </copyright>
// <summary>
//   Represents a key. A key can be build up using several user key data sources
//   like a password, a key file, the currently logged on user credentials,
//   the current computer ID, etc.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if KeePassRT

#endif

namespace WinKeeLib.Keys
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading;

    using Windows.Security.Cryptography.Core;

    using KeePassLib.Keys;
    using KeePassLib.Resources;
    using KeePassLib.Security;
    using KeePassLib.Utility;

    using Org.BouncyCastle.Crypto.Engines;
    using Org.BouncyCastle.Crypto.Parameters;

    /// <summary>
    /// Represents a key. A key can be build up using several user key data sources
    /// like a password, a key file, the currently logged on user credentials,
    /// the current computer ID, etc.
    /// </summary>
    public sealed class CompositeKey
    {
        /// <summary>
        /// The m_v user keys.
        /// </summary>
        private List<IUserKey> m_vUserKeys = new List<IUserKey>();

        /// <summary>
        /// List of all user keys contained in the current composite key.
        /// </summary>
        public IEnumerable<IUserKey> UserKeys
        {
            get
            {
                return this.m_vUserKeys;
            }
        }

        /// <summary>
        /// Gets the user key count.
        /// </summary>
        public uint UserKeyCount
        {
            get
            {
                return (uint)this.m_vUserKeys.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeKey"/> class. 
        /// Construct a new, empty key object.
        /// </summary>
        public CompositeKey()
        {
        }

        // /// <summary>
        // /// Deconstructor, clears up the key.
        // /// </summary>
        // ~CompositeKey()
        // {
        // 	Clear();
        // }

        // /// <summary>
        // /// Clears the key. This function also erases all previously stored
        // /// user key data objects.
        // /// </summary>
        // public void Clear()
        // {
        // 	foreach(IUserKey pKey in m_vUserKeys)
        // 		pKey.Clear();
        // 	m_vUserKeys.Clear();
        // }

        /// <summary>
        /// Add a user key.
        /// </summary>
        /// <param name="pKey">
        /// User key to add.
        /// </param>
        public void AddUserKey(IUserKey pKey)
        {
            Debug.Assert(pKey != null);
            if (pKey == null)
            {
                throw new ArgumentNullException("pKey");
            }

            this.m_vUserKeys.Add(pKey);
        }

        /// <summary>
        /// Remove a user key.
        /// </summary>
        /// <param name="pKey">
        /// User key to remove.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the key was removed successfully.
        /// </returns>
        public bool RemoveUserKey(IUserKey pKey)
        {
            Debug.Assert(pKey != null);
            if (pKey == null)
            {
                throw new ArgumentNullException("pKey");
            }

            Debug.Assert(this.m_vUserKeys.IndexOf(pKey) >= 0);
            return this.m_vUserKeys.Remove(pKey);
        }

#if !KeePassRT
    
    // <summary>
    /// Test whether the composite key contains a specific type of
    /// user keys (password, key file, ...). If at least one user
    /// key of that type is present, the function returns <c>true</c>.
    /// </summary>
    /// <param name="tUserKeyType">User key type.</param>
    /// <returns>Returns <c>true</c>, if the composite key contains
    /// a user key of the specified type.</returns>
		public bool ContainsType(Type tUserKeyType)
		{
			Debug.Assert(tUserKeyType != null);
			if(tUserKeyType == null) throw new ArgumentNullException("tUserKeyType");

			foreach(IUserKey pKey in m_vUserKeys)
			{
				if(tUserKeyType.IsInstanceOfType(pKey))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Get the first user key of a specified type.
		/// </summary>
		/// <param name="tUserKeyType">Type of the user key to get.</param>
		/// <returns>Returns the first user key of the specified type
		/// or <c>null</c> if no key of that type is found.</returns>
		public IUserKey GetUserKey(Type tUserKeyType)
		{
			Debug.Assert(tUserKeyType != null);
			if(tUserKeyType == null) throw new ArgumentNullException("tUserKeyType");

			foreach(IUserKey pKey in m_vUserKeys)
			{
				if(tUserKeyType.IsInstanceOfType(pKey))
					return pKey;
			}

			return null;
		}
#endif

        /// <summary>
        /// Creates the composite key from the supplied user key sources (password,
        /// key file, user account, computer ID, etc.).
        /// </summary>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        private byte[] CreateRawCompositeKey32()
        {
            this.ValidateUserKeys();

            // Concatenate user key data
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (IUserKey pKey in this.m_vUserKeys)
                {
                    ProtectedBinary b = pKey.KeyData;
                    if (b != null)
                    {
                        byte[] pbKeyData = b.ReadData();
                        ms.Write(pbKeyData, 0, pbKeyData.Length);
                        MemUtil.ZeroByteArray(pbKeyData);
                    }
                }

                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                byte[] pbHash = sha256.HashData(ms.ToArray().AsBuffer()).ToArray();

                return pbHash;
            }
        }

        /// <summary>
        /// The equals value.
        /// </summary>
        /// <param name="ckOther">
        /// The ck other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public bool EqualsValue(CompositeKey ckOther)
        {
            if (ckOther == null)
            {
                throw new ArgumentNullException("ckOther");
            }

            byte[] pbThis = this.CreateRawCompositeKey32();
            byte[] pbOther = ckOther.CreateRawCompositeKey32();
            bool bResult = MemUtil.ArraysEqual(pbThis, pbOther);
            Array.Clear(pbOther, 0, pbOther.Length);
            Array.Clear(pbThis, 0, pbThis.Length);

            return bResult;
        }

        /// <summary>
        /// Generate a 32-bit wide key out of the composite key.
        /// </summary>
        /// <param name="pbKeySeed32">
        /// Seed used in the key transformation
        /// rounds. Must be a byte array containing exactly 32 bytes; must
        /// not be null.
        /// </param>
        /// <param name="uNumRounds">
        /// Number of key transformation rounds.
        /// </param>
        /// <returns>
        /// Returns a protected binary object that contains the
        /// resulting 32-bit wide key.
        /// </returns>
        public ProtectedBinary GenerateKey32(byte[] pbKeySeed32, ulong uNumRounds, CancellationToken token)
        {
            Debug.Assert(pbKeySeed32 != null);
            if (pbKeySeed32 == null)
            {
                throw new ArgumentNullException("pbKeySeed32");
            }

            Debug.Assert(pbKeySeed32.Length == 32);
            if (pbKeySeed32.Length != 32)
            {
                throw new ArgumentException("pbKeySeed32");
            }

            byte[] pbRaw32 = this.CreateRawCompositeKey32();
            if ((pbRaw32 == null) || (pbRaw32.Length != 32))
            {
                Debug.Assert(false);
                return null;
            }

            byte[] pbTrf32 = TransformKey(pbRaw32, pbKeySeed32, uNumRounds, token);
            if ((pbTrf32 == null) || (pbTrf32.Length != 32))
            {
                Debug.Assert(false);
                return null;
            }

            ProtectedBinary pbRet = new ProtectedBinary(true, pbTrf32);
            MemUtil.ZeroByteArray(pbTrf32);
            MemUtil.ZeroByteArray(pbRaw32);

            return pbRet;
        }

        /// <summary>
        /// The validate user keys.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private void ValidateUserKeys()
        {
            int nAccounts = 0;

            if (nAccounts >= 2)
            {
                Debug.Assert(false);
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Transform the current key <c>uNumRounds</c> times.
        /// </summary>
        /// <param name="pbOriginalKey32">
        /// The original key which will be transformed.
        /// This parameter won't be modified.
        /// </param>
        /// <param name="pbKeySeed32">
        /// Seed used for key transformations. Must not
        /// be <c>null</c>. This parameter won't be modified.
        /// </param>
        /// <param name="uNumRounds">
        /// Transformation count.
        /// </param>
        /// <returns>
        /// 256-bit transformed key.
        /// </returns>
        private static byte[] TransformKey(byte[] pbOriginalKey32, byte[] pbKeySeed32, ulong uNumRounds, CancellationToken token)
        {
            Debug.Assert((pbOriginalKey32 != null) && (pbOriginalKey32.Length == 32));
            if (pbOriginalKey32 == null)
            {
                throw new ArgumentNullException("pbOriginalKey32");
            }

            if (pbOriginalKey32.Length != 32)
            {
                throw new ArgumentException();
            }

            Debug.Assert((pbKeySeed32 != null) && (pbKeySeed32.Length == 32));
            if (pbKeySeed32 == null)
            {
                throw new ArgumentNullException("pbKeySeed32");
            }

            if (pbKeySeed32.Length != 32)
            {
                throw new ArgumentException();
            }

            byte[] pbNewKey = new byte[32];
            Array.Copy(pbOriginalKey32, pbNewKey, pbNewKey.Length);

            if (TransformKeyManaged(pbNewKey, pbKeySeed32, uNumRounds, token) == false)
            {
                return null;
            }

            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            return sha256.HashData(pbNewKey.AsBuffer()).ToArray();
        }

        /// <summary>
        /// The transform key managed.
        /// </summary>
        /// <param name="pbNewKey32">
        /// The pb new key 32.
        /// </param>
        /// <param name="pbKeySeed32">
        /// The pb key seed 32.
        /// </param>
        /// <param name="uNumRounds">
        /// The u num rounds.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool TransformKeyManaged(byte[] pbNewKey32, byte[] pbKeySeed32, ulong uNumRounds, CancellationToken token)
        {
            var kp = new KeyParameter(pbKeySeed32);
            var aes = new AesEngine();
            aes.Init(true, kp);

            for (ulong i = 0; i < uNumRounds; ++i)
            {
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }

                aes.ProcessBlock(pbNewKey32, 0, pbNewKey32, 0);
                aes.ProcessBlock(pbNewKey32, 16, pbNewKey32, 16);
            }

            return true;
        }
    }

    /// <summary>
    /// The invalid composite key exception.
    /// </summary>
    public sealed class InvalidCompositeKeyException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCompositeKeyException"/> class. 
        /// Construct a new invalid composite key exception.
        /// </summary>
        public InvalidCompositeKeyException()
        {
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
                return KLRes.InvalidCompositeKey + Environment.NewLine + KLRes.InvalidCompositeKeyHint;
            }
        }

        #endregion
    }
}