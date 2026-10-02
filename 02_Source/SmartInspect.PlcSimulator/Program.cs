using NModbus;
using NModbus.Data;
using SmartInspect.Communication;
using System.Net;
using System.Net.Sockets;

const byte slaveId = 1;
const int port = 5020;

Console.WriteLine("Modbus PLC 模拟器正在启动...");
Console.WriteLine("IP：127.0.0.1");
Console.WriteLine("Port：" + port);
Console.WriteLine("Slave ID：" + slaveId);
Console.WriteLine();

TcpListener listener =
    new TcpListener(
        IPAddress.Loopback,
        port);

listener.Start();

ModbusFactory factory =
    new ModbusFactory();

ISlaveDataStore dataStore =
    new SlaveDataStore();

IModbusSlave slave =
    factory.CreateSlave(
        slaveId,
        dataStore);

IModbusSlaveNetwork network =
    factory.CreateSlaveNetwork(
        listener);

network.AddSlave(slave);

// 初始化 Trigger
dataStore.HoldingRegisters.WritePoints(
    PlcRegisterMap.Trigger,
    new ushort[] { 0 });

// 初始化 Busy
dataStore.HoldingRegisters.WritePoints(
    PlcRegisterMap.Busy,
    new ushort[] { 0 });

// 初始化 Result
dataStore.HoldingRegisters.WritePoints(
    PlcRegisterMap.Result,
    new ushort[] { PlcResultCode.None });

// 初始化 Done
dataStore.HoldingRegisters.WritePoints(
    PlcRegisterMap.Done,
    new ushort[] { 0 });

// 初始化 Heartbeat
dataStore.HoldingRegisters.WritePoints(
    PlcRegisterMap.Heartbeat,
    new ushort[] { 0 });

Console.WriteLine("Modbus TCP 模拟 PLC 已启动");
Console.WriteLine();
Console.WriteLine("寄存器地址：");
Console.WriteLine("0 = Trigger");
Console.WriteLine("1 = Busy");
Console.WriteLine("2 = Result");
Console.WriteLine("3 = Done");
Console.WriteLine("4 = Heartbeat");
Console.WriteLine();
Console.WriteLine("等待 Modbus 主站连接...");

await network.ListenAsync();