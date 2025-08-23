using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PINGIP_ORG.Services
{
    public class IPCleanupService : BackgroundService
    {
        private readonly GlobalPingIPDictionaryService _globalIPDictionary;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _cleanupTimeSpanDate = TimeSpan.FromMinutes(3);
        private readonly ILogger<IPCleanupService> _logger;

        public IPCleanupService(GlobalPingIPDictionaryService globalIPDictionary, ILogger<IPCleanupService> logger)
        {
            _globalIPDictionary = globalIPDictionary;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running IP cleanup at: {time}", DateTime.Now);

                foreach (var item in _globalIPDictionary.GetAllRequestsFromDic())
                {
                    DateTime timestamp = item.Value;

                    if ((DateTime.Now - timestamp) > _cleanupTimeSpanDate)
                    {
                        _globalIPDictionary.TryRemoveRequestsFromDic(item.Key);

                        //_logger.LogInformation("Removed stale key: {key}", item.Key);
                    }

                }

                foreach (var item in _globalIPDictionary.GetAllRequestsToDic())
                {
                    DateTime timestamp = item.Value;

                    if ((DateTime.Now - timestamp) > _cleanupTimeSpanDate)
                    {
                        _globalIPDictionary.TryRemoveRequestsToDic(item.Key);

                        //_logger.LogInformation("Removed stale key: {key}", item.Key);
                    }

                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
}
