using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client_Protection
{
    class Program
    {
        static void WriteLog(string text = "", params object[] obj)
        {
            Console.WriteLine(text, obj);
            //if (!File.Exists("BLogs.logs"))
            //    File.Create("BLogs.logs");
            //using (var writer = new StreamWriter("BLogs.logs"))
            //{
            //    writer.WriteLine("[" + GetTime() + "]" + text, obj);
            //    writer.Close();
            //}
        }
        static byte[] ConquerHash, MagicHash, MyHash;
        static Socket Client;
        public static string
            MyProtection = "",
            ProtectedConquer = "",
            ServerIP = "127.0.0.1";
        public static int Port = 9230;
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
            if (args.Length == 0)
                ShowWindow(GetConsoleWindow(), SW_HIDE);
            using (var reader = new StreamReader(@"blizzard.cfg"))
            {
                string[] data = reader.ReadToEnd().Split('#');
                ServerIP = data[0];
                Port = int.Parse(data[1]);
            }
            byte[] buff = new byte[1];
            ConquerHash = GetFileHash(@"Conquer.exe");
            MagicHash = GetFileHash(@"ini\MagicEffect.ini");
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            while (true)
            {
                try
                {
                    Client.Connect(new IPEndPoint(IPAddress.Parse(ServerIP), Port));
                    Program.WriteLog("[Socket] Connected to Server!");
                    break;
                }
                catch
                {
                    Program.WriteLog("[Socket] Cannot connect --> Server is offline, Reconnecting in 10 seconds.");
                    Thread.Sleep(10000);
                }
            }

            Client.Send(Packets.WelcomePacket(ConquerHash, MagicHash));
            Client.Receive(buff, 0, buff.Length, SocketFlags.None);
            if (buff[0] == 10)
            {
                Client.Disconnect(false);
                Program.WriteLog("[Files Validation] Invalid client files.");
                Console.ReadLine();
                Environment.Exit(0);
                return;
            }
            else if (buff[0] == 20)
                Program.WriteLog("[Files Validation] Valid client files.");
            #region Launch Conquer
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory, "Conquer.exe"), "blacknull");
            p.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            p.Start();
            ProtectedConquer = p.MainModule.FileName;
            MyProtection = Process.GetCurrentProcess().MainModule.FileName;
            #endregion
            string pr = Process.GetCurrentProcess().ProcessName.Replace(".exe", "");
            var List = Process.GetProcessesByName(pr);
            Program.WriteLog("[Protection] Checking for running shield --> " + pr);
            if (List.Count() == 1)
            {
                Program.WriteLog("[Protection] Can`t find running shields...");
                Program.WriteLog("[Protection] Starting Protection thread.");
                Program.WriteLog();
                ThreadStamp = DateTime.Now;
                new Thread(new ThreadStart(Wrld)).Start();
            }
            else
            {
                Environment.Exit(0);
                return;
            }
            while (true)
                Console.ReadLine();
        }
        static DateTime ThreadStamp;
        static void Wrld()
        {
            while (true)
            {
                if (DateTime.Now > ThreadStamp.AddSeconds(10))
                {
                    ThreadStamp = DateTime.Now;
                    try
                    {
                        string currentPath = Process.GetCurrentProcess().MainModule.FileName.Replace(Process.GetCurrentProcess().ProcessName.ToString() + ".exe", "").ToLower();
                        var allProcceses = Process.GetProcesses();
                        bool found = false;


                        int skipped = 0;
                        foreach (var p in allProcceses)
                        {
                            try
                            {
                                string filename = p.MainModule.FileName;
                                string desc = p.MainModule.FileVersionInfo.FileDescription;
                                if (filename.ToLower().Contains(Environment.CurrentDirectory.ToLower()) || desc.ToLower().Contains("Conquer"))
                                    if ((filename.ToLower() != ProtectedConquer.ToLower()) &&
                                        (filename.ToLower() != MyProtection.ToLower()))
                                    {
                                        Program.WriteLog("[Killing] Process --> " + p.ProcessName);
                                        p.Kill();
                                    }
                                if (p.MainModule.FileName == ProtectedConquer)
                                    found = true;
                            }
                            catch
                            {
                                skipped++;
                                continue;
                            }

                        }
                        Program.WriteLog("[Process] Skipped --> " + skipped + " processes.");
                        if (!found)
                        {
                            Client.Disconnect(false);
                            Program.WriteLog("[Error] Can`t find the client, Exiting.. in 2 seconds.");
                            Thread.Sleep(2000);
                            Environment.Exit(0);
                        }

                        Client.Send(Packets.Beep());
                        Program.WriteLog("[Packets] Sent --> Beep on [" + GetTime() + "]");
                    }
                    catch (SocketException)
                    {

                        Program.WriteLog("[Socket] Connection is died.. Reconnecting....");
                        byte[] buff = new byte[1];
                        try
                        {
                            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            Client.Connect(new IPEndPoint(IPAddress.Parse(ServerIP), 9230));
                            Client.Send(Packets.WelcomePacket(ConquerHash, MagicHash));
                            Client.Receive(buff, 0, buff.Length, SocketFlags.None);
                            Program.WriteLog("[Socket] Reconnection --> Succeed..");
                            if (buff[0] == 10)
                            {
                                Client.Disconnect(false);
                                Program.WriteLog("[Files Validation] Invalid client files.");
                                Console.ReadLine();
                                Environment.Exit(0);
                                return;
                            }
                            else if (buff[0] == 20)
                                Program.WriteLog("[Files Validation] Valid client files.");
                        }
                        catch
                        {
                            Program.WriteLog("[Socket] Reconnection failed.");
                        }

                    }
                    catch (Exception e)
                    {
                        Program.WriteLog(e.ToString());
                    }
                }

            }
        }

        private static string GetTime()
        {
            return string.Format("{0}:{1}:{2}", DateTime.Now.Hour
                , DateTime.Now.Minute
                , DateTime.Now.Second);
        }

        static byte[] GetFileHash(string fileName)
        {
            HashAlgorithm sha1 = HashAlgorithm.Create();
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                return sha1.ComputeHash(stream);
        }
        public static void WriteByteArray(int pid, int address, params byte[] bytes)
        {
            IntPtr processHandle = OpenProcess(0x1F0FFF, false, pid);
            int bytesWritten = 0;
            WriteProcessMemory((int)processHandle, address, bytes, bytes.Length, ref bytesWritten);

        }
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress,
            byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
    }
}
