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
        private readonly GlobalPortCheckIPDictionaryService _globalIPDictionary;

        private readonly ILogger<PortCheckService> _logger;

        public PortCheckService(GlobalPortCheckIPDictionaryService globalIPDictionary, ILogger<PortCheckService> logger)
        {
            _globalIPDictionary = globalIPDictionary;

            _logger = logger;
        }

        public async Task<string> PortCheck(string ipAddress, string remoteIpAddress, int port, IpAddressType ipType)
        {
            string ipAddressToPrint = ipType == IpAddressType.IPv6 ? $"[{ipAddress}]" : ipAddress;

            if (_globalIPDictionary.TryGet(remoteIpAddress, out DateTime lastPing))
            {
                if (DateTime.Now - lastPing < Globals.minPortCheckTimeSpan)
                {
                    return $"Requests from your IP-Address are too frequent. Please wait {(int)((Globals.minPortCheckTimeSpan - (DateTime.Now - lastPing)).TotalSeconds)} seconds.";
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

            StringBuilder result = new StringBuilder();

            result.Append($"Source: {GlobalServerIPAddress.ServerIPAddress}").Append("\n");
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
