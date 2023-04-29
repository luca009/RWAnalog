using System.Windows;
using System.Windows.Shapes;

namespace RWAnalog.Classes
{
    public static class DynamicLine
    {
        public static void RecalculateConnection(this Line line, double height, Point? point1 = null, Point? point2 = null)
        {
            if (line == null || point1 == null || point2 == null)
                return;

            if (point1 != null)
            {
                line.X1 = point1.Value.X;
                line.Y1 = height - point1.Value.Y;
            }
            if (point2 != null)
            {
                line.X2 = point2.Value.X;
                line.Y2 = height - point2.Value.Y;
            }
        }

        public static void RecalculateConnection(this Line line, Point point1, double height)
        {
            if (line == null || point1 == null)
                return;
            line.X1 = point1.X;
            line.Y1 = height - point1.Y;
        }
    }
}
