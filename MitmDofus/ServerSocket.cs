using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MitmDofus
{
    public class ServerSocket
    {
        private Socket SocketServer { get; set; }
        private Socket SocketClient { get; set; }
        private byte[] BufferServer { get; set; }
        private byte[] BufferClient { get; set; }

        public ServerSocket (Socket socket, string ip)
        {
            // Server Socket
            SocketServer = socket;
            BufferServer = new byte[socket.ReceiveBufferSize];
            SocketServer.BeginReceive(BufferServer, 0, BufferServer.Length, 0, new AsyncCallback(ServerReceiveCallBack), SocketServer);

            // Client Socket
            SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            BufferClient = new byte[SocketClient.ReceiveBufferSize];
            SocketClient.BeginConnect(IPAddress.Parse(ip), 443, new AsyncCallback(ClientConnectCallback), SocketClient);
        }

        private void ClientConnectCallback(IAsyncResult result)
        {
            try
            {
                Console.WriteLine("Client Connected");
                SocketClient = result.AsyncState as Socket;
                SocketClient.EndConnect(result);
                SocketClient.BeginReceive(BufferClient, 0, BufferServer.Length, SocketFlags.None, new AsyncCallback(ClientReceiveCallBack), SocketClient);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ServerReceiveCallBack(IAsyncResult result)
        {
            int bytes_read = SocketServer.EndReceive(result, out SocketError reply);

            if (bytes_read > 0 && reply == SocketError.Success)
            {
                string datas = Encoding.UTF8.GetString(BufferServer, 0, bytes_read);

                foreach (var packet in datas.Replace("\x0a", string.Empty).Split('\0').Where(x => x != string.Empty))
                {
                    Console.WriteLine("CMSG " + packet);
                }

                SocketClient.Send(Encoding.UTF8.GetBytes(datas));
                SocketServer.BeginReceive(BufferServer, 0, BufferServer.Length, SocketFlags.None, new AsyncCallback(ServerReceiveCallBack), SocketServer);
            }
        }

        public void ClientReceiveCallBack(IAsyncResult result)
        {
            int bytes_read = SocketClient.EndReceive(result, out SocketError reply);

            if (bytes_read > 0 && reply == SocketError.Success)
            {
                string datas = Encoding.UTF8.GetString(BufferClient, 0, bytes_read);

                foreach (var packet in datas.Replace("\x0a", string.Empty).Split('\0').Where(x => x != string.Empty))
                {
                    Console.WriteLine("SMSG " + packet);

                    if (packet.StartsWith("AXK"))
                        Console.WriteLine(Hash.DecryptIp(packet.Substring(3, 8)) + " " + Hash.DecryptPort(packet.Substring(11, 3).ToCharArray()));
                    if (packet.StartsWith("SL") && packet[2] != 'o')
                    {
                        Console.WriteLine("Spells");
                        datas = datas.Replace("~1~", "~6~");
                    }
                }

                SocketServer.Send(Encoding.UTF8.GetBytes(datas));
                SocketClient.BeginReceive(BufferClient, 0, BufferClient.Length, SocketFlags.None, new AsyncCallback(ClientReceiveCallBack), SocketClient);
            }
        }
    }
}
