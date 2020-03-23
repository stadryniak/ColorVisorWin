using System;
using System.Drawing;

namespace ColorVisor
{
    class AdvColor
    {
        public static AdvColor CreateInstance2(int r, int g, int b, string name)
        {
            return new AdvColor(r, g, b, name);
        }

        public static AdvColor CreateInstance1(int r, int g, int b)
        {
            return new AdvColor(r, g, b);
        }

        public static AdvColor CreateInstance(Color color)
        {
            return new AdvColor(color);
        }

        public string name { get; set; }
        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                RgbToXyz(_color.R, _color.G, _color.B);
                XyzToLab(Xyz[0], Xyz[1], Xyz[2]);
            }
        }
        public double[] Xyz { get; }
        public double[] Lab { get; }

        private const double XyzWhiteReferenceX = 95.047;
        private const double XyzWhiteReferenceY = 100;
        private const double XyzWhiteReferenceZ = 108.883;
        private const double XyzEpsilon = 0.008856;
        private const double XyzKappa = 903.3;

        public AdvColor(Color color)
        {
            _color = color;
            Xyz = new double[3];
            Lab = new double[3];
            RgbToXyz(color.R, color.G, color.B);
            XyzToLab(Xyz[0], Xyz[1], Xyz[2]);
        }

        public AdvColor(int r, int g, int b)
        {
            _color = Color.FromArgb(r, g, b);
            Xyz = new double[3];
            Lab = new double[3];
            RgbToXyz(r, g, b);
            XyzToLab(Xyz[0], Xyz[1], Xyz[2]);
        }

        public AdvColor(int r, int g, int b, string name)
        {
            _color = Color.FromArgb(r, g, b);
            Xyz = new double[3];
            Lab = new double[3];
            RgbToXyz(r, g, b);
            XyzToLab(Xyz[0], Xyz[1], Xyz[2]);
            this.name = name;
        }

        private void RgbToXyz(int r, int g, int b)
        {
            double sr = r / 255.0;
            sr = sr < 0.04045 ? sr / 12.92 : Math.Pow((sr + 0.055) / 1.055, 2.4);
            double sg = g / 255.0;
            sg = sg < 0.04045 ? sg / 12.92 : Math.Pow((sg + 0.055) / 1.055, 2.4);
            double sb = b / 255.0;
            sb = sb < 0.04045 ? sb / 12.92 : Math.Pow((sb + 0.055) / 1.055, 2.4);

            Xyz[0] = 100 * (sr * 0.4124 + sg * 0.3576 + sb * 0.1805);
            Xyz[1] = 100 * (sr * 0.2126 + sg * 0.7152 + sb * 0.0722);
            Xyz[2] = 100 * (sr * 0.0193 + sg * 0.1192 + sb * 0.9505);
        }

        private void XyzToLab(double x, double y, double z)
        {
            x = PivotXyzComponent(x / XyzWhiteReferenceX);
            y = PivotXyzComponent(y / XyzWhiteReferenceY);
            z = PivotXyzComponent(z / XyzWhiteReferenceZ);
            Lab[0] = Math.Max(0, 116 * y - 16);
            Lab[1] = 500 * (x - y);
            Lab[2] = 200 * (y - z);
        }

        private static double PivotXyzComponent(double component)
        {
            return component > XyzEpsilon ? Math.Pow(component, 1 / 3.0) : (XyzKappa * component + 16) / 116;
        }
    }
}
