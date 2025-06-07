using System.Collections.Concurrent;

namespace PINGIP_ORG.Services
{
    public class GlobalTraceRouteIPDictionaryService
    {
        private readonly ConcurrentDictionary<string, DateTime> _data = new();

        public IEnumerable<KeyValuePair<string, DateTime>> GetAll() => _data;

        public void AddOrUpdate(string key, DateTime value)
        {
            _data[key] = value; // adds or updates
        }

        public bool TryRemove(string key)
        {
            return _data.TryRemove(key, out _);
        }

        public bool TryGet(string key, out DateTime value)
        {
            return _data.TryGetValue(key, out value);
        }
    }
}
