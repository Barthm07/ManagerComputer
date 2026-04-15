namespace ManagerComputer.Network
{
    public enum PacketType
    {
        ScreenFrame,
        LockCommand,
        UnlockCommand
    }

    public class Packet
    {
        public PacketType Type { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public byte[] Serialize()
        {
            var typeBytes = BitConverter.GetBytes((int)Type);
            var lengthBytes = BitConverter.GetBytes(Data.Length);
            var result = new byte[8 + Data.Length];
            Buffer.BlockCopy(typeBytes, 0, result, 0, 4);
            Buffer.BlockCopy(lengthBytes, 0, result, 4, 4);
            Buffer.BlockCopy(Data, 0, result, 8, Data.Length);
            return result;
        }

        public static Packet Deserialize(byte[] header, byte[] data)
        {
            var type = (PacketType)BitConverter.ToInt32(header, 0);
            return new Packet { Type = type, Data = data };
        }
    }
}