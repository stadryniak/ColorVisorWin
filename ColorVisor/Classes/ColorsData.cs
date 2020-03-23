using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

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
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}

