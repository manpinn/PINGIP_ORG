using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PINGIP_ORG.Services
{
    public class IPCleanupService : BackgroundService
    {
        private readonly GlobalPingIPDictionaryService _globalPingIPDictionary;
        private readonly GlobalPortCheckIPDictionaryService _globalPortCheckIPDictionary;
        private readonly GlobalTraceRouteIPDictionaryService _globalTraceRouteIPDictionary;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _cleanupTimeSpanDate = TimeSpan.FromMinutes(3);
        private readonly ILogger<IPCleanupService> _logger;

        public IPCleanupService(
            GlobalPingIPDictionaryService globalPingIPDictionary, 
            GlobalPortCheckIPDictionaryService globalPortCheckIPDictionary,
            GlobalTraceRouteIPDictionaryService globalTraceRouteIPDictionary,
            ILogger<IPCleanupService> logger)
        {
            _globalPingIPDictionary = globalPingIPDictionary;
            _globalPortCheckIPDictionary = globalPortCheckIPDictionary;
            _globalTraceRouteIPDictionary = globalTraceRouteIPDictionary;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Running IP cleanup at: {timestamp}", DateTime.Now);

                    CleanupOldEntries(
                        () => _globalPingIPDictionary.GetAllRequestsFromDic(),
                        key => _globalPingIPDictionary.TryRemoveRequestsFromDic(key)
                    );

                    CleanupOldEntries(
                        () => _globalPingIPDictionary.GetAllRequestsToDic(),
                        key => _globalPingIPDictionary.TryRemoveRequestsToDic(key)
                    );

                    CleanupOldEntries(
                        () => _globalPortCheckIPDictionary.GetAllRequestsFromDic(),
                        key => _globalPortCheckIPDictionary.TryRemoveRequestsFromDic(key)
                    );

                    CleanupOldEntries(
                        () => _globalPortCheckIPDictionary.GetAllRequestsToDic(),
                        key => _globalPortCheckIPDictionary.TryRemoveRequestsToDic(key)
                    );

                    CleanupOldEntries(
                        () => _globalTraceRouteIPDictionary.GetAllRequestsFromDic(),
                        key => _globalTraceRouteIPDictionary.TryRemoveRequestsFromDic(key)
                    );

                    CleanupOldEntries(
                        () => _globalTraceRouteIPDictionary.GetAllRequestsToDic(),
                        key => _globalTraceRouteIPDictionary.TryRemoveRequestsToDic(key)
                    );

                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during IP cleanup  at: {timestamp}", DateTime.Now);
            }
        }

        private void CleanupOldEntries(
            Func<ConcurrentDictionary<string, DateTime>> getObjects,
            Func<string, bool> destroyObjects)
        {
            getObjects()
                .Where(item => (DateTime.Now - item.Value) > _cleanupTimeSpanDate)
                .Select(item => destroyObjects(item.Key))
                .ToList();
        }
    }
}
