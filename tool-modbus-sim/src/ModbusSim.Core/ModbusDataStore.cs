namespace ModbusSim.Core;

/// <summary>Describes a change applied to the data store.</summary>
public sealed class DataStoreChangedEventArgs(ModbusTable table, ushort startAddress, int count, bool fromWire)
    : EventArgs
{
    public ModbusTable Table { get; } = table;
    public ushort StartAddress { get; } = startAddress;
    public int Count { get; } = count;

    /// <summary>True when the change came from an incoming Modbus write; false when set locally (UI / config).</summary>
    public bool FromWire { get; } = fromWire;
}

/// <summary>
/// Thread-safe backing store for the four Modbus tables. Each table spans the full
/// 0..65535 address space. Over-the-wire reads/writes are range-checked and throw
/// <see cref="ModbusProtocolException"/>; local setters are unchecked helpers for the UI.
/// </summary>
public sealed class ModbusDataStore
{
    public const int AddressSpace = 65536;

    // Per-spec quantity limits.
    public const int MaxReadBits = 2000;
    public const int MaxReadRegisters = 125;
    public const int MaxWriteBits = 1968;
    public const int MaxWriteRegisters = 123;

    private readonly Lock _gate = new();
    private readonly bool[] _coils = new bool[AddressSpace];
    private readonly bool[] _discreteInputs = new bool[AddressSpace];
    private readonly ushort[] _holdingRegisters = new ushort[AddressSpace];
    private readonly ushort[] _inputRegisters = new ushort[AddressSpace];

    /// <summary>Raised after any successful mutation. May fire on a background thread.</summary>
    public event EventHandler<DataStoreChangedEventArgs>? Changed;

    private bool[] Bits(ModbusTable table) => table switch
    {
        ModbusTable.Coils => _coils,
        ModbusTable.DiscreteInputs => _discreteInputs,
        _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Not a bit table."),
    };

    private ushort[] Registers(ModbusTable table) => table switch
    {
        ModbusTable.HoldingRegisters => _holdingRegisters,
        ModbusTable.InputRegisters => _inputRegisters,
        _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Not a register table."),
    };

    private static void CheckRange(ushort start, int count, int maxCount, string what)
    {
        if (count < 1 || count > maxCount)
            throw new ModbusProtocolException(ModbusExceptionCode.IllegalDataValue,
                $"{what}: quantity {count} outside 1..{maxCount}.");
        if (start + count > AddressSpace)
            throw new ModbusProtocolException(ModbusExceptionCode.IllegalDataAddress,
                $"{what}: address {start}+{count} exceeds {AddressSpace}.");
    }

    // ---- wire reads (range-checked) --------------------------------------

    public bool[] ReadBits(ModbusTable table, ushort start, int count)
    {
        CheckRange(start, count, MaxReadBits, "read bits");
        var src = Bits(table);
        var result = new bool[count];
        lock (_gate)
            Array.Copy(src, start, result, 0, count);
        return result;
    }

    public ushort[] ReadRegisters(ModbusTable table, ushort start, int count)
    {
        CheckRange(start, count, MaxReadRegisters, "read registers");
        var src = Registers(table);
        var result = new ushort[count];
        lock (_gate)
            Array.Copy(src, start, result, 0, count);
        return result;
    }

    // ---- wire writes (range-checked, raise Changed) ----------------------

    public void WriteCoils(ushort start, ReadOnlySpan<bool> values)
    {
        CheckRange(start, values.Length, MaxWriteBits, "write coils");
        lock (_gate)
            for (int i = 0; i < values.Length; i++)
                _coils[start + i] = values[i];
        RaiseChanged(ModbusTable.Coils, start, values.Length, fromWire: true);
    }

    public void WriteHoldingRegisters(ushort start, ReadOnlySpan<ushort> values)
    {
        CheckRange(start, values.Length, MaxWriteRegisters, "write registers");
        lock (_gate)
            for (int i = 0; i < values.Length; i++)
                _holdingRegisters[start + i] = values[i];
        RaiseChanged(ModbusTable.HoldingRegisters, start, values.Length, fromWire: true);
    }

    // ---- local setters (unchecked helpers for UI / config) --------------

    public bool GetBit(ModbusTable table, ushort address)
    {
        lock (_gate)
            return Bits(table)[address];
    }

    public ushort GetRegister(ModbusTable table, ushort address)
    {
        lock (_gate)
            return Registers(table)[address];
    }

    public void SetBit(ModbusTable table, ushort address, bool value, bool fromWire = false)
    {
        lock (_gate)
            Bits(table)[address] = value;
        RaiseChanged(table, address, 1, fromWire);
    }

    public void SetRegister(ModbusTable table, ushort address, ushort value, bool fromWire = false)
    {
        lock (_gate)
            Registers(table)[address] = value;
        RaiseChanged(table, address, 1, fromWire);
    }

    /// <summary>Copies a contiguous block of register values in for the UI grid / import.</summary>
    public void SetRegisterBlock(ModbusTable table, ushort start, ReadOnlySpan<ushort> values)
    {
        var dst = Registers(table);
        lock (_gate)
            for (int i = 0; i < values.Length && start + i < AddressSpace; i++)
                dst[start + i] = values[i];
        RaiseChanged(table, start, values.Length, fromWire: false);
    }

    /// <summary>Copies a contiguous block of bit values in for the UI grid / import.</summary>
    public void SetBitBlock(ModbusTable table, ushort start, ReadOnlySpan<bool> values)
    {
        var dst = Bits(table);
        lock (_gate)
            for (int i = 0; i < values.Length && start + i < AddressSpace; i++)
                dst[start + i] = values[i];
        RaiseChanged(table, start, values.Length, fromWire: false);
    }

    /// <summary>Snapshot of a register range for display. Clamped to the address space.</summary>
    public ushort[] SnapshotRegisters(ModbusTable table, ushort start, int count)
    {
        count = Math.Clamp(count, 0, AddressSpace - start);
        var src = Registers(table);
        var result = new ushort[count];
        lock (_gate)
            Array.Copy(src, start, result, 0, count);
        return result;
    }

    /// <summary>Snapshot of a bit range for display. Clamped to the address space.</summary>
    public bool[] SnapshotBits(ModbusTable table, ushort start, int count)
    {
        count = Math.Clamp(count, 0, AddressSpace - start);
        var src = Bits(table);
        var result = new bool[count];
        lock (_gate)
            Array.Copy(src, start, result, 0, count);
        return result;
    }

    /// <summary>Zeroes every table.</summary>
    public void Reset()
    {
        lock (_gate)
        {
            Array.Clear(_coils);
            Array.Clear(_discreteInputs);
            Array.Clear(_holdingRegisters);
            Array.Clear(_inputRegisters);
        }
        foreach (var t in Enum.GetValues<ModbusTable>())
            RaiseChanged(t, 0, AddressSpace, fromWire: false);
    }

    private void RaiseChanged(ModbusTable table, ushort start, int count, bool fromWire) =>
        Changed?.Invoke(this, new DataStoreChangedEventArgs(table, start, count, fromWire));
}
