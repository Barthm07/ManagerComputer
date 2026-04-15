namespace ManagerComputer.Models
{
    public class Computer
    {
        public string Name { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public bool IsLocked { get; set; } = false;
        public bool IsOnline { get; set; } = false;
    }
}