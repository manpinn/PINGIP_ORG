using PINGIP_ORG.Common;
using PINGIP_ORG.Enums;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace PINGIP_ORG.Services
{
    public class PortCheckService
    {
        private readonly GlobalPortCheckIPDictionaryService _globalIPDictionaryService;

        private readonly ILogger<PortCheckService> _logger;

        public PortCheckService(GlobalPortCheckIPDictionaryService globalIPDictionaryService, ILogger<PortCheckService> logger)
        {
            _globalIPDictionaryService = globalIPDictionaryService;

            _logger = logger;
        }

        public async Task<string> PortCheck(string ipAddress, string remoteIpAddress, int port, IpAddressType ipType)
        {
            string ipAddressToPrint = ipType == IpAddressType.IPv6 ? $"[{ipAddress}]" : ipAddress;

            var (requestState, message) = _globalIPDictionaryService.RequestFrequencyState(remoteIpAddress, ipAddress);

            if (requestState != RequestState.Pass) return message;

            StringBuilder result = new StringBuilder();

            result.Append($"Source (Our Server): {GlobalServerIPAddress.ServerIPAddress}").Append("\n");
            result.Append($"Target: {ipAddress}").Append("\n").Append("\n");

            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    await tcpClient.ConnectAsync(ipAddress, port).WaitAsync(TimeSpan.FromSeconds(5));

                    result.Append($"Connection to {ipAddressToPrint}:{port} was successful.").Append("\n");

                    _logger.LogInformation($"Port Check: Connected to {ipAddressToPrint}:{port} succesfully.");
                }
                catch (SocketException ex)
                {
                    result.Append($"Failed to connect to {ipAddressToPrint}:{port} !")
                        .Append("\n").Append(ex.Message);
                }
                catch (Exception ex)
                {
                    result.Append($"Failed to connect to {ipAddressToPrint}:{port} !")
                        .Append("\n").Append(ex.Message);
                }
            }


            return result.ToString();
        }
    }
}
