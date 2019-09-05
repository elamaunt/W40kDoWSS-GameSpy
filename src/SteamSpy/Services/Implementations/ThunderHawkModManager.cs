﻿using Framework;
using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ThunderHawkModManager : IThunderHawkModManager
    {
        const string RepositoryUrl = @"https://github.com/elamaunt/ThunderHawk-Soulstorm-Mod.git";
        const string ClonePath = @"Mod";

        public string ModName => "ThunderHawk";
        public string ModVersion { get; private set; } = "";

        public string ActiveModRevision { get; private set; } = "---";

        Task _currentLoadingTask;

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
            if (_currentLoadingTask != null)
                return _currentLoadingTask;

            return _currentLoadingTask = Task.Factory.StartNew(() =>
            {
                if (Directory.Exists(ClonePath))
                {
                    try
                    {
                        Directory.Delete(ClonePath, true);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Mod folder was corrupted. Please delete Mod folder and restart the launcher.");
                        throw;
                    }
                }

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
                _currentLoadingTask = null;
            }, token);
        }

        public Task UpdateMod(string gamePath, CancellationToken token, Action<float> progressReporter)
        {
            if (_currentLoadingTask != null)
                return _currentLoadingTask;

            return _currentLoadingTask = Task.Factory.StartNew(() =>
            {
                try
                {

                    using (var repo = new Repository(ClonePath))
                    {
                        var originRemoteRep = repo.Network.Remotes["origin"];

                        var pullOptions = new PullOptions()
                        {
                            FetchOptions = new FetchOptions()
                            {
                                OnTransferProgress = progress =>
                                {
                                    var percent = (float)progress.ReceivedObjects / progress.TotalObjects;
                                    progressReporter?.Invoke(percent);
                                    return true;
                                }
                            },

                            MergeOptions = new MergeOptions()
                            {
                            }
                        };

                        var signature = new LibGit2Sharp.Signature(new Identity("Anonymous", "elamaunt@gmail.com"), DateTimeOffset.Now);

                        Commands.Pull(repo, signature, pullOptions);

                        ActiveModRevision = repo.Head.Commits.FirstOrDefault()?.Message;
                    }

                    RewriteModModule(gamePath);
                    _currentLoadingTask = null;

                }
                catch(Exception ex)
                {
                    _currentLoadingTask = null;
                    DownloadMod(gamePath, token, progressReporter).Wait();
                }
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
