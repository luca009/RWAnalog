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
        float controllerValue = -1;
        float trainControlValue = -1;
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

        public void SetControllerValue(float controllerValue, float trainControlValue = -1)
        {
            this.controllerValue = controllerValue;
            this.trainControlValue = trainControlValue;

            gridControllerValue.Margin = new Thickness(GetPositionOnCanvas(new Point(0, 0), new Point(65535, 0),
                                                        new GraphPoint(controllerValue, 0)).X, 0, 0, 0);

            if (trainControlValue < 0)
            {
                gridTrainControlValue.Visibility = Visibility.Collapsed;
                return;
            }

            gridTrainControlValue.Visibility = Visibility.Visible;
            gridTrainControlValue.Margin = new Thickness(0, 0, 0, GetPositionOnCanvas(new Point(0, Minimum), new Point(65535, Maximum),
                                                        new GraphPoint(0, trainControlValue)).Y);
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

                previousPos = pos;
                previousControl = pointControl;
            }

            gridHorizontalZero.Margin = new Thickness(0, 0, 0, GetPositionOnCanvas(min, max, new GraphPoint(0, 0)).Y);
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
        }

        private Point GetPositionOnCanvas(Point min, Point max, GraphPoint point)
        {
            //Convert to 0-1 scale
            double x = (point.X) * (1 / (max.X));
            double y = (point.Y) * (1 / (max.Y));

            double rangeY = Math.Abs(Minimum - Maximum);
            double stepSizeY = CanvasHeight / rangeY;
            double stepsY = CanvasHeight / stepSizeY;
            double originY = rangeY / stepsY * Math.Abs(Minimum) * stepSizeY;

            x *= CanvasWidth;
            y = LinearRemap(y, 0, max.Y, originY, CanvasHeight);

            return new Point(x, y);
        }

        private GraphPoint GetPositionOnGraph(Point graphMin, Point graphMax, Point point)
        {
            double rangeY = Math.Abs(Minimum - Maximum);
            double stepSizeY = CanvasHeight / rangeY;
            double stepsY = CanvasHeight / stepSizeY;
            double originY = rangeY / stepsY * Math.Abs(Minimum) * stepSizeY;

            point.Y = LinearRemap(point.Y, originY, CanvasHeight, 0, graphMax.Y);

            return new GraphPoint((float)(point.X / CanvasWidth * 65535), (float)point.Y);
        }

        private double LinearRemap(double value, double originMin, double originMax, double resultMin, double resultMax)
        {
            return (value - originMin) * (resultMax - resultMin) / (originMax - originMin) + resultMin;
        }

        private void bAddPoint_Click(object sender, RoutedEventArgs e)
        {
            GraphPoint point = new GraphPoint(controllerValue, trainControlValue);
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
