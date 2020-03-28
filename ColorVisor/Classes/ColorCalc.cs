using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]

namespace ColorVisor.Classes
{
    /// <summary>
    /// Static class containing method to calculate dE2000 color distance
    /// </summary>
    static class ColorCalc
    {
        static readonly double TOLERANCE = 1e-7;

        /// <summary>
        /// Calculates dE2000 distance betweeen colors
        /// Reference:
        /// http://www2.ece.rochester.edu/~gsharma/ciede2000/ciede2000noteCRNA.pdf
        /// </summary>
        /// <param name="color1">Color 1 to compare</param>
        /// <param name="color2">Color 2 to compare</param>
        /// <returns></returns>
        public static double DeltaE2000(AdvColor color1, AdvColor color2)
        {
            // Setting variables
            double[] lab1 = color1.Lab;
            double[] lab2 = color2.Lab;

            double l1 = lab1[0];
            double a1 = lab1[1];
            double b1 = lab1[2];

            double l2 = lab2[0];
            double a2 = lab2[1];
            double b2 = lab2[2];

            //step 1:
            // C*ab = Sqrt(a* ^ 2 + b* ^ 2)
            double cs1 = Math.Sqrt(a1 * a1 + b1 * b1);
            double cs2 = Math.Sqrt(a2 * a2 + b2 * b2);
            // C*Average = (C*1 + C*2)/2
            double csAverage = (cs1 + cs2) / 2;
            // G
            double csAverage7 = Math.Pow(csAverage, 7);
            double g = (1 - Math.Sqrt(csAverage7 / (csAverage7 + 6103515625.0))) / 2;
            // a' = (1 + G) * a*
            double ap1 = (1 + g) * a1;
            double ap2 = (1 + g) * a2;
            // C' = Sqrt(a' ^ 2 + b* ^ 2)
            double cp1 = Math.Sqrt(ap1 * ap1 + b1 * b1);
            double cp2 = Math.Sqrt(ap2 * ap2 + b2 * b2);
            // h' = atan2(b*, a')
            double hp1;
            if (Math.Abs(b1) < TOLERANCE && Math.Abs(ap1) < TOLERANCE)
            {
                hp1 = 0;
            }
            else
            {
                hp1 = ConvertToDegrees(Math.Atan2(b1, ap1));
                hp1 = (hp1 + 360) % 360;
            }

            double hp2;
            if (Math.Abs(b2) < TOLERANCE && Math.Abs(ap2) < TOLERANCE)
            {
                hp2 = 0;
            }
            else
            {
                hp2 = ConvertToDegrees(Math.Atan2(b2, ap2));
                hp2 = (hp2 + 360) % 360;
            }

            //step 2
            // dLl
            double dLp = l2 - l1;
            // dCp
            double dCp = cp2 - cp1;
            // dhp
            double dhp;
            double cp1Cp2 = cp1 * cp2;
            if (Math.Abs(cp1Cp2) < TOLERANCE)
            {
                dhp = 0;
            }
            else if (Math.Abs(hp2 - hp1) <= 180)
            {
                dhp = hp2 - hp1;
            }
            else if ((hp2 - hp1) > 180)
            {
                dhp = (hp2 - hp1) - 360;
            }
            else if ((hp2 - hp1) < -180)
            {
                dhp = (hp2 - hp1) + 360;
            }
            else
            {
                return -1;
            }

            // dHp
            double dHp = 2 * Math.Sqrt(cp1Cp2) * Math.Sin(ConvertToRadians(dhp / 2));

            //step 3
            // L'Average
            double lpAverage = (l1 + l2) / 2;
            // C'Average
            double cpAverage = (cp1 + cp2) / 2;
            // h'Average
            double hpAverage;
            if (Math.Abs(cp1Cp2) < TOLERANCE)
            {
                hpAverage = hp1 + hp2;
            }
            else if (Math.Abs(hp1 - hp2) <= 180)
            {
                hpAverage = (hp1 + hp2) / 2;
            }
            else if (Math.Abs(hp1 - hp2) > 180 && hp1 + hp2 < 360)
            {
                hpAverage = (hp1 + hp2 + 360) / 2;
            }
            else if (Math.Abs(hp1 - hp2) > 180 && hp1 + hp2 >= 360)
            {
                hpAverage = (hp1 + hp2 - 360) / 2;
            }
            else
            {
                return -1;
            }

            //T
            double T = 1 - 0.17 * Math.Cos(ConvertToRadians(hpAverage - 30)) +
                       0.24 * Math.Cos(ConvertToRadians(2 * hpAverage)) +
                       0.32 * Math.Cos(ConvertToRadians(3 * hpAverage + 6)) -
                       0.2 * Math.Cos(ConvertToRadians(4 * hpAverage - 63));
            // dTheta
            double dTheta = 30 * Math.Exp(-1 * ((hpAverage - 275) / 25) * ((hpAverage - 275) / 25));
            // Rc
            double cpAverage7 = Math.Pow(cpAverage, 7);
            double rc = 2 * Math.Sqrt(cpAverage7 / (cpAverage7 + 6103515625.0));
            // Sl
            double lpAverageMinus50Square = (lpAverage - 50) * (lpAverage - 50);
            double sl = 1 + (0.015 * lpAverageMinus50Square) / (Math.Sqrt(20 + lpAverageMinus50Square));
            // Sc
            double sc = 1 + 0.045 * cpAverage;
            // Sh
            double sh = 1 + 0.015 * cpAverage * T;
            //Rt
            double rt = -1 * Math.Sin(ConvertToRadians(2 * dTheta)) * rc;

            //Finally
            return Math.Sqrt((dLp / sl) * (dLp / sl) + (dCp / sc) * (dCp / sc) + (dHp / sh) * (dHp / sh) +
                             rt * (dCp / sc) * (dHp / sh));
        }

        private static double ConvertToDegrees(double radians)
        {
            return (180 / Math.PI) * radians;
        }

        private static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}
