namespace PINGIP_ORG.Models
{
    public class PingInput
    {
        public string? ipAddress { get; set; }

        public bool isHostname { get; set; }

        public bool whois { get; set; }

        public bool dns { get; set; }
    }
}
