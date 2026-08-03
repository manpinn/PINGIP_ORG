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

        public async Task<string> PortCheck(string ipAddress, string remoteIpAddress, int port, IpAddressType ipType, Enums.ProtocolType connectionType)
        {
            string ipAddressToPrint = ipType == IpAddressType.IPv6 ? $"[{ipAddress}]" : ipAddress;

            var (requestState, message) = _globalIPDictionaryService.RequestFrequencyState(remoteIpAddress, ipAddress);

            if (requestState != RequestState.Pass) return message ?? "Request not allowed.";

            StringBuilder result = new StringBuilder();

            result.Append($"Source (Our Server): {GlobalServerIPAddress.ServerIPAddress}").Append("\n");
            result.Append($"Target: {ipAddress}").Append("\n").Append("\n");


            if (connectionType == Enums.ProtocolType.TCP)
            {
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
                        result.Append($"SocketException. Failed to connect to {ipAddressToPrint}:{port} !")
                            .Append("\n").Append(ex.Message);

                        if (ex.InnerException != null)
                            result.Append("\n").Append($"Inner Exception: {ex.InnerException.Message}\n");
                    }
                    catch (Exception ex)
                    {
                        result.Append($"Failed to connect to {ipAddressToPrint}:{port} !")
                            .Append("\n").Append(ex.Message);

                        if (ex.InnerException != null)
                            result.Append("\n").Append($"Inner Exception: {ex.InnerException.Message}\n");
                    }
                }
            }
            else if (connectionType == Enums.ProtocolType.UDP)
            {
                using (UdpClient udp = new UdpClient())
                {
                    udp.Client.ReceiveTimeout = 3;

                    try
                    {
                        udp.Connect(ipAddress, port);

                        byte[] data = Encoding.ASCII.GetBytes("ping");

                        await udp.SendAsync(data, data.Length);

                        var response = await udp.ReceiveAsync();

                        result.Append($"UDP response from {ipAddress}:{port}: {Encoding.ASCII.GetString(response.Buffer)}\n");
                    }
                    catch (SocketException ex)
                    {
                        result.Append($"SocketException({ex.SocketErrorCode}, {(SocketError)ex.SocketErrorCode}): UDP port {port} CLOSED or unreachable.\nError: {ex.Message}\n");

                        if (ex.InnerException != null)
                            result.Append("\n").Append($"Inner Exception: {ex.InnerException.Message}\n");
                    }
                    catch (Exception ex)
                    {
                        result.Append($"UDP check failed for {ipAddress}:{port}.\n{ex.Message}\n");

                        if (ex.InnerException != null)
                            result.Append("\n").Append($"Inner Exception: {ex.InnerException.Message}\n");
                    }
                }
            }


            return result.ToString();
        }
    }
}
