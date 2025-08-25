using PINGIP_ORG.Common;
using PINGIP_ORG.Enums;
using System.Collections.Concurrent;

namespace PINGIP_ORG.Services
{
    public class GlobalPingIPDictionaryService : IPDictionaryService
    {
        public GlobalPingIPDictionaryService() : base(
            messageIPFromBlocked: "Ping from your IP-Address is too frequent.",

            messageIPToBlocked: "Ping to the Target-IP-Address is too frequent."
            )
        {
        }
    }
}
