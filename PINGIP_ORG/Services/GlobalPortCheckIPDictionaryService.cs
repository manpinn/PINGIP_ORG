using PINGIP_ORG.Common;
using PINGIP_ORG.Enums;
using System.Collections.Concurrent;

namespace PINGIP_ORG.Services
{
    public class GlobalPortCheckIPDictionaryService : IPDictionaryService
    {
        public GlobalPortCheckIPDictionaryService() : base(
            messageIPFromBlocked: "Requests from your IP-Address are too frequent.",

            messageIPToBlocked: "Requests to the Target-IP-Address are too frequent."
        )
        {
        }
    }
}
