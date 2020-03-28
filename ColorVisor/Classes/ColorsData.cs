using System;
using System.Collections.Generic;
using System.IO;

namespace ColorVisor.Classes
{
    /// <summary>
    /// Static class with method to load data from "data.csv" file and stores data in list.
    /// </summary>
    static class ColorsData
    {
        // List of colors loaded from data file
        public static List<AdvColor> Colors { get; }

        static ColorsData()
        {
            Colors = new List<AdvColor>();
        }

        /// <summary>
        /// Load data from "data.csv" file.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if data is already loaded</exception>
        public static void LoadData()
        {
            // check if colors are loaded
            if (Colors.Count != 0)
            {
                throw new InvalidOperationException("Data already loaded");
            }
            using var reader = new StreamReader(@"data.csv");
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line == null) break;
                string[] val = line.Split(",");
                byte[] bytes = StringToByteArray(val[1].Substring(1));
                Colors.Add(new AdvColor(bytes[0], bytes[1], bytes[2], val[0]));
            }
        }

        /// <summary>
        /// Converts hex string to byte array
        /// </summary>
        /// <param name="hex">Hex string eg. #F123AB</param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts numeric value of hex char
        /// </summary>
        /// <param name="hex"></param>
        /// <returns>Decimal value of hex</returns>
        private static int GetHexVal(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}

