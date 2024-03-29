﻿using System.Collections.Generic;

namespace RWAnalog.Classes
{
    public class InputGraph
    {
        public List<GraphPoint> Points { get; set; }

        public InputGraph(bool initializePoints = true)
        {
            Points = new List<GraphPoint>();

            if (!initializePoints)
                return;

            Points.Add(new GraphPoint(0f, 0f));
            Points.Add(new GraphPoint(65535f, 1f));
        }

        public float Evaluate(float x)
        {
            Points.Sort((value1, value2) => { return value1.X.CompareTo(value2.X); });

            for (int i = 0; i < Points.Count - 1; i++)
            {
                GraphPoint pointLow = Points[i];
                GraphPoint pointHigh = Points[i + 1];

                if (pointLow.X <= x && x <= pointHigh.X)
                {
                    float result = lerp(pointLow.Y, pointHigh.Y, (x - pointLow.X) / (pointHigh.X - pointLow.X));
                    return result;
                }
            }

            return 0f;
        }

        private float lerp(float min, float max, float x)
        {
            return min * (1 - x) + max * x;
        }
    }

    public struct GraphPoint
    {
        public float X { get; }
        public float Y { get; }
        public GraphPoint(float x, float y) {  X = x; Y = y; }
    }
}
