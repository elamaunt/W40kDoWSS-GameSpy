using System;
using System.Net;
using System.Runtime.InteropServices;

namespace GSMasterServer.Data
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NatNegSteamPeer
    {
        [FieldOffset(0)]
        public ulong SteamId;
        [FieldOffset(8)]
        public int ConnectionId;
        [FieldOffset(12)]
        public bool IsHost;

        [FieldOffset(0)]
        public fixed byte Sequence[13];

        public byte[] Bytes
        {
            get
            {
                fixed (byte* bytes = Sequence)
                {
                    var array = new byte[13];
                    Marshal.Copy(new IntPtr((void*)bytes), array, 0, 13);
                    return array;
                }
            }

            set
            {
                fixed (byte* bytes = Sequence)
                {
                    if (value == null)
                        Marshal.Copy(new byte[13], 0, new IntPtr((void*)bytes), 13);
                    else
                        Marshal.Copy(value, 0, new IntPtr((void*)bytes), 13);
                }
            }
        }
    }

    public class NatNegSteamClient
    {
        public int ConnectionId;

        public IPEndPoint HostPoint;
        public NatNegSteamPeer? Host;

        public IPEndPoint GuestPoint;
        public NatNegSteamPeer? Guest;

        public NatNegSteamClient(int id)
        {
            ConnectionId = id;
        }
    }
}
