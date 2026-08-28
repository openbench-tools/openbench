namespace ModbusSim.Core.Framing;

/// <summary>
/// A transport-independent Modbus frame: a unit id, a PDU, and (TCP only) a
/// transaction id that the reply must echo.
/// </summary>
public readonly record struct ModbusFrame(byte UnitId, byte[] Pdu, ushort TransactionId = 0);
