namespace ModbusSim.Core;

/// <summary>CRC-16/MODBUS (poly 0xA001, init 0xFFFF), transmitted low byte first.</summary>
public static class Crc16
{
    private static readonly ushort[] Table = BuildTable();

    private static ushort[] BuildTable()
    {
        var table = new ushort[256];
        for (ushort i = 0; i < 256; i++)
        {
            ushort crc = i;
            for (int bit = 0; bit < 8; bit++)
                crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
            table[i] = crc;
        }
        return table;
    }

    /// <summary>Computes the CRC over <paramref name="data"/>.</summary>
    public static ushort Compute(ReadOnlySpan<byte> data)
    {
        ushort crc = 0xFFFF;
        foreach (byte b in data)
            crc = (ushort)((crc >> 8) ^ Table[(crc ^ b) & 0xFF]);
        return crc;
    }

    /// <summary>True when the trailing two bytes of <paramref name="frame"/> are a valid CRC over the rest.</summary>
    public static bool Validate(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 3)
            return false;
        ushort expected = Compute(frame[..^2]);
        ushort actual = (ushort)(frame[^2] | (frame[^1] << 8));
        return expected == actual;
    }

    /// <summary>Appends the little-endian CRC to <paramref name="body"/>.</summary>
    public static byte[] Append(ReadOnlySpan<byte> body)
    {
        var result = new byte[body.Length + 2];
        body.CopyTo(result);
        ushort crc = Compute(body);
        result[^2] = (byte)(crc & 0xFF);
        result[^1] = (byte)(crc >> 8);
        return result;
    }
}
