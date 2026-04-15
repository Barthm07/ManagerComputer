using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using ManagerComputer.Models;
using ManagerComputer.Network;

namespace ManagerComputer.Services
{
    public class ComputerManager
    {
        public ObservableCollection<Computer> Computers { get; } = new();
        private readonly Dictionary<Computer, NetworkStream> _streams = new();

        public event Action<Computer, byte[]>? FrameReceived;

        public void AddClient(TcpClient client)
        {
            var ip = (client.Client.RemoteEndPoint?.ToString() ?? "Unknown").Split(':')[0];
            var computer = new Computer
            {
                Name = $"PC - {ip}",
                IpAddress = ip,
                IsOnline = true
            };

            var stream = client.GetStream();
            _streams[computer] = stream;
            Computers.Add(computer);

            _ = Task.Run(() => ListenForFrames(computer, stream));
        }

        private async Task ListenForFrames(Computer computer, NetworkStream stream)
        {
            try
            {
                var header = new byte[8];
                while (true)
                {
                    await ReadExactAsync(stream, header, 8);
                    var length = BitConverter.ToInt32(header, 4);
                    var data = new byte[length];
                    await ReadExactAsync(stream, data, length);

                    var packet = Packet.Deserialize(header, data);
                    if (packet.Type == PacketType.ScreenFrame)
                        FrameReceived?.Invoke(computer, data);
                }
            }
            catch
            {
                // Client disconnected
                computer.IsOnline = false;
                _streams.Remove(computer);
            }
        }

        private static async Task ReadExactAsync(Stream stream, byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int read = await stream.ReadAsync(buffer, offset, count - offset);
                if (read == 0) throw new EndOfStreamException();
                offset += read;
            }
        }

        public void SendLock(Computer computer)
        {
            if (_streams.TryGetValue(computer, out var stream))
            {
                var packet = new Packet { Type = PacketType.LockCommand };
                stream.Write(packet.Serialize());
            }
        }

        public void SendUnlock(Computer computer)
        {
            if (_streams.TryGetValue(computer, out var stream))
            {
                var packet = new Packet { Type = PacketType.UnlockCommand };
                stream.Write(packet.Serialize());
            }
        }

        public void LockAll()
        {
            foreach (var computer in Computers)
                SendLock(computer);
        }

        public void UnlockAll()
        {
            foreach (var computer in Computers)
                SendUnlock(computer);
        }
    }
}