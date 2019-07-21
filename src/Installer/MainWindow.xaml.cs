using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string Entries = @"134.209.198.2 ocs.thq.com
134.209.198.2 www.dawnofwargame.com
134.209.198.2 gmtest.master.gamespy.com

134.209.198.2 whammer40kdc.master.gamespy.com
134.209.198.2 whammer40kdc.gamespy.com
134.209.198.2 whammer40kdc.ms7.gamespy.com
134.209.198.2 whammer40kdc.available.gamespy.com
134.209.198.2 whammer40kdc.gamestats.gamespy.com
134.209.198.2 whammer40kdc.natneg0.gamespy.com
134.209.198.2 whammer40kdc.natneg1.gamespy.com
134.209.198.2 whammer40kdc.natneg2.gamespy.com
134.209.198.2 whammer40kdc.natneg3.gamespy.com

134.209.198.2 whammer40kdcam.master.gamespy.com
134.209.198.2 whammer40kdcam.gamespy.com
134.209.198.2 whammer40kdcam.ms5.gamespy.com
134.209.198.2 whammer40kdcam.available.gamespy.com
134.209.198.2 whammer40kdcam.gamestats.gamespy.com
134.209.198.2 whammer40kdcam.natneg0.gamespy.com
134.209.198.2 whammer40kdcam.natneg1.gamespy.com
134.209.198.2 whammer40kdcam.natneg2.gamespy.com
134.209.198.2 whammer40kdcam.natneg3.gamespy.com


134.209.198.2 whamdowfr.master.gamespy.com
134.209.198.2 whamdowfr.gamespy.com
134.209.198.2 whamdowfr.ms9.gamespy.com
134.209.198.2 whamdowfr.ms11.gamespy.com
134.209.198.2 whamdowfr.available.gamespy.com
134.209.198.2 whamdowfr.available.gamespy.com
134.209.198.2 whamdowfr.natneg0.gamespy.com
134.209.198.2 whamdowfr.natneg1.gamespy.com
134.209.198.2 whamdowfr.natneg2.gamespy.com
134.209.198.2 whamdowfr.natneg3.gamespy.com
134.209.198.2 whamdowfr.gamestats.gamespy.com

134.209.198.2 whamdowfram.master.gamespy.com
134.209.198.2 whamdowfram.gamespy.com
134.209.198.2 whamdowfram.ms9.gamespy.com
134.209.198.2 whamdowfram.ms11.gamespy.com
134.209.198.2 whamdowfram.available.gamespy.com
134.209.198.2 whamdowfram.available.gamespy.com
134.209.198.2 whamdowfram.natneg0.gamespy.com
134.209.198.2 whamdowfram.natneg1.gamespy.com
134.209.198.2 whamdowfram.natneg2.gamespy.com
134.209.198.2 whamdowfram.natneg3.gamespy.com
134.209.198.2 whamdowfram.gamestats.gamespy.com

134.209.198.2 gamespy.net
134.209.198.2 gamespygp
134.209.198.2 motd.gamespy.com
134.209.198.2 peerchat.gamespy.com
134.209.198.2 gamestats.gamespy.com
134.209.198.2 gpcm.gamespy.com
134.209.198.2 gpsp.gamespy.com
134.209.198.2 key.gamespy.com
134.209.198.2 master.gamespy.com
134.209.198.2 master0.gamespy.com
134.209.198.2 natneg1.gamespy.com
134.209.198.2 natneg2.gamespy.com
134.209.198.2 natneg3.gamespy.com
134.209.198.2 chat.gamespynetwork.com
134.209.198.2 available.gamespy.com
134.209.198.2 gamespy.com
134.209.198.2 gamespyarcade.com
134.209.198.2 www.gamespy.com
134.209.198.2 www.gamespyarcade.com
134.209.198.2 chat.master.gamespy.com
134.209.198.2 thq.vo.llnwd.net
134.209.198.2 gamespyid.com
134.209.198.2 nat.gamespy.com";

        public MainWindow()
        {
            InitializeComponent();
        }

        public string GetFileByBrowser(string filter = null)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = filter;

            fileDialog.ShowDialog();

            return fileDialog.FileName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PathToGame.Text = GetFileByBrowser("exe files (*.exe)|*.exe");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var path = PathToGame.Text;

            if (path == null || !File.Exists(path))
            {
                MessageBox.Show("Ошибка, неверный фаил");
                return;
            }
            
            var versionInfo = FileVersionInfo.GetVersionInfo(path);

            if (versionInfo.ProductName == "Warhammer 40 000: Dawn of War")
            {
                if (ModifyHostsFile(Entries.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)))
                {
                    MessageBox.Show("Все готово! Теперь в игре идете в \"Коллективная игра/Интернет, создаете аккаунт и играете\"");
                }
            }
            else
            {
                MessageBox.Show("Ошибка, неверный фаил");
            }
        }

        public static bool ModifyHostsFile(string[] entries)
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

                if (!File.Exists(path))
                    File.CreateText(path);

                var lines = File.ReadAllLines(path);

                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine("\n");

                    for (int i = 0; i < entries.Length; i++)
                    {
                        var entry = entries[i];
                        if (string.IsNullOrWhiteSpace(entry))
                            continue;

                        if (!lines.Contains(entry))
                            w.WriteLine(entry);
                    }

                    w.Flush();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка, может не хватает прав Администратора?");
                return false;
            }
        }
    }
}
