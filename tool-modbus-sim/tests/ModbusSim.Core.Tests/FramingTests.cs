using ModbusSim.Core.Framing;

namespace ModbusSim.Core.Tests;

public class FramingTests
{
    [Fact]
    public async Task Tcp_frame_round_trips_through_a_stream()
    {
        var frame = new ModbusFrame(UnitId: 17, Pdu: [0x03, 0x00, 0x00, 0x00, 0x02], TransactionId: 0xBEEF);
        var bytes = TcpFrameCodec.Encode(frame);

        using var stream = new MemoryStream(bytes);
        var read = await TcpFrameCodec.ReadAsync(stream, CancellationToken.None);

        Assert.NotNull(read);
        Assert.Equal(17, read!.Value.UnitId);
        Assert.Equal(0xBEEF, read.Value.TransactionId);
        Assert.Equal(frame.Pdu, read.Value.Pdu);
    }

    [Fact]
    public async Task Tcp_read_returns_null_on_clean_eof()
    {
        using var stream = new MemoryStream([]);
        Assert.Null(await TcpFrameCodec.ReadAsync(stream, CancellationToken.None));
    }

    [Fact]
    public async Task Tcp_read_throws_on_partial_header()
    {
        using var stream = new MemoryStream([0x00, 0x01, 0x00]);
        await Assert.ThrowsAsync<InvalidDataException>(
            async () => await TcpFrameCodec.ReadAsync(stream, CancellationToken.None));
    }

    [Fact]
    public void Rtu_frame_round_trips()
    {
        var frame = new ModbusFrame(UnitId: 1, Pdu: [0x03, 0x00, 0x00, 0x00, 0x0A]);
        var bytes = RtuFrameCodec.Encode(frame);

        Assert.True(RtuFrameCodec.TryDecode(bytes, out var decoded));
        Assert.Equal(1, decoded.UnitId);
        Assert.Equal(frame.Pdu, decoded.Pdu);
    }

    [Fact]
    public void Rtu_decode_rejects_bad_crc()
    {
        var bytes = RtuFrameCodec.Encode(new ModbusFrame(1, [0x03, 0x00, 0x00, 0x00, 0x0A]));
        bytes[^1] ^= 0xFF;
        Assert.False(RtuFrameCodec.TryDecode(bytes, out _));
    }
}
