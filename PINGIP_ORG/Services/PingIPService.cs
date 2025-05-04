using System.Net.NetworkInformation;
using System.Text;
using PINGIP_ORG.Common;

namespace PINGIP_ORG.Services
{
    public class PingIPService
    {
        private readonly GlobalIPDictionaryService _globalIPDictionary;

        private readonly ILogger<PingIPService> _logger;

        public PingIPService(GlobalIPDictionaryService globalIPDictionary, ILogger<PingIPService> logger)
        {
            _globalIPDictionary = globalIPDictionary;

            _logger = logger;
        }

        public string PingIP(string ipAddress, string remoteIpAddress)
        {
            if (_globalIPDictionary.TryGet(remoteIpAddress, out DateTime lastPing))
            {
                if (DateTime.Now - lastPing < Globals.minPingTimeSpan)
                {
                    return $"Ping from your IP-address is too frequent. Please wait {(int)((Globals.minPingTimeSpan - (DateTime.Now - lastPing)).TotalSeconds)} seconds.";
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

            StringBuilder result = new StringBuilder();

            result.Append($"Pinged {ipAddress} with {buffer.Length} bytes of data:").Append("<br>");

            for (int i = 0; i < pingCount; i++)
            {
                try
                {
                    PingReply reply = pingSender.Send(ipAddress, timeout, buffer);
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

                        result.Append($"Reply from {reply.Address}: bytes={replyBufferLength} time={time}ms TTL={replyOptionsTtl}").Append("<br>");
                    }
                    else
                    {
                        lost++;
                        result.Append($"Request timed out.").Append("<br>");
                    }
                }
                catch (Exception ex)
                {
                    lost++;

                    string message = ex.Message;

                    if (ex.InnerException != null) message += "; " + ex.InnerException.Message;

                    result.Append($"Ping failed: {message}").Append("<br>");
                }

                Thread.Sleep(1000); // Wait 1 second between pings
            }

            result.Append($"Ping statistics for {ipAddress}:").Append("<br>");
            result.Append($"    Packets: Sent = {sent}, Received = {received}, Lost = {lost} ({((double)(lost * 100)) / (double)sent}% loss),").Append("<br>");

            if (received > 0)
            {
                result.Append("Approximate round trip times in milli-seconds:").Append("<br>");
                result.Append($"    Minimum = {minTime}ms, Maximum = {maxTime}ms, Average = {totalTime / received}ms").Append("<br>");

                _logger.LogInformation($"Pinged {ipAddress} from {remoteIpAddress}: Sent={sent}, Received={received}, Lost={lost}, MinTime={minTime}ms, MaxTime={maxTime}ms, AvgTime={totalTime / received}ms");
            }
            else
            {
                _logger.LogInformation($"Pinged {ipAddress} from {remoteIpAddress}: Sent={sent}, Received={received}, Lost={lost}, MinTime={minTime}ms, MaxTime={maxTime}ms");
            }

            return result.ToString();
        }
    }
}
