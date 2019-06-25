using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BF2Statistics.Gamespy.Net;
using BF2Statistics.Logging;
using BF2Statistics.Net;

namespace BF2Statistics.Gamespy
{
    /// <summary>
    /// This class emulates the master.gamespy.com TCP server on port 28910.
    /// This server is responisible for sending server lists to the online
    /// server browser in the BF2 multiplayer menu
    /// </summary>
    public class ServerListRetrieveSocket : GamespyTcpSocket
    {
        /// <summary>
        /// Max number of concurrent open and active connections.
        /// <remarks>Connections to the Master server are short lived</remarks>
        /// </summary>
        public const int MaxConnections = 8;

        /// <summary>
        /// A List of sucessfully active connections (ConnectionId => Client Obj) on the MasterServer TCP line
        /// </summary>
        private static ConcurrentDictionary<int, MasterClient> Clients = new ConcurrentDictionary<int, MasterClient>();

        public ServerListRetrieveSocket() : base(28910, MaxConnections)
        {
            // Start accepting connections
            MasterClient.OnDisconnect += MasterClient_OnDisconnect;
            base.StartAcceptAsync();
        }

        /// <summary>
        /// Shutsdown the underlying sockets
        /// </summary>
        public void Shutdown()
        {
            // Stop accepting new connections
            base.IgnoreNewConnections = true;

            // Unregister events so we dont get a shit ton of calls
            MasterClient.OnDisconnect -= MasterClient_OnDisconnect;

            // Disconnected all connected clients
            foreach (MasterClient client in Clients.Values)
                client.Dispose(true);

            // Update Connected Clients in the Database
            Clients.Clear();

            // Shutdown the listener socket
            base.ShutdownSocket();

            // Tell the base to dispose all free objects
            base.Dispose();
        }

        /// <summary>
        /// Accepts a TcpClient, and begin the serverlist fetching process for the client. 
        /// This method is executed when the user updates his server browser ingame
        /// </summary>
        protected override void ProcessAccept(GamespyTcpStream Stream)
        {
            // End the operation and display the received data on  
            // the console.
            try
            {
                // Convert the TcpClient to a MasterClient
                MasterClient client = new MasterClient(Stream);
                Clients.TryAdd(client.ConnectionId, client);

                // Begin accepting data now that we are fully connected
                Stream.BeginReceive();
            }
            catch (Exception e)
            {
                L.LogError("WARNING: An Error occured at [MstrServer.AcceptClient] : Generating Exception Log");
                ExceptionHandler.GenerateExceptionLog(e);
                base.Release(Stream);
            }
        }

        /// <summary>
        /// Callback for when a connection had disconnected
        /// </summary>
        private void MasterClient_OnDisconnect(MasterClient client)
        {
            // Remove client, and call OnUpdate Event
            try
            {
                // Release this stream's AsyncEventArgs to the object pool
                base.Release(client.Stream);

                // Remove client from online list
                if (Clients.TryRemove(client.ConnectionId, out client) && !client.Disposed)
                    client.Dispose();
            }
            catch (Exception e)
            {
                L.LogError("An Error occured at [MasterServer.OnDisconnect] : Generating Exception Log");
                ExceptionHandler.GenerateExceptionLog(e);
            }
        }
    }
}
