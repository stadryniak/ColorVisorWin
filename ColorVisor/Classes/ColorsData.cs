using System;
using System.Collections.Generic;
using System.IO;

namespace ColorVisor.Classes
{
    static class ColorsData
    {
        public static List<AdvColor> Colors { get; set; }

        static ColorsData()
        {
            Colors = new List<AdvColor>();
        }
        public static void LoadData()
        {
            using var reader = new StreamReader(@"data.csv");
            int c = 0;
            while (!reader.EndOfStream)
            {
                if (c == 270)
                {
                    Console.WriteLine(c);
                }
                c++;
                var line = reader.ReadLine();
                if (line == null) break;
                var val = line.Split(",");
                var bytes = StringToByteArray(val[1].Substring(1));
                Colors.Add(new AdvColor(bytes[0], bytes[1], bytes[2], val[0]));
            }
        }

        private static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}

