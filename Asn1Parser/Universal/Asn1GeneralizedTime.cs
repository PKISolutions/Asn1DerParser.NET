﻿using System;

namespace SysadminsLV.Asn1Parser.Universal;

/// <summary>
/// Represents an ASN.1 <strong>GeneralizedTime</strong> data type.
/// </summary>
public sealed class Asn1GeneralizedTime : Asn1DateTime {
    const Asn1Type TYPE = Asn1Type.GeneralizedTime;

    /// <summary>
    /// Initializes a new instance of the <strong>Asn1GeneralizedTime</strong> class from a date time object
    /// to encode and value that indicates whether to include millisecond information.
    /// </summary>
    /// <param name="time">A <see cref="DateTime"/> object in local time zone.</param>
    /// <param name="preciseTime">
    /// <strong>True</strong> if encoded value should contain millisecond information, otherwise <strong>False</strong>.
    /// </param>
    public Asn1GeneralizedTime(DateTime time, Boolean preciseTime) : this(time, null, preciseTime) { }
    /// <summary>
    /// Initializes a new instance of the <strong>Asn1GeneralizedTime</strong> class from a date time object
    /// to encode, time zone information and value that indicates whether to include millisecond information.
    /// </summary>
    /// <param name="time">
    ///     A <see cref="DateTime"/> object in destination time zone if zone information is provided.
    ///     Local time zone is assumed if zone information is not provided (null).
    /// </param>
    /// <param name="zone">A <see cref="TimeZoneInfo"/> object that represents time zone information.</param>
    /// <param name="preciseTime">
    /// <strong>True</strong> if encoded value should contain millisecond information, otherwise <strong>False</strong>.
    /// </param>
    public Asn1GeneralizedTime(DateTime time, TimeZoneInfo? zone = null, Boolean preciseTime = false) : base(TYPE, time, zone, preciseTime) { }
    /// <summary>
    /// Initializes a new instance of the <strong>Asn1GeneralizedTime</strong> class from an existing
    /// <see cref="Asn1Reader"/> object.
    /// </summary>
    /// <param name="asn"><see cref="Asn1Reader"/> object in the position that represents Generalized Time.</param>
    /// <exception cref="Asn1InvalidTagException">
    /// The current state of <strong>ASN1</strong> object is not Generalized Time.
    /// </exception>
    public Asn1GeneralizedTime(Asn1Reader asn) : base(asn, TYPE) { }
    /// <summary>
    /// Initializes a new instance of the <strong>Asn1GeneralizedTime</strong> class from a byte array that
    /// represents encoded UTC time.
    /// </summary>
    /// <param name="rawData">ASN.1-encoded byte array.</param>
    /// <exception cref="Asn1InvalidTagException">
    /// The current state of <strong>ASN1</strong> object is not Generalized Time.
    /// </exception>
    public Asn1GeneralizedTime(Byte[] rawData) : this(new Asn1Reader(rawData)) { }
}