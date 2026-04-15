using System.Net.Sockets;

namespace ManagerComputer.Network
{
    public class AgentClient
    {
        private TcpClient? _client;
        private NetworkStream? _stream;

        public NetworkStream? Stream => _stream;
        public bool IsConnected => _client?.Connected ?? false;

        public async Task ConnectAsync(string serverIp, int port = 5000)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(serverIp, port);
            _stream = _client.GetStream();
        }

        public async Task SendAsync(byte[] data)
        {
            if (_stream != null)
                await _stream.WriteAsync(data);
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
        }
    }
}