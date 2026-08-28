namespace ModbusSim.Core.Framing;

/// <summary>Modbus RTU framing: unit id + PDU + CRC-16, delimited on the wire by idle time.</summary>
public static class RtuFrameCodec
{
    public const int MinFrameLength = 4; // unit id + 1-byte pdu + 2-byte crc

    /// <summary>Builds <c>[unitId][pdu][crcLo][crcHi]</c>.</summary>
    public static byte[] Encode(in ModbusFrame frame)
    {
        var body = new byte[1 + frame.Pdu.Length];
        body[0] = frame.UnitId;
        frame.Pdu.CopyTo(body, 1);
        return Crc16.Append(body);
    }

    /// <summary>
    /// Validates the CRC of a received RTU frame and splits out the unit id and PDU.
    /// </summary>
    public static bool TryDecode(ReadOnlySpan<byte> frame, out ModbusFrame result)
    {
        result = default;
        if (frame.Length < MinFrameLength || !Crc16.Validate(frame))
            return false;
        result = new ModbusFrame(frame[0], frame[1..^2].ToArray());
        return true;
    }
}
