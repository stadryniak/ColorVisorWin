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
            while (!reader.EndOfStream)
            {
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

            var arr = new byte[hex.Length >> 1];
            for (var i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }
            return arr;
        }

        private static int GetHexVal(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}

