using Server_Protection.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Protection
{
    class Program
    {
        public static byte[] ConquerHash, MagicHash;
        static void Main(string[] args)
        {
            int Port = 9230;
            var server = new MainSocket(Port);
            // Intialize the hashes...
            ConquerHash = GetFileHash(@"Files\Conquer.exe");
            MagicHash = GetFileHash(@"Files\ini\MagicEffect.ini");
            Console.WriteLine($"Socket is alive on port {Port}");
            Console.Title = $"Protection Server - {Port}";

            Console.WriteLine();
            new Thread(new ThreadStart(Protection)).Start();

            while (true)
                Console.ReadLine();
        }
        private static void Protection()
        {
            while (true)
            {
                try
                {
                    foreach (var clients in SocketWrapper.ConnectedClients.Values)
                    {
                        if (DateTime.Now > clients.LastBeep.AddMinutes(2))
                        {
                            // Shield has been killed...
                            clients.Disconnect();
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        static byte[] GetFileHash(string fileName)
        {
            HashAlgorithm sha1 = HashAlgorithm.Create();
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                return sha1.ComputeHash(stream);
        }
        
    }
}
