// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IOConnection.cs" company="">
//   
// </copyright>
// <summary>
//   The io connection.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#if (!KeePassLibSD && !KeePassRT)
using System.Net.Cache;
using System.Net.Security;
#endif
#if !KeePassRT
using System.Security.Cryptography.X509Certificates;
#endif

namespace KeePassLib.Serialization
{
    using System;
    using System.Diagnostics;

    using WinKeeLib;

#if (!KeePassLibSD && !KeePassRT)
	public sealed class IOWebClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);
			IOConnection.ConfigureWebRequest(request);
			return request;
		}
	}
#endif

    /// <summary>
    /// The io connection.
    /// </summary>
    public static class IOConnection
    {
#if (!KeePassLibSD && !KeePassRT)
		private static ProxyServerType m_pstProxyType = ProxyServerType.System;
		private static string m_strProxyAddr = string.Empty;
		private static string m_strProxyPort = string.Empty;
		private static string m_strProxyUserName = string.Empty;
		private static string m_strProxyPassword = string.Empty;

		private static bool m_bSslCertsAcceptInvalid = false;
		internal static bool SslCertsAcceptInvalid
		{
			// get { return m_bSslCertsAcceptInvalid; }
			set { m_bSslCertsAcceptInvalid = value; }
		}
#endif

        // Web request methods
        /// <summary>
        /// The wrm delete file.
        /// </summary>
        public const string WrmDeleteFile = "DELETEFILE";

        /// <summary>
        /// The wrm move file.
        /// </summary>
        public const string WrmMoveFile = "MOVEFILE";

        // Web request headers
        /// <summary>
        /// The wrh move file to.
        /// </summary>
        public const string WrhMoveFileTo = "MoveFileTo";

        /// <summary>
        /// The io access pre.
        /// </summary>
        public static event EventHandler<IOAccessEventArgs> IOAccessPre;

        /// <summary>
        /// The raise io access pre event.
        /// </summary>
        /// <param name="ioc">
        /// The ioc.
        /// </param>
        /// <param name="t">
        /// The t.
        /// </param>
        private static void RaiseIOAccessPreEvent(IOConnectionInfo ioc, IOAccessType t)
        {
            RaiseIOAccessPreEvent(ioc, null, t);
        }

        /// <summary>
        /// The raise io access pre event.
        /// </summary>
        /// <param name="ioc">
        /// The ioc.
        /// </param>
        /// <param name="ioc2">
        /// The ioc 2.
        /// </param>
        /// <param name="t">
        /// The t.
        /// </param>
        private static void RaiseIOAccessPreEvent(IOConnectionInfo ioc, IOConnectionInfo ioc2, IOAccessType t)
        {
            if (ioc == null)
            {
                Debug.Assert(false);
                return;
            }

            // ioc2 may be null
            if (IOConnection.IOAccessPre != null)
            {
                IOConnectionInfo ioc2Lcl = (ioc2 != null) ? ioc2.CloneDeep() : null;
                IOAccessEventArgs e = new IOAccessEventArgs(ioc.CloneDeep(), ioc2Lcl, t);
                IOConnection.IOAccessPre(null, e);
            }
        }
    }
}