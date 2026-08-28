namespace ModbusSim.Core.Protocol;

/// <summary>
/// A Modbus response PDU. The slave builds one and calls <see cref="ToPdu"/>; the
/// master calls <see cref="ParseReply"/> to decode what came back.
/// </summary>
public sealed class ModbusResponse
{
    public required ModbusFunction Function { get; init; }

    public ModbusExceptionCode Exception { get; init; } = ModbusExceptionCode.None;

    public bool IsException => Exception != ModbusExceptionCode.None;

    /// <summary>Bit values for coil / discrete-input read replies.</summary>
    public IReadOnlyList<bool>? Coils { get; init; }

    /// <summary>Register values for holding / input register read replies.</summary>
    public IReadOnlyList<ushort>? Registers { get; init; }

    /// <summary>Echoed address for write replies.</summary>
    public ushort Address { get; init; }

    /// <summary>Echoed quantity for multi-write replies.</summary>
    public int Quantity { get; init; }

    public static ModbusResponse FromException(ModbusFunction function, ModbusExceptionCode code) =>
        new() { Function = function, Exception = code };

    // -- encode (slave side) --------------------------------------------------

    public byte[] ToPdu()
    {
        if (IsException)
            return [(byte)((byte)Function | 0x80), (byte)Exception];

        switch (Function)
        {
            case ModbusFunction.ReadCoils:
            case ModbusFunction.ReadDiscreteInputs:
            {
                var bits = Coils ?? throw new InvalidOperationException("Coils required.");
                var packed = BitPacker.Pack(bits.ToArray());
                var pdu = new byte[2 + packed.Length];
                pdu[0] = (byte)Function;
                pdu[1] = (byte)packed.Length;
                packed.CopyTo(pdu, 2);
                return pdu;
            }

            case ModbusFunction.ReadHoldingRegisters:
            case ModbusFunction.ReadInputRegisters:
            {
                var regs = Registers ?? throw new InvalidOperationException("Registers required.");
                var pdu = new byte[2 + regs.Count * 2];
                pdu[0] = (byte)Function;
                pdu[1] = (byte)(regs.Count * 2);
                for (int i = 0; i < regs.Count; i++)
                    BigEndian.WriteUInt16(pdu, 2 + i * 2, regs[i]);
                return pdu;
            }

            case ModbusFunction.WriteSingleCoil:
            {
                var pdu = new byte[5];
                pdu[0] = (byte)Function;
                BigEndian.WriteUInt16(pdu, 1, Address);
                BigEndian.WriteUInt16(pdu, 3, (Coils?[0] ?? false) ? (ushort)0xFF00 : (ushort)0x0000);
                return pdu;
            }

            case ModbusFunction.WriteSingleRegister:
            {
                var pdu = new byte[5];
                pdu[0] = (byte)Function;
                BigEndian.WriteUInt16(pdu, 1, Address);
                BigEndian.WriteUInt16(pdu, 3, Registers?[0] ?? 0);
                return pdu;
            }

            case ModbusFunction.WriteMultipleCoils:
            case ModbusFunction.WriteMultipleRegisters:
            {
                var pdu = new byte[5];
                pdu[0] = (byte)Function;
                BigEndian.WriteUInt16(pdu, 1, Address);
                BigEndian.WriteUInt16(pdu, 3, (ushort)Quantity);
                return pdu;
            }

            default:
                throw new InvalidOperationException($"Cannot encode response for {Function}.");
        }
    }

    // -- decode (master side) -----------------------------------------------

    /// <summary>Decodes a response PDU in the context of the request that produced it.</summary>
    /// <exception cref="ModbusProtocolException">The reply is malformed or inconsistent with the request.</exception>
    public static ModbusResponse ParseReply(ModbusRequest request, ReadOnlySpan<byte> pdu)
    {
        if (pdu.Length < 2)
            throw new ModbusProtocolException(ModbusExceptionCode.ServerDeviceFailure, "Truncated response.");

        byte fc = pdu[0];
        if (fc == ((byte)request.Function | 0x80))
            return FromException(request.Function, (ModbusExceptionCode)pdu[1]);

        if (fc != (byte)request.Function)
            throw new ModbusProtocolException(ModbusExceptionCode.ServerDeviceFailure,
                $"Response function 0x{fc:X2} does not match request 0x{(byte)request.Function:X2}.");

        switch (request.Function)
        {
            case ModbusFunction.ReadCoils:
            case ModbusFunction.ReadDiscreteInputs:
            {
                int byteCount = pdu[1];
                if (pdu.Length != 2 + byteCount || byteCount != (request.Quantity + 7) / 8)
                    throw Malformed("bit read reply length");
                return new ModbusResponse
                {
                    Function = request.Function,
                    Coils = BitPacker.Unpack(pdu.Slice(2, byteCount), request.Quantity),
                };
            }

            case ModbusFunction.ReadHoldingRegisters:
            case ModbusFunction.ReadInputRegisters:
            {
                int byteCount = pdu[1];
                if (pdu.Length != 2 + byteCount || byteCount != request.Quantity * 2)
                    throw Malformed("register read reply length");
                var regs = new ushort[request.Quantity];
                for (int i = 0; i < regs.Length; i++)
                    regs[i] = BigEndian.ReadUInt16(pdu, 2 + i * 2);
                return new ModbusResponse { Function = request.Function, Registers = regs };
            }

            case ModbusFunction.WriteSingleCoil:
            case ModbusFunction.WriteSingleRegister:
            case ModbusFunction.WriteMultipleCoils:
            case ModbusFunction.WriteMultipleRegisters:
            {
                if (pdu.Length != 5)
                    throw Malformed("write reply length");
                return new ModbusResponse
                {
                    Function = request.Function,
                    Address = BigEndian.ReadUInt16(pdu, 1),
                    Quantity = BigEndian.ReadUInt16(pdu, 3),
                };
            }

            default:
                throw Malformed("unsupported function");
        }
    }

    private static ModbusProtocolException Malformed(string what) =>
        new(ModbusExceptionCode.ServerDeviceFailure, $"Malformed response ({what}).");
}
