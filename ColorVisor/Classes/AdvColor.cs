using System;
using System.Data;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace ColorVisor.Classes
{
    public class AdvColor
    {
        public static AdvColor CreateInstance(int r, int g, int b, string name)
        {
            return new AdvColor(r, g, b, name);
        }

        public static AdvColor CreateInstance(int r, int g, int b)
        {
            return new AdvColor(r, g, b);
        }

        public static AdvColor CreateInstance(Color color)
        {
            return new AdvColor(color);
        }

        public string Name { get; set; }
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

        private double[] _lab;
        public double[] Lab
        {
            get => _lab;
            set
            {
                _lab = value;
                LABToXYZ(_lab[0], _lab[1], _lab[2]);
                XyzToRgb(Xyz[0], Xyz[1], Xyz[2]);
            }
        }

        private const double XyzWhiteReferenceX = 95.047;
        private const double XyzWhiteReferenceY = 100;
        private const double XyzWhiteReferenceZ = 108.883;
        private const double XyzEpsilon = 0.008856;
        private const double XyzKappa = 903.3;

        public AdvColor(Color color)
        {
            _color = color;
            Xyz = new double[3];
            _lab = new double[3];
            RgbToXyz(color.R, color.G, color.B);
            XyzToLab(Xyz[0], Xyz[1], Xyz[2]);
        }

        public AdvColor(int r, int g, int b)
        {
            _color = Color.FromArgb(r, g, b);
            Xyz = new double[3];
            _lab = new double[3];
            RgbToXyz(r, g, b);
            XyzToLab(Xyz[0], Xyz[1], Xyz[2]);
        }

        public AdvColor(int r, int g, int b, string name)
        {
            _color = Color.FromArgb(r, g, b);
            Xyz = new double[3];
            _lab = new double[3];
            RgbToXyz(r, g, b);
            XyzToLab(Xyz[0], Xyz[1], Xyz[2]);
            this.Name = name;
        }

        public AdvColor(double l, double a, double b)
        {
            _lab = new[] { l, a, b };
            Xyz = new double[3];
            LABToXYZ(l, a, b);
            XyzToRgb(Xyz[0], Xyz[1], Xyz[2]);
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
            _lab[0] = Math.Max(0, 116 * y - 16);
            _lab[1] = 500 * (x - y);
            _lab[2] = 200 * (y - z);
        }

        private static double PivotXyzComponent(double component)
        {
            return component > XyzEpsilon ? Math.Pow(component, 1 / 3.0) : (XyzKappa * component + 16) / 116;
        }

        private void LABToXYZ(double l, double a, double b)
        {
            double fy = (l + 16) / 116;
            double fx = a / 500 + fy;
            double fz = fy - b / 200;

            double tmp = Math.Pow(fx, 3);
            double xr = tmp > XyzEpsilon ? tmp : (116 * fx - 16) / XyzKappa;
            double yr = l > XyzKappa * XyzEpsilon ? Math.Pow(fy, 3) : l / XyzKappa;

            tmp = Math.Pow(fz, 3);
            double zr = tmp > XyzEpsilon ? tmp : (116 * fz - 16) / XyzKappa;

            Xyz[0] = xr * XyzWhiteReferenceX;
            Xyz[1] = yr * XyzWhiteReferenceY;
            Xyz[2] = zr * XyzWhiteReferenceZ;
        }

        private void XyzToRgb(double x, double y, double z)
        {
            double r = (x * 3.2406 + y * -1.5372 + z * -0.4986) / 100;
            double g = (x * -0.9689 + y * 1.8758 + z * 0.0415) / 100;
            double b = (x * 0.0557 + y * -0.2040 + z * 1.0570) / 100;

            r = r > 0.0031308 ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r;
            g = g > 0.0031308 ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g;
            b = b > 0.0031308 ? 1.055 * Math.Pow(b, 1 / 2.4) - 0.055 : 12.92 * b;
            r = Math.Round(r * 255);
            g = Math.Round(g * 255);
            b = Math.Round(b * 255);
            r = r > 255 ? 255 : r;
            r = r < 0 ? 0 : r;
            g = g > 255 ? 255 : g;
            g = g < 0 ? 0 : g;
            b = b > 255 ? 255 : b;
            b = b < 0 ? 0 : b;

            _color = Color.FromArgb((int)r, (int)g, (int)b);
        }
    }
}
