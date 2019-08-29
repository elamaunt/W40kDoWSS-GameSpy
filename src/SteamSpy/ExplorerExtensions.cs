using System.IO;

namespace ThunderHawk
{
    public static class ExplorerExtensions
    {
        public static void ClearFlagsInDirFiles(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
        }
    }
}
