using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using ColorVisor.Classes;
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
        private static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int BitBlt(IntPtr hDc, int x, int y, int nWidth, int nHeight, IntPtr hSrcDc, int xSrc,
            int ySrc, int dwRop);

        private readonly AdvColor _advColor = AdvColor.CreateInstance(Color.Black);
        private readonly Bitmap _screenPixel = new Bitmap(1, 1);

        public MainWindow()
        {
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
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
            // set getting color of pixel, calculate closest color and set text every 100ms
            var timer = new Timer(100)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += ColorGetHandler;
        }

        /// <summary>
        /// Sets window topmost property to opposite of current
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void SetTopmost(object sender, RoutedEventArgs routedEventArgs)
        {
            Topmost = !Topmost;
        }

        /// <summary>
        /// Calculates closest color, sets text and backgrounds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsedEventArgs"></param>
        private void ColorGetHandler(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var cursor = new Point();
            GetCursorPos(ref cursor);
            Color c = GetColorAt(cursor);
            string resColorText = CalculateCloseColor(c);
            SetText(resColorText);
            _advColor.Color = c;
            SetBackgrounds(c);
        }

        /// <summary>
        /// Calculate closest color name and return it as string
        /// </summary>
        /// <param name="currentColor"></param>
        /// <returns>Closest color name</returns>
        private string CalculateCloseColor(Color currentColor)
        {
            double res = double.MaxValue;
            var resColor = AdvColor.CreateInstance(Color.Black);
            var current = AdvColor.CreateInstance(currentColor);
            foreach (var color in ColorsData.Colors)
            {
                double tmp = ColorCalc.DeltaE2000(color, current);
                if (!(res > tmp)) continue;
                resColor = color;
                res = tmp;
            }
            return resColor.Name;
        }

        /// <summary>
        /// Gets pixel Color at given location
        /// </summary>
        /// <param name="location"></param>
        /// <returns>Color at location</returns>
        private Color GetColorAt(Point location)
        {
            using (var gdest = Graphics.FromImage(_screenPixel))
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

        /// <summary>
        /// Sets text in window's TextBox
        /// </summary>
        /// <param name="str"></param>
        private void SetText(string str)
        {
            Dispatcher.Invoke(() => { MyText.Text = str; });
        }

        /// <summary>
        /// Sets background of TextBox and Button to Color. Sets text color of TextBox and Button to black/white depending on input color.
        /// </summary>
        /// <param name="color"></param>
        private void SetBackgrounds(Color color)
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
