using System.Net;
using System.Net.Sockets;

namespace ManagerComputer.Network
{
    public class ServerListener
    {
        private TcpListener? _listener;
        public event Action<TcpClient>? ClientConnected;

        public async Task StartAsync(int port = 5000)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                ClientConnected?.Invoke(client);
            }
        }

        public void Stop()
        {
            _listener?.Stop();
        }
    }
}