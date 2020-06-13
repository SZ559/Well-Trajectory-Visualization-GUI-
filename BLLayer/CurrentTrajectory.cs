using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.Numerics;
using System.ComponentModel;
using ValueObject;

namespace BLLayer
{
    public class CurrentTrajectory
    {
        public event Action PropertyChanged;

        public Trajectory Self { get; set; }

        private DistanceUnit unitInUse;

        public DistanceUnit UnitInUse
        {
            get
            {
                return unitInUse;
            }
            set
            {
                unitInUse = value;
                Update();
            }
        }

        public float UnitConversion
        {
            get
            {
                return UnitConvertor.Result(Self.Unit, UnitInUse);
            }
        }

        public string UnitForCaption
        {
            get
            {
                string unitForCaption;
                switch (UnitInUse)
                {
                    case DistanceUnit.Feet:
                        unitForCaption = "ft";
                        break;
                    case DistanceUnit.Meter:
                    default:
                        unitForCaption = "m";
                        break;
                }
                return unitForCaption;
            }
        }

        public List<PointIn3D> Nodes
        {
            get
            {
                return Self.Nodes.Select(x => x.Zoom(UnitConversion)).ToList();
            }
        }

        public List<int> SharpestPointIndex { get; private set; }

        public float DifferenceInXOfTrajectory { get; private set; }
        public float DifferenceInYOfTrajectory { get; private set; }
        public float DifferenceInZOfTrajectory { get; private set; }

        public float MaxX { get; private set; }
        public float MinX { get; private set; }
        public float MaxY { get; private set; }
        public float MinY { get; private set; }
        public float MaxZ { get; private set; }
        public float MinZ { get; private set; }

        public float Radius { get; private set; }
        public Vector3 CenterOfTrajectory { get; private set; }

        public List<Vector2> ProjectionInMainView { get; private set; }
        public List<Vector2> ProjectionInLeftView { get; private set; }
        public List<Vector2> ProjectionInTopView { get; private set; }

        public CurrentTrajectory(Trajectory currentTrajectory)
        {
            Self = currentTrajectory;
            UnitInUse = currentTrajectory.Unit;
            SharpestPointIndex = GetSharpestPointIndex();
            Update();
        }

        private void Update()
        {
            DifferenceInXOfTrajectory = GetDifferenceInXOfTrajectory();
            DifferenceInYOfTrajectory = GetDifferenceInYOfTrajectory();
            DifferenceInZOfTrajectory = GetDifferenceInZOfTrajectory();
            ProjectionInMainView = GetProjectionInMainView();
            ProjectionInLeftView = GetProjectionInLeftView();
            ProjectionInTopView = GetProjectionInTopView();
            Radius = GetRadiusOfMinSphere();
            CenterOfTrajectory = GetCenterOfTrajectory();
            PropertyChanged?.Invoke();
        }

        private List<int> GetSharpestPointIndex()
        {
            double lengthOfVector1, lengthOfVector2, dotProduct;
            Vector3 vector1, vector2;
            double maxCurvature = Math.PI;
            double radian;
            int indexPreventOverlaping_Vector1 = 0;

            List<int> maxCurvaturePointIndex = new List<int>();
            for (int i = 1; i < Self.Nodes.Count - 1; i = i + 1)
            {

                vector1 = Vector3.Subtract(Self[i - indexPreventOverlaping_Vector1 - 1], Self[i]);
                vector2 = Vector3.Subtract(Self[i + 1], Self[i]);

                lengthOfVector1 = vector1.Length();
                lengthOfVector2 = vector2.Length();

                if (lengthOfVector1 == 0)
                {
                    continue;
                }

                if (lengthOfVector2 != 0)
                {
                    dotProduct = Vector3.Dot(vector1, vector2);
                    radian = Math.Round(Math.Acos(dotProduct / (lengthOfVector1 * lengthOfVector2)), 4);

                    if (radian > Math.PI)
                    {
                        radian = 2 * Math.PI - radian;
                    }

                    if (radian < maxCurvature)
                    {
                        maxCurvature = radian;
                        maxCurvaturePointIndex.Clear();
                        maxCurvaturePointIndex.Add(i);
                    }
                    else if (maxCurvature == radian)
                    {
                        maxCurvaturePointIndex.Add(i);
                    }
                    indexPreventOverlaping_Vector1 = 0;
                }
                else
                {
                    indexPreventOverlaping_Vector1 = indexPreventOverlaping_Vector1 + 1;
                }
            }
            return maxCurvaturePointIndex;
        }

        private List<Vector2> GetProjectionInMainView()
        {
            return Projection.GetProjectionInPlane(Nodes, Vector3.UnitY);
        }

        private List<Vector2> GetProjectionInLeftView()
        {
            return Projection.GetProjectionInPlane(Nodes, Vector3.UnitX);
        }

        private List<Vector2> GetProjectionInTopView()
        {
            return Projection.GetProjectionInPlane(Nodes, Vector3.UnitZ);
        }

        private float GetDifferenceInXOfTrajectory()
        {
            float zoomX;
            MaxX = Self.Nodes.Select(x => x.X * UnitConversion).Max();
            MinX = Self.Nodes.Select(x => x.X * UnitConversion).Min();
            zoomX = MaxX - MinX;
            return zoomX > 0 ? zoomX : 1;
        }

        private float GetDifferenceInYOfTrajectory()
        {
            float zoomY;
            MaxY = Self.Nodes.Select(x => x.Y * UnitConversion).Max();
            MinY = Self.Nodes.Select(x => x.Y * UnitConversion).Min();
            zoomY = MaxY - MinY;
            return zoomY > 0 ? zoomY : 1;
        }

        private float GetDifferenceInZOfTrajectory()
        {
            float zoomZ;
            MaxZ = Self.Nodes.Select(x => x.Z * UnitConversion).Max();
            MinZ = Self.Nodes.Select(x => x.Z * UnitConversion).Min();
            zoomZ = MaxZ - MinZ;
            return zoomZ > 0 ? zoomZ : 1;
        }

        private float GetRadiusOfMinSphere()
        {
            return (float)(Math.Sqrt((MaxX - MinX) * (MaxX - MinX) + (MaxY - MinY) * (MaxY - MinY) + (MaxZ - MinZ) * (MaxZ - MinZ)) / 2);
        }

        private Vector3 GetCenterOfTrajectory()
        {
            return new Vector3((MaxX + MinX) / 2, (MaxY + MinY) / 2, (MaxZ + MinZ) / 2);
        }
    }
}
