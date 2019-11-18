﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MitmDofus
{
    public static class Hash
    {
        public static char[] CharsArray = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F',
            'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V',
            'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'
        };

        public static string DecryptIp(string packet)
        {
            StringBuilder ip = new StringBuilder();

            for (int i = 0; i < 8; i += 2)
            {
                int ascii1 = packet[i] - 48;
                int ascii2 = packet[i + 1] - 48;

                if (i != 0)
                    ip.Append('.');

                ip.Append(((ascii1 & 15) << 4) | (ascii2 & 15));
            }
            return ip.ToString();
        }

        public static int DecryptPort(char[] chars)
        {
            if (chars.Length != 3)
                throw new ArgumentOutOfRangeException("The port must be 3-chars coded.");

            int port = 0;
            for (int i = 0; i < 2; i++)
                port += (int)(Math.Pow(64, 2 - i) * GetHash(chars[i]));

            port += GetHash(chars[2]);
            return port;
        }

        public static short GetHash(char ch)
        {
            for (short i = 0; i < CharsArray.Length; i++)
                if (CharsArray[i] == ch)
                    return i;

            throw new IndexOutOfRangeException(ch + " is not in the hash table.");
        }
    }
}
