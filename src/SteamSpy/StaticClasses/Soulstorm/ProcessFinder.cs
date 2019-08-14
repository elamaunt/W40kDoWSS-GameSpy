using System.Diagnostics;

namespace ThunderHawk.StaticClasses.Soulstorm
{
    public static class ProcessFinder
    {
        public static Process[] FindGameProcesses()
        {
            return Process.GetProcessesByName("Soulstorm");
        }

        public static bool IsProcessFound()
        {
            return FindGameProcesses().Length > 0;
        }
    }
}
