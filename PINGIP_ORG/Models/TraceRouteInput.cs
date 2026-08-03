namespace PINGIP_ORG.Models
{
    public class TraceRouteInput()
    {
        public string? ipAddress { get; set; }

        public bool whois { get; set; }

        public bool dns { get; set; }

        public int? timeout { get; set; } = null;

        public int? maxhops { get; set; } = null;
    }
}
