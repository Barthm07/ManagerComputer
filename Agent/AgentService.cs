using ManagerComputer.Network;
using System.IO;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;

namespace ManagerComputer.Agent
{
    public class AgentService
    {
        private readonly AgentClient _client = new();
        private bool _isLocked = false;
        private CancellationTokenSource _cts = new();

        public bool IsLocked => _isLocked;
        public event Action? LockRequested;
        public event Action? UnlockRequested;

        public async Task StartAsync(string serverIp)
        {
            await _client.ConnectAsync(serverIp);
            _ = Task.Run(StreamScreenAsync);
            _ = Task.Run(ListenForCommandsAsync);
        }

        private async Task StreamScreenAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (!_isLocked)
                {
                    var frame = CaptureScreen();
                    var packet = new Packet { Type = PacketType.ScreenFrame, Data = frame };
                    await _client.SendAsync(packet.Serialize());
                }
                await Task.Delay(100); // ~10fps
            }
        }

        private async Task ListenForCommandsAsync()
        {
            var header = new byte[8];
            while (_client.Stream != null)
            {
                await ReadExactAsync(_client.Stream, header, 8);
                var type = (PacketType)BitConverter.ToInt32(header, 0);

                if (type == PacketType.LockCommand)
                {
                    _isLocked = true;
                    LockRequested?.Invoke();
                }
                else if (type == PacketType.UnlockCommand)
                {
                    _isLocked = false;
                    UnlockRequested?.Invoke();
                }
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

        private byte[] CaptureScreen()
        {
            var bounds = WinForms.Screen.PrimaryScreen!.Bounds;
            using var bmp = new Drawing.Bitmap(bounds.Width, bounds.Height);
            using var g = Drawing.Graphics.FromImage(bmp);
            g.CopyFromScreen(bounds.Location, Drawing.Point.Empty, bounds.Size);

            using var ms = new MemoryStream();
            var encoder = Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                .First(c => c.FormatID == Drawing.Imaging.ImageFormat.Jpeg.Guid);
            var encoderParams = new Drawing.Imaging.EncoderParameters(1);
            encoderParams.Param[0] = new Drawing.Imaging.EncoderParameter(
                Drawing.Imaging.Encoder.Quality, 40L);
            bmp.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }

        public void Stop() => _cts.Cancel();
    }
}