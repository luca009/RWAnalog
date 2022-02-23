using RWAnalog.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for GraphEditor.xaml
    /// </summary>
    public partial class GraphEditor : UserControl
    {
        public double CanvasWidth { get; set; }
        public double CanvasHeight { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Range {
            get
            {
                return Math.Abs(Minimum - Maximum);
            }
        }
        Point min;
        Point max;
        double ellipseRadius = 10;
        List<GraphPoint> points = new List<GraphPoint>();

        public GraphEditor()
        {
            InitializeComponent();
        }

        public void UpdateCanvas()
        {
            foreach (UIElement element in canvasGraphContent.Children)
            {
                if (element.GetType() != typeof(PointControl))
                    continue;

                (element as PointControl).CanvasWidth = canvasGraphContent.ActualWidth;
                (element as PointControl).CanvasHeight = canvasGraphContent.ActualHeight;
            }

            //canvasGraphContent.Height = CanvasHeight * Range;
            //canvasScaleTransform.ScaleY = 1 / Range;
            //canvasTranslateTransform.Y = -(CanvasHeight / Range);
        }

        public void SetControllerValue(float value)
        {
            gridControllerValue.Margin = new Thickness(GetPositionOnCanvas(new Point(0, 0), new Point(65535, 0), new GraphPoint(value, 0)).X, 0, 0, 0);
        }

        public void SetPoints(List<GraphPoint> points, double width, double height)
        {
            canvasGraphContent.Children.Clear();
            this.points = points;

            CanvasWidth = width;
            CanvasHeight = height;

            min = new Point();
            max = new Point();

            min.X = points.Min((x) => { return x.X; });
            min.Y = points.Min((x) => { return x.Y; });
            max.X = points.Max((x) => { return x.X; });
            max.Y = points.Max((x) => { return x.Y; });

            points.Sort((value1, value2) => { return value1.X.CompareTo(value2.X); });

            Point previousPos = new Point();
            PointControl previousControl = new PointControl();
            for (int i = 0; i < points.Count; i++)
            {
                Point pos = GetPositionOnCanvas(min, max, points[i]);
                PointControl pointControl = new PointControl() { Width = ellipseRadius, Height = ellipseRadius };

                if (points[i].X == 0 || points[i].X == 65535)
                {
                    pointControl.LockX = true;
                    pointControl.LockDelete = true;
                }

                pointControl.CanvasWidth = width;
                pointControl.CanvasHeight = height;
                canvasGraphContent.Children.Add(pointControl);
                Canvas.SetLeft(pointControl, pos.X - ellipseRadius / 2);
                Canvas.SetBottom(pointControl, pos.Y - ellipseRadius / 2);

                pointControl.UpdatePosition += point_UpdatePosition;
                pointControl.Delete += point_Delete;

                //if (i > 0)
                //{
                //    Line connectingLine = new Line() {
                //        Stroke = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                //        StrokeThickness = 2
                //    };
                //    previousControl.AssociatedLines.Add(connectingLine);
                //    pointControl.AssociatedLines.Add(connectingLine);
                //    connectingLine.RecalculateConnection(point1: previousPos, point2: pos, height: CanvasHeight);
                //    canvasGraphContent.Children.Add(connectingLine);
                //}

                previousPos = pos;
                previousControl = pointControl;
            }

            gridHorizontalZero.Margin = new Thickness(0, 0, 0, GetPositionOnCanvas(min, max, new GraphPoint(0, 0)).Y);

            //DebugRun();
        }

        public List<GraphPoint> GetPoints()
        {
            List<GraphPoint> returnPoints = new List<GraphPoint>();
            foreach (UIElement element in canvasGraphContent.Children)
            {
                if (element.GetType() != typeof(PointControl)) continue;

                Point point = (element as PointControl).Point;
                returnPoints.Add(GetPositionOnGraph(new Point(0, Minimum), new Point(65535, Maximum), new Point(point.X, point.Y)));
            }
            return returnPoints;
        }

        private void point_UpdatePosition(PointControl sender, double x, double y, Point oldPoint)
        {
            Canvas.SetLeft(sender, x);
            Canvas.SetBottom(sender, y);

            //if (lineTaskRunning)
            //    return;

            //lineTaskRunning = true;

            //List<int> removeIndexes = new List<int>();
            //for (int i = 0; i < canvasGraphContent.Children.Count; i++)
            //{
            //    if (canvasGraphContent.Children[i].GetType() != typeof(Line)) continue;

            //    removeIndexes.Add(i);
            //}

            //for (int i = 0; i < removeIndexes.Count; i++)
            //{
            //    try
            //    {
            //        canvasGraphContent.Children.RemoveAt(i);
            //    }
            //    catch (Exception) { }
            //}

            //for (int i = 0; i < points.Count; i++)
            //{
            //    if (i > 0)
            //    {
            //        Line connectingLine = new Line()
            //        {
            //            Stroke = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
            //            StrokeThickness = 2
            //        };
            //        connectingLine.RecalculateConnection(point1: points[i - 1], point2: points[i], height: CanvasHeight);
            //        canvasGraphContent.Children.Add(connectingLine);
            //    }
            //}

            //lineTaskRunning = false;
        }

        private Point GetPositionOnCanvas(Point min, Point max, GraphPoint point)
        {
            //Cast to 0-1 range
            //double rangeX = Math.Abs(min.X - max.X);
            //double rangeY = Math.Abs(min.Y - max.Y);

            //min.X /= rangeX;
            //min.X *= 2;
            //min.Y /= rangeY;
            //min.Y *= 2;
            //output.X = point.X / rangeX + min.X;
            //output.Y = point.Y / rangeY + min.Y;

            ////Cast to canvas range
            //double newMin = Minimum;
            //newMin /= 2 + Minimum;
            //newMin *= height;

            //output.X *= width;
            //output.Y *= (point.Y * height - newMin) / 2;
            //Convert to 0-1 scale
            double x = (point.X) * (1 / (max.X));
            double y = (point.Y) * (1 / (max.Y));

            double rangeY = Math.Abs(Minimum - Maximum);
            double stepSizeY = CanvasHeight / rangeY;
            double stepsY = CanvasHeight / stepSizeY;
            double originY = rangeY / stepsY * Math.Abs(Minimum) * stepSizeY;

            x *= CanvasWidth;
            y = LinearRemap(y, 0, max.Y, originY, CanvasHeight);

            //y *= CanvasHeight;

            return new Point(x, y);
        }

        private GraphPoint GetPositionOnGraph(Point graphMin, Point graphMax, Point point)
        {
            double rangeY = Math.Abs(Minimum - Maximum);
            double stepSizeY = CanvasHeight / rangeY;
            double stepsY = CanvasHeight / stepSizeY;
            double originY = rangeY / stepsY * Math.Abs(Minimum) * stepSizeY;

            point.Y = LinearRemap(point.Y, originY, CanvasHeight, 0, graphMax.Y);

            //point.Y = LinearRemap(point.Y, Minimum, Maximum, graphMin.Y, graphMax.Y);

            //float x;
            //float y;

            ////Cast to 0-1 range
            //graphMin.Y /= height;
            //graphMin.Y *= 2;

            //point.X /= width;
            //point.Y = point.Y / height + graphMin.Y;

            ////Cast to graph range
            //double rangeX = Math.Abs(graphMin.X - graphMax.X);
            //double rangeY = Math.Abs(graphMin.Y - graphMax.Y);

            //graphMin.X /= 2;
            //graphMin.X *= rangeX;
            //graphMin.Y /= 2;
            //graphMin.Y *= rangeY;
            //x = (float)(point.X * rangeX - graphMin.X);
            //y = (float)(point.Y * rangeY - graphMin.Y);

            //point.X /= width;
            //point.Y /= height;


            //double graphRangeY = Math.Abs(graphMin.Y - graphMax.Y);

            //point.Y = LinearRemap(point.Y, 0, Maximum, graphMin.Y, graphMax.Y);
            //point.Y = point.Y / 2 + (rangeY / 2);

            //point.X = point.X * (1 / graphMax.X);
            //point.Y = point.Y * (1 / graphMax.Y);

            //point.X /= CanvasWidth;
            //point.Y /= CanvasHeight;

            return new GraphPoint((float)(point.X / CanvasWidth * 65535), (float)point.Y);
        }

        private double LinearRemap(double value, double originMin, double originMax, double resultMin, double resultMax)
        {
            return (value - originMin) * (resultMax - resultMin) / (originMax - originMin) + resultMin;
        }

        private void bAddPoint_Click(object sender, RoutedEventArgs e)
        {
            GraphPoint point = new GraphPoint(32768, 0.5f);
            points.Add(point);

            Point pos = GetPositionOnCanvas(min, max, point);
            PointControl pointControl = new PointControl() { Width = ellipseRadius, Height = ellipseRadius };

            pointControl.CanvasWidth = CanvasWidth;
            pointControl.CanvasHeight = CanvasHeight;
            canvasGraphContent.Children.Add(pointControl);
            Canvas.SetLeft(pointControl, pos.X - ellipseRadius / 2);
            Canvas.SetBottom(pointControl, pos.Y - ellipseRadius / 2);

            pointControl.UpdatePosition += point_UpdatePosition;
            pointControl.Delete += point_Delete;
        }

        private void point_Delete(PointControl sender)
        {
            int index = canvasGraphContent.Children.IndexOf(sender);
            if (index < 0)
                return;

            canvasGraphContent.Children.RemoveAt(index);
        }
    }
}
