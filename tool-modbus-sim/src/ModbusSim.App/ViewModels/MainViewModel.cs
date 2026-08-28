using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModbusSim.App.Models;
using ModbusSim.Core;
using ModbusSim.Core.Logging;
using ModbusSim.Core.Protocol;
using ModbusSim.Core.Runtime;

namespace ModbusSim.App.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const int MaxLogRows = 1000;

    private readonly ModbusDataStore _store = new();
    private ModbusEndpoint? _endpoint;
    private bool _applyingFromStore;

    public MainViewModel()
    {
        _store.Changed += OnStoreChanged;
        RefreshPorts();
        RebuildRows();
    }

    // ---- connection settings -------------------------------------------------

    public IReadOnlyList<ModbusRole> Roles { get; } = [ModbusRole.Slave, ModbusRole.Master];
    public IReadOnlyList<ModbusTransport> Transports { get; } = [ModbusTransport.Tcp, ModbusTransport.Rtu];
    public IReadOnlyList<int> BaudRates => SettingsCatalog.BaudRates;
    public IReadOnlyList<Parity> Parities => SettingsCatalog.Parities;
    public IReadOnlyList<int> DataBitOptions => SettingsCatalog.DataBitOptions;
    public IReadOnlyList<StopBits> StopBitOptions => SettingsCatalog.StopBitOptions;
    public IReadOnlyList<ModbusTable> Tables => SettingsCatalog.Tables;

    [ObservableProperty] public partial ModbusRole Role { get; set; } = ModbusRole.Slave;
    [ObservableProperty] public partial ModbusTransport Transport { get; set; } = ModbusTransport.Tcp;

    [ObservableProperty] public partial string Host { get; set; } = "127.0.0.1";
    [ObservableProperty] public partial int Port { get; set; } = 502;

    public ObservableCollection<string> SerialPorts { get; } = [];
    [ObservableProperty] public partial string? SerialPort { get; set; }
    [ObservableProperty] public partial int BaudRate { get; set; } = 19200;
    [ObservableProperty] public partial Parity Parity { get; set; } = Parity.Even;
    [ObservableProperty] public partial int DataBits { get; set; } = 8;
    [ObservableProperty] public partial StopBits StopBits { get; set; } = StopBits.One;

    [ObservableProperty] public partial int UnitId { get; set; } = 1;
    [ObservableProperty] public partial int PollIntervalMs { get; set; } = 1000;
    [ObservableProperty] public partial int ResponseTimeoutMs { get; set; } = 1000;

    public bool IsTcp => Transport == ModbusTransport.Tcp;
    public bool IsRtu => Transport == ModbusTransport.Rtu;
    public bool IsMaster => Role == ModbusRole.Master;

    partial void OnTransportChanged(ModbusTransport value)
    {
        OnPropertyChanged(nameof(IsTcp));
        OnPropertyChanged(nameof(IsRtu));
    }

    partial void OnRoleChanged(ModbusRole value)
    {
        OnPropertyChanged(nameof(IsMaster));
        OnPropertyChanged(nameof(RoleHint));
    }

    // ---- run state --------------------------------------------------------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartStopLabel))]
    [NotifyPropertyChangedFor(nameof(SettingsEnabled))]
    public partial bool IsRunning { get; set; }

    [ObservableProperty] public partial string StatusText { get; set; } = "Stopped";

    public bool SettingsEnabled => !IsRunning;
    public string StartStopLabel => IsRunning ? "Stop" : "Start";

    public string RoleHint => Role == ModbusRole.Slave
        ? "Slave: this app answers requests from the register table below."
        : "Master: this app polls a remote device and mirrors the results below.";

    // ---- register table --------------------------------------------------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTableIsBits))]
    public partial ModbusTable SelectedTable { get; set; } = ModbusTable.HoldingRegisters;

    [ObservableProperty] public partial int StartAddress { get; set; }
    [ObservableProperty] public partial int RowCount { get; set; } = 20;

    public bool SelectedTableIsBits => SelectedTable is ModbusTable.Coils or ModbusTable.DiscreteInputs;

    public ObservableCollection<RegisterRow> Rows { get; } = [];

    partial void OnSelectedTableChanged(ModbusTable value) => RebuildRows();
    partial void OnStartAddressChanged(int value) => RebuildRows();
    partial void OnRowCountChanged(int value) => RebuildRows();

    private void RebuildRows()
    {
        Rows.Clear();
        int start = Math.Clamp(StartAddress, 0, ModbusDataStore.AddressSpace - 1);
        int count = Math.Clamp(RowCount, 1, 500);
        count = Math.Min(count, ModbusDataStore.AddressSpace - start);

        _applyingFromStore = true;
        for (int i = 0; i < count; i++)
        {
            var address = (ushort)(start + i);
            var row = new RegisterRow(SelectedTable, address);
            if (row.IsBitTable)
                row.BitValue = _store.GetBit(SelectedTable, address);
            else
                row.Value = _store.GetRegister(SelectedTable, address);
            row.PropertyChanged += OnRowPropertyChanged;
            Rows.Add(row);
        }
        _applyingFromStore = false;
    }

    private void OnRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is RegisterRow row && e.PropertyName is nameof(RegisterRow.Value) or nameof(RegisterRow.BitValue))
            OnRowEdited(row);
    }

    private void OnRowEdited(RegisterRow row)
    {
        if (_applyingFromStore)
            return;

        if (Role == ModbusRole.Master && _endpoint is ModbusMaster master && IsRunning)
        {
            master.Enqueue(row.IsBitTable
                ? new PollDefinition { Function = ModbusFunction.WriteSingleCoil, Address = row.Address, WriteCoils = [row.BitValue] }
                : new PollDefinition { Function = ModbusFunction.WriteSingleRegister, Address = row.Address, WriteRegisters = [row.Value] });
            return;
        }

        if (row.IsBitTable)
            _store.SetBit(row.Table, row.Address, row.BitValue);
        else
            _store.SetRegister(row.Table, row.Address, row.Value);
    }

    // ---- traffic log -----------------------------------------------------

    public ObservableCollection<LogRow> Log { get; } = [];

    [RelayCommand]
    private void ClearLog() => Log.Clear();

    /// <summary>Writes the current log to <paramref name="path"/> as CSV.</summary>
    public Task ExportLogAsync(string path) =>
        CsvLog.WriteFileAsync(path, Log.Select(r => r.Entry).ToArray());

    // ---- port list -------------------------------------------------------

    [RelayCommand]
    private void RefreshPorts() => RefreshPortsCore();

    private void RefreshPortsCore()
    {
        var names = SettingsCatalog.AvailablePorts().OrderBy(n => n).ToArray();
        SerialPorts.Clear();
        foreach (var n in names)
            SerialPorts.Add(n);
        SerialPort ??= SerialPorts.FirstOrDefault();
    }

    // ---- start / stop --------------------------------------------------------

    [RelayCommand]
    private async Task StartStopAsync()
    {
        if (IsRunning)
        {
            await StopAsync();
            return;
        }

        try
        {
            _endpoint = Role == ModbusRole.Slave ? BuildSlave() : BuildMaster();
            _endpoint.Logged += OnEndpointLogged;
            _endpoint.StateChanged += OnEndpointStateChanged;
            await _endpoint.StartAsync();
            IsRunning = true;
            StatusText = $"{Role} running ({DescribeTransport()})";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to start: {ex.Message}";
            AppendLog(new ModbusLogEntry { Direction = LogDirection.Error, Detail = ex.Message });
            await DisposeEndpointAsync();
        }
    }

    private async Task StopAsync()
    {
        await DisposeEndpointAsync();
        IsRunning = false;
        StatusText = "Stopped";
    }

    private async Task DisposeEndpointAsync()
    {
        if (_endpoint is null)
            return;
        _endpoint.Logged -= OnEndpointLogged;
        _endpoint.StateChanged -= OnEndpointStateChanged;
        await _endpoint.DisposeAsync();
        _endpoint = null;
    }

    private ModbusSlave BuildSlave()
    {
        var options = new SlaveOptions
        {
            Transport = Transport,
            UnitId = (byte)UnitId,
            Tcp = new TcpOptions { BindAddress = "0.0.0.0", Port = Port },
            Serial = BuildSerialOptions(),
        };
        return new ModbusSlave(options, _store);
    }

    private ModbusMaster BuildMaster()
    {
        var options = new MasterOptions
        {
            Transport = Transport,
            UnitId = (byte)UnitId,
            PollIntervalMs = Math.Max(50, PollIntervalMs),
            ResponseTimeoutMs = Math.Max(100, ResponseTimeoutMs),
            Tcp = new TcpOptions { Host = Host, Port = Port },
            Serial = BuildSerialOptions(),
        };

        var readFunction = SelectedTable switch
        {
            ModbusTable.Coils => ModbusFunction.ReadCoils,
            ModbusTable.DiscreteInputs => ModbusFunction.ReadDiscreteInputs,
            ModbusTable.InputRegisters => ModbusFunction.ReadInputRegisters,
            _ => ModbusFunction.ReadHoldingRegisters,
        };
        int max = SelectedTableIsBits ? ModbusDataStore.MaxReadBits : ModbusDataStore.MaxReadRegisters;
        int quantity = Math.Clamp(Rows.Count, 1, max);
        var poll = PollDefinition.Read(readFunction, (ushort)Math.Clamp(StartAddress, 0, 65535), quantity);

        return new ModbusMaster(options, _store, [poll]);
    }

    private SerialOptions BuildSerialOptions() => new()
    {
        PortName = SerialPort ?? "COM1",
        BaudRate = BaudRate,
        Parity = Parity,
        DataBits = DataBits,
        StopBits = StopBits,
    };

    private string DescribeTransport() => Transport == ModbusTransport.Tcp
        ? (Role == ModbusRole.Slave ? $"TCP :{Port}" : $"TCP {Host}:{Port}")
        : $"{SerialPort} {BaudRate} {Parity.ToString()[0]}{DataBits}{(StopBits == StopBits.Two ? 2 : 1)}";

    // ---- event plumbing ----------------------------------------------------

    private void OnEndpointLogged(object? sender, ModbusLogEntry entry) =>
        Dispatcher.UIThread.Post(() => AppendLog(entry));

    private void OnEndpointStateChanged(object? sender, EndpointState state) =>
        Dispatcher.UIThread.Post(() =>
        {
            if (state == EndpointState.Faulted)
            {
                IsRunning = false;
                StatusText = "Faulted — see log";
            }
        });

    private void AppendLog(ModbusLogEntry entry)
    {
        Log.Add(new LogRow(entry));
        while (Log.Count > MaxLogRows)
            Log.RemoveAt(0);
    }

    private void OnStoreChanged(object? sender, DataStoreChangedEventArgs e) =>
        Dispatcher.UIThread.Post(() =>
        {
            if (e.Table != SelectedTable)
                return;

            _applyingFromStore = true;
            try
            {
                foreach (var row in Rows)
                {
                    if (row.Address < e.StartAddress || row.Address >= e.StartAddress + e.Count)
                        continue;
                    if (row.IsBitTable)
                        row.BitValue = _store.GetBit(row.Table, row.Address);
                    else
                        row.Value = _store.GetRegister(row.Table, row.Address);
                    if (e.FromWire)
                        FlashRow(row);
                }
            }
            finally
            {
                _applyingFromStore = false;
            }
        });

    private static async void FlashRow(RegisterRow row)
    {
        row.RecentlyChanged = true;
        await Task.Delay(700);
        row.RecentlyChanged = false;
    }
}
