using System.Net;
using System.Net.Sockets;
using ModbusSim.Core.Framing;
using ModbusSim.Core.Logging;
using ModbusSim.Core.Protocol;

namespace ModbusSim.Core.Runtime;

/// <summary>
/// Emulates a Modbus device: listens on TCP or a serial port and answers requests
/// from the shared <see cref="ModbusDataStore"/>.
/// </summary>
public sealed class ModbusSlave(SlaveOptions options, ModbusDataStore store) : ModbusEndpoint
{
    private readonly SlaveEngine _engine = new(store);
    private TcpListener? _listener;
    private SerialFrameChannel? _serial;

    public SlaveOptions Options { get; } = options;
    public ModbusDataStore Store { get; } = store;

    /// <summary>The TCP port actually bound (useful when <see cref="TcpOptions.Port"/> is 0). 0 for serial.</summary>
    public int BoundPort { get; private set; }

    protected override Task OpenAsync(CancellationToken ct)
    {
        if (Options.Transport == ModbusTransport.Tcp)
        {
            var address = IPAddress.Parse(Options.Tcp.BindAddress);
            _listener = new TcpListener(address, Options.Tcp.Port);
            _listener.Start();
            BoundPort = ((IPEndPoint)_listener.LocalEndpoint).Port;
            Log(Info($"Listening on {address}:{BoundPort} (unit {Options.UnitId})"));
        }
        else
        {
            _serial = new SerialFrameChannel(Options.Serial);
            Log(Info($"Open on {Options.Serial.PortName} @ {Options.Serial.BaudRate} (unit {Options.UnitId})"));
        }
        return Task.CompletedTask;
    }

    protected override async Task RunAsync(CancellationToken ct)
    {
        if (_listener is not null)
            await RunTcpAsync(_listener, ct).ConfigureAwait(false);
        else if (_serial is not null)
            await ServeChannelAsync(_serial, ct).ConfigureAwait(false);
    }

    protected override Task CloseAsync()
    {
        _listener?.Stop();
        _listener = null;
        return Task.CompletedTask;
    }

    private async Task RunTcpAsync(TcpListener listener, CancellationToken ct)
    {
        var clients = new List<Task>();
        while (!ct.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await listener.AcceptTcpClientAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            client.NoDelay = true;
            var remote = client.Client.RemoteEndPoint?.ToString() ?? "?";
            Log(Info($"Client connected: {remote}"));

            var channel = new StreamFrameChannel(client.GetStream(), remote, client);
            clients.Add(HandleClientAsync(channel, remote, ct));
            clients.RemoveAll(t => t.IsCompleted);
        }
        await Task.WhenAll(clients).ConfigureAwait(false);
    }

    private async Task HandleClientAsync(IFrameChannel channel, string remote, CancellationToken ct)
    {
        try
        {
            await ServeChannelAsync(channel, ct).ConfigureAwait(false);
        }
        finally
        {
            await channel.DisposeAsync().ConfigureAwait(false);
            Log(Info($"Client disconnected: {remote}"));
        }
    }

    private async Task ServeChannelAsync(IFrameChannel channel, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            ModbusFrame? incoming;
            try
            {
                incoming = await channel.ReadFrameAsync(ct).ConfigureAwait(false);
            }
            catch (InvalidDataException ex)
            {
                Log(new ModbusLogEntry { Direction = LogDirection.Error, Detail = ex.Message });
                if (Options.Transport == ModbusTransport.Tcp)
                    break; // framing desync on a stream is unrecoverable
                continue;
            }

            if (incoming is not { } frame)
                break;

            if (!ShouldAnswer(frame.UnitId))
                continue;

            ModbusRequest request;
            try
            {
                request = ModbusRequest.Parse(frame.Pdu);
            }
            catch (ModbusProtocolException ex)
            {
                Log(new ModbusLogEntry
                {
                    Direction = LogDirection.Rx,
                    UnitId = frame.UnitId,
                    Exception = ex.Code,
                    Detail = ex.Message,
                    Raw = Raw(frame),
                });
                continue;
            }

            Log(new ModbusLogEntry
            {
                Direction = LogDirection.Rx,
                UnitId = frame.UnitId,
                Function = request.Function,
                Address = request.Address,
                Quantity = request.Quantity,
                Detail = LogFormat.Request(request),
                Raw = Raw(frame),
            });

            var response = _engine.Process(request);
            var replyFrame = new ModbusFrame(frame.UnitId, response.ToPdu(), frame.TransactionId);
            await channel.WriteFrameAsync(replyFrame, ct).ConfigureAwait(false);

            Log(new ModbusLogEntry
            {
                Direction = LogDirection.Tx,
                UnitId = frame.UnitId,
                Function = response.Function,
                Address = request.Address,
                Quantity = request.Quantity,
                Exception = response.Exception,
                Detail = LogFormat.Response(response),
                Raw = Options.CaptureRawBytes ? EncodeRaw(replyFrame) : null,
            });
        }
    }

    private bool ShouldAnswer(byte unitId) =>
        Options.Transport == ModbusTransport.Tcp || unitId == Options.UnitId || unitId == 0;

    private byte[]? Raw(in ModbusFrame frame) => Options.CaptureRawBytes ? EncodeRaw(frame) : null;

    private byte[] EncodeRaw(in ModbusFrame frame) => Options.Transport == ModbusTransport.Tcp
        ? TcpFrameCodec.Encode(frame)
        : RtuFrameCodec.Encode(frame);

    private static ModbusLogEntry Info(string message) =>
        new() { Direction = LogDirection.Info, Detail = message };
}
