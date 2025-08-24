using PINGIP_ORG.Common;
using PINGIP_ORG.Enums;
using System.Collections.Concurrent;

namespace PINGIP_ORG.Services
{
    public class GlobalTraceRouteIPDictionaryService
    {
        #region Properties
        
        private readonly ConcurrentDictionary<string, DateTime> _requestsFromDic = new();

        private readonly ConcurrentDictionary<string, DateTime> _requestsToDic = new();

        public IEnumerable<KeyValuePair<string, DateTime>> GetAllRequestsFromDic() => _requestsFromDic;

        public IEnumerable<KeyValuePair<string, DateTime>> GetAllRequestsToDic() => _requestsToDic;

        #endregion

        #region RequestsFromDic

        public void AddOrUpdateRequestsFromDic(string key, DateTime value)
        {
            _requestsFromDic[key] = value; // adds or updates
        }

        public bool TryRemoveRequestsFromDic(string key)
        {
            return _requestsFromDic.TryRemove(key, out _);
        }

        public bool TryGetRequestsFromDic(string key, out DateTime value)
        {
            return _requestsFromDic.TryGetValue(key, out value);
        }

        #endregion

        #region RequestsToDic

        public void AddOrUpdateRequestsToDic(string key, DateTime value)
        {
            _requestsFromDic[key] = value; // adds or updates
        }

        public bool TryRemoveRequestsToDic(string key)
        {
            return _requestsFromDic.TryRemove(key, out _);
        }

        public bool TryGetRequestsToDic(string key, out DateTime value)
        {
            return _requestsFromDic.TryGetValue(key, out value);
        }

        #endregion

        #region RequestFrequencyState

        public (RequestState requestState, string? message) RequestFrequencyState(string sourceIpAddress, string targetIpAddress)
        {
            if (this.TryGetRequestsFromDic(sourceIpAddress, out DateTime lastPingFrom) && DateTime.Now - lastPingFrom < Globals.minTraceRouteFromTimeSpan)
            {
                return (RequestState.RequestsFromIPTooFrequent, $"TraceRoute from your IP-Address is too frequent. Please wait {(int)((Globals.minTraceRouteFromTimeSpan - (DateTime.Now - lastPingFrom)).TotalSeconds)} seconds.");
            }
            else if (this.TryGetRequestsToDic(targetIpAddress, out DateTime lastPingTo) && DateTime.Now - lastPingTo < Globals.minTraceRouteToTimeSpan)
            {
                return (RequestState.RequestsToIPTooFrequent, $"TraceRoute to the Target-IP-Address is too frequent. Please wait {(int)((Globals.minTraceRouteToTimeSpan - (DateTime.Now - lastPingTo)).TotalSeconds)} seconds.");
            }
            else
            {
                this.AddOrUpdateRequestsFromDic(sourceIpAddress, DateTime.Now);

                this.AddOrUpdateRequestsToDic(targetIpAddress, DateTime.Now);

                return (RequestState.Pass, null);
            }
        }

        #endregion
    }
}
