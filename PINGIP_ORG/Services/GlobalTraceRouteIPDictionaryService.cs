using PINGIP_ORG.Common;
using PINGIP_ORG.Enums;
using System.Collections.Concurrent;

namespace PINGIP_ORG.Services
{
    public class GlobalTraceRouteIPDictionaryService : IPDictionaryService
    {
        public GlobalTraceRouteIPDictionaryService() : base(
            messageIPFromBlocked: "TraceRoute from your IP-Address is too frequent.",

            messageIPToBlocked: "TraceRoute to the Target-IP-Address is too frequent."
        )
        {
        }
    }
}
