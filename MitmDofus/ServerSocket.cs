using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private bool IsInFight = false;
        private int CharacterId = 0;
        private ConcurrentDictionary<int, int> MonstersCells = new ConcurrentDictionary<int, int>();

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
                    if (packet.StartsWith("ASK"))
                    {
                        string[] splittedData = packet.Substring(4).Split('|');
                        CharacterId = int.Parse(splittedData[0]);
                    }
                    if (packet.StartsWith("GJK"))
                    {
                        IsInFight = true;
                        MonstersCells = new ConcurrentDictionary<int, int>();
                        Debug.WriteLine("Fight start");
                        Debug.WriteLine("join fight");
                        SocketClient.Send(Encoding.UTF8.GetBytes("GA903" + CharacterId + ";" + CharacterId + "\n\x00"));
                        SocketClient.Send(Encoding.UTF8.GetBytes("GA903" + CharacterId + ";" + CharacterId + "\n\x00"));
                    }  
                    if (packet.StartsWith("GE"))
                    {
                        IsInFight = false;
                        Debug.WriteLine("Fight end");
                    }
                    if (packet.StartsWith("GA") && !packet.StartsWith("GAS") && !packet.StartsWith("GAF"))
                    {
                        string[] splittedData = packet.Substring(2).Split(';');
                        int actionId = int.Parse(splittedData[1]);
                        if (actionId > 0)
                        {
                            switch(actionId)
                            {
                                case 1:
                                    int entityId = int.Parse(splittedData[2]);
                                    Debug.WriteLine("Entity at : " + Hash.Get_Cell_From_Hash(splittedData[3].Substring(splittedData[3].Length - 2)));
                                    MonstersCells[entityId] = Hash.Get_Cell_From_Hash(splittedData[3].Substring(splittedData[3].Length - 2));
                                    break;
                                case 5:
                                    splittedData = splittedData[3].Split(',');
                                    Debug.WriteLine("Entity at : " + int.Parse(splittedData[1]));
                                    MonstersCells[int.Parse(splittedData[0])] = int.Parse(splittedData[1]);
                                    break;
                                case 103:
                                    Debug.WriteLine("Entity dead");
                                    MonstersCells.TryRemove(int.Parse(splittedData[3]), out int test);
                                    break;
                            }
                        }
                    }
                    if (packet.StartsWith("GTS"))
                    {
                        Debug.WriteLine("Turn start");
                        foreach (var key in MonstersCells.Keys)
                        {
                            if (key != CharacterId && int.Parse(packet.Substring(3).Split('|')[0]) == CharacterId)
                            {
                                Debug.WriteLine("cast spell " + "GA300167;" + MonstersCells[key]);
                                Console.WriteLine("GA300167;" + MonstersCells[key]);
                                SocketClient.Send(Encoding.UTF8.GetBytes("GA300167;" + MonstersCells[key] + "\n\x00"));
                            } else if ((key == CharacterId) && int.Parse(packet.Substring(3).Split('|')[0]) == CharacterId)
                            {
                                Debug.WriteLine("cast spell " + "GA300172;" + MonstersCells[key]);
                                Console.WriteLine("GA300172;" + MonstersCells[key]);
                                SocketClient.Send(Encoding.UTF8.GetBytes("GA300172;" + MonstersCells[key] + "\n\x00"));
                            }
                        }
                    }
                    if (packet.StartsWith("Gt"))
                    {
                        Debug.WriteLine("join fight");
                        SocketClient.Send(Encoding.UTF8.GetBytes("GA903" + CharacterId + ";" + CharacterId + "\n\x00"));
                    }
                }

                SocketServer.Send(Encoding.UTF8.GetBytes(datas));
                SocketClient.BeginReceive(BufferClient, 0, BufferClient.Length, SocketFlags.None, new AsyncCallback(ClientReceiveCallBack), SocketClient);
            }
        }
    }
}
