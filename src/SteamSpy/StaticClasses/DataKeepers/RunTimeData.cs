using System;

namespace SteamSpy.StaticClasses.DataKeepers
{
    public static class RunTimeData
    {
        public static string GamePath { get; private set; }

        public static void SetGamePath(string path)
        {
            GamePath = path;
            //OnPathUpdated?.Invoke();
        }

        //public static event Action OnPathUpdated;

    }
}
