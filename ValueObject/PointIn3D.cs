using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Numerics;

namespace ValueObject
{
    [BsonIgnoreExtraElements]
    public class PointIn3D
    {
        [BsonElement("x")]
        public float X { get; set; }

        [BsonElement("y")]
        public float Y { get; set; }

        [BsonElement("z")]
        public float Z { get; set; }

        public PointIn3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static PointIn3D operator -(PointIn3D a, PointIn3D b)
        {
            return new PointIn3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static PointIn3D operator +(PointIn3D a, PointIn3D b)
        {
            return new PointIn3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static implicit operator Vector3(PointIn3D p)
        {
            return new Vector3(p.X, p.Y, p.Z);
        }

        public static implicit operator PointIn3D(Vector3 v)
        {
            return new PointIn3D(v.X, v.Y, v.Z);
        }

        public bool IsEqual(PointIn3D point)
        {
            return this.X == point.X && this.Y == point.Y && this.Z == point.Z;
        }

        public override string ToString()
        {
            return "(" + Math.Round(X, 1) + "," + Math.Round(Y, 1) + "," + Math.Round(Z, 1) + ")";
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public PointIn3D Copy()
        {
            return new PointIn3D(X, Y, Z);
        }

        public PointIn3D Zoom(float zoomParameter)
        {
            return new PointIn3D(X * zoomParameter, Y * zoomParameter, Z * zoomParameter);
        }
    }
}
