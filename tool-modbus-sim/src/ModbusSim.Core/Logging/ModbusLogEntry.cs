namespace ModbusSim.Core.Logging;

public enum LogDirection
{
    /// <summary>A frame received from the peer.</summary>
    Rx,

    /// <summary>A frame sent to the peer.</summary>
    Tx,

    /// <summary>Lifecycle / connection information.</summary>
    Info,

    /// <summary>A protocol or transport error.</summary>
    Error,
}

/// <summary>One line in the live traffic log. Immutable; safe to hand to the UI thread.</summary>
public sealed record ModbusLogEntry
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public LogDirection Direction { get; init; }
    public byte UnitId { get; init; }
    public ModbusFunction? Function { get; init; }
    public ushort? Address { get; init; }
    public int? Quantity { get; init; }
    public ModbusExceptionCode Exception { get; init; } = ModbusExceptionCode.None;

    /// <summary>Human-readable values or message.</summary>
    public string Detail { get; init; } = "";

    /// <summary>Raw ADU bytes, when captured.</summary>
    public byte[]? Raw { get; init; }

    public string RawHex => Raw is null ? "" : Convert.ToHexString(Raw);
}
