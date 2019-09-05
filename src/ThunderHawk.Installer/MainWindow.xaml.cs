using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ThunderHawk.Installer
{
    public partial class MainWindow : Window
    {
        const string ApiKey = "AIzaSyD8_wjaIxgrQG0m-DwBYLR3ZDQiQjPB7bk";
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        string[] Scopes = { DriveService.Scope.DriveReadonly };
        string ApplicationName = "ThunderHawk";

        UserCredential _credential;
        DriveService _service;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Text;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            StartLoading();
        }

        private void StartLoading()
        {
            Task.Factory.StartNew(async () =>
            {
                var listRequest = _service.Files.List();
                listRequest.Q = "'1xi63t6lKE_EkldNWz9l-QM_8y99d_q9H' in parents";

                var list = await listRequest.ExecuteAsync();

                var files = list.Files.Where(x => x.MimeType == "application/rar").OrderByDescending(x => x.Name, new VersionComparerImpl()).ToArray();

                var newestVersionFile = files.FirstOrDefault();

                var request = _service.Files.Get(newestVersionFile.Id);
                var fileStream = System.IO.File.Create(newestVersionFile.Name);

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
                                    fileStream.Dispose();
                                    Install(newestVersionFile.Name);
                                    File.Delete(newestVersionFile.Name);
                                    break;
                                }
                            case DownloadStatus.Failed:
                                {
                                    break;
                                }
                        }
                    };

                request.Download(fileStream);
            }).Unwrap();
        }

        string InstallPath;
        string _lastOpenedFolderPath;

        void Install(string fileName)
        {
            Thread.Sleep(2000);

            if (fileName == null)
            {
                MessageBox.Show("Nothing to install. You can install launcher manually from https://drive.google.com/drive/folders/1xi63t6lKE_EkldNWz9l-QM_8y99d_q9H");
                return;
            }

            var archive = ZipFile.Open(fileName, ZipArchiveMode.Read);

            var previosDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = InstallPath = Path.Text;

            RemoveOldFiles();

            var entries = archive.Entries.ToArray();

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                var path = entry.FullName;

                if (path.StartsWith("ThunderHawk\\", System.StringComparison.OrdinalIgnoreCase))
                    path = path.Substring("ThunderHawk\\".Length);

                if (string.IsNullOrWhiteSpace(path))
                    continue;

                try
                {
                    entry.ExtractToFile(path, true);
                }
                catch (Exception ex)
                {

                }
            }

            Environment.CurrentDirectory = previosDirectory;

            archive.Dispose();
            File.Delete(fileName);
        }

        void RemoveOldFiles()
        {
            foreach (var directory in Directory.EnumerateDirectories(Environment.CurrentDirectory).ToArray())
            {
                if (ShouldSkipDirectory(directory))
                    continue;

                try
                {
                    Directory.Delete(directory, true);
                }
                catch (Exception ex)
                {

                }

            }

            foreach (var file in Directory.EnumerateFiles(Environment.CurrentDirectory).ToArray())
            {
                if (ShouldSkipFile(file))
                    continue;

                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {

                }
            }
        }

        bool ShouldSkipDirectory(string directory)
        {
            if (directory.EndsWith("mod\\", StringComparison.OrdinalIgnoreCase))
                return true;

            if (directory.EndsWith("\\mod", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        bool ShouldSkipFile(string file)
        {
            if (System.IO.Path.GetFullPath(file) == InstallPath)

                if (file.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
                    return true;

            if (file.EndsWith("ThunderHawk.Updater.exe", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            Path.Text = GetDirectoryByBrowser(_lastOpenedFolderPath);
        }
        public string GetDirectoryByBrowser(string root = null)
        {
            var folderDialog = new FolderBrowserDialog();
            if (root == null)
                folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            else
                folderDialog.SelectedPath = root;
            folderDialog.ShowDialog();

            if (folderDialog.SelectedPath != null)
                _lastOpenedFolderPath = folderDialog.SelectedPath;

            return folderDialog.SelectedPath;
        }

        async Task<DriveService> CreateDriveService()
        {
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(System.IO.Path.Combine(Environment.CurrentDirectory, credPath), true));
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
