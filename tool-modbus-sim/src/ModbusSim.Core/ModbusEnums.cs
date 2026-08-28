namespace ModbusSim.Core;

/// <summary>Standard Modbus function codes supported by the simulator.</summary>
public enum ModbusFunction : byte
{
    ReadCoils = 0x01,
    ReadDiscreteInputs = 0x02,
    ReadHoldingRegisters = 0x03,
    ReadInputRegisters = 0x04,
    WriteSingleCoil = 0x05,
    WriteSingleRegister = 0x06,
    WriteMultipleCoils = 0x0F,
    WriteMultipleRegisters = 0x10,
}

/// <summary>Modbus exception codes returned in an error response.</summary>
public enum ModbusExceptionCode : byte
{
    None = 0x00,
    IllegalFunction = 0x01,
    IllegalDataAddress = 0x02,
    IllegalDataValue = 0x03,
    ServerDeviceFailure = 0x04,
}

/// <summary>The four Modbus data tables.</summary>
public enum ModbusTable
{
    Coils,
    DiscreteInputs,
    HoldingRegisters,
    InputRegisters,
}

/// <summary>Which transport a connection uses.</summary>
public enum ModbusTransport
{
    Tcp,
    Rtu,
}

/// <summary>Simulator role.</summary>
public enum ModbusRole
{
    /// <summary>Acts as a device: listens for requests and answers from the data store.</summary>
    Slave,

    /// <summary>Acts as a client: polls a remote device on an interval.</summary>
    Master,
}
