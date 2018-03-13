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
        private static int GetWirelessAdapterFirstLineNumber(string[] ipconfig)
        {
            for (int i = ipconfig.Length - 1; i >= 0; i--)
            {
                if (ipconfig[i].StartsWith("Wireless LAN adapter Wi-Fi"))
                {
                    return i;
                }
            }
            throw new NoWirelessAdapterException();
        }
        public static string GetWirelessAdapterName()
        {
            string[] ipconfig = GetIpConfig();
            int i = GetWirelessAdapterFirstLineNumber(ipconfig);
            for (; i < ipconfig.Length; i++)
            {
                if (ipconfig[i].Trim().StartsWith("Description"))
                {
                    break;
                }
            }
            if (i == ipconfig.Length)
            {
                throw new NoWirelessAdapterException();
            }
            return ipconfig[i].Split(':')[1].Trim();
        }
    }
}
