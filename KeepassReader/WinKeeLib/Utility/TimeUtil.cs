// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeUtil.cs" company="">
//   
// </copyright>
// <summary>
//   Contains various static time structure manipulation and conversion
//   routines.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace KeePassLib.Utility
{
    using System;
    using System.Diagnostics;

    using KeePassLib.Interfaces;

    /// <summary>
    /// Contains various static time structure manipulation and conversion
    /// routines.
    /// </summary>
    public static class TimeUtil
    {
        /// <summary>
        /// Length of a compressed <c>PW_TIME</c> structure in bytes.
        /// </summary>
        public const int PwTimeLength = 7;

        /// <summary>
        /// Pack a <c>DateTime</c> object into 5 bytes. Layout: 2 zero bits,
        /// year 12 bits, month 4 bits, day 5 bits, hour 5 bits, minute 6
        /// bits, second 6 bits.
        /// </summary>
        /// <param name="dt">
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        public static byte[] PackTime(DateTime dt)
        {
            byte[] pb = new byte[5];

            // Pack time to 5 byte structure:
            // Byte bits: 11111111 22222222 33333333 44444444 55555555
            // Contents : 00YYYYYY YYYYYYMM MMDDDDDH HHHHMMMM MMSSSSSS
            pb[0] = (byte)((dt.Year >> 6) & 0x3F);
            pb[1] = (byte)(((dt.Year & 0x3F) << 2) | ((dt.Month >> 2) & 0x03));
            pb[2] = (byte)(((dt.Month & 0x03) << 6) | ((dt.Day & 0x1F) << 1) | ((dt.Hour >> 4) & 0x01));
            pb[3] = (byte)(((dt.Hour & 0x0F) << 4) | ((dt.Minute >> 2) & 0x0F));
            pb[4] = (byte)(((dt.Minute & 0x03) << 6) | (dt.Second & 0x3F));

            return pb;
        }

        /// <summary>
        /// Unpack a packed time (5 bytes, packed by the <c>PackTime</c>
        /// member function) to a <c>DateTime</c> object.
        /// </summary>
        /// <param name="pb">
        /// Packed time, 5 bytes.
        /// </param>
        /// <returns>
        /// Unpacked <c>DateTime</c> object.
        /// </returns>
        public static DateTime UnpackTime(byte[] pb)
        {
            Debug.Assert((pb != null) && (pb.Length == 5));
            if (pb == null)
            {
                throw new ArgumentNullException("pb");
            }

            if (pb.Length != 5)
            {
                throw new ArgumentException();
            }

            int n1 = pb[0], n2 = pb[1], n3 = pb[2], n4 = pb[3], n5 = pb[4];

            // Unpack 5 byte structure to date and time
            int nYear = (n1 << 6) | (n2 >> 2);
            int nMonth = ((n2 & 0x00000003) << 2) | (n3 >> 6);
            int nDay = (n3 >> 1) & 0x0000001F;
            int nHour = ((n3 & 0x00000001) << 4) | (n4 >> 4);
            int nMinute = ((n4 & 0x0000000F) << 2) | (n5 >> 6);
            int nSecond = n5 & 0x0000003F;

            return new DateTime(nYear, nMonth, nDay, nHour, nMinute, nSecond);
        }

        /// <summary>
        /// Pack a <c>DateTime</c> object into 7 bytes (<c>PW_TIME</c>).
        /// </summary>
        /// <param name="dt">
        /// Object to be encoded.
        /// </param>
        /// <returns>
        /// Packed time, 7 bytes (<c>PW_TIME</c>).
        /// </returns>
        public static byte[] PackPwTime(DateTime dt)
        {
            Debug.Assert(PwTimeLength == 7);

            byte[] pb = new byte[7];
            pb[0] = (byte)(dt.Year & 0xFF);
            pb[1] = (byte)(dt.Year >> 8);
            pb[2] = (byte)dt.Month;
            pb[3] = (byte)dt.Day;
            pb[4] = (byte)dt.Hour;
            pb[5] = (byte)dt.Minute;
            pb[6] = (byte)dt.Second;

            return pb;
        }

        /// <summary>
        /// Unpack a packed time (7 bytes, <c>PW_TIME</c>) to a <c>DateTime</c> object.
        /// </summary>
        /// <param name="pb">
        /// Packed time, 7 bytes.
        /// </param>
        /// <returns>
        /// Unpacked <c>DateTime</c> object.
        /// </returns>
        public static DateTime UnpackPwTime(byte[] pb)
        {
            Debug.Assert(PwTimeLength == 7);

            Debug.Assert(pb != null);
            if (pb == null)
            {
                throw new ArgumentNullException("pb");
            }

            Debug.Assert(pb.Length == 7);
            if (pb.Length != 7)
            {
                throw new ArgumentException();
            }

            return new DateTime(((int)pb[1] << 8) | (int)pb[0], (int)pb[2], (int)pb[3], (int)pb[4], (int)pb[5], (int)pb[6]);
        }

        /// <summary>
        /// Convert a <c>DateTime</c> object to a displayable string.
        /// </summary>
        /// <param name="dt">
        /// <c>DateTime</c> object to convert to a string.
        /// </param>
        /// <returns>
        /// String representing the specified <c>DateTime</c> object.
        /// </returns>
        public static string ToDisplayString(DateTime dt)
        {
            return dt.ToString();
        }

        /// <summary>
        /// The to display string date only.
        /// </summary>
        /// <param name="dt">
        /// The dt.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToDisplayStringDateOnly(DateTime dt)
        {
            return dt.ToString("d");
        }

        /// <summary>
        /// The from display string.
        /// </summary>
        /// <param name="strDisplay">
        /// The str display.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public static DateTime FromDisplayString(string strDisplay)
        {
            DateTime dt;

#if !KeePassLibSD
            if (DateTime.TryParse(strDisplay, out dt))
            {
                return dt;
            }

#else
			try { dt = DateTime.Parse(strDisplay); return dt; }
			catch(Exception) { }
#endif

            Debug.Assert(false);
            return DateTime.Now;
        }

        /// <summary>
        /// The serialize utc.
        /// </summary>
        /// <param name="dt">
        /// The dt.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string SerializeUtc(DateTime dt)
        {
            string str = dt.ToUniversalTime().ToString("s");
            if (str.EndsWith("Z") == false)
            {
                str += "Z";
            }

            return str;
        }

        /// <summary>
        /// The try deserialize utc.
        /// </summary>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="dt">
        /// The dt.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static bool TryDeserializeUtc(string str, out DateTime dt)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            if (str.EndsWith("Z"))
            {
                str = str.Substring(0, str.Length - 1);
            }

            bool bResult = StrUtil.TryParseDateTime(str, out dt);
            if (bResult)
            {
                dt = dt.ToLocalTime();
            }

            return bResult;
        }

        /// <summary>
        /// The m_dt unix root.
        /// </summary>
        private static DateTime? m_dtUnixRoot = null;

        /// <summary>
        /// The convert unix time.
        /// </summary>
        /// <param name="dtUnix">
        /// The dt unix.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public static DateTime ConvertUnixTime(double dtUnix)
        {
            try
            {
                if (!m_dtUnixRoot.HasValue)
                {
                    m_dtUnixRoot = (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).ToLocalTime();
                }

                return m_dtUnixRoot.Value.AddSeconds(dtUnix);
            }
            catch (Exception)
            {
                Debug.Assert(false);
            }

            return DateTime.Now;
        }

#if !KeePassLibSD

        /// <summary>
        /// The m_v us months.
        /// </summary>
        private static string[] m_vUSMonths = null;

        /// <summary>
        /// Parse a US textual date string, like e.g. "January 02, 2012".
        /// </summary>
        /// <param name="strDate">
        /// The str Date.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime?"/>.
        /// </returns>
        public static DateTime? ParseUSTextDate(string strDate)
        {
            if (strDate == null)
            {
                Debug.Assert(false);
                return null;
            }

            if (m_vUSMonths == null)
            {
                m_vUSMonths = new[]
                                  {
                                      "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", 
                                      "December"
                                  };
            }

            string str = strDate.Trim();
            for (int i = 0; i < m_vUSMonths.Length; ++i)
            {
                if (str.StartsWith(m_vUSMonths[i], StrUtil.CaseIgnoreCmp))
                {
                    str = str.Substring(m_vUSMonths[i].Length);
                    string[] v = str.Split(new[] { ',', ';' });
                    if ((v == null) || (v.Length != 2))
                    {
                        return null;
                    }

                    string strDay = v[0].Trim().TrimStart('0');
                    int iDay, iYear;
                    if (int.TryParse(strDay, out iDay) && int.TryParse(v[1].Trim(), out iYear))
                    {
                        return new DateTime(iYear, i + 1, iDay);
                    }
                    else
                    {
                        Debug.Assert(false);
                        return null;
                    }
                }
            }

            return null;
        }

#endif

        /// <summary>
        /// The m_dt inv min.
        /// </summary>
        private static readonly DateTime m_dtInvMin = new DateTime(2999, 12, 27, 23, 59, 59);

        /// <summary>
        /// The m_dt inv max.
        /// </summary>
        private static readonly DateTime m_dtInvMax = new DateTime(2999, 12, 29, 23, 59, 59);

        /// <summary>
        /// The compare.
        /// </summary>
        /// <param name="dtA">
        /// The dt a.
        /// </param>
        /// <param name="dtB">
        /// The dt b.
        /// </param>
        /// <param name="bUnkIsPast">
        /// The b unk is past.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int Compare(DateTime dtA, DateTime dtB, bool bUnkIsPast)
        {
            if (bUnkIsPast)
            {
                // 2999-12-28 23:59:59 in KeePass 1.x means 'unknown';
                // expect time zone corruption (twice)
                // bool bInvA = ((dtA.Year == 2999) && (dtA.Month == 12) &&
                // 	(dtA.Day >= 27) && (dtA.Day <= 29) && (dtA.Minute == 59) &&
                // 	(dtA.Second == 59));
                // bool bInvB = ((dtB.Year == 2999) && (dtB.Month == 12) &&
                // 	(dtB.Day >= 27) && (dtB.Day <= 29) && (dtB.Minute == 59) &&
                // 	(dtB.Second == 59));
                // Faster due to internal implementation of DateTime:
                bool bInvA = (dtA >= m_dtInvMin) && (dtA <= m_dtInvMax) && (dtA.Minute == 59) && (dtA.Second == 59);
                bool bInvB = (dtB >= m_dtInvMin) && (dtB <= m_dtInvMax) && (dtB.Minute == 59) && (dtB.Second == 59);

                if (bInvA)
                {
                    return bInvB ? 0 : -1;
                }

                if (bInvB)
                {
                    return 1;
                }
            }

            return dtA.CompareTo(dtB);
        }

        /// <summary>
        /// The compare last mod.
        /// </summary>
        /// <param name="tlA">
        /// The tl a.
        /// </param>
        /// <param name="tlB">
        /// The tl b.
        /// </param>
        /// <param name="bUnkIsPast">
        /// The b unk is past.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        internal static int CompareLastMod(ITimeLogger tlA, ITimeLogger tlB, bool bUnkIsPast)
        {
            if (tlA == null)
            {
                Debug.Assert(false);
                return (tlB == null) ? 0 : -1;
            }

            if (tlB == null)
            {
                Debug.Assert(false);
                return 1;
            }

            return Compare(tlA.LastModificationTime, tlB.LastModificationTime, bUnkIsPast);
        }
    }
}