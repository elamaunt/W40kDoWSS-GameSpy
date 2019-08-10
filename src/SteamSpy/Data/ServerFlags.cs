using System;

namespace ThunderHawk.Data
{
    [Flags]
    public enum ServerFlags
    {
        UNSOLICITED_UDP_FLAG = 1,
        PRIVATE_IP_FLAG = 2,
        CONNECT_NEGOTIATE_FLAG = 4,
        ICMP_IP_FLAG = 8,
        NONSTANDARD_PORT_FLAG = 16,
        NONSTANDARD_PRIVATE_PORT_FLAG = 32,
        HAS_KEYS_FLAG = 64,
        HAS_FULL_RULES_FLAG = 128
    }
}
