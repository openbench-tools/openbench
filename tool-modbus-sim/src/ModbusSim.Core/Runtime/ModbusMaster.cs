using System.Collections.Concurrent;
using System.Net.Sockets;
using ModbusSim.Core.Framing;
using ModbusSim.Core.Logging;
using ModbusSim.Core.Protocol;

namespace ModbusSim.Core.Runtime;

/// <summary>
/// Acts as a Modbus client: connects over TCP or serial and issues a fixed set of
/// polls on an interval, mirroring read results into the shared data store.
/// </summary>
public sealed class ModbusMaster : ModbusEndpoint
{
    private readonly MasterOptions _options;
    private readonly ModbusDataStore _store;
    private readonly List<PollDefinition> _polls;
    private readonly ConcurrentQueue<PollDefinition> _oneShots = new();
    private IFrameChannel? _channel;
    private ushort _transactionId;

    public ModbusMaster(MasterOptions options, ModbusDataStore store, IEnumerable<PollDefinition> polls)
    {
        _options = options;
        _store = store;
        _polls = polls.ToList();
    }

    public MasterOptions Options => _options;
    public IReadOnlyList<PollDefinition> Polls => _polls;

    /// <summary>Queues a request to run once, before the next poll cycle. Thread-safe.</summary>
    public void Enqueue(PollDefinition request) => _oneShots.Enqueue(request);

    protected override async Task OpenAsync(CancellationToken ct)
    {
        if (_options.Transport == ModbusTransport.Tcp)
        {
            var client = new TcpClient { NoDelay = true };
            await client.ConnectAsync(_options.Tcp.Host, _options.Tcp.Port, ct).ConfigureAwait(false);
            _channel = new StreamFrameChannel(client.GetStream(), $"{_options.Tcp.Host}:{_options.Tcp.Port}", client);
            Log(Info($"Connected to {_options.Tcp.Host}:{_options.Tcp.Port}"));
        }
        else
        {
            _channel = new SerialFrameChannel(_options.Serial);
            Log(Info($"Open on {_options.Serial.PortName} @ {_options.Serial.BaudRate}"));
        }
    }

    protected override async Task RunAsync(CancellationToken ct)
    {
        var channel = _channel ?? throw new InvalidOperationException("Channel not open.");

        if (_polls.Count == 0)
            Log(Info("No polls configured — master is idle."));

        while (!ct.IsCancellationRequested)
        {
            while (_oneShots.TryDequeue(out var oneShot))
            {
                ct.ThrowIfCancellationRequested();
                await ExecutePollAsync(channel, oneShot, ct).ConfigureAwait(false);
            }

            foreach (var poll in _polls)
            {
                ct.ThrowIfCancellationRequested();
                await ExecutePollAsync(channel, poll, ct).ConfigureAwait(false);
            }

            await Task.Delay(_options.PollIntervalMs, ct).ConfigureAwait(false);
        }
    }

    protected override async Task CloseAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync().ConfigureAwait(false);
        _channel = null;
    }

    private async Task ExecutePollAsync(IFrameChannel channel, PollDefinition poll, CancellationToken ct)
    {
        var request = poll.ToRequest();
        ushort txn = unchecked(++_transactionId);
        var requestFrame = new ModbusFrame(_options.UnitId, request.ToPdu(), txn);

        await channel.WriteFrameAsync(requestFrame, ct).ConfigureAwait(false);
        Log(new ModbusLogEntry
        {
            Direction = LogDirection.Tx,
            UnitId = _options.UnitId,
            Function = request.Function,
            Address = request.Address,
            Quantity = request.Quantity,
            Detail = LogFormat.Request(request),
            Raw = Capture(requestFrame),
        });

        ModbusFrame? reply;
        using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            timeout.CancelAfter(_options.ResponseTimeoutMs);
            try
            {
                reply = await channel.ReadFrameAsync(timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                Log(Error(request, "response timeout"));
                return;
            }
            catch (InvalidDataException ex)
            {
                Log(Error(request, ex.Message));
                return;
            }
        }

        if (reply is not { } frame)
        {
            Log(Error(request, "connection closed by peer"));
            throw new IOException("Master connection closed.");
        }

        if (_options.Transport == ModbusTransport.Tcp && frame.TransactionId != txn)
        {
            Log(Error(request, $"transaction id mismatch (sent {txn}, got {frame.TransactionId})"));
            return;
        }

        ModbusResponse response;
        try
        {
            response = ModbusResponse.ParseReply(request, frame.Pdu);
        }
        catch (ModbusProtocolException ex)
        {
            Log(Error(request, ex.Message));
            return;
        }

        if (response.IsException)
        {
            Log(new ModbusLogEntry
            {
                Direction = LogDirection.Rx,
                UnitId = frame.UnitId,
                Function = response.Function,
                Address = request.Address,
                Exception = response.Exception,
                Detail = LogFormat.Response(response),
                Raw = Capture(frame),
            });
            return;
        }

        ApplyToStore(poll, response);
        Log(new ModbusLogEntry
        {
            Direction = LogDirection.Rx,
            UnitId = frame.UnitId,
            Function = response.Function,
            Address = request.Address,
            Quantity = request.Quantity,
            Detail = LogFormat.Response(response),
            Raw = Capture(frame),
        });
    }

    private void ApplyToStore(PollDefinition poll, ModbusResponse response)
    {
        if (poll.ResultTable is not { } table)
            return;

        if (response.Coils is { } bits)
            _store.SetBitBlock(table, poll.Address, bits.ToArray());
        else if (response.Registers is { } regs)
            _store.SetRegisterBlock(table, poll.Address, regs.ToArray());
    }

    private byte[]? Capture(in ModbusFrame frame)
    {
        if (!_options.CaptureRawBytes)
            return null;
        return _options.Transport == ModbusTransport.Tcp
            ? TcpFrameCodec.Encode(frame)
            : RtuFrameCodec.Encode(frame);
    }

    private static ModbusLogEntry Info(string message) => new() { Direction = LogDirection.Info, Detail = message };

    private static ModbusLogEntry Error(ModbusRequest request, string message) => new()
    {
        Direction = LogDirection.Error,
        Function = request.Function,
        Address = request.Address,
        Detail = message,
    };
}
