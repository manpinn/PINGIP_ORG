using PINGIP_ORG.Common;
using PINGIP_ORG.Enums;
using System.Collections.Concurrent;

namespace PINGIP_ORG.Services
{
    public class GlobalPortCheckIPDictionaryService
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
            if (this.TryGetRequestsFromDic(sourceIpAddress, out DateTime lastPingFrom) && DateTime.Now - lastPingFrom < Globals.minPortCheckFromTimeSpan)
            {
                return (RequestState.RequestsFromIPTooFrequent, $"Requests from your IP-Address are too frequent. Please wait {(int)((Globals.minPortCheckFromTimeSpan - (DateTime.Now - lastPingFrom)).TotalSeconds)} seconds.");
            }
            else if (this.TryGetRequestsToDic(targetIpAddress, out DateTime lastPingTo) && DateTime.Now - lastPingTo < Globals.minPortCheckToTimeSpan)
            {
                return (RequestState.RequestsToIPTooFrequent, $"Requests to the Target-IP-Address are too frequent. Please wait {(int)((Globals.minPortCheckToTimeSpan - (DateTime.Now - lastPingTo)).TotalSeconds)} seconds.");
            }
            else
            {
                this.AddOrUpdateRequestsFromDic(sourceIpAddress, DateTime.Now);

                this.AddOrUpdateRequestsToDic(sourceIpAddress, DateTime.Now);

                return (RequestState.Pass, null);
            }
        }

        #endregion
    }
}
