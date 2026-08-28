namespace ModbusSim.Core;

/// <summary>
/// Thrown while processing a request when the Modbus spec requires an exception
/// response. The <see cref="Code"/> is encoded into the response PDU.
/// </summary>
public sealed class ModbusProtocolException(ModbusExceptionCode code, string? message = null)
    : Exception(message ?? $"Modbus exception {(byte)code:X2} ({code})")
{
    public ModbusExceptionCode Code { get; } = code;
}
