namespace ModbusSim.Core.Protocol;

/// <summary>
/// A decoded Modbus request PDU. Also builds request PDUs for the master side
/// via <see cref="ToPdu"/>.
/// </summary>
public sealed class ModbusRequest
{
    public required ModbusFunction Function { get; init; }

    /// <summary>Start address (reads / multi-writes) or single-item address.</summary>
    public required ushort Address { get; init; }

    /// <summary>Item count for reads and multi-writes; 1 for single writes.</summary>
    public int Quantity { get; init; } = 1;

    /// <summary>Bit values for coil writes (single = length 1).</summary>
    public IReadOnlyList<bool>? Coils { get; init; }

    /// <summary>Register values for register writes (single = length 1).</summary>
    public IReadOnlyList<ushort>? Registers { get; init; }

    public bool IsWrite => Function is ModbusFunction.WriteSingleCoil or ModbusFunction.WriteSingleRegister
        or ModbusFunction.WriteMultipleCoils or ModbusFunction.WriteMultipleRegisters;

    /// <summary>Builds a read request for one of the four read function codes.</summary>
    public static ModbusRequest Read(ModbusFunction function, ushort address, int quantity)
    {
        if (function is not (ModbusFunction.ReadCoils or ModbusFunction.ReadDiscreteInputs
            or ModbusFunction.ReadHoldingRegisters or ModbusFunction.ReadInputRegisters))
            throw new ArgumentOutOfRangeException(nameof(function), function, "Not a read function.");
        return new ModbusRequest { Function = function, Address = address, Quantity = quantity };
    }

    // -- decode -----------------------------------------------------------

    /// <summary>Parses a request PDU (function code + data, no MBAP / slave id / CRC).</summary>
    /// <exception cref="ModbusProtocolException">The PDU is malformed or the function is unsupported.</exception>
    public static ModbusRequest Parse(ReadOnlySpan<byte> pdu)
    {
        if (pdu.Length < 1)
            throw new ModbusProtocolException(ModbusExceptionCode.ServerDeviceFailure, "Empty PDU.");

        var fc = (ModbusFunction)pdu[0];
        switch (fc)
        {
            case ModbusFunction.ReadCoils:
            case ModbusFunction.ReadDiscreteInputs:
            case ModbusFunction.ReadHoldingRegisters:
            case ModbusFunction.ReadInputRegisters:
            {
                Expect(pdu.Length == 5, "read request length");
                return new ModbusRequest
                {
                    Function = fc,
                    Address = BigEndian.ReadUInt16(pdu, 1),
                    Quantity = BigEndian.ReadUInt16(pdu, 3),
                };
            }

            case ModbusFunction.WriteSingleCoil:
            {
                Expect(pdu.Length == 5, "write single coil length");
                ushort raw = BigEndian.ReadUInt16(pdu, 3);
                bool on = raw switch
                {
                    0xFF00 => true,
                    0x0000 => false,
                    _ => throw new ModbusProtocolException(ModbusExceptionCode.IllegalDataValue,
                        "Write single coil value must be 0xFF00 or 0x0000."),
                };
                return new ModbusRequest { Function = fc, Address = BigEndian.ReadUInt16(pdu, 1), Coils = new[] { on } };
            }

            case ModbusFunction.WriteSingleRegister:
            {
                Expect(pdu.Length == 5, "write single register length");
                return new ModbusRequest
                {
                    Function = fc,
                    Address = BigEndian.ReadUInt16(pdu, 1),
                    Registers = new[] { BigEndian.ReadUInt16(pdu, 3) },
                };
            }

            case ModbusFunction.WriteMultipleCoils:
            {
                Expect(pdu.Length >= 7, "write multiple coils length");
                ushort start = BigEndian.ReadUInt16(pdu, 1);
                int qty = BigEndian.ReadUInt16(pdu, 3);
                int byteCount = pdu[5];
                Expect(qty is >= 1 and <= ModbusDataStore.MaxWriteBits, "coil quantity");
                Expect(byteCount == (qty + 7) / 8 && pdu.Length == 6 + byteCount, "coil byte count");
                return new ModbusRequest
                {
                    Function = fc,
                    Address = start,
                    Quantity = qty,
                    Coils = BitPacker.Unpack(pdu.Slice(6, byteCount), qty),
                };
            }

            case ModbusFunction.WriteMultipleRegisters:
            {
                Expect(pdu.Length >= 8, "write multiple registers length");
                ushort start = BigEndian.ReadUInt16(pdu, 1);
                int qty = BigEndian.ReadUInt16(pdu, 3);
                int byteCount = pdu[5];
                Expect(qty is >= 1 and <= ModbusDataStore.MaxWriteRegisters, "register quantity");
                Expect(byteCount == qty * 2 && pdu.Length == 6 + byteCount, "register byte count");
                var regs = new ushort[qty];
                for (int i = 0; i < qty; i++)
                    regs[i] = BigEndian.ReadUInt16(pdu, 6 + i * 2);
                return new ModbusRequest { Function = fc, Address = start, Quantity = qty, Registers = regs };
            }

            default:
                throw new ModbusProtocolException(ModbusExceptionCode.IllegalFunction,
                    $"Unsupported function 0x{(byte)fc:X2}.");
        }
    }

    private static void Expect(bool condition, string what)
    {
        if (!condition)
            throw new ModbusProtocolException(ModbusExceptionCode.IllegalDataValue, $"Malformed PDU ({what}).");
    }

    // -- encode (master side) -------------------------------------------------

    /// <summary>Serializes this request to a PDU.</summary>
    public byte[] ToPdu()
    {
        switch (Function)
        {
            case ModbusFunction.ReadCoils:
            case ModbusFunction.ReadDiscreteInputs:
            case ModbusFunction.ReadHoldingRegisters:
            case ModbusFunction.ReadInputRegisters:
            {
                var pdu = new byte[5];
                pdu[0] = (byte)Function;
                BigEndian.WriteUInt16(pdu, 1, Address);
                BigEndian.WriteUInt16(pdu, 3, (ushort)Quantity);
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
            {
                var bits = (Coils ?? throw new InvalidOperationException("Coils required.")).ToArray();
                var packed = BitPacker.Pack(bits);
                var pdu = new byte[6 + packed.Length];
                pdu[0] = (byte)Function;
                BigEndian.WriteUInt16(pdu, 1, Address);
                BigEndian.WriteUInt16(pdu, 3, (ushort)bits.Length);
                pdu[5] = (byte)packed.Length;
                packed.CopyTo(pdu, 6);
                return pdu;
            }

            case ModbusFunction.WriteMultipleRegisters:
            {
                var regs = (Registers ?? throw new InvalidOperationException("Registers required.")).ToArray();
                var pdu = new byte[6 + regs.Length * 2];
                pdu[0] = (byte)Function;
                BigEndian.WriteUInt16(pdu, 1, Address);
                BigEndian.WriteUInt16(pdu, 3, (ushort)regs.Length);
                pdu[5] = (byte)(regs.Length * 2);
                for (int i = 0; i < regs.Length; i++)
                    BigEndian.WriteUInt16(pdu, 6 + i * 2, regs[i]);
                return pdu;
            }

            default:
                throw new InvalidOperationException($"Cannot encode function {Function}.");
        }
    }
}
