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
            InstallButton.IsEnabled = false;

            var path = Path.Text;
            InstallPath = path;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (HasAnyCyrillicChars(path))
            {
                MessageBox.Show("Installation path must contains only English characters");
                InstallButton.IsEnabled = true;
                return;
            }

            if (HasAnyFilesOrFolders(path))
            {
                MessageBox.Show("Installation folder must be empty");
                InstallButton.IsEnabled = true;
                return;
            }

            Indicator.Visibility = Visibility.Visible;
            StartLoading();
        }

        private bool HasAnyFilesOrFolders(string path)
        {
            return Directory.EnumerateFiles(path).Any() || Directory.EnumerateDirectories(path).Any();
        }

        private bool HasAnyCyrillicChars(string path)
        {
            return path.Any(x => "йцукенгшщзхъфывапролджэячсмитьбюё".Contains(char.ToLowerInvariant(x)));
        }

        private void StartLoading()
        {
            Task.Factory.StartNew(async () =>
            {
                if (_service == null)
                    _service = await CreateDriveService();

                var listRequest = _service.Files.List();
                listRequest.Q = "'1xi63t6lKE_EkldNWz9l-QM_8y99d_q9H' in parents";

                var list = await listRequest.ExecuteAsync();

                var files = list.Files.Where(x => x.MimeType == "application/x-zip-compressed").OrderByDescending(x => x.Name, new VersionComparerImpl()).ToArray();
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
                                    Dispatcher.Invoke(() =>
                                    {
                                        MessageBox.Show("Nothing to install. You can install launcher manually from https://drive.google.com/drive/folders/1xi63t6lKE_EkldNWz9l-QM_8y99d_q9H");
                                        InstallButton.Content = "Install";
                                        InstallButton.IsEnabled = true;
                                        Indicator.Visibility = Visibility.Hidden;
                                    });
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
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Nothing to install. You can install launcher manually from https://drive.google.com/drive/folders/1xi63t6lKE_EkldNWz9l-QM_8y99d_q9H");
                    InstallButton.Content = "Install";
                    InstallButton.IsEnabled = true;
                    Indicator.Visibility = Visibility.Hidden;
                });
                return;
            }

            var archive = ZipFile.Open(fileName, ZipArchiveMode.Read);

            var previosDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = InstallPath;

            //RemoveOldFiles();

            var entries = archive.Entries.ToArray();

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                var path = entry.FullName;

                if (path.StartsWith("ThunderHawk/", System.StringComparison.OrdinalIgnoreCase))
                    path = path.Substring("ThunderHawk/".Length);

                if (string.IsNullOrWhiteSpace(path))
                    continue;

                try
                {
                    var folder = System.IO.Path.GetDirectoryName(path);

                    if (!string.IsNullOrWhiteSpace(folder))
                        Directory.CreateDirectory(folder);

                    entry.ExtractToFile(path, true);
                }
                catch (Exception ex)
                {

                }
            }

            archive.Dispose();
            File.Delete(fileName);

            Dispatcher.Invoke(() =>
            {
                InstallButton.Content = "Install";
                InstallButton.IsEnabled = true;
                MessageBox.Show("Launcher succesfully installed!");
                Indicator.Visibility = Visibility.Hidden;

                if (StartLauncher.IsChecked ?? false)
                {
                    Process.Start(new ProcessStartInfo(System.IO.Path.Combine(InstallPath, "ThunderHawk.exe"))
                    {
                        UseShellExecute = true,
                        WorkingDirectory = InstallPath
                    });

                    Environment.Exit(0);
                }
            });
        }

        /*void RemoveOldFiles()
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
        }*/

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
