using System;
using System.IO;
using Logger = ThunderHawk.Core.Logger;

namespace ThunderHawk.StaticClasses.Soulstorm
{
    public static class InGameHelper
    {
        public static (string, string) DetectCurrentMode()
        {
            try
            {
                //Open the stream and read it back.
                FileInfo soulstormConsole = new FileInfo(PathFinder.GamePath + "\\warnings.log");

                string activeMod = null;
                
                using (var streamReader = new StreamReader(soulstormConsole.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (streamReader.Peek() > -1) 
                    {
                        var line = streamReader.ReadLine();

                        if (line == null)
                            continue;

                        var str = "MOD -- Initializing Mod";
                        var index = line.IndexOf(str, StringComparison.OrdinalIgnoreCase);

                        if (index != -1)
                            activeMod = line.Substring(index + str.Length);
                    }

                    if (activeMod != null)
                    {
                        var split = activeMod.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (split.Length > 1)
                            return (split[0], split[1]);
                    }
                }  
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return (null, null);
        }
    }
}
