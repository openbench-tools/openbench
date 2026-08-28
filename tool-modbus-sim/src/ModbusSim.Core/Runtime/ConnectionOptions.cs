using System.IO.Ports;

namespace ModbusSim.Core.Runtime;

/// <summary>TCP endpoint settings (host is ignored for slave, which always binds locally).</summary>
public sealed record TcpOptions
{
    public string Host { get; init; } = "127.0.0.1";
    public int Port { get; init; } = 502;

    /// <summary>Address the slave listener binds to. <c>IPAddress.Any</c> equivalent is "0.0.0.0".</summary>
    public string BindAddress { get; init; } = "127.0.0.1";
}

/// <summary>Serial port settings for Modbus RTU.</summary>
public sealed record SerialOptions
{
    public string PortName { get; init; } = "COM1";
    public int BaudRate { get; init; } = 19200;
    public int DataBits { get; init; } = 8;
    public Parity Parity { get; init; } = Parity.Even;
    public StopBits StopBits { get; init; } = StopBits.One;

    /// <summary>
    /// Idle time that marks the end of an RTU frame. The spec says 3.5 character
    /// times; on a PC the OS timer resolution forces something larger.
    /// </summary>
    public int InterFrameGapMs { get; init; } = 25;
}

/// <summary>Slave (device emulation) settings.</summary>
public sealed record SlaveOptions
{
    public ModbusTransport Transport { get; init; } = ModbusTransport.Tcp;
    public TcpOptions Tcp { get; init; } = new();
    public SerialOptions Serial { get; init; } = new();

    /// <summary>Unit id this slave answers to. It also always answers unit id 0 (broadcast).</summary>
    public byte UnitId { get; init; } = 1;

    /// <summary>Capture raw ADU bytes into log entries.</summary>
    public bool CaptureRawBytes { get; init; } = true;
}

/// <summary>Master (polling client) settings.</summary>
public sealed record MasterOptions
{
    public ModbusTransport Transport { get; init; } = ModbusTransport.Tcp;
    public TcpOptions Tcp { get; init; } = new();
    public SerialOptions Serial { get; init; } = new();

    public byte UnitId { get; init; } = 1;
    public int PollIntervalMs { get; init; } = 1000;
    public int ResponseTimeoutMs { get; init; } = 1000;
    public bool CaptureRawBytes { get; init; } = true;
}
