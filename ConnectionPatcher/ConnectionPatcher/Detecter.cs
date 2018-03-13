using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConnectionPatcher
{
    public static class Detecter
    {
        private static string[] GetIpConfig()
        {
            var ipconfig = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ipconfig.exe",
                    Arguments = "/all",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            ipconfig.Start();
            return ipconfig.StandardOutput.ReadToEnd()
                .Split(new[] { "\n" }, StringSplitOptions.None);
        }
        private static int GetWirelessLanAdapterFirstLineNumber(string[] ipconfig)
        {
            for (int i = ipconfig.Length - 1; i >= 0; i--)
            {
                if (ipconfig[i].StartsWith("Wireless LAN adapter Wi-Fi"))
                {
                    return i;
                }
            }
            throw new Exception("No wireless LAN adapter detected.");
        }
        public static string GetWirelessLanAdapterName()
        {
            string[] ipconfig = GetIpConfig();
            int i = GetWirelessLanAdapterFirstLineNumber(ipconfig);
            return ipconfig[i + 3].Split(':')[1].Trim();
        }
    }
}
