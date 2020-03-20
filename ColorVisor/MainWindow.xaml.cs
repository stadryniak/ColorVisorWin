using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc,
            int ySrc, int dwRop);

        private readonly Timer timer;

        public MainWindow()
        {
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;

            InitializeComponent();
            myText.TextWrapping = TextWrapping.Wrap;
            myText.IsReadOnly = true;
            myText.BorderThickness = new Thickness(0);
            myText.FontSize = 20;

            timer = new Timer(100)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += MouseCheck;
        }


        private void MouseCheck(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);
            var c = GetColorAt(cursor);
            SetText(c);
            SetBackground(c);
        }

        readonly Bitmap screenPixel = new Bitmap(1, 1);

        public Color GetColorAt(Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero);
                IntPtr hSrcDC = gsrc.GetHdc();
                IntPtr hDC = gdest.GetHdc();
                int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                gdest.ReleaseHdc();
                gsrc.ReleaseHdc();
            }
            return screenPixel.GetPixel(0, 0);
        }

        private void SetText(Color mColor)
        {
            Dispatcher.Invoke(() =>
                {
                    myText.Text = mColor.R + " " + mColor.G + " " + mColor.B;
                }
            );
        }

        private void SetBackground(Color color)
        {
            Dispatcher.Invoke(() =>
            {
                System.Windows.Media.Color mColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                Background = new SolidColorBrush(mColor);
                myText.Background = new SolidColorBrush(mColor);
            });
        }
    }
}
