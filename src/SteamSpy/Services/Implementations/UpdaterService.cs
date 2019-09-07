using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class UpdaterService : IUpdaterService
    {
        const string ApiKey = "AIzaSyD8_wjaIxgrQG0m-DwBYLR3ZDQiQjPB7bk";

        const string Version = "1.09-beta";
        const string VersionForUI = "BETA 1.09";

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        string[] Scopes = { DriveService.Scope.DriveReadonly };
        string ApplicationName = "ThunderHawk";

        UserCredential _credential;
        DriveService _service;

        Google.Apis.Drive.v3.Data.File _newestVersionFile;

        public string CurrentVersion { get; } = Version;
        public string CurrentVersionUI { get; } = VersionForUI;

        public string NewestVersion { get; private set; } = Version;

        public event Action<string> NewVersionAvailable;
        public bool CanUpdate { get; private set; }

        public IComparer<string> VersionComparer { get; } = new VersionComparerImpl();

        public Task<bool> CheckForUpdates()
        {
            return Task.Factory.StartNew(async () =>
            {
                if (_service == null)
                    _service = await CreateDriveService();

                var listRequest = _service.Files.List();
                listRequest.Q = "'1xi63t6lKE_EkldNWz9l-QM_8y99d_q9H' in parents";

                var list = await listRequest.ExecuteAsync();

                var files = list.Files.Where(x => x.MimeType == "application/x-zip-compressed").OrderByDescending(x => x.Name, VersionComparer).ToArray();

                _newestVersionFile = files.FirstOrDefault();

                if (VersionComparer.Compare(CurrentVersion, _newestVersionFile.Name) == -1)
                {
                    NewVersionAvailable?.Invoke(_newestVersionFile.Name);
                    NewestVersion = _newestVersionFile.Name;

                    var request = _service.Files.Get(_newestVersionFile.Id);

                    if (System.IO.File.Exists(_newestVersionFile.Name))
                        System.IO.File.Delete(_newestVersionFile.Name);

                    var fileStream = System.IO.File.Create(_newestVersionFile.Name);

                    request.MediaDownloader.ProgressChanged +=
                        (IDownloadProgress progress) =>
                        {
                            switch (progress.Status)
                            {
                                case DownloadStatus.Downloading:
                                    {
                                        break;
                                    }
                                case DownloadStatus.Completed:
                                    {
                                        NewVersionAvailable?.Invoke(_newestVersionFile.Name);
                                        NewestVersion = _newestVersionFile.Name;
                                        CanUpdate = true;
                                        fileStream.Dispose();
                                        var result = MessageBox.Show("Update of ThuderHawk launcher is ready to install!\nInstall it now?", "Update available", MessageBoxButton.YesNo);

                                        if (result == MessageBoxResult.Yes)
                                            Update();
                                        break;
                                    }
                                case DownloadStatus.Failed:
                                    {
                                        break;
                                    }
                            }
                        };

                    request.Download(fileStream);
                    return true;
                }

                return false;
            }).Unwrap();
        }

        private async Task<Google.Apis.Drive.v3.Data.File> GetFileFromFolder(string id)
        {
            var listRequest = _service.Files.List();
            listRequest.Q = $@"'{_newestVersionFile.Id}' in parents";

            var list = await listRequest.ExecuteAsync();

            return list.Files.FirstOrDefault();
        }

        public void Update()
        {
            if (!CanUpdate)
                return;

            Process.Start(new ProcessStartInfo("ThunderHawk.Updater.exe", _newestVersionFile.Name)
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory
            });

            Environment.Exit(0);
        }

        async Task<DriveService> CreateDriveService()
        {
            using (var stream = new FileStream(Path.Combine(Environment.CurrentDirectory, "credentials.json"), FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(Path.Combine(Environment.CurrentDirectory, credPath), true));
            }

            // Create Drive API service.
            return _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = ApplicationName,
                ApiKey = ApiKey
            });
        }

        private class VersionComparerImpl : IComparer<string>
        {
            string[] _split = { "thunderhawk", "-", ".zip" };

            public int Compare(string x, string y)
            {
                if (x.Equals(y, StringComparison.OrdinalIgnoreCase))
                    return 0;

                var splitX = x.ToLowerInvariant().Split(_split, StringSplitOptions.RemoveEmptyEntries);
                var splitY = y.ToLowerInvariant().Split(_split, StringSplitOptions.RemoveEmptyEntries);

                var xVersion = float.Parse(splitX[0], CultureInfo.InvariantCulture);
                var yVersion = float.Parse(splitY[0], CultureInfo.InvariantCulture);

                if (xVersion > yVersion)
                    return 1;

                if (xVersion < yVersion)
                    return -1;

                if (splitX.Length > 1 && splitY.Length > 1)
                    return 0;

                if (splitX.Length > 1)
                    return -1;

                if (splitY.Length > 1)
                    return 1;

                return 0;
            }
        }
    }
}
