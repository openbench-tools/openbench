using System.Globalization;
using System.Text;

namespace ModbusSim.Core.Logging;

/// <summary>Renders <see cref="ModbusLogEntry"/> rows as RFC-4180 CSV.</summary>
public static class CsvLog
{
    private const string Header = "Timestamp,Direction,UnitId,Function,Address,Quantity,Exception,Detail,RawHex";

    public static string Write(IEnumerable<ModbusLogEntry> entries)
    {
        var sb = new StringBuilder();
        sb.Append(Header).Append("\r\n");
        foreach (var e in entries)
            AppendRow(sb, e);
        return sb.ToString();
    }

    public static async Task WriteFileAsync(string path, IEnumerable<ModbusLogEntry> entries, CancellationToken ct = default)
        => await File.WriteAllTextAsync(path, Write(entries), new UTF8Encoding(false), ct).ConfigureAwait(false);

    private static void AppendRow(StringBuilder sb, ModbusLogEntry e)
    {
        sb.Append(e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)).Append(',');
        sb.Append(e.Direction).Append(',');
        sb.Append(e.UnitId).Append(',');
        sb.Append(Field(e.Function?.ToString() ?? "")).Append(',');
        sb.Append(e.Address?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',');
        sb.Append(e.Quantity?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',');
        sb.Append(e.Exception == ModbusExceptionCode.None ? "" : e.Exception.ToString()).Append(',');
        sb.Append(Field(e.Detail)).Append(',');
        sb.Append(e.RawHex).Append("\r\n");
    }

    private static string Field(string value)
    {
        if (value.Length == 0)
            return value;
        bool mustQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!mustQuote)
            return value;
        return '"' + value.Replace("\"", "\"\"") + '"';
    }
}
