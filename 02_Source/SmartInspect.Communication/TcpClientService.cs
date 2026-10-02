using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SmartInspect.Communication
{
    public class TcpClientService
    {
        private TcpClient? client;
        private NetworkStream? stream;

        public bool IsConnected
        {
            get
            {
                return client != null &&
                       client.Connected;
            }
        }

        public async Task ConnectAsync(
            string ipAddress,
            int port)
        {
            client = new TcpClient();

            await client.ConnectAsync(
                ipAddress,
                port);

            stream = client.GetStream();
        }

        public async Task SendAsync(string message)
        {
            if (stream == null)
            {
                throw new InvalidOperationException(
                    "TCP 尚未连接。");
            }

            byte[] data =
                Encoding.UTF8.GetBytes(message);

            await stream.WriteAsync(
                data,
                0,
                data.Length);
        }

        public async Task<string> ReceiveAsync()
        {
            if (stream == null)
            {
                throw new InvalidOperationException(
                    "TCP 尚未连接。");
            }

            byte[] buffer =
                new byte[1024];

            using var timeout = new System.Threading.CancellationTokenSource(3000);

            int bytesRead;
            try
            {
                bytesRead = await stream.ReadAsync(
                    buffer.AsMemory(),
                    timeout.Token);
            }
            catch (OperationCanceledException ex) when (timeout.IsCancellationRequested)
            {
                throw new TimeoutException("PLC 接收超时（3000 ms），未收到回复。", ex);
            }

            if (bytesRead == 0)
            {
                return "";
            }

            string message =
                Encoding.UTF8.GetString(
                    buffer,
                    0,
                    bytesRead);

            return message;
        }

        public void Disconnect()
        {
            stream?.Close();
            client?.Close();

            stream = null;
            client = null;
        }
    }
}
