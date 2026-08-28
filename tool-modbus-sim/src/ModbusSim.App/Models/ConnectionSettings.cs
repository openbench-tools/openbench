using System.IO.Ports;
using ModbusSim.Core;
using ModbusSim.Core.Runtime;

namespace ModbusSim.App.Models;

/// <summary>Plain option lists surfaced in the connection panel.</summary>
public static class SettingsCatalog
{
    public static IReadOnlyList<int> BaudRates { get; } =
        [1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200];

    public static IReadOnlyList<Parity> Parities { get; } =
        [Parity.None, Parity.Even, Parity.Odd];

    public static IReadOnlyList<int> DataBitOptions { get; } = [7, 8];

    public static IReadOnlyList<StopBits> StopBitOptions { get; } =
        [StopBits.One, StopBits.Two];

    public static IReadOnlyList<ModbusTable> Tables { get; } =
        [ModbusTable.Coils, ModbusTable.DiscreteInputs, ModbusTable.HoldingRegisters, ModbusTable.InputRegisters];

    public static string[] AvailablePorts() => SerialFrameChannel.AvailablePorts();
}
