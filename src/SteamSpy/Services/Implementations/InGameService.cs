using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SharedServices;
using ThunderHawk.Core;
using ThunderHawk.StaticClasses.Soulstorm;

namespace ThunderHawk
{
    public class InGameService : IInGameService
    {
        public float apmCurrent { get; private set; }
        public float apmAverageGame { get; set; }
        public bool isGameNow { get; private set; }
        public string inGameMap { get; private set; }
        
        public bool errorOccured { get; private set; }
        public ObservableCollection<ChatUserItemViewModel> serverOnlinePlayers { private get; set; }
        public InGamePlayer[] inGamePlayers { get; set; }

        private string activeProfilePath;

        private int totalActions = 0;
        private int actionsIn4Seconds = 0;

        private long lastMaxOffset = 100500;

        public InGameService()
        {
            Task.Run(ReadGameConsole);
        }

        public void DropSsConsoleOffset()
        {
            lastMaxOffset = 0;
        }


        /**
         * Read file testStats.Lua in profile folder. This file example:
        GSGameStats =  
        {
	        Players = 2,
	        WinBy = "",
	        Teams = 2,
	        player_0 =  
	        {
		        PRace = "sisters_race",
		        PHuman = 1,
		        PFnlState = 1,
		        PTeam = 1,
		        PName = "Huibus",
		        PTtlSc = 38740,
	        },
	        player_1 =  
	        {
		        PRace = "space_marine_race",
		        PHuman = 0,
		        PFnlState = 1,
		        PTeam = 0,
		        PName = "Computer 1",
		        PTtlSc = 26592,
	        },
	        Duration = 1982,
	        Scenario = "2p_Deadly_Fun_Archeology",
        }
         */
        private void readTestStats()
        {
            try
            {
                var testStatsPath = Path.Combine(activeProfilePath, "testStats.Lua");
                FileInfo testStats = new FileInfo(testStatsPath);

                using (var streamReader =
                    new StreamReader(testStats.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    int playerIndex = -1;

                    while (streamReader.Peek() > -1)
                    {
                        var line = streamReader.ReadLine();

                        if (line == null)
                            continue;

                        var str = "Players = ";
                        var index = line.IndexOf(str, StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            string playersAmount = Regex.Replace(line, @".*Players = (\d).*", "$1");
                            // Determine array size depends on testStats.Lua players count
                            inGamePlayers = new InGamePlayer[Int32.Parse(playersAmount)];
                        }

                        str = "Scenario = ";
                        index = line.IndexOf(str, StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            string map = Regex.Replace(line, ".*?\"(.*?)\".*", "$1");
                            inGameMap = map;
                        }

                        // Search player block to increment index
                        str = "player_";
                        index = line.IndexOf(str, StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            string playerNumber = Regex.Replace(line, @".*player_(\d).*", "$1");
                            playerIndex = Int32.Parse(playerNumber);
                            inGamePlayers[playerIndex] = new InGamePlayer();
                        }

                        str = "PName =";
                        index = line.IndexOf(str, StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            string playerName = Regex.Replace(line, ".*?\"(.*?)\".*", "$1");
                            inGamePlayers[playerIndex].Name = playerName;
                            switch (inGamePlayers.Length)
                            {
                                case 2:
                                    inGamePlayers[playerIndex].Mmr = findPlayerMmr(playerName, 1);
                                    break;
                                case 4:
                                    inGamePlayers[playerIndex].Mmr = findPlayerMmr(playerName, 2);
                                    break;
                                case 6:
                                case 8:
                                    inGamePlayers[playerIndex].Mmr = findPlayerMmr(playerName, 3);
                                    break;
                                default:
                                    // 3x3&4x4 mmr is more suitable for FFA games (но это не точно)
                                    inGamePlayers[playerIndex].Mmr = findPlayerMmr(playerName, 3);
                                    break;
                            }
                        }

                        str = "PRace =";
                        index = line.IndexOf(str, StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            string playerRace = Regex.Replace(line, ".*?\"(.*?)\".*", "$1");
                            inGamePlayers[playerIndex].Race = getRaceEnum(playerRace);
                        }

                        str = "PTeam =";
                        index = line.IndexOf(str, StringComparison.OrdinalIgnoreCase);
                        if (index != -1)
                        {
                            string playerTeam = Regex.Replace(line, @".*PTeam = (\d).*", "$1");
                            inGamePlayers[playerIndex].Team = Int32.Parse(playerTeam);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private long findPlayerMmr(string name, int rateType)
        {
            foreach (var chatUserItemViewModel in serverOnlinePlayers)
            {
                if (chatUserItemViewModel.Info.Name != null)
                {
                    if (chatUserItemViewModel.Info.Name.Equals(name))
                    {
                        switch (rateType)
                        {
                            case 1:
                                if (chatUserItemViewModel.Info.Score1v1 != null)
                                    return chatUserItemViewModel.Info.Score1v1.Value;
                                else return 0;
                            case 2:
                                if (chatUserItemViewModel.Info.Score2v2 != null)
                                    return chatUserItemViewModel.Info.Score2v2.Value;
                                else return 0;
                            case 3:
                                if (chatUserItemViewModel.Info.Score3v3 != null)
                                    return chatUserItemViewModel.Info.Score3v3.Value;
                                else return 0;
                            default: return 0;
                        }
                    }
                }
            }

            return 0;
        }

        private Race getRaceEnum(string race)
        {
            switch (race)
            {
                case "space_marine_race": return Race.space_marine_race;
                case "chaos_marine_race": return Race.chaos_marine_race;
                case "ork_race": return Race.ork_race;
                case "eldar_race": return Race.eldar_race;
                case "guard_race": return Race.guard_race;
                case "necron_race": return Race.necron_race;
                case "tau_race": return Race.tau_race;
                case "dark_eldar_race": return Race.dark_eldar_race;
                case "sisters_race": return Race.sisters_race;
                default: return Race.unknown;
            }
        }

        private void ReadGameConsole()
        {
            try
            {
                //Open the stream and read it back.
                FileInfo soulstormConsole = new FileInfo(PathFinder.GamePath + "\\warnings.log");

                string activeMod = null;

                using (var streamReader =
                    new StreamReader(soulstormConsole.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while (true)
                    {
                        Thread.Sleep(100);


                        //if the file size has not changed, idle
                        if (streamReader.BaseStream.Length == lastMaxOffset)
                            continue;

                        //seek to the last max offset
                        streamReader.BaseStream.Seek(lastMaxOffset, SeekOrigin.Begin);

                        //read out of the file until the EOF
                        string line = "";
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var searchLine =
                                "GAME -- Initializing Team Colour Systems"; // before this initializing, ss write to teststats
                            var index = line.IndexOf(searchLine, StringComparison.OrdinalIgnoreCase);
                            if (index != -1)
                            {
                                isGameNow = true;
                                readTestStats();
                            }
                            
                            
                            // Если мы присоединились к игре или покинули список игр, ставим флаг-костыль, чтобы игры не обновлялись дважды
                            searchLine = "Lobby - Join success"; 
                            var searchLine2 = "Lobby - LOE_News received"; 
                            index = line.IndexOf(searchLine, StringComparison.OrdinalIgnoreCase);
                            var index2 = line.IndexOf(searchLine2, StringComparison.OrdinalIgnoreCase);
                            if (index != -1 || index2 != -1)
                            {
                                SingleClientServer.ShouldShowGames = false;
                                readTestStats();
                            }

                            searchLine = "finished loading";
                            index = line.IndexOf(searchLine, StringComparison.OrdinalIgnoreCase);
                            if (index != -1)
                            {
                                string playerFinishLoad =
                                    Regex.Replace(line, @".*\((.*), .*\) finished loading.*", "$1");
                                UpdatePlayerFinishLoadStatus(playerFinishLoad);
                            }

                            searchLine = "APP -- Game Stop";
                            index = line.IndexOf(searchLine, StringComparison.OrdinalIgnoreCase);
                            if (index != -1) isGameNow = false;

                            searchLine = "SOULSTORM started";
                            index = line.IndexOf(searchLine, StringComparison.OrdinalIgnoreCase);
                            if (index != -1) isGameNow = false;


                            searchLine = "GAME -- Using player profile";
                            index = line.IndexOf(searchLine, StringComparison.OrdinalIgnoreCase);
                            if (index != -1)
                            {
                                SetupActiveProfileFolder();
                            }
                        }
                        //update the last max offset

                        lastMaxOffset = streamReader.BaseStream.Position;
                    }
                }
            }
            catch(Exception e)
            {
                errorOccured = true;
                Logger.Error(e);
            }
            
        }

        private void UpdatePlayerFinishLoadStatus(string playerName)
        {
            foreach (var inGamePlayer in inGamePlayers)
            {
                if (inGamePlayer.Name.Equals(playerName))
                    inGamePlayer.IsLoadComplete = true;
            }
        }

        private void SetupActiveProfileFolder()
        {
            var profiles = Directory.GetDirectories(Path.Combine(PathFinder.GamePath, "Profiles"));
            DateTime timeStamp = DateTime.MinValue;
            foreach (var profile in profiles)
            {
                DateTime profileLastActivity = File.GetLastWriteTimeUtc(profile + "\\playercfg.lua");
                if (timeStamp < profileLastActivity)
                {
                    timeStamp = profileLastActivity;
                    activeProfilePath = profile;
                }
            }
        }
    }
}