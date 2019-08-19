using System;
using System.IO;
using ThunderHawk.Core.Services;

namespace ThunderHawk.Tweaks
{
    public class Camera : ITweak
    {
        public void ApplyTweak(string gamePath)
        {
            //TODO: Need this or no?
            /*var cameraDir = Path.Combine(Directory.GetCurrentDirectory(), "LauncherFiles\\Camera");

            if (!Directory.Exists(cameraDir))
                throw new Exception("Could not find camera in LauncherFiles!");

            var targetDir = Path.Combine(gamePath, "W40k/Data/");

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            */
        }

        public bool CheckTweak()
        {
            return false;
        }
    }
}
