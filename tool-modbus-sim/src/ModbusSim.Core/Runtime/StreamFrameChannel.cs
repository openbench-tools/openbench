using ModbusSim.Core.Framing;

namespace ModbusSim.Core.Runtime;

/// <summary>An <see cref="IFrameChannel"/> over a byte stream using MBAP (Modbus TCP) framing.</summary>
public sealed class StreamFrameChannel(Stream stream, string description, IDisposable? owner = null) : IFrameChannel
{
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public string Description { get; } = description;

    public Task<ModbusFrame?> ReadFrameAsync(CancellationToken ct) => TcpFrameCodec.ReadAsync(stream, ct);

    public async Task WriteFrameAsync(ModbusFrame frame, CancellationToken ct)
    {
        var bytes = TcpFrameCodec.Encode(frame);
        await _writeLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await stream.WriteAsync(bytes, ct).ConfigureAwait(false);
            await stream.FlushAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await stream.DisposeAsync().ConfigureAwait(false);
        owner?.Dispose();
        _writeLock.Dispose();
    }
}
