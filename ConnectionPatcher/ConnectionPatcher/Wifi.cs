using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NativeWifi;

namespace ConnectionPatcher
{
    public class Wifi
    {
        private WlanClient.WlanInterface Interface { get; }
        public string SSID { get; set; }

        public Wifi(string ssid)
        {
            Interface = new WlanClient().Interfaces.First();
            SSID = ssid;
        }

        public void PersistentlyTryReconnectWifi(int attemptsPerSecond)
        {
            int attempts = 0;
            bool reconnected;
            int tick = 1000 / attemptsPerSecond;
            DateTime start = DateTime.Now;
            do
            {
                attempts++;
                reconnected = TryReconnectWifi();
                if (!reconnected)
                {
                    Thread.Sleep(tick);
                }
            }
            while (!reconnected);
            DateTime end = DateTime.Now;
            double delta = (end - start).TotalMilliseconds;
            Console.WriteLine("Succesfully (re-)connected. ({0} reconnection attempt(s) in {1} ms)", attempts, delta);
        }
        private bool TryReconnectWifi()
        {
            try
            {
                ReconnectWifi();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void ReconnectWifi()
        {
            var network = Interface.GetAvailableNetworkList(0).Where(n => GetSsidString(n.dot11Ssid) == SSID).First();
            var profile = Interface.GetProfiles().Where(p => p.profileName == SSID).First();
            Interface.SetProfile(Wlan.WlanProfileFlags.AllUser, Interface.GetProfileXml(profile.profileName), true);
            Interface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profile.profileName);
        }
        private string GetSsidString(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }
    }
}
