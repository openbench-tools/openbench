using ModbusSim.Core.Protocol;

namespace ModbusSim.Core.Runtime;

/// <summary>One request the master issues every poll cycle.</summary>
public sealed record PollDefinition
{
    public required ModbusFunction Function { get; init; }
    public required ushort Address { get; init; }
    public int Quantity { get; init; } = 1;

    /// <summary>Values for a register write function.</summary>
    public IReadOnlyList<ushort>? WriteRegisters { get; init; }

    /// <summary>Values for a coil write function.</summary>
    public IReadOnlyList<bool>? WriteCoils { get; init; }

    /// <summary>The local table that a read result is mirrored into; <c>null</c> for writes.</summary>
    public ModbusTable? ResultTable => Function switch
    {
        ModbusFunction.ReadCoils => ModbusTable.Coils,
        ModbusFunction.ReadDiscreteInputs => ModbusTable.DiscreteInputs,
        ModbusFunction.ReadHoldingRegisters => ModbusTable.HoldingRegisters,
        ModbusFunction.ReadInputRegisters => ModbusTable.InputRegisters,
        _ => null,
    };

    public ModbusRequest ToRequest() => new()
    {
        Function = Function,
        Address = Address,
        Quantity = Quantity,
        Coils = WriteCoils,
        Registers = WriteRegisters,
    };

    public static PollDefinition Read(ModbusFunction function, ushort address, int quantity) =>
        new() { Function = function, Address = address, Quantity = quantity };
}
