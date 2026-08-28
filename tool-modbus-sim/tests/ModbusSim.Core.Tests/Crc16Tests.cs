using ModbusSim.Core;

namespace ModbusSim.Core.Tests;

public class Crc16Tests
{
    // From the Modbus over Serial Line spec: CRC of {0x02, 0x07} is 0x1241,
    // transmitted low byte (0x41) first.
    [Fact]
    public void Compute_matches_spec_example()
    {
        Assert.Equal(0x1241, Crc16.Compute([0x02, 0x07]));
    }

    [Fact]
    public void Append_puts_low_byte_first()
    {
        Assert.Equal([0x02, 0x07, 0x41, 0x12], Crc16.Append([0x02, 0x07]));
    }

    [Fact]
    public void Validate_accepts_good_frame_and_rejects_tampered()
    {
        var frame = Crc16.Append([0x01, 0x03, 0x00, 0x00, 0x00, 0x0A]);
        Assert.True(Crc16.Validate(frame));

        frame[2] ^= 0xFF;
        Assert.False(Crc16.Validate(frame));
    }
}
