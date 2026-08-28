using ModbusSim.Core;
using ModbusSim.Core.Protocol;

namespace ModbusSim.Core.Tests;

public class ModbusRequestTests
{
    [Fact]
    public void Parses_read_holding_registers()
    {
        var r = ModbusRequest.Parse([0x03, 0x00, 0x6B, 0x00, 0x03]);

        Assert.Equal(ModbusFunction.ReadHoldingRegisters, r.Function);
        Assert.Equal(0x6B, r.Address);
        Assert.Equal(3, r.Quantity);
        Assert.False(r.IsWrite);
    }

    [Fact]
    public void Parses_write_single_register()
    {
        var r = ModbusRequest.Parse([0x06, 0x00, 0x01, 0x00, 0x03]);

        Assert.Equal(ModbusFunction.WriteSingleRegister, r.Function);
        Assert.Equal(1, r.Address);
        Assert.Equal<ushort>(3, r.Registers![0]);
    }

    [Fact]
    public void Parses_write_single_coil_on_and_off()
    {
        Assert.True(ModbusRequest.Parse([0x05, 0x00, 0x02, 0xFF, 0x00]).Coils![0]);
        Assert.False(ModbusRequest.Parse([0x05, 0x00, 0x02, 0x00, 0x00]).Coils![0]);
    }

    [Fact]
    public void Rejects_write_single_coil_bad_value()
    {
        var ex = Assert.Throws<ModbusProtocolException>(() => ModbusRequest.Parse([0x05, 0x00, 0x02, 0x12, 0x34]));
        Assert.Equal(ModbusExceptionCode.IllegalDataValue, ex.Code);
    }

    [Fact]
    public void Parses_write_multiple_registers()
    {
        var r = ModbusRequest.Parse([0x10, 0x00, 0x01, 0x00, 0x02, 0x04, 0x00, 0x0A, 0x01, 0x02]);

        Assert.Equal(ModbusFunction.WriteMultipleRegisters, r.Function);
        Assert.Equal(1, r.Address);
        Assert.Equal([(ushort)10, (ushort)0x0102], r.Registers!);
    }

    [Fact]
    public void Parses_write_multiple_coils()
    {
        // start 0x0013, qty 10, data 0xCD 0x01 => bits 1,0,1,1,0,0,1,1, 1,0
        var r = ModbusRequest.Parse([0x0F, 0x00, 0x13, 0x00, 0x0A, 0x02, 0xCD, 0x01]);

        Assert.Equal(0x13, r.Address);
        Assert.Equal(10, r.Quantity);
        Assert.Equal(
            [true, false, true, true, false, false, true, true, true, false],
            r.Coils!);
    }

    [Fact]
    public void Rejects_unknown_function()
    {
        var ex = Assert.Throws<ModbusProtocolException>(() => ModbusRequest.Parse([0x07]));
        Assert.Equal(ModbusExceptionCode.IllegalFunction, ex.Code);
    }

    [Fact]
    public void Rejects_truncated_read()
    {
        var ex = Assert.Throws<ModbusProtocolException>(() => ModbusRequest.Parse([0x03, 0x00, 0x6B, 0x00]));
        Assert.Equal(ModbusExceptionCode.IllegalDataValue, ex.Code);
    }

    [Theory]
    [InlineData(ModbusFunction.ReadCoils, (ushort)0x0013, 37)]
    [InlineData(ModbusFunction.ReadInputRegisters, (ushort)0x0008, 1)]
    public void Read_request_round_trips_through_pdu(ModbusFunction fn, ushort addr, int qty)
    {
        var original = ModbusRequest.Read(fn, addr, qty);
        var reparsed = ModbusRequest.Parse(original.ToPdu());

        Assert.Equal(fn, reparsed.Function);
        Assert.Equal(addr, reparsed.Address);
        Assert.Equal(qty, reparsed.Quantity);
    }

    [Fact]
    public void Write_multiple_registers_round_trips_through_pdu()
    {
        var original = new ModbusRequest
        {
            Function = ModbusFunction.WriteMultipleRegisters,
            Address = 100,
            Registers = [1, 2, 65535],
        };

        var reparsed = ModbusRequest.Parse(original.ToPdu());
        Assert.Equal([(ushort)1, (ushort)2, (ushort)65535], reparsed.Registers!);
    }
}
