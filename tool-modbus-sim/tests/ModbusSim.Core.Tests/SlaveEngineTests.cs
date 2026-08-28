using ModbusSim.Core;
using ModbusSim.Core.Protocol;

namespace ModbusSim.Core.Tests;

public class SlaveEngineTests
{
    private static SlaveEngine NewEngine(out ModbusDataStore store)
    {
        store = new ModbusDataStore();
        return new SlaveEngine(store);
    }

    [Fact]
    public void Read_holding_registers_returns_store_values()
    {
        var engine = NewEngine(out var store);
        store.SetRegister(ModbusTable.HoldingRegisters, 10, 1111);
        store.SetRegister(ModbusTable.HoldingRegisters, 11, 2222);

        var resp = engine.Process(ModbusRequest.Read(ModbusFunction.ReadHoldingRegisters, 10, 2));

        Assert.False(resp.IsException);
        Assert.Equal([(ushort)1111, (ushort)2222], resp.Registers!);
    }

    [Fact]
    public void Read_past_end_of_space_is_illegal_data_address()
    {
        var engine = NewEngine(out _);

        var resp = engine.Process(ModbusRequest.Read(ModbusFunction.ReadHoldingRegisters, 65535, 2));

        Assert.True(resp.IsException);
        Assert.Equal(ModbusExceptionCode.IllegalDataAddress, resp.Exception);
    }

    [Fact]
    public void Read_too_many_registers_is_illegal_data_value()
    {
        var engine = NewEngine(out _);

        var resp = engine.Process(ModbusRequest.Read(ModbusFunction.ReadHoldingRegisters, 0, 126));

        Assert.True(resp.IsException);
        Assert.Equal(ModbusExceptionCode.IllegalDataValue, resp.Exception);
    }

    [Fact]
    public void Write_single_register_updates_store_and_echoes()
    {
        var engine = NewEngine(out var store);

        var resp = engine.Process(new ModbusRequest
        {
            Function = ModbusFunction.WriteSingleRegister,
            Address = 7,
            Registers = [4242],
        });

        Assert.False(resp.IsException);
        Assert.Equal<ushort>(4242, store.GetRegister(ModbusTable.HoldingRegisters, 7));
        Assert.Equal(7, resp.Address);
    }

    [Fact]
    public void Write_multiple_coils_updates_store()
    {
        var engine = NewEngine(out var store);

        var resp = engine.Process(new ModbusRequest
        {
            Function = ModbusFunction.WriteMultipleCoils,
            Address = 0,
            Quantity = 3,
            Coils = [true, false, true],
        });

        Assert.False(resp.IsException);
        Assert.True(store.GetBit(ModbusTable.Coils, 0));
        Assert.False(store.GetBit(ModbusTable.Coils, 1));
        Assert.True(store.GetBit(ModbusTable.Coils, 2));
        Assert.Equal(3, resp.Quantity);
    }

    [Fact]
    public void Write_raises_changed_event_flagged_from_wire()
    {
        var engine = NewEngine(out var store);
        DataStoreChangedEventArgs? seen = null;
        store.Changed += (_, e) => seen = e;

        engine.Process(new ModbusRequest
        {
            Function = ModbusFunction.WriteSingleCoil,
            Address = 5,
            Coils = [true],
        });

        Assert.NotNull(seen);
        Assert.Equal(ModbusTable.Coils, seen!.Table);
        Assert.True(seen.FromWire);
    }

    [Fact]
    public void Response_pdu_round_trips_to_master_view()
    {
        var engine = NewEngine(out var store);
        store.SetRegister(ModbusTable.InputRegisters, 8, 999);
        var request = ModbusRequest.Read(ModbusFunction.ReadInputRegisters, 8, 1);

        var pdu = engine.Process(request).ToPdu();
        var decoded = ModbusResponse.ParseReply(request, pdu);

        Assert.Equal<ushort>(999, decoded.Registers![0]);
    }
}
