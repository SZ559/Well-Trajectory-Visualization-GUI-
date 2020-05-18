using System;
using System.Collections.Generic;
using System.Numerics;

namespace GeometricObject
{
    public class Projection
    {
        public List<PointIn2D> GetProjectionInPlane(List<Vector3> wellTrajectory, Vector3 normalVector)
        {
            List<PointIn2D> projectionInPlane = new List<PointIn2D>();
            switch (GetProjectionView(normalVector))
            {
                case "Main":
                    foreach (var point in wellTrajectory)
                    {
                        projectionInPlane.Add(new PointIn2D(point.X, point.Z));
                    }
                    break;
                case "Left":
                    foreach (var point in wellTrajectory)
                    {
                        projectionInPlane.Add(new PointIn2D(point.Y, point.Z));
                    }
                    break;
                case "Top":
                    foreach (var point in wellTrajectory)
                    {
                        projectionInPlane.Add(new PointIn2D(point.X, point.Y));
                    }
                    break;
                default:
                    foreach (var point in wellTrajectory)
                    {
                        Vector3 projectionPointIn3D = GetCoordinatesOfPointIn3D_AfterProjection_UsingNormalVector(normalVector, point, Vector3.Zero);

                        Vector3 unitXOfProjectionPlane = GetCoordinatesOfPointIn3D_AfterProjection_UsingNormalVector(normalVector, Vector3.UnitX, Vector3.Zero);
                        //Vector3 unitVectorOfUnitXProjectionIn3D = Vector3.Normalize(unitXProjectionIn3D);
                        projectionInPlane.Add(GetCoordinatesOfProjection_InPlaneThroughOrigin(projectionPointIn3D, unitXOfProjectionPlane));
                    }
                    break;
            }
            return projectionInPlane;
        }

        public string GetProjectionView(Vector3 normalVector)
        {
            Vector3 unitNormalVector = Vector3.Normalize(normalVector);
            if (unitNormalVector == Vector3.UnitX)
            {
                return "Left";
            }
            if (unitNormalVector == Vector3.UnitZ)
            {
                return "Top";
            }
            if (unitNormalVector == Vector3.UnitY)
            {
                return "Main";
            }
            return "Projection";
        }
       
        //vector 3 contains x, y, z in Single type
        public Vector3 GetCoordinatesOfPointIn3D_AfterProjection_UsingNormalVector(Vector3 normalVector, Vector3 pointToProject, Vector3 onePointOnProjectionPlane)
        {
            Single numerator = (normalVector.X * onePointOnProjectionPlane.X + normalVector.Y * onePointOnProjectionPlane.Y + normalVector.Z * onePointOnProjectionPlane.Z) - (normalVector.X * pointToProject.X + normalVector.Y * pointToProject.Y + normalVector.Z * pointToProject.Z);
            Single denominator = normalVector.X * normalVector.X + normalVector.Y * normalVector.Y + normalVector.Z * normalVector.Z;
            Single t = numerator / denominator;

            Single x = pointToProject.X + normalVector.X * t;
            Single y = pointToProject.Y + normalVector.Y * t;
            Single z = pointToProject.Z + normalVector.Z * t;

            return new Vector3(x, y, z);
        }

        public PointIn2D GetCoordinatesOfProjection_InPlaneThroughOrigin(Vector3 projectionPoint, Vector3 unitXOfProjectionPlane)
        {
            Single abscissa = Vector3.Dot(projectionPoint, unitXOfProjectionPlane) / unitXOfProjectionPlane.Length();
            Vector3 projectionVector = Vector3.Multiply (unitXOfProjectionPlane, abscissa / unitXOfProjectionPlane.Length());
            Vector3 rejection = Vector3.Subtract(projectionPoint, projectionVector);
            Single ordinate = rejection.Length();
            if (rejection.Z < 0)
            {
                ordinate = -ordinate;
            }
            return new PointIn2D(abscissa, ordinate);
        }
    }
}
