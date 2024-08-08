﻿using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SysadminsLV.Asn1Parser.Utils;

static class DateTimeUtils {
    public static Byte[] Encode(DateTime time, TimeZoneInfo? zone, Boolean UTC, Boolean usePrecise) {
        String suffix = String.Empty;
        String preValue;
        String format = UTC
            ? UTCFormat
            : GtFormat;
        if (usePrecise) {
            // encode milliseconds using minimum bytes, i.e. do not encode trailing zeros
            // in worst case, when milliseconds is zero, then omit entire fraction despite
            // it was requested. See ITU-T X.690, section 11.7
            suffix += (time.Millisecond / 1000d).ToString(CultureInfo.InvariantCulture).Substring(1);
        }
        if (zone == null) {
            preValue = time.ToUniversalTime().ToString(format) + suffix + "Z";
        } else {
            suffix += zone.BaseUtcOffset is { Hours: >= 0, Minutes: >= 0 }
                ? "+"
                : "-";
            suffix +=
                Math.Abs(zone.BaseUtcOffset.Hours).ToString("d2") +
                Math.Abs(zone.BaseUtcOffset.Minutes).ToString("d2");
            preValue = time.ToString(format) + suffix;
        }
        Byte[] rawData = new Byte[preValue.Length];
        for (Int32 index = 0; index < preValue.Length; index++) {
            Char element = preValue[index];
            rawData[index] = Convert.ToByte(element);
        }
        return rawData;
    }
    // rawData is pure value without header
    public static DateTime Decode(Asn1Reader asn, out TimeZoneInfo? zone) {
        var SB = new StringBuilder();
        for (Int32 i = asn.PayloadStartOffset; i < asn.PayloadStartOffset + asn.PayloadLength; i++) {
            SB.Append(Convert.ToChar(asn[i]));
        }

        return extractDateTime(SB.ToString(), out zone);
    }

    static DateTime extractDateTime(String strValue, out TimeZoneInfo zone) {
        Int32 delimiterIndex;
        zone = TimeZoneInfo.FindSystemTimeZoneById("Greenwich Standard Time");
        if (strValue.ToUpper().Contains("Z")) {
            delimiterIndex = strValue.ToUpper().IndexOf('Z');
            return extractZulu(strValue, delimiterIndex);
        }
        Boolean hasZone = extractZoneShift(strValue, out Int32 hours, out Int32 minutes, out delimiterIndex);
        Int32 milliseconds = extractMilliseconds(strValue, delimiterIndex, out Int32 msDelimiter);
        DateTime retValue = extractDateTime(strValue, msDelimiter, delimiterIndex);
        if (hasZone) {
            zone = bindZone(hours, minutes);
            retValue = retValue.AddHours(hours);
            retValue = retValue.AddMinutes(minutes);
        }
        retValue = retValue.AddMilliseconds(milliseconds);
        return retValue;
    }
    static DateTime extractZulu(String strValue, Int32 zoneDelimiter) {
        return zoneDelimiter switch {
            12 => parseExactUtc(strValue.Replace("Z", null), UTCFormat).ToLocalTime(),
            16 => parseExactUtc(strValue.Replace("Z", null), UTCPreciseFormat).ToLocalTime(),
            14 => DateTime.ParseExact(strValue.Replace("Z", null), GtFormat, null).ToLocalTime(),
            18 => DateTime.ParseExact(strValue.Replace("Z", null), GtPreciseFormat, null).ToLocalTime(),
            _  => throw new ArgumentException("Time zone suffix is not valid.")
        };
    }
    static Boolean extractZoneShift(String strValue, out Int32 hours, out Int32 minutes, out Int32 delimiterIndex) {
        if (strValue.Contains('+')) {
            delimiterIndex = strValue.IndexOf('+');
            hours = Int32.Parse(strValue.Substring(delimiterIndex, 3));
        } else if (strValue.Contains('-')) {
            delimiterIndex = strValue.IndexOf('-');
            hours = -Int32.Parse(strValue.Substring(delimiterIndex, 3));
        } else {
            hours = minutes = delimiterIndex = 0;
            return false;
        }
        minutes = strValue.Length > delimiterIndex + 3
            ? -Int32.Parse(strValue.Substring(delimiterIndex + 3, 2))
            : 0;

        return true;
    }
    static Int32 extractMilliseconds(String strValue, Int32 zoneDelimiter, out Int32 msDelimiter) {
        msDelimiter = -1;
        if (!strValue.Contains(".")) { return 0; }
        msDelimiter = strValue.IndexOf('.');
        Int32 precisionLength = zoneDelimiter > 0
            ? zoneDelimiter - msDelimiter - 1
            : strValue.Length - msDelimiter - 1;
        return Int32.Parse(strValue.Substring(msDelimiter + 1, precisionLength));
    }
    static DateTime parseExactUtc(String strValue, String format) {
        // fix: .NET 'yy' format works in range between 1930-2030. As per RFC5280,
        // dates must be between 1950-2049. In .NET, years between 30 and 50 are treated
        // as 1930-1950, while it should be 2030-2050. So, fix the range between 30 and 50
        // by adding a century.
        var dateTime = DateTime.ParseExact(strValue, format, null);
        // not inclusive. Starting with 2050, GeneralizedTime is used, so 50+ values will go
        // to 20th century as in .NET
        if (dateTime.Year < 1950) {
            dateTime = dateTime.AddYears(100);
        }
        return dateTime;
    }
    static DateTime extractDateTime(String strValue, Int32 msDelimiter, Int32 zoneDelimiter) {
        String rawString;
        if (msDelimiter < 0 && zoneDelimiter < 0) {
            // Zulu time zone, no milliseconds
            rawString = strValue;
        } else if (msDelimiter < 0) {
            // Custom time zone, no milliseconds
            rawString = strValue.Substring(0, zoneDelimiter);
        } else {
            // Milliseconds
            rawString = strValue.Substring(0, msDelimiter);
        }

        return rawString.Length switch {
            12 => parseExactUtc(rawString, UTCFormat),
            14 => DateTime.ParseExact(rawString, GtFormat, null),
            _  => throw new ArgumentException("Time zone suffix is not valid.")
        };
    }
    static TimeZoneInfo bindZone(Int32 hours, Int32 minutes) {
        foreach (TimeZoneInfo zone in TimeZoneInfo.GetSystemTimeZones().Where(zone => zone.BaseUtcOffset.Hours == hours && zone.BaseUtcOffset.Minutes == minutes)) {
            return zone;
        }
        return TimeZoneInfo.FindSystemTimeZoneById("Greenwich Standard Time");
    }

    #region Constants
    const String UTCFormat        = "yyMMddHHmmss";
    const String UTCPreciseFormat = "yyMMddHHmmss.FFF";
    const String GtFormat         = "yyyyMMddHHmmss";
    const String GtPreciseFormat  = "yyyyMMddHHmmss.FFF";
    #endregion
}