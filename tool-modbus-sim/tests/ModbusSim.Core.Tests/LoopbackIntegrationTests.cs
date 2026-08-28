using ModbusSim.Core;
using ModbusSim.Core.Protocol;
using ModbusSim.Core.Runtime;

namespace ModbusSim.Core.Tests;

public class LoopbackIntegrationTests
{
    private static SlaveOptions SlaveOnEphemeralPort() => new()
    {
        Transport = ModbusTransport.Tcp,
        Tcp = new TcpOptions { BindAddress = "127.0.0.1", Port = 0 },
    };

    private static async Task WaitUntil(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return;
            await Task.Delay(25);
        }
        throw new TimeoutException("Condition not met in time.");
    }

    [Fact]
    public async Task Master_reads_holding_registers_from_slave_over_tcp()
    {
        var slaveStore = new ModbusDataStore();
        slaveStore.SetRegister(ModbusTable.HoldingRegisters, 0, 100);
        slaveStore.SetRegister(ModbusTable.HoldingRegisters, 1, 200);
        slaveStore.SetRegister(ModbusTable.HoldingRegisters, 2, 300);

        await using var slave = new ModbusSlave(SlaveOnEphemeralPort(), slaveStore);
        await slave.StartAsync();

        var masterStore = new ModbusDataStore();
        var masterOptions = new MasterOptions
        {
            Transport = ModbusTransport.Tcp,
            Tcp = new TcpOptions { Host = "127.0.0.1", Port = slave.BoundPort },
            PollIntervalMs = 50,
        };
        await using var master = new ModbusMaster(masterOptions, masterStore,
            [PollDefinition.Read(ModbusFunction.ReadHoldingRegisters, 0, 3)]);
        await master.StartAsync();

        await WaitUntil(() => masterStore.GetRegister(ModbusTable.HoldingRegisters, 2) == 300);

        Assert.Equal<ushort>(100, masterStore.GetRegister(ModbusTable.HoldingRegisters, 0));
        Assert.Equal<ushort>(200, masterStore.GetRegister(ModbusTable.HoldingRegisters, 1));
    }

    [Fact]
    public async Task Master_writes_multiple_registers_into_slave_over_tcp()
    {
        var slaveStore = new ModbusDataStore();
        await using var slave = new ModbusSlave(SlaveOnEphemeralPort(), slaveStore);
        await slave.StartAsync();

        var masterOptions = new MasterOptions
        {
            Transport = ModbusTransport.Tcp,
            Tcp = new TcpOptions { Host = "127.0.0.1", Port = slave.BoundPort },
            PollIntervalMs = 50,
        };
        var writePoll = new PollDefinition
        {
            Function = ModbusFunction.WriteMultipleRegisters,
            Address = 20,
            WriteRegisters = [11, 22, 33],
        };
        await using var master = new ModbusMaster(masterOptions, new ModbusDataStore(), [writePoll]);
        await master.StartAsync();

        await WaitUntil(() => slaveStore.GetRegister(ModbusTable.HoldingRegisters, 22) == 33);

        Assert.Equal<ushort>(11, slaveStore.GetRegister(ModbusTable.HoldingRegisters, 20));
        Assert.Equal<ushort>(22, slaveStore.GetRegister(ModbusTable.HoldingRegisters, 21));
    }

    [Fact]
    public async Task Master_logs_exception_response_for_bad_address()
    {
        var slaveStore = new ModbusDataStore();
        await using var slave = new ModbusSlave(SlaveOnEphemeralPort(), slaveStore);
        await slave.StartAsync();

        var masterOptions = new MasterOptions
        {
            Transport = ModbusTransport.Tcp,
            Tcp = new TcpOptions { Host = "127.0.0.1", Port = slave.BoundPort },
            PollIntervalMs = 50,
        };
        ModbusExceptionCode? seen = null;
        await using var master = new ModbusMaster(masterOptions, new ModbusDataStore(),
            [PollDefinition.Read(ModbusFunction.ReadHoldingRegisters, 65535, 5)]);
        master.Logged += (_, e) =>
        {
            if (e.Exception != ModbusExceptionCode.None)
                seen = e.Exception;
        };
        await master.StartAsync();

        await WaitUntil(() => seen is not null);
        Assert.Equal(ModbusExceptionCode.IllegalDataAddress, seen);
    }
}
