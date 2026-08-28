using System.Buffers.Binary;

namespace ModbusSim.Core.Protocol;

/// <summary>Big-endian (network order) helpers — Modbus wire format for 16-bit fields.</summary>
internal static class BigEndian
{
    public static ushort ReadUInt16(ReadOnlySpan<byte> src, int offset) =>
        BinaryPrimitives.ReadUInt16BigEndian(src.Slice(offset, 2));

    public static void WriteUInt16(Span<byte> dst, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16BigEndian(dst.Slice(offset, 2), value);
}
