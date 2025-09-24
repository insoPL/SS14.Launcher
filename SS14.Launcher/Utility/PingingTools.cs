using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SS14.Launcher.Utility
{
    public static class PingingTools
    {
        /// <summary>
        /// Gets the ping time for a host.
        /// </summary>
        public static async Task<long> GetPingTime(string? host)
        {
            if (host == null) return -1;
            host = NormalizeToHost(host);
            if (host == string.Empty) return -1;

            var roundtripTimes = new List<long>();
            const int numPings = 3;
            short timeOuts = 0;

            for (int i = 0; i < numPings; i++)
            {
                try
                {
                    using (var pingSender = new System.Net.NetworkInformation.Ping())
                    {
                        var reply = await pingSender.SendPingAsync(host, 750);
                        if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        {
                            roundtripTimes.Add(reply.RoundtripTime);
                        }
                        else if (reply.Status == System.Net.NetworkInformation.IPStatus.TimedOut)
                        {
                            roundtripTimes.Add(750);
                            timeOuts++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    timeOuts++;
                    Log.Error(ex, "An error occurred during pinging server {ServerAddress}", host);
                }

            }

            if (timeOuts == numPings)
            {
                return -2;
            }
            else if (roundtripTimes.Any())
            {
                // Return the average of the successful pings.
                return (long)roundtripTimes.Average();
            }

            return -1;
        }

        public static string NormalizeToHost(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri result))
            {
                return result.Host;
            }

            return string.Empty;
        }

    }
}
