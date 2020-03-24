using System;
using System.Drawing;
using System.IO.Enumeration;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using ColorVisor.Classes;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Timer = System.Timers.Timer;

namespace ColorVisor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDc, int x, int y, int nWidth, int nHeight, IntPtr hSrcDc, int xSrc,
            int ySrc, int dwRop);

        private readonly AdvColor AdvColor = AdvColor.CreateInstance(Color.Black);
        private readonly Bitmap _screenPixel = new Bitmap(1, 1);

        public MainWindow()
        {
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            //Topmost = true;
            // Initialize data structure
            ColorsData.LoadData();

            InitializeComponent();
            // set text and button
            MyText.TextWrapping = TextWrapping.Wrap;
            MyText.IsReadOnly = true;
            MyText.BorderThickness = new Thickness(0);
            MyText.FontSize = 15;
            MyText.TextAlignment = TextAlignment.Center;

            // set button click listener
            TopButton.Click += SetTopmost;

            var timer = new Timer(100)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += ColorGetHandler;
        }

        private void SetTopmost(object sender, RoutedEventArgs routedEventArgs)
        {
            Topmost = !Topmost;
        }

        private void CalculateCloseColor(Color currentColor)
        {
            double res = 10000000000;
            AdvColor resColor = AdvColor.CreateInstance(Color.Black);
            AdvColor current = AdvColor.CreateInstance(currentColor);
            foreach (var color in ColorsData.Colors)
            {
                double tmp = ColorCalc.DeltaE2000(color, current);
                if (!(res > tmp)) continue;
                resColor = color;
                res = tmp;
                // System.Diagnostics.Debug.Print(res.ToString());
            }
            SetText(resColor.Name);
        }

        private void ColorGetHandler(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);
            var c = GetColorAt(cursor);
            CalculateCloseColor(c);
            AdvColor.Color = c;

            // SetText(AdvColor.Color);
            SetBackground(c);
        }

        private Color GetColorAt(Point location)
        {
            using (Graphics gdest = Graphics.FromImage(_screenPixel))
            {
                using var gsrc = Graphics.FromHwnd(IntPtr.Zero);
                IntPtr hSrcDc = gsrc.GetHdc();
                IntPtr hDc = gdest.GetHdc();
                int retval = BitBlt(hDc, 0, 0, 1, 1, hSrcDc, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                gdest.ReleaseHdc();
                gsrc.ReleaseHdc();
            }
            return _screenPixel.GetPixel(0, 0);
        }

        private void SetText(Color mColor)
        {
            Dispatcher.Invoke(() =>
                {
                    MyText.Text = mColor.R + " " + mColor.G + " " + mColor.B;
                }
            );
        }

        private void SetText(string str)
        {
            Dispatcher.Invoke(() => { MyText.Text = str; });
        }

        private void SetBackground(Color color)
        {
            Dispatcher.Invoke(() =>
            {
                var mColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                Background = new SolidColorBrush(mColor);
                if (ColorCalc.DeltaE2000(new AdvColor(color), new AdvColor(Color.Black)) < 30)
                {
                    MyText.Foreground = System.Windows.Media.Brushes.White;
                    TopButton.Foreground = System.Windows.Media.Brushes.White;
                }
                else
                {
                    MyText.Foreground = System.Windows.Media.Brushes.Black;
                    TopButton.Foreground = System.Windows.Media.Brushes.Black;
                }
                MyText.Background = new SolidColorBrush(mColor);
                TopButton.Background = new SolidColorBrush(mColor);
            });
        }
    }
}
