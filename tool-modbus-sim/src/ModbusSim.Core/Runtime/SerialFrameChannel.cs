using System.IO.Ports;
using ModbusSim.Core.Framing;

namespace ModbusSim.Core.Runtime;

/// <summary>
/// An <see cref="IFrameChannel"/> over a serial port using Modbus RTU framing.
/// Frame boundaries are detected by an idle gap on the line.
/// </summary>
public sealed class SerialFrameChannel : IFrameChannel
{
    private readonly SerialPort _port;
    private readonly int _gapMs;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly byte[] _readBuffer = new byte[512];

    public SerialFrameChannel(SerialOptions options)
    {
        _gapMs = Math.Max(5, options.InterFrameGapMs);
        _port = new SerialPort(options.PortName, options.BaudRate, options.Parity, options.DataBits, options.StopBits)
        {
            ReadTimeout = _gapMs,
            WriteTimeout = 1000,
            Handshake = Handshake.None,
        };
        _port.Open();
        _port.DiscardInBuffer();
    }

    public string Description => _port.PortName;

    /// <summary>Port names the OS currently reports.</summary>
    public static string[] AvailablePorts() => SerialPort.GetPortNames();

    public async Task<ModbusFrame?> ReadFrameAsync(CancellationToken ct)
    {
        var frame = new List<byte>(64);
        while (!ct.IsCancellationRequested)
        {
            int read;
            try
            {
                read = await Task.Run(() => _port.Read(_readBuffer, 0, _readBuffer.Length), CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                // Idle gap: end of frame if we have bytes, otherwise keep waiting.
                if (frame.Count == 0)
                    continue;
                if (RtuFrameCodec.TryDecode(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(frame), out var decoded))
                    return decoded;
                throw new InvalidDataException($"Discarded {frame.Count}-byte RTU frame with bad CRC.");
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or UnauthorizedAccessException)
            {
                return null; // port closed underneath us
            }

            for (int i = 0; i < read; i++)
                frame.Add(_readBuffer[i]);
        }
        ct.ThrowIfCancellationRequested();
        return null;
    }

    public async Task WriteFrameAsync(ModbusFrame frame, CancellationToken ct)
    {
        var bytes = RtuFrameCodec.Encode(frame);
        await _writeLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await Task.Run(() => _port.Write(bytes, 0, bytes.Length), ct).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        try { _port.Dispose(); } catch { /* already gone */ }
        _writeLock.Dispose();
        return ValueTask.CompletedTask;
    }
}
