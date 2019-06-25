using BF2Statistics.Gamespy.Net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BF2Statistics.Gamespy
{
    public enum LoginStatus { None, Processing, Completed, Disconnected }

    /// <summary>
    /// Gamespy Client Manager
    /// This class is used to proccess the client login process,
    /// create new user accounts, and fetch profile information
    /// <remarks>gpcm.gamespy.com</remarks>
    /// </summary>
    public class GpcmClient : IDisposable
    {
        #region Variables

        /// <summary>
        /// Gets the current login status
        /// </summary>
        public LoginStatus Status { get; protected set; }

        /// <summary>
        /// The connected clients Player Id
        /// </summary>
        public int PlayerId { get; protected set; }

        /// <summary>
        /// The connected clients Nick
        /// </summary>
        public string PlayerNick { get; protected set; }

        /// <summary>
        /// The connected clients Email Address
        /// </summary>
        public string PlayerEmail { get; protected set; }

        /// <summary>
        /// The connected clients country code
        /// </summary>
        public string PlayerCountryCode { get; protected set; }

        /// <summary>
        /// The TcpClient's Endpoint
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; protected set; }

        /// <summary>
        /// The users session key,and Crc Checksum of the PlayerNick
        /// </summary>
        public ushort SessionKey { get; protected set; }

        /// <summary>
        /// The clients password, MD5 hashed from UTF8 bytes
        /// </summary>
        private string PasswordHash;

        /// <summary>
        /// The profile id parameter that is sent back to the client is initially 2, 
        /// and then 5 everytime after that. So we set here, whether we have sent the 
        /// profile to the client initially (with \id\2) yet.
        /// </summary>
        private bool ProfileSent = false;

        /// <summary>
        /// The Servers challange key, sent when the client first connects.
        /// This is used as part of the hash used to "prove" to the client
        /// that the password in our database matches what the user enters
        /// </summary>
        private string ServerChallengeKey;

        /// <summary>
        /// Variable that determines if the client is disconnected,
        /// and this object can be cleared from memory
        /// </summary>
        public bool Disposed { get; protected set; }

        /// <summary>
        /// Indicates the connection ID for this connection
        /// </summary>
        public readonly int ConnectionId;

        /// <summary>
        /// Indicates the date and time this connection was created
        /// </summary>
        public readonly DateTime Created = DateTime.Now;

        /// <summary>
        /// The clients socket network stream
        /// </summary>
        public GamespyTcpStream Stream { get; protected set; }

        /// <summary>
        /// A random... random
        /// </summary>
        private Random RandInstance = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// Our CRC16 object for generating Checksums
        /// </summary>
        protected static Crc16 Crc = new Crc16(Crc16Mode.Standard);

        /// <summary>
        /// Array of characters used in generating a signiture
        /// </summary>
        private static readonly char[] AlphaChars = {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
                'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };

        /// <summary>
        /// An array of Alpha Numeric characters used in generating a random string
        /// </summary>
        private static readonly char[] AlphaNumChars = { 
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
                'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
                'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };

        /// <summary>
        /// Array of Hex cahracters, with no leading 0
        /// </summary>
        private static readonly char[] HexChars = {
                '1', '2', '3', '4', '5', '6', '7', '8', '9',
                'a', 'b', 'c', 'd', 'e', 'f'
            };

        /// <summary>
        /// An Event that is fired when the client successfully logs in.
        /// </summary>
        public static event ConnectionUpdate OnSuccessfulLogin;

        /// <summary>
        /// Event fired when the remote connection gets disconnected.
        /// </summary>
        public static event GpcmConnectionClosed OnDisconnect;

        #endregion Variables

        /// <summary>
        /// Constructor
        /// </summary>
        public GpcmClient(GamespyTcpStream ConnectionStream, int ConnectionId)
        {
            // Set default variable values
            PlayerNick = "Connecting...";
            PlayerId = 0;
            RemoteEndPoint = (IPEndPoint)ConnectionStream.RemoteEndPoint;
            Disposed = false;
            Status = LoginStatus.None;

            // Set the connection ID
            this.ConnectionId = ConnectionId;

            // Create our Client Stream
            Stream = ConnectionStream;
            Stream.OnDisconnect += Stream_OnDisconnect;
            Stream.DataReceived += Stream_DataReceived;
            Stream.BeginReceive();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~GpcmClient()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes of the client object. The connection is no longer
        /// closed here and the Disconnect even is NO LONGER fired
        /// </summary>
        public void Dispose()
        {
            // Preapare to be unloaded from memory
            if (Disposed) return;
            Disposed = true;

            // Disconenct if we havent already
            if (Status != LoginStatus.Disconnected)
                Disconnect(1);
        }

        /// <summary>
        /// Logs the client out of the game client, and closes the stream
        /// </summary>
        /// <param name="code">
        /// The disconnect code. If set to 9, the OnDisconect event will not be called, the database
        /// will not be updated to reset everyone's session code, and the EventArgs objects will NOT 
        /// be returned to the IO pool. You should only set to 9 for a planned server shutdown.
        /// </param>
        /// <remarks>
        ///   Codes:
        ///     0 => Client sends the "logout" command
        ///     1 => The login timer elapsed and the client wasnt logged in or this object was disposed, forcefully disconnected
        ///     2 => Invalid login query, or username was incorrect
        ///     3 => Incorrect Password
        ///     4 => An error occured while trying to login the client (could be database related)
        ///     5 => Cant create account, username exists already
        ///     6 => Error Creating new account in database
        ///     7 => Invalid query for account creation, or an exception was thrown while trying to create account
        ///     8 => Remote Connection closed the Stream or was un-readchable
        ///     9 => Forced server shutdown [No events called, database sessions are not updated, and EventArgs are disposed]
        /// </remarks>
        public void Disconnect(int code)
        {
            // Make sure we arent disposed
            if (Disposed) return;

            // Update database session
            if (Status == LoginStatus.Completed && code < 9)
            {
                try
                {
                    using (GamespyDatabase Database = new GamespyDatabase())
                        Database.Execute("UPDATE accounts SET session=0 WHERE id=" + PlayerId);
                }
                catch 
                {
                    // We could be shutting this server down because of DB connection issues, don't do anything here.
                }
            }

            // Unregister for stream events and close the connection
            Stream.OnDisconnect -= Stream_OnDisconnect;
            Stream.DataReceived -= Stream_DataReceived;
            Stream.Close(code == 9);

            // Set status and log
            if (code == 1 && Status == LoginStatus.Processing)
                GpcmServer.Log("Login Timeout:  {0} - {1} - {2}", PlayerNick, PlayerId, RemoteEndPoint);
            else if (Status != LoginStatus.Disconnected)
                GpcmServer.Log("Client Logout:  {0} - {1} - {2}, Code={3}", PlayerNick, PlayerId, RemoteEndPoint, code);

            // Preapare to be unloaded from memory
            Status = LoginStatus.Disconnected;
            Disposed = true;

            // Call disconnect event
            if (OnDisconnect != null)
                OnDisconnect(this);
        }

        #region Stream Callbacks

        /// <summary>
        /// Event called when a complete message has been recieved
        /// </summary>
        /// <param name="Message"></param>
        private void Stream_DataReceived(string message)
        {
            // Read client message, and parse it into key value pairs
            string[] recieved = message.TrimStart('\\').Split('\\');
            switch (recieved[0])
            {
                case "newuser":
                    CreateNewUser(ConvertToKeyValue(recieved));
                    break;
                case "login":
                    ProcessLogin(ConvertToKeyValue(recieved));
                    break;
                case "getprofile":
                    SendProfile();
                    break;
                case "updatepro":
                    UpdateUser(ConvertToKeyValue(recieved));
                    break;
                case "logout":
                    Disconnect(0);
                    break;
                default:
                    Stream.SendAsync(@"\error\\err\0\fatal\\errmsg\Invalid Query!\id\1\final\");
                    GpcmServer.Log("NOTICE: [GpcmClient.Stream_DataReceived] Unkown Message Passed: {0}", message);
                    break;
            }
        }

        /// <summary>
        /// Event fired when the stream disconnects.
        /// Even though its 1 line, we un-register it at one point, so it needs to be here
        /// </summary>
        private void Stream_OnDisconnect()
        {
            Disconnect(8);
        }

        #endregion Stream Callbacks

        #region Login Steps

        /// <summary>
        ///  This method starts off by sending a random string 10 characters
        ///  in length, known as the Server challenge key. This is used by 
        ///  the client to return a client challenge key, which is used
        ///  to validate login information later.
        /// </summary>
        public void SendServerChallenge()
        {
            // Only send the login challenge once
            if (Status != LoginStatus.None)
                throw new Exception("The server challenge has already been sent. Cannot send another login challenge.");

            // First we need to create a random string the length of 10 characters
            StringBuilder Temp = new StringBuilder(10);
            for (int i = 0; i < 10; i++)
                Temp.Append(AlphaChars[RandInstance.Next(AlphaChars.Length)]);

            // Next we send the client the challenge key
            ServerChallengeKey = Temp.ToString();
            Status = LoginStatus.Processing;
            Stream.SendAsync(@"\lc\1\challenge\{0}\id\1\final\", ServerChallengeKey);
        }

        /// <summary>
        /// This method verifies the login information sent by
        /// the client, and returns encrypted data for the client
        /// to verify as well
        /// </summary>
        public void ProcessLogin(Dictionary<string, string> Recv)
        {
            // Make sure we have all the required data to process this login
            if (!Recv.ContainsKey("uniquenick") || !Recv.ContainsKey("challenge") || !Recv.ContainsKey("response"))
            {
                Stream.SendAsync(@"\error\\err\0\fatal\\errmsg\Invalid Query!\id\1\final\");
                Disconnect(2);
                return;
            }

            // Warp this in a try/catch, incase database is offline or something
            try
            {
                using (GamespyDatabase Conn = new GamespyDatabase())
                {
                    // Try and fetch the user from the database
                    Dictionary<string, object> User = Conn.GetUser(Recv["uniquenick"]);
                    if (User == null)
                    {
                        Stream.SendAsync(@"\error\\err\265\fatal\\errmsg\The uniquenick provided is incorrect!\id\1\final\");
                        Disconnect(2);
                        return;
                    }

                    // Set player variables
                    PlayerId = Int32.Parse(User["id"].ToString());
                    PlayerNick = Recv["uniquenick"];
                    PlayerEmail = User["email"].ToString();
                    PlayerCountryCode = User["country"].ToString();
                    PasswordHash = User["password"].ToString();

                    // Use the GenerateProof method to compare with the "response" value. This validates the given password
                    if (Recv["response"] == GenerateProof(Recv["challenge"], ServerChallengeKey))
                    {
                        // Password is correct, Create session key and respond
                        SessionKey = Crc.ComputeChecksum(PlayerNick);
                        Stream.SendAsync(
                            @"\lc\2\sesskey\{0}\proof\{1}\userid\{2}\profileid\{2}\uniquenick\{3}\lt\{4}__\id\1\final\",
                            SessionKey,
                            GenerateProof(ServerChallengeKey, Recv["challenge"]), // Do this again, Params are reversed!
                            PlayerId,
                            PlayerNick,
                            GenerateRandomString(22) // Generate LT whatever that is (some sort of random string, 22 chars long)
                        );

                        // Log, Update database, and call event
                        GpcmServer.Log("Client Login:   {0} - {1} - {2}", PlayerNick, PlayerId, RemoteEndPoint);
                        Conn.Execute("UPDATE accounts SET lastip=@P0, session=@P1 WHERE id=@P2", RemoteEndPoint.Address, SessionKey, PlayerId);

                        // Update status last, and call success login
                        Status = LoginStatus.Completed;
                        if (OnSuccessfulLogin != null)
                            OnSuccessfulLogin(this);
                    }
                    else
                    {
                        // The proof string failed, so the password provided was incorrect
                        GpcmServer.Log("Failed Login Attempt: {0} - {1} - {2}", PlayerNick, PlayerId, RemoteEndPoint);
                        Stream.SendAsync(@"\error\\err\260\fatal\\errmsg\The password provided is incorrect.\id\1\final\");
                        Disconnect(3);
                    }
                }
            }
            catch
            {
                Disconnect(4);
                return;
            }
        }

        /// <summary>
        /// This method is called when the client requests for the Account profile
        /// </summary>
        private void SendProfile()
        {
            Stream.SendAsync(
                @"\pi\\profileid\{0}\nick\{1}\userid\{0}\email\{2}\sig\{3}\uniquenick\{1}\pid\0\firstname\\lastname\" +
                @"\countrycode\{4}\birthday\16844722\lon\0.000000\lat\0.000000\loc\\id\{5}\\final\",
                PlayerId, PlayerNick, PlayerEmail, GenerateSig(), PlayerCountryCode, (ProfileSent ? "5" : "2")
            );

            // Set that we send the profile initially
            if (!ProfileSent) ProfileSent = true;
        }

        /// <summary>
        /// Whenever the "newuser" command is recieved, this method is called to
        /// add the new users information into the database
        /// </summary>
        /// <param name="Recv">Array of parms sent by the server</param>
        private void CreateNewUser(Dictionary<string, string> Recv)
        {
            // Make sure the user doesnt exist already
            try
            {
                using (GamespyDatabase Database = new GamespyDatabase())
                {
                    // Check to see if user exists
                    if (Database.UserExists(Recv["nick"]))
                    {
                        Stream.SendAsync(@"\error\\err\516\fatal\\errmsg\This account name is already in use!\id\1\final\");
                        Disconnect(5);
                        return;
                    }

                    // We need to decode the Gamespy specific encoding for the password
                    string Password = GamespyUtils.DecodePassword(Recv["passwordenc"]);
                    string Cc = (RemoteEndPoint.AddressFamily == AddressFamily.InterNetwork)
                        ? Ip2nation.GetCountryCode(RemoteEndPoint.Address)
                        : Program.Config.ASP_LocalIpCountryCode;

                    // Attempt to create account. If Pid is 0, then we couldnt create the account
                    if ((PlayerId = Database.CreateUser(Recv["nick"], Password, Recv["email"], Cc)) == 0)
                    {
                        Stream.SendAsync(@"\error\\err\516\fatal\\errmsg\Error creating account!\id\1\final\");
                        Disconnect(6);
                        return;
                    }

                    Stream.SendAsync(@"\nur\\userid\{0}\profileid\{0}\id\1\final\", PlayerId);
                }
            }
            catch(Exception e)
            {
                // Check for invalid query params
                if (e is KeyNotFoundException)
                {
                    Stream.SendAsync(@"\error\\err\0\fatal\\errmsg\Invalid Query!\id\1\final\");
                }
                else
                {
                    Stream.SendAsync(@"\error\\err\516\fatal\\errmsg\Error creating account!\id\1\final\");
                    GpcmServer.Log("ERROR: [Gpcm.CreateNewUser] An error occured while trying to create a new User account :: " + e.Message);
                }

                Disconnect(7);
                return;
            }
        }


        /// <summary>
        /// Updates the Users Country code when sent by the client
        /// </summary>
        /// <param name="recv">Array of information sent by the server</param>
        private void UpdateUser(Dictionary<string, string> Recv)
        {
            // Set clients country code
            try
            {
                using (GamespyDatabase Conn = new GamespyDatabase())
                    Conn.UpdateUser(PlayerNick, Recv["countrycode"]);
            }
            catch
            {
                //Dispose();
            }
        }

        /// <summary>
        /// Polls the connection, and checks for drops
        /// </summary>
        public void SendKeepAlive()
        {
            if (Status == LoginStatus.Completed)
            {
                // Try and send a Keep-Alive
                try
                {
                    Stream.SendAsync(@"\ka\\final\");
                }
                catch
                {
                    Disconnect(8);
                }
            }
        }

        #endregion

        #region Misc Methods

        /// <summary>
        /// Converts a recived parameter array from the client string to a keyValue pair dictionary
        /// </summary>
        /// <param name="parts">The array of data from the client</param>
        /// <returns></returns>
        private static Dictionary<string, string> ConvertToKeyValue(string[] parts)
        {
            Dictionary<string, string> Dic = new Dictionary<string, string>();
            try
            {
                for (int i = 0; i < parts.Length; i += 2)
                {
                    if (!Dic.ContainsKey(parts[i]))
                        Dic.Add(parts[i], parts[i + 1]);
                }
            }
            catch (IndexOutOfRangeException) { }

            return Dic;
        }

        /// <summary>
        /// Generates an MD5 hash, which is used to verify the clients login information
        /// </summary>
        /// <param name="challenge1">First challenge key</param>
        /// <param name="challenge2">Second challenge key</param>
        /// <returns>
        ///     The proof verification MD5 hash string that can be compared to what the client sends,
        ///     to verify that the users entered password matches the password in the database.
        /// </returns>
        private string GenerateProof(string challenge1, string challenge2)
        {
            // Generate our string to be hashed
            StringBuilder HashString = new StringBuilder(PasswordHash);
            HashString.Append(' ', 48); // 48 spaces
            HashString.Append(PlayerNick);
            HashString.Append(challenge1);
            HashString.Append(challenge2);
            HashString.Append(PasswordHash);
            return HashString.ToString().GetMD5Hash(false);
        }

        /// <summary>
        /// Generates a random alpha-numeric string
        /// </summary>
        /// <param name="length">The lenght of the string to be generated</param>
        /// <returns></returns>
        private string GenerateRandomString(int length)
        {
            StringBuilder Response = new StringBuilder();
            for (int i = 0; i < length; i++)
                Response.Append(AlphaNumChars[RandInstance.Next(62)]);

            return Response.ToString();
        }

        /// <summary>
        /// Generates a random signature
        /// </summary>
        /// <returns></returns>
        private string GenerateSig()
        {
            StringBuilder Response = new StringBuilder();
            for (int length = 0; length < 32; length++)
                Response.Append(HexChars[RandInstance.Next(14)]);

            return Response.ToString();
        }

        #endregion

        public override bool Equals(object Obj)
        {
            if (Obj is GpcmClient)
            {
                GpcmClient Compare = Obj as GpcmClient;
                return (Compare.PlayerId == this.PlayerId || Compare.PlayerNick == this.PlayerNick);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return PlayerId;
        }
    }
}
