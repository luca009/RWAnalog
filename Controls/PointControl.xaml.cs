using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RWAnalog.Controls
{
    /// <summary>
    /// Interaction logic for PointControl.xaml
    /// </summary>
    public partial class PointControl : UserControl
    {
        public delegate void UpdatePositionDelegate(PointControl sender, double x, double y, Point oldPoint);
        public event UpdatePositionDelegate UpdatePosition;
        public delegate void DeleteDelegate(PointControl sender);
        public event DeleteDelegate Delete;
        public bool LockX { get; set; }
        public bool LockDelete { get; set; }
        public double CanvasWidth { get; set; }
        public double CanvasHeight { get; set; }
        public Point Point {
            get
            {
                return new Point(Canvas.GetLeft(this) + 5, Canvas.GetBottom(this) + 5);
            }
        }
        public Point RawPoint
        {
            get
            {
                return new Point(Canvas.GetLeft(this), Canvas.GetBottom(this));
            }
        }
        public List<Line> AssociatedLines { get; set; }
        bool mouseDown = false;
        Point dragOffset;
        double originX;
        double originY;

        public PointControl()
        {
            InitializeComponent();
            AssociatedLines = new List<Line>();
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
            dragOffset = e.GetPosition(gridContent);
            originX = Canvas.GetLeft(this);
            originY = Canvas.GetBottom(this);
            ((Ellipse)sender).CaptureMouse();
        }

        private void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            ((Ellipse)sender).ReleaseMouseCapture();
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown || UpdatePosition == null)
                return;

            Point mousePos = e.GetPosition(gridContent);
            mousePos.X += Canvas.GetLeft(this) - originX;
            mousePos.Y -= Canvas.GetBottom(this) - originY;
            double x = originX + mousePos.X - dragOffset.X;
            double y = originY - mousePos.Y + dragOffset.Y;


            if (LockX)
                UpdatePosition(this, originX, Clamp(y, 0 - Height / 2, CanvasHeight - Height / 2), RawPoint);
            else
                UpdatePosition(this, Clamp(x, 0 - Width / 2, CanvasWidth + Width / 2), Clamp(y, 0 - Height / 2, CanvasHeight - Height / 2), RawPoint);
        }

        private double Clamp(double x, double min, double max)
        {
            if (x < min)
                return min;
            if (x > max)
                return max;
            return x;
        }

        private void Ellipse_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Delete != null && !LockDelete)
                Delete(this);
        }
    }
}
