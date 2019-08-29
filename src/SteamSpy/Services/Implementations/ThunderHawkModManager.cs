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

        public string ActiveModRevision { get; private set; } = "---";

        public bool CheckIsModExists(string gamePath)
        {
            if (!Repository.IsValid(ClonePath))
                return false;

            using (var repo = new Repository(ClonePath))
            {
                var originRemoteRep = repo.Network.Remotes["origin"];

                ActiveModRevision = repo.Head.Commits.FirstOrDefault()?.Message;

                if (originRemoteRep.PushUrl != RepositoryUrl)
                    return false;
            }

            return true;
        }

        public Task DownloadMod(string gamePath, CancellationToken token, Action<float> progressReporter)
        {
            if (!Repository.IsValid(ClonePath))
                return Task.Factory.StartNew(() =>
                {
                    if (Directory.Exists(ClonePath))
                        Directory.Delete(ClonePath, true);
                    Repository.Clone(RepositoryUrl, ClonePath, new CloneOptions()
                    {
                        OnTransferProgress = progress =>
                        {
                            var percent = (float)progress.ReceivedObjects / progress.TotalObjects;
                            progressReporter?.Invoke(percent);
                            return true;
                        }
                    });

                    using (var repo = new Repository(ClonePath))
                        ActiveModRevision = repo.Head.Commits.FirstOrDefault()?.Message;
                    RewriteModModule(gamePath);
                }, token);

            return Task.CompletedTask;
        }

        public Task UpdateMod(string gamePath, CancellationToken token, Action<float> progressReporter)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var repo = new Repository(ClonePath))
                {
                    var originRemoteRep = repo.Network.Remotes["origin"];

                    Commands.Fetch(repo, originRemoteRep.Name, originRemoteRep.FetchRefSpecs.Select(x => x.Specification), new FetchOptions()
                    {
                        OnTransferProgress = progress =>
                        {
                            var percent = (float)progress.ReceivedObjects / progress.TotalObjects;
                            progressReporter?.Invoke(percent);
                            return true;
                        }
                    }, string.Empty);

                    // TODO: merge?

                    ActiveModRevision = repo.Head.Commits.FirstOrDefault()?.Message;
                }

                RewriteModModule(gamePath);
            }, token);
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
