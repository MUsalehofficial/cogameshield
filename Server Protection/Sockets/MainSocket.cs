using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server_Protection.Sockets
{
    public class MainSocket
    {
        Socket sock;
        int Port;
        public MainSocket(int Port)
        {
            this.Port = Port;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(new IPEndPoint(IPAddress.Any, this.Port));
            sock.Listen(0);
            sock.BeginAccept(new AsyncCallback(BeginAccept), null);
        }

        private void BeginAccept(IAsyncResult ar)
        {
            try
            {
                Socket listener = sock.EndAccept(ar);
                try
                {
                    var client = new SocketWrapper(listener, this);
                    client.TryRec();
                }
                catch
                {
                    listener.Disconnect(false);
                }
                finally
                {
                    sock.BeginAccept(new AsyncCallback(BeginAccept), null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                sock.BeginAccept(new AsyncCallback(BeginAccept), null);
            }

        }
    }
}
