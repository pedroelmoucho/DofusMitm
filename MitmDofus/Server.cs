using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MitmDofus
{
    public class Server
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private int connNumber = 0;

        public Server() { }

        public void StartListening()
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 5565);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Start");

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (connNumber < 2)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept( new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                    connNumber++;
                }
                Console.WriteLine("Press enter to relaunch the server");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("Accept new server client " + connNumber);

            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.  
            var handler = (Socket)ar.AsyncState;
            handler = handler.EndAccept(ar);

            if (connNumber == 0)
                new ServerSocket(handler, "34.251.172.139");
            else
                new ServerSocket(handler, "52.17.83.159");
        }
    }
}
