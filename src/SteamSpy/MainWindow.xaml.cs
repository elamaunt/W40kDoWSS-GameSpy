using SteamSpy.Utils;
using Steamworks;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SteamSpy
{
    public partial class MainWindow : Window
    {
        //Process _soulstormProcess;

        public MainWindow()
        {
            InitializeComponent();
            
            CompositionTarget.Rendering += OnRender;


            if (SteamAPI.RestartAppIfNecessary(new AppId_t(9450))) 
            //if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Console.WriteLine("APP RESTART REQUESTED");
                Environment.Exit(0);
            }

            if (SteamAPI.Init())
                Console.WriteLine("Steam inited");
            else
            {
                MessageBox.Show("Клиент Steam не запущен");
                Environment.Exit(0);
                return;
            }


            var appId = SteamUtils.GetAppID();

            if (appId.m_AppId != 9450)
            {
                MessageBox.Show("Программа запущена не от имени Sousltorm. Необходимо переместить все файлы этой программы в папку с игрой в Steam, предварительно сохранив оригинальный Soulstorm.exe. Запустить программу, а потом запустить SS 1.2 с модом Soulstorm Bugfix Mod 1.56a.");
                Environment.Exit(0);
                return;
            }
            
            var currentMoscowTime = new DateTime(1970, 1, 1).AddSeconds(SteamUtils.GetServerRealTime()).AddHours(3);

            if (currentMoscowTime > new DateTime(2019, 8, 4, 19, 0,0))
            {
                MessageBox.Show($@"Событие было завершено");
                Environment.Exit(0);
                return;
            }

            var steamId = SteamUser.GetSteamID().m_SteamID;

            if (!RegisteredIds.Contains(steamId))
            {
                MessageBox.Show($@"Ваш SteamID {steamId} не был зарегистрирован для участия. Обратитесь к elamaunt'у.");
                Environment.Exit(0);
                return;
            }

            CoreContext.ServerListRetrieve.StartReloadingTimer();
        }

        public static ulong[] RegisteredIds => new ulong[]
            {
                76561198001658409ul, // elamaunt
                76561198064050301ul, // SunRay
                76561198137977374ul, // Anibus

                76561198011215928ul, // Bukan1
                76561198005325871ul, // Bukan2

                76561198858224000ul, // Igor_kocerev
                76561198233237924ul, // Flashbang
                76561198855597623ul, // onebillygrimm
                76561198856913976ul, // Jabka_X
                                     // Mizopolak
                76561198842926724ul, // veloziraptor
                76561198098129338ul, // DAG05 Simon
                76561198021093802ul, // ki4a
                76561198072623723ul, // Master Yoba
                76561198137292489ul, // ZADGE
                76561198116829514ul, // Maugan Ra
                76561198036915935ul, // SorroWfuL LivED
                76561198143540732ul, // YbuBaKa
                76561198981516933ul, // Cg_JGHAMO
                76561198003604494ul, // Gigamok
                76561198360256453ul, // Gedeon
                76561198225092112ul, // vladirus
                76561198090267618ul, // deREXte
                76561198027618614ul, // Sm0kEZ
                76561198107179356ul, // Made in USSR
                76561198132447203ul, // Dolorosa
                76561198386642785ul  // Super_cega
            };

        private void OnRender(object sender, EventArgs e)
        {
            GameServer.RunCallbacks();
            SteamAPI.RunCallbacks();
            PortBindingManager.UpdateFrame();
        }
    }
}
