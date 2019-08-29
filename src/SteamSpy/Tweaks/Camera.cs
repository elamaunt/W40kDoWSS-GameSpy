using System;
using System.IO;
using System.Linq;
using ThunderHawk.Core.Services;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk.Tweaks
{
    public class Camera : ITweak
    {
        public string TweakTitle => Core.CoreContext.LangService.GetString("CameraTweakTitle");

        public string TweakDescription => Core.CoreContext.LangService.GetString("CameraTweakDescription");

        public bool IsRecommendedTweak { get; } = true;

        private readonly string[] cameraFiles = new string[] { "camera_high.lua", /*"camera_me.lua",*/ "camera_low.lua" }; 

        private string[] CheckFolders(string gamePath)
        {
            return new string[]
            {
                Path.Combine(gamePath, "W40k", "Data"),
                Path.Combine(gamePath, "DXP2", "Data")
            };
        }

        public bool CheckTweak()
        {
            var gamePath = PathFinder.GamePath;
            var foundCamera = false;
            foreach(var cFolder in CheckFolders(gamePath))
            {
                if (foundCamera)
                    break;

                foundCamera = true;
                foreach(var cameraFile in cameraFiles)
                {
                    if (!File.Exists(Path.Combine(cFolder, cameraFile)))
                        foundCamera = false;
                }
            }
            return foundCamera;
        }

        public void EnableTweak()
        {
            var gamePath = PathFinder.GamePath;
            var cameraDir = Path.Combine("GameFiles", "Tweaks", "Camera");
            if (!Directory.Exists(cameraDir))
                throw new Exception("Could not find Camera in GameFiles!");
            var dirCameraFiles = Directory.GetFiles(cameraDir);
            if (!dirCameraFiles.Select(c => Path.GetFileName(c)).ToArray().SequenceEqual(cameraFiles))
                throw new Exception("Could not find Camera Files in GameFiles!");

            var targetDir = CheckFolders(gamePath)[0]; // W40k/Data
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            foreach (var cameraFile in dirCameraFiles)
            {
                File.Copy(cameraFile, Path.Combine(targetDir, Path.GetFileName(cameraFile)), true);
            }
        }

        public void DisableTweak()
        {
            var gamePath = PathFinder.GamePath;
            foreach (var cFolder in CheckFolders(gamePath))
            {
                foreach (var cameraFile in cameraFiles)
                {
                    var filePath = Path.Combine(cFolder, cameraFile);
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            }

        }
    }
}
