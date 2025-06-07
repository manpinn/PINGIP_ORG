using PINGIP_ORG.Common;
using System;
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

        public async Task<string> PortCheck(string ipAddress, string remoteIpAddress, int port)
        {
            if (_globalIPDictionary.TryGet(remoteIpAddress, out DateTime lastPing))
            {
                if (DateTime.Now - lastPing < Globals.minPortCheckTimeSpan)
                {
                    return $"Requests from your IP-address is are too frequent. Please wait {(int)((Globals.minPortCheckTimeSpan - (DateTime.Now - lastPing)).TotalSeconds)} seconds.";
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

            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    await tcpClient.ConnectAsync(ipAddress, port).WaitAsync(TimeSpan.FromSeconds(5));

                    result.Append($"Connection to {ipAddress}:{port} was successful.").Append("<br>");

                    _logger.LogInformation($"Port Check: Connected to {ipAddress}:{port} succesfully from {remoteIpAddress}.");
                }
                catch (SocketException ex)
                {
                    result.Append($"Failed to connect to {ipAddress}:{port} !")
                        .Append("<br>").Append(ex.Message);
                }
                catch (Exception ex)
                {
                    result.Append($"Failed to connect to {ipAddress}:{port} !")
                        .Append("<br>").Append(ex.Message);
                }
            }


            return result.ToString();
        }
    }
}
