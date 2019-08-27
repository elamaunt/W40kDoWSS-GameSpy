using System.Diagnostics;

namespace ThunderHawk.StaticClasses.Soulstorm
{
    public static class ProcessManager
    {
        private static Process[] GetGameProcesses()
        {
            return Process.GetProcessesByName("Soulstorm");
        }

        public static bool GameIsRunning()
        {
            return GetGameProcesses().Length >= 1;
        }

        /// <summary>
        /// Получает процесс игры только в случае если игра не багнулась(запущен лишь один процесс игры)
        /// </summary>
        /// <returns></returns>
        public static Process GetGameProcess()
        {
            var gameProcs = GetGameProcesses();
            if (gameProcs.Length == 1)
            {
                return gameProcs[0];
            }
            else
            {
                return null;
            }
        }

        public static void KillAllGameProccesses()
        {
            var gameProcs = GetGameProcesses();
            if (gameProcs.Length > 1)
            {
                foreach (var proc in gameProcs)
                {
                    proc.Kill();
                }
            }
        }
    }
}
