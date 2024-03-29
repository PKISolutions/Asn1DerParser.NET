﻿using System;

namespace SysadminsLV.Asn1Parser;

/// <summary>
/// Contains values that specify how line breaks are handled during byte array formatting.
/// </summary>
[Flags]
public enum EncodingFormat : UInt32 {
    /// <summary>
    /// Appends a carriage return/line feed (CR/LF) pair (0x0D/0x0A) to the encoded string.
    /// </summary>
    CRLF = 0,
    /// <summary>
    /// Do not append any new line characters to the encoded string. The default behavior is to use a carriage return/line
    /// feed (CR/LF) pair (0x0D/0x0A) to represent a new line.
    /// </summary>
    /// <remarks>
    ///     This flag is ignored when encoding binary to string using <strong>HexAddress</strong>,
    ///     <strong>HexAscii</strong> or <strong>HexAsciiAddress</strong> encoding type.
    /// </remarks>
    NOCRLF = 0x40000000,
    /// <summary>
    /// Only use the line feed (LF) character (0x0A) for a new line. The default behavior is to use a CR/LF pair
    /// (0x0D/0x0A) to represent a new line.
    /// </summary>
    NOCR = 0x80000000
}