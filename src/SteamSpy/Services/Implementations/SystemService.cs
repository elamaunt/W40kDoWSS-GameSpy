using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
//using DesktopNotificationManagerCompat = DesktopNotifications.DesktopNotificationManagerCompat;

namespace ThunderHawk
{
    public class SystemService : ISystemService
    {
        //readonly ToastNotifier _desktopNotifier;
        readonly Notifier _localNotifier;

        volatile int _idCounter;

        public string GetSteamExePath()
        {
            return Path.Combine(PathFinder.FindSteamPath(), "Steam.exe");
        }

        public bool IsSteamRunning => Process.GetProcessesByName("Steam").Length > 0;

        public SystemService()
        {
            try
            {
                //_desktopNotifier = DesktopNotificationManagerCompat.CreateToastNotifier();

                _localNotifier = new Notifier(cfg =>
                {
                    cfg.PositionProvider = new WindowPositionProvider(
                        parentWindow: Application.Current.MainWindow,
                        corner: Corner.TopRight,
                        offsetX: 10,
                        offsetY: 10);

                    cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                        notificationLifetime: TimeSpan.FromSeconds(3),
                        maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                    cfg.Dispatcher = Application.Current.Dispatcher;
                });
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
        }

        public bool CheckIsItInStartup()
        {
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var value = key.GetValue("ThunderHawk");

            if (value == null)
                return false;

            if (String.Equals((string)value, "\"" + Path.Combine(Directory.GetCurrentDirectory(), @"ThunderHawk.exe") + "\" -silence"))
                return true;

            return false;
        }

        public void AddInStartup()
        {
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("ThunderHawk", "\""+Path.Combine(Directory.GetCurrentDirectory(), @"ThunderHawk.exe")+"\" -silence");
        }
        public void RemoveFromStartup()
        {
            var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.DeleteValue("ThunderHawk", false);
        }

        public void NotifyAsSystemToastMessage(MessageInfo info)
        {
           /* if (!AppSettings.DesktopNotificationsEnabled)
                return;
            try
            {
                // Construct the visuals of the toast (using Notifications library)
                ToastContent toastContent = new ToastContent()
                {
                    // Arguments when the user taps body of toast
                    Launch = "action=viewConversation&conversationId=5",

                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                        {
                             new AdaptiveText()
                             {
                                Text = info.Author.UIName,
                                HintAlign = AdaptiveTextAlign.Left
                             },

                            new AdaptiveText()
                            {
                                Text = info.Text,
                                HintAlign = AdaptiveTextAlign.Left
                            }
                        }
                        }
                    }
                };

                toastContent.Header = new ToastHeader(Interlocked.Increment(ref _idCounter).ToString(), "ThunderHawk", "ThunderHawk");

                // Create the XML document (BE SURE TO REFERENCE WINDOWS.DATA.XML.DOM)
                var doc = new XmlDocument();
                doc.LoadXml(toastContent.GetContent());

                // And create the toast notification
                var toast = new ToastNotification(doc);
                toast.Group = "chat";

                // And then show it
                //_desktopNotifier.Show(toast);

            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }*/
        }

        public void NotifyAsSystemToastMessage(string title, string text)
        {
          /*  if (!AppSettings.DesktopNotificationsEnabled)
                return;

            try
            {
                ToastContent toastContent = new ToastContent()
                {
                    // Arguments when the user taps body of toast
                    Launch = "action=viewConversation&conversationId=5",

                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                        {
                             new AdaptiveText()
                             {
                                Text = title,
                                HintAlign = AdaptiveTextAlign.Left
                             },

                            new AdaptiveText()
                            {
                                Text = text,
                                HintAlign = AdaptiveTextAlign.Left
                            }
                        }
                        }
                    }
                };

                toastContent.Header = new ToastHeader(Interlocked.Increment(ref _idCounter).ToString(), "ThunderHawk", "");

                // Create the XML document (BE SURE TO REFERENCE WINDOWS.DATA.XML.DOM)
                var doc = new XmlDocument();
                doc.LoadXml(toastContent.GetContent());

                // And create the toast notification
                var toast = new ToastNotification(doc);
                toast.Group = "info";

                // And then show it
                //_desktopNotifier.Show(toast);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }*/
        }

        public void OpenLink(Uri uri)
        {
            try
            {
                System.Diagnostics.Process.Start(uri.ToString());
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
            }
        }

        public Task<bool> AskUser(string question)
        {
            var result = MessageBox.Show(question, "ThunderHawk", MessageBoxButton.YesNo, MessageBoxImage.Question);

            return Task.FromResult(result == MessageBoxResult.Yes);
        }

        public void ShowMessageWindow(string message)
        {
            MessageBox.Show(message, "ThunderHawk", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
