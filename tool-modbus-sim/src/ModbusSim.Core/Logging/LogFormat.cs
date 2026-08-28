using ModbusSim.Core.Protocol;

namespace ModbusSim.Core.Logging;

/// <summary>Turns requests and responses into short human-readable log detail strings.</summary>
public static class LogFormat
{
    public static string Request(ModbusRequest r) => r.Function switch
    {
        ModbusFunction.ReadCoils or ModbusFunction.ReadDiscreteInputs
            or ModbusFunction.ReadHoldingRegisters or ModbusFunction.ReadInputRegisters
            => $"addr {r.Address} x{r.Quantity}",
        ModbusFunction.WriteSingleCoil => $"addr {r.Address} = {Bit(r.Coils?[0] ?? false)}",
        ModbusFunction.WriteSingleRegister => $"addr {r.Address} = {r.Registers?[0]}",
        ModbusFunction.WriteMultipleCoils => $"addr {r.Address} = {Bits(r.Coils)}",
        ModbusFunction.WriteMultipleRegisters => $"addr {r.Address} = {Regs(r.Registers)}",
        _ => $"addr {r.Address}",
    };

    public static string Response(ModbusResponse r)
    {
        if (r.IsException)
            return $"EXCEPTION {r.Exception}";
        if (r.Coils is { } c)
            return Bits(c);
        if (r.Registers is { } regs)
            return Regs(regs);
        return $"ok (addr {r.Address}{(r.Quantity > 0 ? $" x{r.Quantity}" : "")})";
    }

    private static string Bit(bool b) => b ? "1" : "0";

    private static string Bits(IReadOnlyList<bool>? bits) =>
        bits is null ? "[]" : "[" + string.Join(",", bits.Select(b => b ? "1" : "0")) + "]";

    private static string Regs(IReadOnlyList<ushort>? regs) =>
        regs is null ? "[]" : "[" + string.Join(",", regs) + "]";
}
