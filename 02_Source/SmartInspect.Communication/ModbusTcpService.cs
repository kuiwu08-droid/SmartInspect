using NModbus;
using System.Net.Sockets;

namespace SmartInspect.Communication;

public class ModbusTcpService
{
    private readonly object sync = new();
    private TcpClient? client;
    private IModbusMaster? master;
    public bool IsConnected => client?.Connected == true && master != null;

    public void Connect(string ipAddress, int port)
    {
        lock (sync)
        {
            Disconnect();
            var socket = new TcpClient();
            try
            {
                socket.ConnectAsync(ipAddress, port).WaitAsync(TimeSpan.FromSeconds(3)).GetAwaiter().GetResult();
                socket.ReceiveTimeout = socket.SendTimeout = 3000;
                master = new ModbusFactory().CreateMaster(socket);
                master.Transport.ReadTimeout = master.Transport.WriteTimeout = 3000;
                master.Transport.Retries = 0;
                client = socket;
            }
            catch { socket.Dispose(); throw; }
        }
    }

    public void Disconnect()
    {
        lock (sync)
        {
            master?.Dispose();
            client?.Dispose();
            master = null;
            client = null;
        }
    }

    public ushort ReadRegister(byte slaveId, ushort address)
    {
        lock (sync) return GetMaster().ReadHoldingRegisters(slaveId, address, 1)[0];
    }

    public void WriteRegister(byte slaveId, ushort address, ushort value)
    {
        lock (sync) GetMaster().WriteSingleRegister(slaveId, address, value);
    }

    public void WriteInspectionState(ushort busy, ushort result, ushort done)
    {
        lock (sync) GetMaster().WriteMultipleRegisters(1, PlcRegisterMap.Busy, [busy, result, done]);
    }

    private IModbusMaster GetMaster() => master ?? throw new InvalidOperationException("Modbus TCP 尚未连接。");
}
