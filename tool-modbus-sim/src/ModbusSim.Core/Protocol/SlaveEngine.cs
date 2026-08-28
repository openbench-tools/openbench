namespace ModbusSim.Core.Protocol;

/// <summary>
/// Applies a decoded request to a <see cref="ModbusDataStore"/> and produces the
/// response. Protocol errors are turned into Modbus exception responses rather
/// than thrown.
/// </summary>
public sealed class SlaveEngine(ModbusDataStore store)
{
    public ModbusDataStore Store { get; } = store;

    public ModbusResponse Process(ModbusRequest request)
    {
        try
        {
            return request.Function switch
            {
                ModbusFunction.ReadCoils => new ModbusResponse
                {
                    Function = request.Function,
                    Coils = Store.ReadBits(ModbusTable.Coils, request.Address, request.Quantity),
                },
                ModbusFunction.ReadDiscreteInputs => new ModbusResponse
                {
                    Function = request.Function,
                    Coils = Store.ReadBits(ModbusTable.DiscreteInputs, request.Address, request.Quantity),
                },
                ModbusFunction.ReadHoldingRegisters => new ModbusResponse
                {
                    Function = request.Function,
                    Registers = Store.ReadRegisters(ModbusTable.HoldingRegisters, request.Address, request.Quantity),
                },
                ModbusFunction.ReadInputRegisters => new ModbusResponse
                {
                    Function = request.Function,
                    Registers = Store.ReadRegisters(ModbusTable.InputRegisters, request.Address, request.Quantity),
                },
                ModbusFunction.WriteSingleCoil => WriteCoils(request, single: true),
                ModbusFunction.WriteMultipleCoils => WriteCoils(request, single: false),
                ModbusFunction.WriteSingleRegister => WriteRegisters(request, single: true),
                ModbusFunction.WriteMultipleRegisters => WriteRegisters(request, single: false),
                _ => throw new ModbusProtocolException(ModbusExceptionCode.IllegalFunction),
            };
        }
        catch (ModbusProtocolException ex)
        {
            return ModbusResponse.FromException(request.Function, ex.Code);
        }
    }

    private ModbusResponse WriteCoils(ModbusRequest request, bool single)
    {
        var bits = (request.Coils ?? throw new ModbusProtocolException(ModbusExceptionCode.IllegalDataValue)).ToArray();
        Store.WriteCoils(request.Address, bits);
        return single
            ? new ModbusResponse { Function = request.Function, Address = request.Address, Coils = bits }
            : new ModbusResponse { Function = request.Function, Address = request.Address, Quantity = bits.Length };
    }

    private ModbusResponse WriteRegisters(ModbusRequest request, bool single)
    {
        var regs = (request.Registers ?? throw new ModbusProtocolException(ModbusExceptionCode.IllegalDataValue)).ToArray();
        Store.WriteHoldingRegisters(request.Address, regs);
        return single
            ? new ModbusResponse { Function = request.Function, Address = request.Address, Registers = regs }
            : new ModbusResponse { Function = request.Function, Address = request.Address, Quantity = regs.Length };
    }
}
