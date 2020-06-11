using System;
using System.Collections.Generic;
using System.Numerics;
using ValueObject;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System.Drawing;

namespace BLLayer
{
    public class Projection
    {
        public static List<Vector2> GetProjectionInPlane(List<PointIn3D> wellTrajectory, Vector3 normalVector)
        {
            List<Vector2> projectionInPlane = new List<Vector2>();
            switch (GetProjectionView(normalVector))
            {
                case "Main":
                    foreach (var point in wellTrajectory)
                    {
                        projectionInPlane.Add(new Vector2(point.X, point.Z));
                    }
                    break;
                case "Left":
                    foreach (var point in wellTrajectory)
                    {
                        projectionInPlane.Add(new Vector2(point.Y, point.Z));
                    }
                    break;
                case "Top":
                    foreach (var point in wellTrajectory)
                    {
                        projectionInPlane.Add(new Vector2(point.X, point.Y));
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

        public static string GetProjectionView(Vector3 normalVector)
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
        public static Vector3 GetCoordinatesOfPointIn3D_AfterProjection_UsingNormalVector(Vector3 normalVector, Vector3 pointToProject, Vector3 onePointOnProjectionPlane)
        {
            Single numerator = (normalVector.X * onePointOnProjectionPlane.X + normalVector.Y * onePointOnProjectionPlane.Y + normalVector.Z * onePointOnProjectionPlane.Z) - (normalVector.X * pointToProject.X + normalVector.Y * pointToProject.Y + normalVector.Z * pointToProject.Z);
            Single denominator = normalVector.X * normalVector.X + normalVector.Y * normalVector.Y + normalVector.Z * normalVector.Z;
            Single t = numerator / denominator;

            Single x = pointToProject.X + normalVector.X * t;
            Single y = pointToProject.Y + normalVector.Y * t;
            Single z = pointToProject.Z + normalVector.Z * t;

            return new Vector3(x, y, z);
        }

        public static Vector2 GetCoordinatesOfProjection_InPlaneThroughOrigin(Vector3 projectionPoint, Vector3 unitXOfProjectionPlane)
        {
            Single abscissa = Vector3.Dot(projectionPoint, unitXOfProjectionPlane) / unitXOfProjectionPlane.Length();
            Vector3 projectionVector = Vector3.Multiply(unitXOfProjectionPlane, abscissa / unitXOfProjectionPlane.Length());
            Vector3 rejection = Vector3.Subtract(projectionPoint, projectionVector);
            Single ordinate = rejection.Length();
            if (rejection.Z < 0)
            {
                ordinate = -ordinate;
            }
            return new Vector2(abscissa, ordinate);
        }

        public static Vector3 GetCoordinatesInWorldCoordinatesSystem(Vector3 point, Vector3 centerOfTranformation, double angleX, double angleZ)
        {
            var M = Matrix<double>.Build;
            var V = MathNet.Numerics.LinearAlgebra.Vector<double>.Build;

            var vectorOfPointInWorld = V.DenseOfArray(new double[] { point.X, point.Y, point.Z, 1 });
            var matrixTranslation = M.DenseOfArray(new double[,] { { 1,0,0,-1*centerOfTranformation.X},
            { 0,1,0,-1*centerOfTranformation.Y},
            { 0,0,1,-1*centerOfTranformation.Z},
            { 0,0,0,1} });
            var matrixRotationZ = M.DenseOfArray(new double[,] {{ Math.Cos(angleZ), Math.Sin(angleZ), 0, 0 },
                                                              { -1 * Math.Sin(angleZ), Math.Cos(angleZ), 0, 0 },
                                                              { 0, 0, 1, 0 },
                                                              { 0, 0, 0, 1 }});
            var matrixRotationX = M.DenseOfArray(new double[,] {{ 1, 0, 0, 0 },
                                                              { 0, Math.Cos(angleX), Math.Sin(angleX), 0 },
                                                              { 0, -1 * Math.Sin(angleX), Math.Cos(angleX), 0 },
                                                              { 0, 0, 0, 1 }});
            var vectorOfPointInCamera = (M.DenseDiagonal(4, 2) - matrixTranslation) * matrixRotationZ * matrixRotationX * matrixTranslation * vectorOfPointInWorld;
            return new Vector3((float)vectorOfPointInCamera.At(0), (float)vectorOfPointInCamera.At(1), (float)vectorOfPointInCamera.At(2));
        }

        public static Vector3 GetCoordinatesInCameraCoordinatesSystem(Vector3 point, Vector3 originOfCamera)
        {
            var M = Matrix<float>.Build;
            var V = MathNet.Numerics.LinearAlgebra.Vector<float>.Build;

            var vectorOfPointInWorld = V.DenseOfArray(new float[] { point.X, point.Y, point.Z, 1 });
            var matrixTranslation = M.DenseOfArray(new float[,] {{ 1, 0, 0, -1 * originOfCamera.X },
                                                                                    { 0, 1, 0, -1 * originOfCamera.Y },
                                                                                    { 0, 0, 1, -1 * originOfCamera.Z },
                                                                                    { 0, 0, 0, 1 }});
            var matrixRotation = M.DenseOfArray(new float[,] {{ 1, 0, 0, 0 },
                                                              { 0, 0, -1, 0 },
                                                              { 0, 1, 0, 0 },
                                                              { 0, 0, 0, 1 }});
            var vectorOfPointInCamera = matrixRotation * matrixTranslation * vectorOfPointInWorld;
            return new Vector3(vectorOfPointInCamera.At(0), vectorOfPointInCamera.At(1), vectorOfPointInCamera.At(2));
        }

        public static Vector2 GetParallelCoordinatesInImageCoordinatesSystem(Vector3 point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Vector2 GetPerspectiveCoordinatesInImageCoordinatesSystem(Vector3 point, float distanceBetweenCameraAndImage)
        {
            return new Vector2(point.X * distanceBetweenCameraAndImage / point.Z, point.Y * distanceBetweenCameraAndImage / point.Z);
        }

        public static int[] GetRasterCoordinateInCanvasCoordiantesSystem(Vector2 point, float sizeOfScreen, int widthOfCanvas, int heightOfCanvas)
        {
            var normalizedX = (point.X + sizeOfScreen / 2) / sizeOfScreen;
            var normalizedY = (point.Y + sizeOfScreen / 2) / sizeOfScreen;
            var rasterX = Math.Floor(normalizedX * widthOfCanvas);
            var rasterY = Math.Floor((1 - normalizedY) * heightOfCanvas);
            return new int[] { (int)rasterX, (int)rasterY };
        }
    }
}
