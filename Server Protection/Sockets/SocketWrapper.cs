using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server_Protection.Sockets
{
    public class SocketWrapper
    {
        public DateTime LastBeep;
        MainSocket MainServer;
        Socket Socket;
        byte[] Buffer;
        public string IP
        {
            get { return (Socket.RemoteEndPoint as IPEndPoint).Address.ToString(); }
        }
        public SocketWrapper(Socket Socket, MainSocket MainServer, int BufferLength = 2048)
        {
            this.Socket = Socket;
            this.MainServer = MainServer;
            this.Buffer = new byte[BufferLength];
            Console.WriteLine($"[{IP}] Connection --> New Connection.");
        }
        public void TryRec()
        {
            try
            {
                Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(BeginRec), Socket);
            }
            catch
            {
                Socket.Disconnect(false);
            }
        }
        private void BeginRec(IAsyncResult ar)
        {
            try
            {
                int recLength = Socket.EndReceive(ar);
                if (recLength != 0)
                {
                    byte[] buffer = new byte[recLength];
                    Array.Copy(Buffer, buffer, recLength);
                    HandlePacket(buffer, this);
                    TryRec();
                }
            }
            catch
            {
                Disconnect();
            }
        }
        public void Disconnect(string Reason = "")
        {
            SocketWrapper wrapp;
            ConnectedClients.TryRemove(IP, out wrapp);
            Socket.Disconnect(false);
            Console.WriteLine($"[{IP}] Disconnection --> Killed Protection.");
        }
        private void Send(byte[] p)
        {
            Socket.Send(p);
        }
        public static ConcurrentDictionary<string, SocketWrapper> ConnectedClients = new ConcurrentDictionary<string, SocketWrapper>();
        private void HandlePacket(byte[] buffer, SocketWrapper client)
        {
            try
            {
                int pId = BitConverter.ToUInt16(buffer, 0);
                switch (pId)
                {
                    #region Welcome Packet
                    case 1:// Welcome Packet
                        {
                            string key = Encoding.Default.GetString(buffer, 2, 16);
                            string hwid = Encoding.Default.GetString(buffer, 18, 16);
                            byte[] chash = new byte[20];
                            byte[] mhash = new byte[20];
                            for (int i = 0; i < 20; i++)
                                chash[i] = buffer[34 + i];
                            for (int i = 0; i < 20; i++)
                                mhash[i] = buffer[54 + i];
                            bool valid = true;
                            if (ValidateBuf(chash, Program.ConquerHash)) Console.WriteLine($"[{IP}] Valid --> Conquer.exe");
                            else
                            {
                                Console.WriteLine("Invalid Conquer hash");
                                valid = false;
                            }
                            if (ValidateBuf(mhash, Program.MagicHash)) Console.WriteLine($"[{IP}] Valid --> Magiceffect.ini");
                            else
                            {
                                Console.WriteLine("Invalid magic hash");
                                valid = false;
                            }
                            byte[] res = new byte[1];
                            if (valid) res[0] = 20;
                            else res[0] = 10;
                            client.Send(res);
                            break;
                        }
                    #endregion Welcome packet

                    #region Beep Packet
                    case 2:
                        {
                            string Beep = Encoding.Default.GetString(buffer, 2, 4);
                            if (Beep != "Beep")
                            {
                                Console.WriteLine($"[{IP}] Disconnection --> Invalid Beep..");
                                client.Disconnect();
                                return;
                            }
                            LastBeep = DateTime.Now;
                            if (!ConnectedClients.ContainsKey(IP))
                            {
                                ConnectedClients.TryAdd(IP, this);
                                Console.WriteLine($"[{IP}] Permission --> Can login.");
                            }
                            Console.WriteLine($"[{IP}] Valid --> Beep");
                            break;
                        }
                        #endregion
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{IP}] Disconnection --> Invalid Packet");
                client.Disconnect();
            }

        }
        public bool ValidateBuf(byte[] p1, byte[] p2)
        {
            for (int i = 0; i < p1.Length; i++)
                if (p1[i] != p2[i])
                    return false;
            return true;
        }
    }
}
