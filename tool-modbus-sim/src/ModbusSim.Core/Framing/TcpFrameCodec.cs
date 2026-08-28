using ModbusSim.Core.Protocol;

namespace ModbusSim.Core.Framing;

/// <summary>MBAP (Modbus TCP) framing: 7-byte header + PDU.</summary>
public static class TcpFrameCodec
{
    public const int HeaderLength = 7;
    public const int MaxPduLength = 253;

    /// <summary>Wraps a PDU in an MBAP header.</summary>
    public static byte[] Encode(in ModbusFrame frame)
    {
        var buffer = new byte[HeaderLength + frame.Pdu.Length];
        BigEndian.WriteUInt16(buffer, 0, frame.TransactionId);
        BigEndian.WriteUInt16(buffer, 2, 0); // protocol id
        BigEndian.WriteUInt16(buffer, 4, (ushort)(frame.Pdu.Length + 1)); // unit id + pdu
        buffer[6] = frame.UnitId;
        frame.Pdu.CopyTo(buffer, HeaderLength);
        return buffer;
    }

    /// <summary>
    /// Reads one MBAP frame from <paramref name="stream"/>. Returns <c>null</c> on a
    /// clean end of stream (peer closed between frames).
    /// </summary>
    /// <exception cref="InvalidDataException">The header is structurally invalid or the stream ended mid-frame.</exception>
    public static async Task<ModbusFrame?> ReadAsync(Stream stream, CancellationToken ct)
    {
        var header = new byte[HeaderLength];
        int got = await ReadAtLeastAsync(stream, header, required: true, ct).ConfigureAwait(false);
        if (got == 0)
            return null;

        ushort protocolId = BigEndian.ReadUInt16(header, 2);
        int length = BigEndian.ReadUInt16(header, 4);
        if (protocolId != 0)
            throw new InvalidDataException($"Unexpected MBAP protocol id {protocolId}.");
        if (length is < 2 or > MaxPduLength + 1)
            throw new InvalidDataException($"MBAP length field {length} out of range.");

        var pdu = new byte[length - 1];
        await ReadAtLeastAsync(stream, pdu, required: false, ct).ConfigureAwait(false);

        return new ModbusFrame(header[6], pdu, BigEndian.ReadUInt16(header, 0));
    }

    private static async Task<int> ReadAtLeastAsync(Stream stream, byte[] buffer, bool required, CancellationToken ct)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int n = await stream.ReadAsync(buffer.AsMemory(offset), ct).ConfigureAwait(false);
            if (n == 0)
            {
                if (offset == 0 && required)
                    return 0; // clean EOF at a frame boundary
                throw new InvalidDataException("Stream ended mid-frame.");
            }
            offset += n;
        }
        return offset;
    }
}
