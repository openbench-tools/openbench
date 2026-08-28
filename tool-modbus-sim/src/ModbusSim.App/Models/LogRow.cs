using System.Globalization;
using ModbusSim.Core.Logging;

namespace ModbusSim.App.Models;

/// <summary>Display projection of a <see cref="ModbusLogEntry"/> for the traffic grid.</summary>
public sealed class LogRow(ModbusLogEntry entry)
{
    public ModbusLogEntry Entry { get; } = entry;

    public string Time => Entry.Timestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
    public string Direction => Entry.Direction.ToString().ToUpperInvariant();
    public string Unit => Entry.Direction is LogDirection.Info or LogDirection.Error && Entry.Function is null
        ? ""
        : Entry.UnitId.ToString(CultureInfo.InvariantCulture);
    public string Function => Entry.Function?.ToString() ?? "";
    public string Detail => Entry.Exception != Core.ModbusExceptionCode.None && !Entry.Detail.Contains("EXCEPTION")
        ? $"EXCEPTION {Entry.Exception} — {Entry.Detail}"
        : Entry.Detail;
    public string RawHex => Entry.RawHex;

    public bool IsError => Entry.Direction == LogDirection.Error || Entry.Exception != Core.ModbusExceptionCode.None;
    public bool IsInfo => Entry.Direction == LogDirection.Info;
}
