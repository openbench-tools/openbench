namespace ModbusSim.Core.Protocol;

/// <summary>Packs/unpacks coil and discrete-input bits into the LSB-first byte layout Modbus uses.</summary>
internal static class BitPacker
{
    public static byte[] Pack(ReadOnlySpan<bool> bits)
    {
        int byteCount = (bits.Length + 7) / 8;
        var bytes = new byte[byteCount];
        for (int i = 0; i < bits.Length; i++)
            if (bits[i])
                bytes[i / 8] |= (byte)(1 << (i % 8));
        return bytes;
    }

    public static bool[] Unpack(ReadOnlySpan<byte> bytes, int count)
    {
        var bits = new bool[count];
        for (int i = 0; i < count; i++)
            bits[i] = (bytes[i / 8] & (1 << (i % 8))) != 0;
        return bits;
    }
}
