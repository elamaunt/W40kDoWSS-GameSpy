using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ThunderHawkModManager : IThunderHawkModManager
    {
        const string ModFolderName = @"Mod";

        public string JBugfixModName => "JBugfixMod";
        public string BattleRoyaleModName => "BattleRoyale";
        public string ValidModName => "ThunderHawk";
        public string ValidModVersion => "1.6.0";

        public string CurrentModName { get; set; } = "---";

        public string CurrentModVersion { get; set; } = "---";

        public void DeployModAndModule(string gamePath, string modName)
        {
            try
            {
                if (Directory.Exists(Path.Combine("Mod", modName)))
                    Directory.Delete(Path.Combine("Mod", modName), true);

                if (Directory.Exists(Path.Combine("Mod", ".git")))
                    Directory.Delete(Path.Combine("Mod", ".git"), true);

                var launcherModulePath = Path.Combine(ModFolderName, modName + ".module");
                var modulePath = Path.Combine(gamePath, modName + ".module");
                var modPath = Path.Combine(gamePath, modName);

                if (File.Exists(modulePath))
                {
                    var bytes = File.ReadAllBytes(modulePath);
                    var currentBytes = File.ReadAllBytes(launcherModulePath);

                    if (ArrayEquals(bytes, currentBytes) && Directory.Exists(modPath))
                        return;
                }
                
                // Начинается долгая распаковка мода, надо предупредить юзера, что придется подождать
                CoreContext.LaunchService.IsGamePreparingToStart = true;
                CoreContext.LaunchService.PreparingModName = modName;
                
                File.Copy(launcherModulePath, modulePath, true);

                if (Directory.Exists(modPath))
                    Directory.Delete(modPath, true);

                if (File.Exists(Path.Combine(gamePath, "KEYDEFAULTS.LUA")))
                    File.Delete(Path.Combine(gamePath, "KEYDEFAULTS.LUA"));

                Directory.CreateDirectory(modPath);

                Action<double> act = delegate(double res) { setModUnpackProgress(res); };
                IProgress<double> progressChange = new BasicProgress<double>(act);



                ExtractToDirectory(Path.Combine(ModFolderName, "Mod.zip"), gamePath, progressChange);
                /*using (var archive = ZipFile.Open(Path.Combine(ModFolderName, "Mod.zip"), ZipArchiveMode.Read))
                    archive.ExtractToDirectory(gamePath);*/
                CoreContext.LaunchService.IsGamePreparingToStart = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                CoreContext.LaunchService.IsGamePreparingToStart = false;
            }
        }
        
        
        
        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, IProgress<double> progress)
        {
            using (ZipArchive archive = ZipFile.Open(sourceArchiveFileName, ZipArchiveMode.Read))
            {
                double totalBytes = archive.Entries.Sum(e => e.Length);
                long currentBytes = 0;

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if(entry.Name == "") continue;
                    string fileName = Path.Combine(destinationDirectoryName, entry.FullName);

                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    using (Stream inputStream = entry.Open())
                    using(Stream outputStream = File.OpenWrite(fileName))
                    {
                        Stream progressStream = new StreamWithProgress(outputStream, null,
                            new BasicProgress<int>(i =>
                            {
                                currentBytes += i;
                                progress.Report(currentBytes / totalBytes);
                            }));

                        inputStream.CopyTo(progressStream);
                    }

                    File.SetLastWriteTime(fileName, entry.LastWriteTime.LocalDateTime);
                }
            }
        }
        
        class BasicProgress<T> : IProgress<T>
        {
            private readonly Action<T> _handler;

            public BasicProgress(Action<T> handler)
            {
                _handler = handler;
            }

            void IProgress<T>.Report(T value)
            {
                _handler(value);
            }
        }
        
        class StreamWithProgress : Stream
        {
            // NOTE: for illustration purposes. For production code, one would want to
            // override *all* of the virtual methods, delegating to the base _stream object,
            // to ensure performance optimizations in the base _stream object aren't
            // bypassed.

            private readonly Stream _stream;
            private readonly IProgress<int> _readProgress;
            private readonly IProgress<int> _writeProgress;

            public StreamWithProgress(Stream stream, IProgress<int> readProgress, IProgress<int> writeProgress)
            {
                _stream = stream;
                _readProgress = readProgress;
                _writeProgress = writeProgress;
            }

            public override bool CanRead { get { return _stream.CanRead; } }
            public override bool CanSeek {  get { return _stream.CanSeek; } }
            public override bool CanWrite {  get { return _stream.CanWrite; } }
            public override long Length {  get { return _stream.Length; } }
            public override long Position
            {
                get { return _stream.Position; }
                set { _stream.Position = value; }
            }

            public override void Flush() { _stream.Flush(); }
            public override long Seek(long offset, SeekOrigin origin) { return _stream.Seek(offset, origin); }
            public override void SetLength(long value) { _stream.SetLength(value); }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int bytesRead = _stream.Read(buffer, offset, count);

                _readProgress?.Report(bytesRead);
                return bytesRead;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
                _writeProgress?.Report(count);
            }
        }

        private bool ArrayEquals(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }

            return true;
        }

        private void setModUnpackProgress(double progress)
        {
            CoreContext.LaunchService.PreparingProgress = progress;
        }
    }
}
