using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Diagnostics;
using System.Threading;
using System.Configuration;

namespace ConnectionPatcher
{
    public class Program
    {
        private static readonly string
            QUERY = ConfigurationManager.AppSettings["Query"],
            DEFAULT_GATEWAY = ConfigurationManager.AppSettings["DefaultGateway"],
            SSID = ConfigurationManager.AppSettings["Ssid"];
        private static readonly int
            PACKETS_PER_TEST = int.Parse(ConfigurationManager.AppSettings["PacketsPerTest"]),
            TIMEOUT_WAIT_MS = int.Parse(ConfigurationManager.AppSettings["TimeoutWaitMs"]),
            ACCEPTABLE_PACKET_LOSS_PERCENTAGE = int.Parse(ConfigurationManager.AppSettings["AcceptablePacketLossPercentage"]),
            ACCEPTABLE_LATENCY_MS = int.Parse(ConfigurationManager.AppSettings["AcceptableLatencyMs"]),
            RECONNECTION_ATTEMPTS_PER_SECOND = int.Parse(ConfigurationManager.AppSettings["ReconnectionAttemptsPerSecond"]);

        private Wifi Wifi { get; }
        private NetworkAdapter Adapter { get; }
        private Process Ping { get; }

        public static void Main(string[] args)
        {
            new Program().Start();
        }

        public Program()
        {
            // wrap wifi connection
            Wifi = new Wifi(SSID);
            // wrap network adapter
            string query = string.Format(QUERY, Detecter.GetWirelessLanAdapterName());
            var result = new ManagementObjectSearcher(query).Get();
            foreach (ManagementObject obj in result) // by lack of .first() method
            {
                Adapter = new NetworkAdapter(obj);
            }
            // wrap ping process
            object[] args = { DEFAULT_GATEWAY, PACKETS_PER_TEST, TIMEOUT_WAIT_MS };
            Ping = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ping.exe",
                    Arguments = string.Format("{0} -n {1} -w {2}", args),
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
        }

        public void Start()
        {
            while (true)
            {
                Cause cause = TestConnection();
                switch (cause)
                {
                    case Cause.NONE:
                        Console.WriteLine("Test results within acceptable range.");
                        break;
                    case Cause.PACKET_LOSS:
                        Console.WriteLine("Timeout percentage below acceptable range.");
                        break;
                    case Cause.HIGH_LATENCY:
                        Console.WriteLine("Latency above acceptable range.");
                        break;
                }
                if (cause != Cause.NONE)
                {
                    ResetConnection();
                }
            }
        }
        private Cause TestConnection()
        {
            int timeouts = 0, latency = 0;
            Ping.Start();
            while (!Ping.StandardOutput.EndOfStream)
            {
                string line = Ping.StandardOutput.ReadLine();
                Console.WriteLine(line);
                if (line.StartsWith("Request timed out.")
                    || line.EndsWith("Destination host unreachable."))
                {
                    timeouts++;
                    latency += TIMEOUT_WAIT_MS;
                }
                else if (line.Contains("time="))
                {
                    latency += int.Parse(line.Split(' ')[4].Split('=')[1].Split('m')[0]);
                }
            }
            Cause cause = Cause.NONE;
            if (timeouts * 100 / PACKETS_PER_TEST > ACCEPTABLE_PACKET_LOSS_PERCENTAGE)
            {
                cause = Cause.PACKET_LOSS;
            }
            else if (latency / PACKETS_PER_TEST > ACCEPTABLE_LATENCY_MS)
            {
                cause = Cause.HIGH_LATENCY;
            }
            return cause;
        }
        private void ResetConnection()
        {
            Console.WriteLine("Resetting adapter.");
            Adapter.Disable();
            Adapter.Enable();
            Console.WriteLine("Reconnecting to wi-fi network.");
            Wifi.PersistentlyTryReconnectWifi(RECONNECTION_ATTEMPTS_PER_SECOND);
        }
    }
}
