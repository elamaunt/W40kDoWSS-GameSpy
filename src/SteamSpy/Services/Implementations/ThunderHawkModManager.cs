using Framework;
using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ThunderHawkModManager : IThunderHawkModManager
    {
        const string RepositoryUrl = @"https://github.com/elamaunt/ThunderHawk-Soulstorm-Mod.git";
        const string ClonePath = @"Mod";

        public string ModName => "ThunderHawk";

        public bool CheckIsModExists(string gamePath)
        {
            if (!Repository.IsValid(ClonePath))
                return false;

            using (var repo = new Repository(ClonePath))
            {
                var p = repo.Info.Path;
            }

            return true;
        }


        public async Task<bool> CheckIsLastVersion(string gamePath)
        {
            if (!Repository.IsValid(ClonePath))
                return false;

            using (var repo = new Repository(ClonePath))
            {
                // git show-branch *master
                var commits = repo.Head.Commits.ToArray();
            }

            return true;
        }

        public Task DownloadMod(string gamePath, CancellationToken token)
        {
            if (!Repository.IsValid(ClonePath))
                return Task.Factory.StartNew(() =>
                {
                    if (Directory.Exists(ClonePath))
                        Directory.Delete(ClonePath, true);
                    Repository.Clone(RepositoryUrl, ClonePath);

                    RewriteModModule(gamePath);
                });

            return Task.CompletedTask;
        }

        public Task UpdateMod(string gamePath, CancellationToken token)
        {
            RewriteModModule(gamePath);
            return Task.CompletedTask;
        }

        void RewriteModModule(string gamePath)
        {
            try
            {
                var moduleLines = File.ReadAllLines(Path.Combine(ClonePath, "ThunderHawk.module"));
                moduleLines.Replace("ModFolder = ThunderHawk", $@"ModFolder = { Path.Combine(Directory.GetCurrentDirectory(), ClonePath, "ThunderHawk") }");
                File.WriteAllLines(Path.Combine(gamePath, "ThunderHawk.module"), moduleLines);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
