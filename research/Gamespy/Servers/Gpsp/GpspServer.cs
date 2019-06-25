using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using BF2Statistics.Gamespy.Net;
using BF2Statistics.Net;

namespace BF2Statistics.Gamespy
{
    /// <summary>
    /// This class emulates the Gamespy Search Provider Server on port 29901.
    /// This server is responsible for fetching all associates accounts with
    /// the provided email and password, as well as verifying a player account ID.
    /// </summary>
    public class GpspServer : GamespyTcpSocket
    {
        /// <summary>
        /// Max number of concurrent open and active connections.
        /// </summary>
        /// <remarks>Connections to the Gpsp server are short lived</remarks>
        public const int MaxConnections = 8;

        /// <summary>
        /// A List of sucessfully active connections (ConnId => Client Obj) on the MasterServer TCP line
        /// </summary>
        private static ConcurrentDictionary<int, GpspClient> Clients = new ConcurrentDictionary<int, GpspClient>();

        public GpspServer() : base(29901, MaxConnections)
        {
            // Register for disconnect
            GpspClient.OnDisconnect += GpspClient_OnDisconnect;

            // Begin accepting connections
            base.StartAcceptAsync();
        }

        /// <summary>
        /// Shutsdown the GPSP server and socket
        /// </summary>
        public void Shutdown()
        {
            // Stop accepting new connections
            base.IgnoreNewConnections = true;

            // Unregister events so we dont get a shit ton of calls
            GpspClient.OnDisconnect -= GpspClient_OnDisconnect;

            // Disconnected all connected clients
            foreach (GpspClient C in Clients.Values)
                C.Dispose(true);

            // clear clients
            Clients.Clear();

            // Shutdown the listener socket
            base.ShutdownSocket();

            // Tell the base to dispose all free objects
            base.Dispose();
        }

        /// <summary>
        /// When a new connection is established, we the parent class are responsible
        /// for handling the processing
        /// </summary>
        /// <param name="Stream">A GamespyTcpStream object that wraps the I/O AsyncEventArgs and socket</param>
        protected override void ProcessAccept(GamespyTcpStream Stream)
        {
            try
            {
                // Convert the TcpClient to a MasterClient
                GpspClient client = new GpspClient(Stream);
                Clients.TryAdd(client.ConnectionId, client);

                // Begin accepting data now that we are fully connected
                Stream.BeginReceive();
            }
            catch (Exception e)
            {
                L.LogError("WARNING: An Error occured at [Gpsp.ProcessAccept] : Generating Exception Log");
                ExceptionHandler.GenerateExceptionLog(e);
                base.Release(Stream);
            }
        }

        /// <summary>
        /// Callback for when a connection had disconnected
        /// </summary>
        /// <param name="sender">The client object whom is disconnecting</param>
        private void GpspClient_OnDisconnect(GpspClient client)
        {
            // Release this stream's AsyncEventArgs to the object pool
            base.Release(client.Stream);
            if (Clients.TryRemove(client.ConnectionId, out client) && !client.Disposed)
                client.Dispose();
        }
    }
}
