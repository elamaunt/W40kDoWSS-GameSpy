using System;
using System.IO;
using System.Windows.Forms;
using BF2Statistics.Database;
using BF2Statistics.Logging;

namespace BF2Statistics.Gamespy
{
    /// <summary>
    /// The Gamespy Server is used to emulate the Official Gamespy Login servers,
    /// and provide players the ability to create fake "Online" accounts.
    /// </summary>
    public static class GamespyEmulator
    {
        /// <summary>
        /// Returns whether the login server is running or not
        /// </summary>
        private static bool bIsRunning = false;

        /// <summary>
        /// Returns whether the login server is running or not
        /// </summary>
        public static bool IsRunning
        {
            get { return bIsRunning; }
        }

        /// <summary>
        /// Returns a list of all the connected clients
        /// </summary>
        public static GpcmClient[] ConnectedClients
        {
            get 
            { 
                return (IsRunning) ? ClientManager.ConnectedClients : new GpcmClient[0]; 
            }
        }

        /// <summary>
        /// Returns the number of connected players that are logged in
        /// </summary>
        public static int NumClientsConnected
        {
            get { return (IsRunning) ? ClientManager.NumClients : 0; }
        }

        /// <summary>
        /// The Number of servers that are currently online and actively
        /// reporting to this master server
        /// </summary>
        public static int ServerCount
        {
            get { return MasterServer.Servers.Count;  }
        }

        /// <summary>
        /// Gamespy Client Manager Server Object
        /// </summary>
        private static GpcmServer ClientManager;

        /// <summary>
        /// The Gamespy Search Provider Server Object
        /// </summary>
        private static GpspServer SearchProvider;

        /// <summary>
        /// The Gamespy Master Server
        /// </summary>
        private static MasterServer MasterServer;

        /// <summary>
        /// The Gamespy CDKey server
        /// </summary>
        private static CDKeyServer CDKeyServer;

        /// <summary>
        /// The Gamespy Debug Log
        /// </summary>
        private static LogWriter DebugLog;

        /// <summary>
        /// Event that is fired when the login server is started
        /// </summary>
        public static event StartupEventHandler Started;

        /// <summary>
        /// Event that is fired when the login server is shutdown
        /// </summary>
        public static event ShutdownEventHandler Stopped;

        /// <summary>
        /// Event fires when a player logs in or disconnects from the login server
        /// </summary>
        public static event EventHandler OnClientsUpdate;

        /// <summary>
        /// Event fires when a server is added or removed from the online serverlist
        /// </summary>
        public static event EventHandler OnServerlistUpdate;

        static GamespyEmulator()
        {
            // Create our log file
            DebugLog = new LogWriter(Path.Combine(Program.RootPath, "Logs", "GamespyDebug.log"));

            // Register for events
            GpcmServer.OnClientsUpdate += (s, e) => OnClientsUpdate(s, e);
            MasterServer.OnServerlistUpdate += (s, e) => OnServerlistUpdate(s, e);
        }

        /// <summary>
        /// Starts the Login Server listeners, and begins accepting new connections
        /// </summary>
        public static void Start()
        {
            // Make sure we arent already running!
            if (bIsRunning) return;

            // Start the DB Connection
            using (GamespyDatabase Database = new GamespyDatabase()) 
            {
                // First, make sure our account table exists
                if (!Database.TablesExist)
                {
                    string message = "In order to use the Gamespy Emulation feature of this program, we need to setup a database. "
                    + "You may choose to do this later by clicking \"Cancel\". Would you like to setup the database now?";
                    DialogResult R = MessageBox.Show(message, "Gamespy Database Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (R == DialogResult.Yes)
                        SetupManager.ShowDatabaseSetupForm(DatabaseMode.Gamespy, MainForm.Instance);

                    // Call the stoOnShutdown event to Re-enable the main forms buttons
                    Stopped();
                    return;
                }
                else if (Database.NeedsUpdated)
                {
                    // We cannot run an outdated database
                    DialogResult R = MessageBox.Show(
                        String.Format(
                            "The Gamespy database tables needs to be updated to version {0} before using this feature. Would you like to do this now?",
                            GamespyDatabase.LatestVersion
                        ) + Environment.NewLine.Repeat(1) + 
                        "NOTE: You should backup your gamespy account table if you are unsure as this update cannot be undone!", 
                        "Gamespy Database Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question
                    );

                    // If the user doesnt migrate the database tables, quit
                    if (R != DialogResult.Yes)
                    {
                        // Call the stoOnShutdown event to Re-enable the main forms buttons
                        Stopped();
                        return;
                    }
                    
                    // Do table migrations
                    Database.MigrateTables();
                }
            }

            // Bind gpcm server on port 29900
            int port = 29900;

            // Setup the DebugLog
            DebugLog.LoggingEnabled = Program.Config.GamespyServerDebug;
            if (Program.Config.GamespyServerDebug)
                DebugLog.ClearLog();

            try 
            {
                // Begin logging
                DebugLog.Write("=== Gamespy Emulator Initializing ===");
                DebugLog.Write("Starting Client Manager");

                // Start the client manager
                ClientManager = new GpcmServer();

                // Begin logging
                DebugLog.Write("Bound to TCP port: " + port);
                DebugLog.Write("Starting Account Service Provider");

                // Start search provider server
                port++;
                SearchProvider = new GpspServer();

                // Begin logging
                DebugLog.Write("Bound to TCP port: " + port);
                DebugLog.Write("Starting Master Server");

                // Start then Master Server
                MasterServer = new MasterServer(ref port, DebugLog);

                // Start CDKey Server
                port = 29910;
                DebugLog.Write("Starting Cdkey Server");
                CDKeyServer = new CDKeyServer(DebugLog);

                // Begin logging
                DebugLog.Write("=== Gamespy Emulator Initialized ===");
            }
            catch (Exception E) 
            {
                Notify.Show(
                    "Failed to Start Gamespy Servers!", 
                    "Error binding to port " + port + ": " + Environment.NewLine + E.Message, 
                    AlertType.Warning
                );

                // Append log
                if (DebugLog != null)
                {
                    DebugLog.Write("=== Failed to Start Emulator Servers! ===");
                    DebugLog.Write("Error binding to port " + port + ": " + E.Message);
                }

                // Shutdown all started servers
                if (ClientManager != null && ClientManager.IsListening) ClientManager.Shutdown();
                if (SearchProvider != null && SearchProvider.IsListening) SearchProvider.Shutdown();
                if (MasterServer != null && MasterServer.IsRunning) MasterServer.Shutdown();
                // Cdkey server must have throwm the exception at this point, since it starts last

                // Throw excpetion to parent
                throw;
            }

            // Let the client know we are ready for connections
            bIsRunning = true;
            Started();
        }

        /// <summary>
        /// Shutsdown all of the Gamespy Servers
        /// </summary>
        public static void Shutdown()
        {
            // Shutdown Login Servers
            ClientManager.Shutdown();
            SearchProvider.Shutdown();
            MasterServer.Shutdown();
            CDKeyServer.Shutdown();

            // Update status
            bIsRunning = false;

            // Trigger the OnShutdown Event
            Stopped();
        }

        /// <summary>
        /// Forces the logout of a connected client
        /// </summary>
        public static bool ForceLogout(int Pid)
        {
            return (IsRunning) ? ClientManager.ForceLogout(Pid) : false;
        }

        /// <summary>
        /// Returns whether the specified player is currently connected
        /// </summary>
        public static bool IsPlayerConnected(int Pid)
        {
            return (IsRunning) ? ClientManager.IsConnected(Pid) : false;
        }
    }
}
