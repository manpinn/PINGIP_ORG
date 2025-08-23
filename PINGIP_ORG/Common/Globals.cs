namespace PINGIP_ORG.Common
{
    public static class Globals
    {
        public static readonly TimeSpan minPingTimeFromSpan = TimeSpan.FromSeconds(30);
        
        public static readonly TimeSpan minPingTimeToSpan = TimeSpan.FromSeconds(30);

        public static readonly TimeSpan minPortCheckFromTimeSpan = TimeSpan.FromSeconds(10);
        
        public static readonly TimeSpan minPortCheckToTimeSpan = TimeSpan.FromSeconds(10);

        public static readonly TimeSpan minTraceRouteFromTimeSpan = TimeSpan.FromSeconds(30);

        public static readonly TimeSpan minTraceRouteToTimeSpan = TimeSpan.FromSeconds(30);
    }
}
