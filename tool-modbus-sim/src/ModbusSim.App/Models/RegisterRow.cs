using CommunityToolkit.Mvvm.ComponentModel;
using ModbusSim.Core;

namespace ModbusSim.App.Models;

/// <summary>One editable row in the register grid.</summary>
public partial class RegisterRow : ObservableObject
{
    public RegisterRow(ModbusTable table, ushort address)
    {
        Table = table;
        Address = address;
    }

    public ModbusTable Table { get; }
    public ushort Address { get; }

    public bool IsBitTable => Table is ModbusTable.Coils or ModbusTable.DiscreteInputs;

    /// <summary>Conventional Modbus reference number (e.g. holding register 0 shows as 40001).</summary>
    public string Reference => Table switch
    {
        ModbusTable.Coils => $"{Address + 1:00000}",
        ModbusTable.DiscreteInputs => $"1{Address + 1:0000}",
        ModbusTable.InputRegisters => $"3{Address + 1:0000}",
        ModbusTable.HoldingRegisters => $"4{Address + 1:0000}",
        _ => Address.ToString(),
    };

    [ObservableProperty]
    public partial ushort Value { get; set; }

    [ObservableProperty]
    public partial bool BitValue { get; set; }

    [ObservableProperty]
    public partial bool RecentlyChanged { get; set; }

    /// <summary>Hex view of a register value; blank for bit tables.</summary>
    public string HexValue => IsBitTable ? "" : $"0x{Value:X4}";

    partial void OnValueChanged(ushort value) => OnPropertyChanged(nameof(HexValue));
}
