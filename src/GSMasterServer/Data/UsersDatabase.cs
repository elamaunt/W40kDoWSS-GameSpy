using GSMasterServer.Utils;
using Reality.Net.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace GSMasterServer.Data
{
    public class UsersDatabase : IDisposable
    {
        private const string Category = "UsersDatabase";
        
        private static UsersDatabase _instance;

        private SQLiteConnection _db;

        private delegate bool EventHandler(CtrlType sig);
        private static EventHandler _closeHandler;

        private SQLiteCommand _getUsersByProfileId;
        private SQLiteCommand _getUserStatsByProfileId;
        private SQLiteCommand _getUserStatsByNick;
        private SQLiteCommand _getUsersByName;
        private SQLiteCommand _getUsersByEmail;
        private SQLiteCommand _updateUser;
        private SQLiteCommand _createUser;
        private SQLiteCommand _countUsers;
        private SQLiteCommand _logUser;
        private SQLiteCommand _logUserUpdateCountry;
        private SQLiteCommand _updateUserStats;
        
        // we're not going to have 100 million users using this login database
        private const int UserIdOffset = 200000000;
        private const int ProfileIdOffset = 100000000;

        private readonly object _dbLock = new object();

        public static void Initialize(string databasePath)
        {
            // we need to safely dispose of the database when the application closes
            // this is a console app, so we need to hook into the console ctrl signal
            _closeHandler += CloseHandler;

            //SetConsoleCtrlHandler(_closeHandler, true);
            
            _instance = new UsersDatabase();

            databasePath = Path.GetFullPath(databasePath);

            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }

            if (File.Exists(databasePath))
            {
                SQLiteConnectionStringBuilder connBuilder = new SQLiteConnectionStringBuilder()
                {
                    DataSource = databasePath,
                    Version = 3,
                    PageSize = 4096,
                    CacheSize = 10000,
                    JournalMode = SQLiteJournalModeEnum.Wal,
                    LegacyFormat = false,
                    DefaultTimeout = 500
                };

                _instance._db = new SQLiteConnection(connBuilder.ToString());
                _instance._db.Open();

                if (_instance._db.State == ConnectionState.Open)
                {
                    bool read = false;
                    using (SQLiteCommand queryTables = new SQLiteCommand("SELECT * FROM sqlite_master WHERE type='table' AND name='users'", _instance._db))
                    {
                        using (SQLiteDataReader reader = queryTables.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                read = true;
                                break;
                            }
                        }
                    }

                    if (!read)
                    {
                        L.Log(Category, "No database found, creating now");
                        using (SQLiteCommand createTables = new SQLiteCommand(@"CREATE TABLE users (
id INTEGER PRIMARY KEY, 
steamid INTEGER NULL DEFAULT '0',
name TEXT NOT NULL,
password TEXT NOT NULL, 
email TEXT NOT NULL, 
country TEXT NOT NULL, 
lastip TEXT NOT NULL, 
lasttime INTEGER NULL DEFAULT '0', 
session INTEGER NULL DEFAULT '0',

score1v1 INTEGER NULL DEFAULT 1000,
score2v2 INTEGER NULL DEFAULT 1000,
score3v3 INTEGER NULL DEFAULT 1000,

disconnects INTEGER NULL DEFAULT '0',
allingameticks INTEGER NULL DEFAULT '0',
best1v1winstreak INTEGER NULL DEFAULT '0',
current1v1winstreak INTEGER NULL DEFAULT '0',
modified INTEGER NULL DEFAULT '0',

smgamescount INTEGER NULL DEFAULT '0', 
csmgamescount INTEGER NULL DEFAULT '0', 
orkgamescount INTEGER NULL DEFAULT '0', 
eldargamescount INTEGER NULL DEFAULT '0',
iggamescount INTEGER NULL DEFAULT '0',
necrgamescount INTEGER NULL DEFAULT '0',
taugamescount INTEGER NULL DEFAULT '0',
degamescount INTEGER NULL DEFAULT '0',
sobgamescount INTEGER NULL DEFAULT '0',

smwincount INTEGER NULL DEFAULT '0', 
csmwincount INTEGER NULL DEFAULT '0', 
orkwincount INTEGER NULL DEFAULT '0', 
eldarwincount INTEGER NULL DEFAULT '0',
igwincount INTEGER NULL DEFAULT '0',
necrwincount INTEGER NULL DEFAULT '0',
tauwincount INTEGER NULL DEFAULT '0',
dewincount INTEGER NULL DEFAULT '0',
sobwincount INTEGER NULL DEFAULT '0'
)", _instance._db))
                        {
                            createTables.ExecuteNonQuery();
                        }
                        L.Log(Category, "Using " + databasePath);
                        _instance.PrepareStatements();
                        return;
                    }
                    else
                    {
                        L.Log(Category, "Using " + databasePath);
                        _instance.PrepareStatements();
                        return;
                    }
                }
            }

            L.LogError(Category, "Error creating database");

            _instance.Dispose();
            _instance = null;
        }

        private void PrepareStatements()
        {
            _getUsersByProfileId = new SQLiteCommand("SELECT id, password, email, country, session FROM users WHERE id=@id COLLATE NOCASE", _db);
            _getUsersByProfileId.Parameters.Add("@id", DbType.Int32);

            _getUserStatsByNick = new SQLiteCommand(@"SELECT id,
steamid,
score1v1,
score2v2,
score3v3, 
disconnects,
allingameticks,
best1v1winstreak,
current1v1winstreak,
modified,

smgamescount, 
csmgamescount, 
orkgamescount, 
eldargamescount,
iggamescount,
necrgamescount,
taugamescount,
degamescount,
sobgamescount,

smwincount, 
csmwincount, 
orkwincount, 
eldarwincount,
igwincount,
necrwincount,
tauwincount,
dewincount,
sobwincount
FROM users WHERE name=@name COLLATE NOCASE", _db);
            _getUserStatsByNick.Parameters.Add("@name", DbType.String);

            _getUserStatsByProfileId = new SQLiteCommand(@"SELECT id,
steamid,
score1v1,
score2v2,
score3v3, 
disconnects,
allingameticks,
best1v1winstreak,
current1v1winstreak,
modified,

smgamescount, 
csmgamescount, 
orkgamescount, 
eldargamescount,
iggamescount,
necrgamescount,
taugamescount,
degamescount,
sobgamescount,

smwincount, 
csmwincount, 
orkwincount, 
eldarwincount,
igwincount,
necrwincount,
tauwincount,
dewincount,
sobwincount
FROM users WHERE id=@id COLLATE NOCASE", _db);
            _getUserStatsByProfileId.Parameters.Add("@id", DbType.Int32);

            _getUsersByName = new SQLiteCommand("SELECT id, steamid, password, email, country, session FROM users WHERE name=@name COLLATE NOCASE", _db);
            _getUsersByName.Parameters.Add("@name", DbType.String);

            _getUsersByEmail = new SQLiteCommand("SELECT id, steamid, name, country, session FROM users WHERE email=@email AND password=@password", _db);
            _getUsersByEmail.Parameters.Add("@email", DbType.String);
            _getUsersByEmail.Parameters.Add("@password", DbType.String);

            _updateUser = new SQLiteCommand("UPDATE users SET password=@pass, email=@email, country=@country, session=@session WHERE name=@name COLLATE NOCASE", _db);
            _updateUser.Parameters.Add("@pass", DbType.String);
            _updateUser.Parameters.Add("@email", DbType.String);
            _updateUser.Parameters.Add("@country", DbType.String);
            _updateUser.Parameters.Add("@session", DbType.Int64);
            _updateUser.Parameters.Add("@name", DbType.String);

            _createUser = new SQLiteCommand("INSERT INTO users (name, steamid, password, email, country, lastip) VALUES ( @name, @pass, @email, @country, @ip )", _db);
            _createUser.Parameters.Add("@name", DbType.String);
            _createUser.Parameters.Add("@steamid", DbType.UInt64);
            _createUser.Parameters.Add("@pass", DbType.String);
            _createUser.Parameters.Add("@email", DbType.String);
            _createUser.Parameters.Add("@country", DbType.String);
            _createUser.Parameters.Add("@ip", DbType.String);

            _countUsers = new SQLiteCommand("SELECT COUNT(*) FROM users WHERE name=@name COLLATE NOCASE", _db);
            _countUsers.Parameters.Add("@name", DbType.String);

            _logUser = new SQLiteCommand("UPDATE users SET lastip=@ip, lasttime=@time WHERE name=@name COLLATE NOCASE", _db);
            _logUser.Parameters.Add("@ip", DbType.String);
            _logUser.Parameters.Add("@time", DbType.Int64);
            _logUser.Parameters.Add("@name", DbType.String);

            _logUserUpdateCountry = new SQLiteCommand("UPDATE users SET country=@country, lastip=@ip, lasttime=@time WHERE name=@name COLLATE NOCASE", _db);
            _logUserUpdateCountry.Parameters.Add("@country", DbType.String);
            _logUserUpdateCountry.Parameters.Add("@ip", DbType.String);
            _logUserUpdateCountry.Parameters.Add("@time", DbType.Int64);
            _logUserUpdateCountry.Parameters.Add("@name", DbType.String);

            // SM, CSM, ORKS, ELD, IG, NECR, TAU, DE, SOB

            _updateUserStats = new SQLiteCommand(@"UPDATE users SET 
score1v1=@score1v1,
score2v2=@score2v2,
score3v3=@score3v3,

disconnects=@disconnects,
allingameticks=@allingameticks,
current1v1winstreak=@current1v1winstreak,
best1v1winstreak=@best1v1winstreak,
modified=@modified,

smgamescount=@smgamescount, 
csmgamescount=@csmgamescount, 
orkgamescount=@orkgamescount, 
eldargamescount=@eldargamescount,
iggamescount=@iggamescount,
necrgamescount=@necrgamescount,
taugamescount=@taugamescount,
degamescount=@degamescount,
sobgamescount=@sobgamescount,

smwincount=@smwincount, 
csmwincount=@csmwincount, 
orkwincount=@orkwincount, 
eldarwincount=@eldarwincount,
igwincount=@igwincount,
necrwincount=@necrwincount,
tauwincount=@tauwincount,
dewincount=@dewincount,
sobwincount=@sobwincount

WHERE id=@id COLLATE NOCASE", _db);
            _updateUserStats.Parameters.Add("@score1v1", DbType.Int64);
            _updateUserStats.Parameters.Add("@score2v2", DbType.Int64);
            _updateUserStats.Parameters.Add("@score3v3", DbType.Int64);

            _updateUserStats.Parameters.Add("@disconnects", DbType.Int64);
            _updateUserStats.Parameters.Add("@allingameticks", DbType.Int64);
            _updateUserStats.Parameters.Add("@best1v1winstreak", DbType.Int64);
            _updateUserStats.Parameters.Add("@current1v1winstreak", DbType.Int64);
            
            _updateUserStats.Parameters.Add("@modified", DbType.Int64);

            _updateUserStats.Parameters.Add("@smgamescount", DbType.Int64);
            _updateUserStats.Parameters.Add("@csmgamescount", DbType.Int64);
            _updateUserStats.Parameters.Add("@orkgamescount", DbType.Int64);
            _updateUserStats.Parameters.Add("@eldargamescount", DbType.Int64);
            _updateUserStats.Parameters.Add("@iggamescount", DbType.Int64);
            _updateUserStats.Parameters.Add("@necrgamescount", DbType.Int64);
            _updateUserStats.Parameters.Add("@taugamescount", DbType.Int64);
            _updateUserStats.Parameters.Add("@degamescount", DbType.Int64);
            _updateUserStats.Parameters.Add("@sobgamescount", DbType.Int64);

            _updateUserStats.Parameters.Add("@smwincount", DbType.Int64);
            _updateUserStats.Parameters.Add("@csmwincount", DbType.Int64);
            _updateUserStats.Parameters.Add("@orkwincount", DbType.Int64);
            _updateUserStats.Parameters.Add("@eldarwincount", DbType.Int64);
            _updateUserStats.Parameters.Add("@igwincount", DbType.Int64);
            _updateUserStats.Parameters.Add("@necrwincount", DbType.Int64);
            _updateUserStats.Parameters.Add("@tauwincount", DbType.Int64);
            _updateUserStats.Parameters.Add("@dewincount", DbType.Int64);
            _updateUserStats.Parameters.Add("@sobwincount", DbType.Int64);

            _updateUserStats.Parameters.Add("@id", DbType.Int64);
        }

        private static bool CloseHandler(CtrlType sig)
        {
            if (_instance != null)
                _instance.Dispose();

            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                default:
                    return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_getUsersByName != null)
                    {
                        _getUsersByName.Dispose();
                        _getUsersByName = null;
                    }
                    if (_getUsersByEmail != null)
                    {
                        _getUsersByEmail.Dispose();
                        _getUsersByEmail = null;
                    }
                    if (_updateUser != null)
                    {
                        _updateUser.Dispose();
                        _updateUser = null;
                    }
                    if (_createUser != null)
                    {
                        _createUser.Dispose();
                        _createUser = null;
                    }
                    if (_countUsers != null)
                    {
                        _countUsers.Dispose();
                        _countUsers = null;
                    }
                    if (_logUser != null)
                    {
                        _logUser.Dispose();
                        _logUser = null;
                    }
                    if (_logUserUpdateCountry != null)
                    {
                        _logUserUpdateCountry.Dispose();
                        _logUserUpdateCountry = null;
                    }
                    if (_updateUserStats != null)
                    {
                        _updateUserStats.Dispose();
                        _updateUserStats = null;
                    }
                    if (_getUserStatsByProfileId != null)
                    {
                        _getUserStatsByProfileId.Dispose();
                        _getUserStatsByProfileId = null;
                    }
                    if (_getUserStatsByNick != null)
                    {
                        _getUserStatsByNick.Dispose();
                        _getUserStatsByNick = null;
                    }

                    if (_db != null)
                    {
                        _db.Close();
                        _db.Dispose();
                        _db = null;
                    }
                    _instance = null;

                    if (_instance != null)
                    {
                        _instance.Dispose();
                        _instance = null;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        ~UsersDatabase()
        {
            Dispose(false);
        }

        public static bool IsInitialized()
        {
            return _instance != null && _instance._db != null;
        }

        public static UsersDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new ArgumentNullException("Instance", "Initialize() must be called first");
                }

                return _instance;
            }
        }

        public UserData GetUserDataByProfileId(long profileId)
        {
            if (_db == null)
                return null;

            lock (_dbLock)
            {
                _getUsersByProfileId.Parameters["@id"].Value = profileId - ProfileIdOffset;

                using (SQLiteDataReader reader = _getUsersByProfileId.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var data = new UserData()
                        {
                            Id = reader["@id"],
                            Name = (string)reader["name"],
                            Passwordenc = (string)reader["password"],
                            Email = (string)reader["email"],
                            Country = (string)reader["country"],
                            UserId = (Int64)reader["id"] + UserIdOffset,
                            ProfileId =  (Int64)reader["id"] + ProfileIdOffset,
                            SteamId = (UInt64)reader["steamid"],
                            Session = (Int64)reader["session"]
                        };
                        
                        return data;
                    }
                }
            }

            return null;
        }

        public StatsData GetStatsDataByNick(string nick)
        {
            if (_db == null)
                return null;

            lock (_dbLock)
            {
                _getUserStatsByNick.Parameters["@name"].Value = nick;

                using (SQLiteDataReader reader = _getUserStatsByNick.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var data = new StatsData();

                        data.Id = reader["id"];
                        data.UserId = (Int64)reader["id"] + UserIdOffset;
                        data.ProfileId = (Int64)reader["id"] + ProfileIdOffset;
                        data.SteamId = (UInt64)reader["steamid"];

                        data.Score1v1 = (Int64)reader["score1v1"];
                        data.Score2v2 = (Int64)reader["score2v2"];
                        data.Score3v3 = (Int64)reader["score3v3"];

                        data.Disconnects = (Int32)(Int64)reader["disconnects"];
                        data.AllInGameTicks = (Int64)reader["allingameticks"];
                        data.Best1v1Winstreak = (Int32)(Int64)reader["best1v1winstreak"];
                        data.Current1v1Winstreak = (Int32)(Int64)reader["current1v1winstreak"];
                        data.Modified = (Int64)reader["modified"];

                        data.Smgamescount = (Int64)reader["smgamescount"];
                        data.Csmgamescount = (Int64)reader["csmgamescount"];
                        data.Orkgamescount = (Int64)reader["orkgamescount"];
                        data.Eldargamescount = (Int64)reader["eldargamescount"];
                        data.Iggamescount = (Int64)reader["iggamescount"];
                        data.Necrgamescount = (Int64)reader["necrgamescount"];
                        data.Taugamescount = (Int64)reader["taugamescount"];
                        data.Degamescount = (Int64)reader["degamescount"];
                        data.Sobgamescount = (Int64)reader["sobgamescount"];

                        data.Smwincount = (Int64)reader["smwincount"];
                        data.Csmwincount = (Int64)reader["csmwincount"];
                        data.Orkwincount = (Int64)reader["orkwincount"];
                        data.Eldarwincount = (Int64)reader["eldarwincount"];
                        data.Igwincount = (Int64)reader["igwincount"];
                        data.Necrwincount = (Int64)reader["necrwincount"];
                        data.Tauwincount = (Int64)reader["tauwincount"];
                        data.Dewincount = (Int64)reader["dewincount"];
                        data.Sobwincount = (Int64)reader["sobwincount"];

                        return data;
                    }
                }
            }

            return null;
        }

        public void UpdateUserStats(StatsData stats)
        {
            if (_db == null)
                return;

            lock (_dbLock)
            {
                _updateUserStats.Parameters["@id"].Value = stats.Id;

                _updateUserStats.Parameters["@score1v1"].Value = stats.Score1v1;
                _updateUserStats.Parameters["@score2v2"].Value = stats.Score2v2;
                _updateUserStats.Parameters["@score3v3"].Value = stats.Score3v3;

                _updateUserStats.Parameters["@disconnects"].Value = stats.Disconnects;
                _updateUserStats.Parameters["@allingameticks"].Value = stats.AllInGameTicks;
                _updateUserStats.Parameters["@current1v1winstreak"].Value = stats.Current1v1Winstreak;
                _updateUserStats.Parameters["@best1v1winstreak"].Value = stats.Best1v1Winstreak;
                _updateUserStats.Parameters["@modified"].Value = stats.Modified;

                _updateUserStats.Parameters["@smgamescount"].Value = stats.Smgamescount;
                _updateUserStats.Parameters["@csmgamescount"].Value = stats.Csmgamescount;
                _updateUserStats.Parameters["@orkgamescount"].Value = stats.Orkgamescount;
                _updateUserStats.Parameters["@eldargamescount"].Value = stats.Eldargamescount;
                _updateUserStats.Parameters["@iggamescount"].Value = stats.Iggamescount;
                _updateUserStats.Parameters["@necrgamescount"].Value = stats.Necrgamescount;
                _updateUserStats.Parameters["@taugamescount"].Value = stats.Taugamescount;
                _updateUserStats.Parameters["@degamescount"].Value = stats.Degamescount;
                _updateUserStats.Parameters["@sobgamescount"].Value = stats.Sobgamescount;

                _updateUserStats.Parameters["@smwincount"].Value = stats.Smwincount;
                _updateUserStats.Parameters["@csmwincount"].Value = stats.Csmwincount;
                _updateUserStats.Parameters["@orkwincount"].Value = stats.Orkwincount;
                _updateUserStats.Parameters["@eldarwincount"].Value = stats.Eldarwincount;
                _updateUserStats.Parameters["@igwincount"].Value = stats.Igwincount;
                _updateUserStats.Parameters["@necrwincount"].Value = stats.Necrwincount;
                _updateUserStats.Parameters["@tauwincount"].Value = stats.Tauwincount;
                _updateUserStats.Parameters["@dewincount"].Value = stats.Dewincount;
                _updateUserStats.Parameters["@sobwincount"].Value = stats.Sobwincount;

                _updateUserStats.ExecuteNonQuery();
            }
        }

        public StatsData GetStatsDataByProfileId(long profileId)
        {
            if (_db == null)
                return null;

            lock (_dbLock)
            {
                _getUserStatsByProfileId.Parameters["@id"].Value = profileId - ProfileIdOffset;

                using (SQLiteDataReader reader = _getUserStatsByProfileId.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var data = new StatsData();
                        
                        data.Id = reader["id"];
                        data.UserId = (Int64)reader["id"] + UserIdOffset;
                        data.ProfileId = (Int64)reader["id"] + ProfileIdOffset;
                        data.SteamId = (UInt64)reader["steamid"];

                        data.Score1v1 = (Int64)reader["score1v1"];
                        data.Score2v2 = (Int64)reader["score2v2"];
                        data.Score3v3 = (Int64)reader["score3v3"];

                        data.Disconnects = (Int32)(Int64)reader["disconnects"];
                        data.AllInGameTicks = (Int64)reader["allingameticks"];
                        data.Best1v1Winstreak = (Int32)(Int64)reader["best1v1winstreak"];
                        data.Current1v1Winstreak = (Int32)(Int64)reader["current1v1winstreak"];

                        data.Modified = (Int64)reader["modified"];

                        data.Smgamescount = (Int64)reader["smgamescount"];
                        data.Csmgamescount = (Int64)reader["csmgamescount"];
                        data.Orkgamescount = (Int64)reader["orkgamescount"];
                        data.Eldargamescount = (Int64)reader["eldargamescount"];
                        data.Iggamescount = (Int64)reader["iggamescount"];
                        data.Necrgamescount = (Int64)reader["necrgamescount"];
                        data.Taugamescount = (Int64)reader["taugamescount"];
                        data.Degamescount = (Int64)reader["degamescount"];
                        data.Sobgamescount = (Int64)reader["sobgamescount"];

                        data.Smwincount = (Int64)reader["smwincount"];
                        data.Csmwincount = (Int64)reader["csmwincount"];
                        data.Orkwincount = (Int64)reader["orkwincount"];
                        data.Eldarwincount = (Int64)reader["eldarwincount"];
                        data.Igwincount = (Int64)reader["igwincount"];
                        data.Necrwincount = (Int64)reader["necrwincount"];
                        data.Tauwincount = (Int64)reader["tauwincount"];
                        data.Dewincount = (Int64)reader["dewincount"];
                        data.Sobwincount = (Int64)reader["sobwincount"];
                        
                        return data;
                    }
                }
            }

            return null;
        }

        public UserData GetUserData(string username)
        {
            if (_db == null)
                return null;

            if (!UserExists(username))
                return null;

            lock (_dbLock)
            {
                _getUsersByName.Parameters["@name"].Value = username;

                using (SQLiteDataReader reader = _getUsersByName.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var data = new UserData()
                        {
                            Id = reader["id"],
                            Name = username,
                            Passwordenc = (string)reader["password"],
                            Email = (string)reader["email"],
                            Country = (string)reader["country"],
                            UserId = (Int64)reader["id"] + UserIdOffset,
                            ProfileId = (Int64)reader["id"] + ProfileIdOffset,
                            SteamId = (UInt64)reader["steamid"],
                            Session = (Int64)reader["session"]
                        };

                        return data;
                    }
                }
            }

            return null;
        }

        public List<UserData> GetAllUserDatas(string email, string passwordEncrypted)
        {
            if (_db == null)
                return null;

            var values = new List<UserData>();

            lock (_dbLock)
            {
                _getUsersByEmail.Parameters["@email"].Value = email.ToLowerInvariant();
                _getUsersByEmail.Parameters["@password"].Value = passwordEncrypted;

                using (SQLiteDataReader reader = _getUsersByEmail.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // loop through all nicks associated with that email/pass combo

                        var data = new UserData()
                        {
                            Id = reader["id"],
                            Name = (string)reader["name"],
                            Passwordenc = (string)reader["password"],
                            Email = (string)reader["email"],
                            Country = (string)reader["country"],
                            UserId = (Int64)reader["id"] + UserIdOffset,
                            ProfileId = (Int64)reader["id"] + ProfileIdOffset,
                            SteamId = (UInt64)reader["steamid"],
                            Session = (Int64)reader["session"]
                        };
                        
                        values.Add(data);
                    }
                }
            }

            return values;
        }

        public void SetUserData(string name, Dictionary<string, object> data)
        {
            var oldValues = GetUserData(name);

            if (oldValues == null)
                return;

            lock (_dbLock)
            {
                _updateUser.Parameters["@pass"].Value = data.ContainsKey("passwordenc") ? data["passwordenc"] : oldValues.Passwordenc;
                _updateUser.Parameters["@email"].Value = data.ContainsKey("email") ? ((string)data["email"]).ToLowerInvariant() : oldValues.Email;
                _updateUser.Parameters["@country"].Value = data.ContainsKey("country") ? data["country"].ToString().ToUpperInvariant() : oldValues.Country;
                _updateUser.Parameters["@session"].Value = data.ContainsKey("session") ? data["session"] : oldValues.Session;
                _updateUser.Parameters["@name"].Value = name;

                _updateUser.ExecuteNonQuery();
            }
        }

        public void LogLogin(string name, IPAddress address)
        {
            if (_db == null)
                return;

            var data = GetUserData(name);
            if (data == null)
                return;

            // for some reason, when creating an account, sometimes the country doesn't get set
            // it gets set to ?? which is the default. probably the message didn't make it through or something
            // but anyway, if it doesn't match what's in the db, then we want to update the country field to the user's
            // country as defined by IP address
            // to save on db writes, we do this as part of logging the ip/time

            string country = "??";
            if (GeoIP.Instance != null && GeoIP.Instance.Reader != null)
            {
               /* try
                {
                    country = GeoIP.Instance.Reader.Omni(address.ToString()).Country.IsoCode.ToUpperInvariant();
                }
                catch (Exception)
                {
                }*/
            }

            if (country != "??" && !data.Country.ToString().Equals(country, StringComparison.InvariantCultureIgnoreCase))
            {
                lock (_dbLock)
                {

                    _logUserUpdateCountry.Parameters["@country"].Value = country;
                    _logUserUpdateCountry.Parameters["@ip"].Value = address.ToString();
                    _logUserUpdateCountry.Parameters["@time"].Value = DateTime.UtcNow.ToEpochInt();
                    _logUserUpdateCountry.Parameters["@name"].Value = name;

                    _logUserUpdateCountry.ExecuteNonQuery();
                }
            }
            else
            {
                lock (_dbLock)
                {
                    _logUser.Parameters["@ip"].Value = address.ToString();
                    _logUser.Parameters["@time"].Value = DateTime.UtcNow.ToEpochInt();
                    _logUser.Parameters["@name"].Value = name;

                    _logUser.ExecuteNonQuery();
                }
            }
        }

        public void CreateUser(string username, string passwordEncrypted, ulong steamId, string email, string country, IPAddress address)
        {
            if (_db == null)
                return;

            if (UserExists(username))
                return;

            lock (_dbLock)
            {
                _createUser.Parameters["@name"].Value = username;
                _createUser.Parameters["@steamid"].Value = steamId;
                _createUser.Parameters["@pass"].Value = passwordEncrypted;
                _createUser.Parameters["@email"].Value = email.ToLowerInvariant();
                _createUser.Parameters["@country"].Value = country.ToUpperInvariant();
                _createUser.Parameters["@ip"].Value = address.ToString();

                _createUser.ExecuteNonQuery();
            }
        }

        public bool UserExists(string username)
        {
            bool existing = false;

            if (_db == null)
                return false;

            lock (_dbLock)
            {
                _countUsers.Parameters["@name"].Value = username;

                using (SQLiteDataReader reader = _countUsers.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // only go once

                        if (reader.FieldCount == 1 && (Int64)reader[0] == 1)
                        {
                            existing = true;
                        }
                    }
                }
            }

            return existing;
        }

       // [DllImport("Kernel32")]
       // private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
    }
}
