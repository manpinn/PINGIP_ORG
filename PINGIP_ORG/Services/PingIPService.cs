using System.Net.NetworkInformation;
using System.Text;
using PINGIP_ORG.Common;

namespace PINGIP_ORG.Services
{
    public class PingIPService
    {
        private readonly GlobalPingIPDictionaryService _globalIPDictionary;

        private readonly ILogger<PingIPService> _logger;

        public PingIPService(GlobalPingIPDictionaryService globalIPDictionary, ILogger<PingIPService> logger)
        {
            _globalIPDictionary = globalIPDictionary;

            _logger = logger;
        }

        public async Task<string> PingIP(string ipAddress, string remoteIpAddress)
        {
            if (_globalIPDictionary.TryGet(remoteIpAddress, out DateTime lastPing))
            {
                if (DateTime.Now - lastPing < Globals.minPingTimeSpan)
                {
                    return $"Ping from your IP-Address is too frequent. Please wait {(int)((Globals.minPingTimeSpan - (DateTime.Now - lastPing)).TotalSeconds)} seconds.";
                }
                else
                {
                    _globalIPDictionary.AddOrUpdate(remoteIpAddress, DateTime.Now);
                }
            }
            else
            {
                _globalIPDictionary.AddOrUpdate(remoteIpAddress, DateTime.Now);
            }

            int pingCount = 4;            // Default number of pings
            int timeout = 1000;           // Timeout in milliseconds
            int sent = 0, received = 0, lost = 0;
            long minTime = long.MaxValue;
            long maxTime = long.MinValue;
            long totalTime = 0;

            byte[] buffer = new byte[32]; // Default ping buffer

            Ping pingSender = new Ping();

            PingOptions options = new PingOptions() { DontFragment = true };

            StringBuilder result = new StringBuilder();

            result.Append($"Source: {GlobalServerIPAddress.ServerIPAddress}").Append("\n");
            result.Append($"Target: {ipAddress}").Append("\n").Append("\n");

            result.Append($"Pinged {ipAddress} with {buffer.Length} bytes of data:").Append("\n").Append("\n");

            for (int i = 0; i < pingCount; i++)
            {
                try
                {
                    PingReply reply = await pingSender.SendPingAsync(ipAddress, timeout, buffer, options);
                    sent++;

                    if (reply != null && reply.Status == IPStatus.Success)
                    {
                        received++;
                        long time = reply.RoundtripTime;
                        minTime = Math.Min(minTime, time);
                        maxTime = Math.Max(maxTime, time);
                        totalTime += time;

                        string replyBufferLength = null;

                        if (reply.Buffer != null) replyBufferLength = reply.Buffer.Length.ToString();

                        string replyOptionsTtl = null;

                        if (reply.Options != null) replyOptionsTtl = reply.Options.Ttl.ToString();

                        result.Append($"Reply from {reply.Address}: bytes={replyBufferLength} time={time}ms TTL={replyOptionsTtl}").Append("\n");
                    }
                    else
                    {
                        lost++;
                        result.Append("\n").Append($"Request timed out.").Append("\n");
                    }
                }
                catch (Exception ex)
                {
                    lost++;

                    string message = ex.Message;

                    if (ex.InnerException != null) message += "; " + ex.InnerException.Message;

                    result.Append("\n").Append($"Ping failed: {message}").Append("\n");
                }

                Thread.Sleep(1000); // Wait 1 second between pings
            }

            result.Append("\n").Append($"Ping statistics for {ipAddress}:").Append("\n").Append("\n");
            result.Append($"Packets: Sent = {sent}, Received = {received}, Lost = {lost} ({((double)(lost * 100)) / (double)sent}% loss),").Append("\n");

            if (received > 0)
            {
                result.Append("Approximate round trip times in milli-seconds:").Append("\n");
                result.Append($"Minimum = {minTime}ms, Maximum = {maxTime}ms, Average = {totalTime / received}ms").Append("\n");

                _logger.LogInformation($"Ping IP: Pinged {ipAddress}: Sent={sent}, Received={received}, Lost={lost}, MinTime={minTime}ms, MaxTime={maxTime}ms, AvgTime={totalTime / received}ms");
            }
            else
            {
                _logger.LogInformation($"Ping IP: Pinged {ipAddress}: Sent={sent}, Received={received}, Lost={lost}, MinTime={minTime}ms, MaxTime={maxTime}ms");
            }

            return result.ToString();
        }
    }
}
