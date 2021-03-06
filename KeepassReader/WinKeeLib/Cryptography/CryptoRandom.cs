// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CryptoRandom.cs" company="">
//   
// </copyright>
// <summary>
//   Cryptographically strong random number generator. The returned values
//   are unpredictable and cannot be reproduced.
//   <c>CryptoRandom</c> is a singleton class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Cryptography
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices.WindowsRuntime;

    using KeePassLib.Utility;

    using Org.BouncyCastle.Crypto.Prng;

    using Windows.Security.Cryptography.Core;

    /// <summary>
    /// Cryptographically strong random number generator. The returned values
    /// are unpredictable and cannot be reproduced.
    /// <c>CryptoRandom</c> is a singleton class.
    /// </summary>
    public sealed class CryptoRandom
    {
        #region Static Fields

        /// <summary>
        /// The m_p instance.
        /// </summary>
        private static CryptoRandom m_pInstance = null;

        #endregion

        #region Fields

        /// <summary>
        /// The m_o sync root.
        /// </summary>
        private object m_oSyncRoot = new object();

        /// <summary>
        /// The m_pb entropy pool.
        /// </summary>
        private byte[] m_pbEntropyPool = new byte[64];

        /// <summary>
        /// The m_rng.
        /// </summary>
        private IRandomGenerator m_rng = new VmpcRandomGenerator();

        /// <summary>
        /// The m_u counter.
        /// </summary>
        private uint m_uCounter;

        /// <summary>
        /// The m_u generated bytes count.
        /// </summary>
        private ulong m_uGeneratedBytesCount = 0;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Prevents a default instance of the <see cref="CryptoRandom"/> class from being created.
        /// </summary>
        private CryptoRandom()
        {
            Random r = new Random();
            this.m_uCounter = (uint)r.Next();

            this.AddEntropy(GetSystemData(r));
            this.AddEntropy(this.GetCspData());
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Event that is triggered whenever the internal <c>GenerateRandom256</c>
        /// method is called to generate random bytes.
        /// </summary>
        public event EventHandler GenerateRandom256Pre;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static CryptoRandom Instance
        {
            get
            {
                if (m_pInstance != null)
                {
                    return m_pInstance;
                }

                m_pInstance = new CryptoRandom();
                return m_pInstance;
            }
        }

        /// <summary>
        /// Get the number of random bytes that this instance generated so far.
        /// Note that this number can be higher than the number of random bytes
        /// actually requested using the <c>GetRandomBytes</c> method.
        /// </summary>
        public ulong GeneratedBytesCount
        {
            get
            {
                ulong u;
                lock (this.m_oSyncRoot)
                {
                    u = this.m_uGeneratedBytesCount;
                }

                return u;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Update the internal seed of the random number generator based
        /// on entropy data.
        /// This method is thread-safe.
        /// </summary>
        /// <param name="pbEntropy">
        /// Entropy bytes.
        /// </param>
        public void AddEntropy(byte[] pbEntropy)
        {
            if (pbEntropy == null)
            {
                Debug.Assert(false);
                return;
            }

            if (pbEntropy.Length == 0)
            {
                Debug.Assert(false);
                return;
            }

            byte[] pbNewData = pbEntropy;
            if (pbEntropy.Length >= 64)
            {
                var shaNew = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);

                pbNewData = shaNew.HashData(pbEntropy.AsBuffer()).ToArray();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                lock (this.m_oSyncRoot)
                {
                    ms.Write(this.m_pbEntropyPool, 0, this.m_pbEntropyPool.Length);
                    ms.Write(pbNewData, 0, pbNewData.Length);

                    byte[] pbFinal = ms.ToArray();
                    Debug.Assert(pbFinal.Length == (64 + pbNewData.Length));
                    var shaPool = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);

                    this.m_pbEntropyPool = shaPool.HashData(pbFinal.AsBuffer()).ToArray();
                }
            }
        }

        /// <summary>
        /// Get a number of cryptographically strong random bytes.
        /// This method is thread-safe.
        /// </summary>
        /// <param name="uRequestedBytes">
        /// Number of requested random bytes.
        /// </param>
        /// <returns>
        /// A byte array consisting of <paramref name="uRequestedBytes"/>
        /// random bytes.
        /// </returns>
        public byte[] GetRandomBytes(uint uRequestedBytes)
        {
            if (uRequestedBytes == 0)
            {
                return new byte[0]; // Allow zero-length array
            }

            byte[] pbRes = new byte[uRequestedBytes];
            long lPos = 0;

            while (uRequestedBytes != 0)
            {
                byte[] pbRandom256 = this.GenerateRandom256();
                Debug.Assert(pbRandom256.Length == 32);

                long lCopy = (long)((uRequestedBytes < 32) ? uRequestedBytes : 32);

#if (!KeePassLibSD && !KeePassRT)
				Array.Copy(pbRandom256, 0, pbRes, lPos, lCopy);
#else
                Array.Copy(pbRandom256, 0, pbRes, (int)lPos, (int)lCopy);
#endif

                lPos += lCopy;
                uRequestedBytes -= (uint)lCopy;
            }

            Debug.Assert((int)lPos == pbRes.Length);
            return pbRes;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get system data.
        /// </summary>
        /// <param name="rWeak">
        /// The r weak.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        private static byte[] GetSystemData(Random rWeak)
        {
            MemoryStream ms = new MemoryStream();
            byte[] pb;

            pb = MemUtil.UInt32ToBytes((uint)Environment.TickCount);
            ms.Write(pb, 0, pb.Length);

            pb = TimeUtil.PackTime(DateTime.Now);
            ms.Write(pb, 0, pb.Length);

#if (!KeePassLibSD && !KeePassRT)
    
    // In try-catch for systems without GUI;
    // https://sourceforge.net/p/keepass/discussion/329221/thread/20335b73/
			try
			{
				Point pt = Cursor.Position;
				pb = MemUtil.UInt32ToBytes((uint)pt.X);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt32ToBytes((uint)pt.Y);
				ms.Write(pb, 0, pb.Length);
			}
			catch(Exception) { }
#endif

            pb = MemUtil.UInt32ToBytes((uint)rWeak.Next());
            ms.Write(pb, 0, pb.Length);

            pb = MemUtil.UInt32ToBytes(2);
            ms.Write(pb, 0, pb.Length);

#if (!KeePassLibSD && !KeePassRT)
			Process p = null;
			try
			{
				pb = MemUtil.UInt32ToBytes((uint)Environment.ProcessorCount);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)Environment.WorkingSet);
				ms.Write(pb, 0, pb.Length);

				Version v = Environment.OSVersion.Version;
				int nv = (v.Major << 28) + (v.MajorRevision << 24) +
					(v.Minor << 20) + (v.MinorRevision << 16) +
					(v.Revision << 12) + v.Build;
				pb = MemUtil.UInt32ToBytes((uint)nv);
				ms.Write(pb, 0, pb.Length);

				p = Process.GetCurrentProcess();

				pb = MemUtil.UInt64ToBytes((ulong)p.Handle.ToInt64());
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt32ToBytes((uint)p.HandleCount);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt32ToBytes((uint)p.Id);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.NonpagedSystemMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PagedMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PagedSystemMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PeakPagedMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PeakVirtualMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PeakWorkingSet64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PrivateMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.StartTime.ToBinary());
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.VirtualMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.WorkingSet64);
				ms.Write(pb, 0, pb.Length);

				// Not supported in Mono 1.2.6:
				// pb = MemUtil.UInt32ToBytes((uint)p.SessionId);
				// ms.Write(pb, 0, pb.Length);
			}
			catch(Exception) { }
			finally
			{
				try { if(p != null) p.Dispose(); }
				catch(Exception) { Debug.Assert(false); }
			}
#endif

            pb = Guid.NewGuid().ToByteArray();
            ms.Write(pb, 0, pb.Length);

            byte[] pbAll = ms.ToArray();
            ms.Dispose();
            return pbAll;
        }

        /// <summary>
        /// The generate random 256.
        /// </summary>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        private byte[] GenerateRandom256()
        {
            if (this.GenerateRandom256Pre != null)
            {
                this.GenerateRandom256Pre(this, EventArgs.Empty);
            }

            byte[] pbFinal;
            lock (this.m_oSyncRoot)
            {
                unchecked
                {
                    this.m_uCounter += 386047;
                }
 // Prime number
                byte[] pbCounter = MemUtil.UInt32ToBytes(this.m_uCounter);

                byte[] pbCspRandom = this.GetCspData();

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(this.m_pbEntropyPool, 0, this.m_pbEntropyPool.Length);
                    ms.Write(pbCounter, 0, pbCounter.Length);
                    ms.Write(pbCspRandom, 0, pbCspRandom.Length);
                    pbFinal = ms.ToArray();
                    Debug.Assert(pbFinal.Length == (this.m_pbEntropyPool.Length + pbCounter.Length + pbCspRandom.Length));
                }

                this.m_uGeneratedBytesCount += 32;
            }

            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            return sha256.HashData(pbFinal.AsBuffer()).ToArray();
        }

        /// <summary>
        /// The get csp data.
        /// </summary>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        private byte[] GetCspData()
        {
            byte[] pbCspRandom = new byte[32];
            this.m_rng.NextBytes(pbCspRandom);
            return pbCspRandom;
        }

        #endregion
    }
}