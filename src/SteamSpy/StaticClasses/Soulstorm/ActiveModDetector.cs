using System;
using System.IO;
using System.Linq;
using System.Text;
using ThunderHawk.Core;
using NLog;
using Logger = ThunderHawk.Core.Logger;

namespace ThunderHawk.StaticClasses.Soulstorm
{
    public static class ActiveModDetector
    {
        public static string detectCurrentMode()
        {
            try
            {
                //Open the stream and read it back.
                FileInfo soulstormConsole = new FileInfo(PathFinder.GamePath + "\\warnings.log");

                var activeMod = "Not found";
                
                using (var streamReader = new StreamReader(soulstormConsole.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (streamReader.Peek() > -1) 
                    {
                        string line = streamReader.ReadLine();
                        if (line != null && line.Contains("MOD -- Initializing Mod"))
                        {
                            activeMod = line.Substring(38);
                        }
                    }
                    return activeMod;
                }
                    
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return "Can't find active mode: " + e.Message;
            }
            
        }
    }
}
