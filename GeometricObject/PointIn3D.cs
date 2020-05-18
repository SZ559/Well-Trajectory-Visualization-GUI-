
namespace GeometricObject
{
    public struct PointIn3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public PointIn3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool IsEqual(PointIn3D point)
        {
            return this.X == point.X && this.Y == point.Y && this.Z == point.Z;
        }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}, Z: {2}", X, Y, Z);
        }
    }
}