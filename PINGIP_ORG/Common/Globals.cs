namespace PINGIP_ORG.Common
{
    public static class Globals
    {
        public static readonly TimeSpan minPingTimeSpan = TimeSpan.FromSeconds(10);

        public static readonly TimeSpan minPortCheckTimeSpan = TimeSpan.FromSeconds(10);

        public static readonly TimeSpan minTraceRouteTimeSpan = TimeSpan.FromSeconds(30);
    }
}
