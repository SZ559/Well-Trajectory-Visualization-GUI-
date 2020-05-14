using System;

namespace GeometricObject
{
    public struct PointIn2D
    {
        public Single X
        {
            get; set;
        }

        public Single Y
        {
            get; set;
        }
        public string Annotation
        {
            get; set;
        }

        public PointIn2D(Single x, Single y, string annotation = "")
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