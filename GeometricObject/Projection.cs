using System;
using System.Collections.Generic;
using System.Numerics;

namespace GeometricObject
{
    public class Projection
    {
        public List<PointIn2D> GetProjectionInPlane(List<Vector3> wellTrajectory, Vector3 normalVector)
        {
            List<PointIn2D> pojectionInPlane = new List<PointIn2D>();
            switch (GetProjectionView(normalVector))
            {
                case "Main":
                    foreach (var point in wellTrajectory)
                    {
                        pojectionInPlane.Add(new PointIn2D(point.X, point.Y));
                    }
                    break;
                case "Left":
                    foreach (var point in wellTrajectory)
                    {
                        pojectionInPlane.Add(new PointIn2D(point.Y, point.Z));
                    }
                    break;
                case "Top":
                    foreach (var point in wellTrajectory)
                    {
                        pojectionInPlane.Add(new PointIn2D(point.X, point.Z));
                    }
                    break;
                case "Projection":
                    foreach (var point in wellTrajectory)
                    {
                        Vector3 porjectionPointIn3D = GetPorjectionPointIn3D_UsingNormalVector(normalVector, point, Vector3.Zero);

                        Vector3 unitXProjectionIn3D = GetPorjectionPointIn3D_UsingNormalVector(normalVector, Vector3.UnitX, Vector3.Zero);
                        //Vector3 unitVectorOfUnitXProjectionIn3D = Vector3.Normalize(unitXProjectionIn3D);
                        pojectionInPlane.Add(GetProjectionPointIn2D_PlaneThroughOrigin(porjectionPointIn3D, unitXProjectionIn3D));
                    }
                    break;
            }
            return pojectionInPlane;
        }

        public string GetProjectionView(Vector3 normalVector)
        {
            Vector3 unitNormalVector = Vector3.Normalize(normalVector);
            if (unitNormalVector == Vector3.UnitX)
            {
                return "Left";
            }
            if (unitNormalVector == Vector3.UnitY)
            {
                return "Top";
            }
            if (unitNormalVector == Vector3.UnitZ)
            {
                return "Main";
            }
            return "Projection";
        }
       
        //vector 3 contains x, y, z in Single type
        public Vector3 GetPorjectionPointIn3D_UsingNormalVector(Vector3 normalVector, Vector3 point, Vector3 onePointOnPlane)
        {
            Single numerator = (normalVector.X * onePointOnPlane.X + normalVector.Y * onePointOnPlane.Y + normalVector.Z * onePointOnPlane.Z) - (normalVector.X * point.X + normalVector.Y * point.Y + normalVector.Z * point.Z);
            Single denominator = normalVector.X * normalVector.X + normalVector.Y * normalVector.Y + normalVector.Z * normalVector.Z;
            Single t = numerator / denominator;

            Single x = point.X + normalVector.X * t;
            Single y = point.Y + normalVector.Y * t;
            Single z = point.Z + normalVector.Z * t;

            return new Vector3(x, y, z);
        }

        public PointIn2D GetProjectionPointIn2D_PlaneThroughOrigin(Vector3 projectionPoint, Vector3 unitX)
        {
            Single abscissa = Vector3.Dot(projectionPoint, unitX) / unitX.Length();
            Vector3 projectionVector = Vector3.Multiply (unitX, abscissa / unitX.Length());
            Vector3 rejection = Vector3.Subtract(projectionPoint, projectionVector);
            Single ordinate = rejection.Length();
            if (rejection.Z < 0)
            {
                rejection = -rejection;
            }
            return new PointIn2D(abscissa, ordinate);
        }

        //useless for now
        public PointIn2D GetProjectionPointIn2D_PlaneParallelToZAxis(PointIn3D projectionPoint, PointIn3D projectionOrigin)
        {
            double[] vectorOriginToPoint = new double[] { projectionPoint.X - projectionOrigin.X, projectionPoint.Y - projectionOrigin.Y };
            double vectorLength = Math.Sqrt(vectorOriginToPoint[0] * vectorOriginToPoint[0] + vectorOriginToPoint[1] * vectorOriginToPoint[1]);
            if (vectorOriginToPoint[0] < 0)
            {
                vectorLength = -vectorLength;
            }
            return new PointIn2D(vectorLength, projectionPoint.Z);
        }
    }
}
