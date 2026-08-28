using ModbusSim.Core.Logging;

namespace ModbusSim.Core.Runtime;

public enum EndpointState
{
    Stopped,
    Running,
    Faulted,
}

/// <summary>Shared lifecycle + logging plumbing for the slave and master hosts.</summary>
public abstract class ModbusEndpoint : IAsyncDisposable
{
    private CancellationTokenSource? _cts;
    private Task? _worker;
    private EndpointState _state = EndpointState.Stopped;

    /// <summary>Raised for every logged event. May fire on a background thread.</summary>
    public event EventHandler<ModbusLogEntry>? Logged;

    public event EventHandler<EndpointState>? StateChanged;

    public EndpointState State
    {
        get => _state;
        private protected set
        {
            if (_state == value)
                return;
            _state = value;
            StateChanged?.Invoke(this, value);
        }
    }

    public bool IsRunning => State == EndpointState.Running;

    /// <summary>Starts the endpoint. Throws synchronously if the transport cannot be opened.</summary>
    public async Task StartAsync(CancellationToken ct = default)
    {
        if (_worker is not null)
            throw new InvalidOperationException("Already started.");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        await OpenAsync(_cts.Token).ConfigureAwait(false);
        State = EndpointState.Running;
        _worker = Task.Run(() => RunGuardedAsync(_cts.Token));
    }

    public async Task StopAsync()
    {
        if (_cts is null)
            return;
        await _cts.CancelAsync().ConfigureAwait(false);
        try
        {
            if (_worker is not null)
                await _worker.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { /* expected */ }
        finally
        {
            _cts.Dispose();
            _cts = null;
            _worker = null;
            await CloseAsync().ConfigureAwait(false);
            State = EndpointState.Stopped;
        }
    }

    private async Task RunGuardedAsync(CancellationToken ct)
    {
        try
        {
            await RunAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // normal shutdown
        }
        catch (Exception ex)
        {
            Log(new ModbusLogEntry { Direction = LogDirection.Error, Detail = ex.Message });
            State = EndpointState.Faulted;
        }
    }

    /// <summary>Opens the transport. Runs before the worker task starts; exceptions surface to the caller.</summary>
    protected abstract Task OpenAsync(CancellationToken ct);

    /// <summary>The main loop. Should honour <paramref name="ct"/>.</summary>
    protected abstract Task RunAsync(CancellationToken ct);

    /// <summary>Releases the transport after the worker has stopped.</summary>
    protected abstract Task CloseAsync();

    protected void Log(ModbusLogEntry entry) => Logged?.Invoke(this, entry);

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
