using System;
using System.Collections.Generic;
using System.Text;

namespace ColorVisor
{
    static class ColorCalc
    {
        static readonly double TOLERANCE = 1e-7;

        public static double DeltaE2000(AdvColor color_1, AdvColor color_2)
        {
            double[] lab1 = color_1.Lab;
            double[] lab2 = color_2.Lab;

            double L1 = lab1[0];
            double a1 = lab1[1];
            double b1 = lab1[2];

            double L2 = lab2[0];
            double a2 = lab2[1];
            double b2 = lab2[2];

            //step 1:
            // C*ab = Sqrt(a* ^ 2 + b* ^ 2)
            double CS_1 = Math.Sqrt(a1 * a1 + b1 * b1);
            double CS_2 = Math.Sqrt(a2 * a2 + b2 * b2);
            // C*Average = (C*1 + C*2)/2
            double CSAverage = (CS_1 + CS_2) / 2;
            // G
            double CSAverage7 = Math.Pow(CSAverage, 7);
            double G = (1 - Math.Sqrt(CSAverage7 / (CSAverage7 + 6103515625.0))) / 2;
            // a' = (1 + G) * a*
            double ap1 = (1 + G) * a1;
            double ap2 = (1 + G) * a2;
            // C' = Sqrt(a' ^ 2 + b* ^ 2)
            double Cp1 = Math.Sqrt(ap1 * ap1 + b1 * b1);
            double Cp2 = Math.Sqrt(ap2 * ap2 + b2 * b2);
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
            double dLp = L2 - L1;
            // dCp
            double dCp = Cp2 - Cp1;
            // dhp
            double dhp;
            double Cp1Cp2 = Cp1 * Cp2;
            if (Cp1Cp2 == 0)
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
            double dHp = 2 * Math.Sqrt(Cp1Cp2) * Math.Sin(ConvertToDegrees(dhp / 2));

            //step 3
            // L'Average
            double LpAverage = (L1 + L2) / 2;
            // C'Average
            double CpAverage = (Cp1 + Cp2) / 2;
            // h'Average
            double hpAverage;
            if (Cp1Cp2 == 0)
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
            double CpAverage7 = Math.Pow(CpAverage, 7);
            double Rc = 2 * Math.Sqrt(CpAverage7 / (CpAverage7 + 6103515625.0));
            // Sl
            double LpAverageMinus50Square = (LpAverage - 50) * (LpAverage - 50);
            double Sl = 1 + (0.015 * LpAverageMinus50Square) / (Math.Sqrt(20 + LpAverageMinus50Square));
            // Sc
            double Sc = 1 + 0.045 * CpAverage;
            // Sh
            double Sh = 1 + 0.015 * CpAverage * T;
            //Rt
            double Rt = -1 * Math.Sin(ConvertToRadians(2 * dTheta)) * Rc;

            //Finally
            return Math.Sqrt((dLp / Sl) * (dLp / Sl) + (dCp / Sc) * (dCp / Sc) + (dHp / Sh) * (dHp / Sh) +
                             Rt * (dCp / Sc) * (dHp / Sh));
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
