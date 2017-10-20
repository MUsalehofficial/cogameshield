using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Client_Protection
{
    public class Packets
    {
        public static byte[] WelcomePacket(byte[] ConquerHash, byte[] MagicHash)
        {
            byte[] b = new byte[100];
            int o = 0;
            Writer.WriteUInt16(1, o, b); o += 2;
            Writer.WriteString("BlizzardConquer", o, b); o += 16;
            Writer.WriteString(GetHwid(), o, b); o += 32;
            for (int i = 0; i < 20; i++)
                b[o++] = ConquerHash[i];
            for (int i = 0; i < 20; i++)
                b[o++] = MagicHash[i];
            return b;
        }
        public static string GetHwid()
        {
            var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
            ManagementObjectCollection mbsList = mbs.Get();
            string id = "";
            foreach (ManagementObject mo in mbsList)
            {
                id = mo["ProcessorId"].ToString();
                break;
            }
            return id;
        }

        public static byte[] Beep()
        {
            byte[] b = new byte[6];
            Writer.WriteUInt16(2, 0, b);
            Writer.WriteString("Beep", 2, b);
            return b;
        }
    }
}
