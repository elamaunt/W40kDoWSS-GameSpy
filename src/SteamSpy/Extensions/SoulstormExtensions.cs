using System.Diagnostics;
using System.Linq;

namespace SteamSpy.Extensions
{
    public static class SoulstormExtensions
    {
        public static Process[] FindGameProcesses()
        {
            return Process.GetProcessesByName("Soulstorm");
        }
        public static bool IsGameRunning()
        {
            return FindGameProcesses().Length > 0;
        }

        public static void LaunchGame()
        {
        }
    }
}
