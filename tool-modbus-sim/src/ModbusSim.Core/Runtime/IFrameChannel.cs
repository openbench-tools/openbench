using ModbusSim.Core.Framing;

namespace ModbusSim.Core.Runtime;

/// <summary>A bidirectional link that carries whole Modbus frames.</summary>
public interface IFrameChannel : IAsyncDisposable
{
    /// <summary>Reads the next frame, or returns <c>null</c> when the link closes cleanly.</summary>
    Task<ModbusFrame?> ReadFrameAsync(CancellationToken ct);

    /// <summary>Sends a frame.</summary>
    Task WriteFrameAsync(ModbusFrame frame, CancellationToken ct);

    /// <summary>A short label for logs (e.g. remote endpoint or port name).</summary>
    string Description { get; }
}
