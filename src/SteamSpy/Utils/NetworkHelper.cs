using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SteamSpy.Utils
{
    public static class NetworkHelper
    {
        public static uint ToInt(string addr)
        {
            return (uint)IPAddress.NetworkToHostOrder((int)IPAddress.Parse(addr).Address);
        }

        public static uint ToInt(IPAddress addr)
        {
            return (uint)IPAddress.NetworkToHostOrder((int)addr.Address);
        }

        public static string ToAddrString(uint address)
        {
            return ToAddr(address).ToString();
        }

        public static IPAddress ToAddr(uint address)
        {
            // return new IPAddress(address);
            return new IPAddress((uint)IPAddress.HostToNetworkOrder((int)address));
        }

        public static string ToAddrString(long address)
        {
            return IPAddress.Parse(address.ToString()).ToString();
            // This also works:
            // return new IPAddress((uint) IPAddress.HostToNetworkOrder(
            //    (int) address)).ToString();
        }

        public static IPAddress[] GetLocalIpAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray();
        }

        public static string GetLocalhostName()
        {
            return Dns.GetHostName();
        }

        public static string GetHostName(IPAddress ip)
        {
            return Dns.GetHostEntry(ip).HostName;
        }

        public static IPAddress GetLocalIpAddressInSameSubnet(string ip)
        {
            var addr = IPAddress.Parse(ip);
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var addressList = host.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToArray();

            return addressList.First(x =>
            {
                return IsInSameSubnet(x, addr, IPAddress.Parse(ReturnSubnetmask(x.ToString())));
            });
        }

        public static bool IsIpAddressInSameSubnet(IPAddress ip1, IPAddress ip2)
        {
            var mask = IPAddress.Parse(ReturnSubnetmask(ip1.ToString()));
            return IsInSameSubnet(ip1, ip2, mask);
        }

        private static bool IsInSameSubnet(IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = GetNetworkAddress(address, subnetMask);
            IPAddress network2 = GetNetworkAddress(address2, subnetMask);

            return network1.Equals(network2);
        }

        private static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        private static string ReturnSubnetmask(string ipaddress)
        {
            uint firstOctet = ReturnFirstOctet(ipaddress);
            if (firstOctet >= 0 && firstOctet <= 127)
                return "255.0.0.0";
            else if (firstOctet >= 128 && firstOctet <= 191)
                return "255.255.0.0";
            else if (firstOctet >= 192 && firstOctet <= 223)
                return "255.255.255.0";
            else return "0.0.0.0";
        }

        private static uint ReturnFirstOctet(string ipAddress)
        {
            IPAddress iPAddress = IPAddress.Parse(ipAddress);
            byte[] byteIP = iPAddress.GetAddressBytes();
            uint ipInUint = (uint)byteIP[0];
            return ipInUint;
        }
    }
}
