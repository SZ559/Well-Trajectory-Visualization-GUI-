using System;

namespace GeometricObject
{
    public struct PointIn2D
    {
        public double X
        {
            get; set;
        }

        public double Y
        {
            get; set;
        }
        public string Annotation
        {
            get; set;
        }

        public PointIn2D(double x, double y, string annotation = "")
        {
            X = x;
            Y = y;
            Annotation = annotation;
        }
                    
        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }

        public bool IsEqual(PointIn2D p)
        {
            return X == p.X && Y == p.Y;
        }

    }

}