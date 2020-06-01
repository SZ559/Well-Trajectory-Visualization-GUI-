using System.Collections.Generic;
using System;
using System.Numerics;

namespace GeometricObject
{
    public class Trajectory
    {
        public List<Vector3> polyLineNodes;
        
        public List<Vector3> PolyLineNodes
        {
            get
            {
                List<Vector3> nodes = new List<Vector3>();
                foreach (var node in polyLineNodes)
                {
                    Vector3 vector3 = new Vector3(node.X * unitConversion, node.Y * unitConversion, node.Z * unitConversion);
                    nodes.Add(vector3);
                }
                return nodes;
            }
        }

        public string SourceFile { get; set; }

        public string TrajectoryName { get; set; }

        public string WellName { get; set; }

        public DistanceUnit Unit { get; set; }

        public DistanceUnit UnitInUse { get; set; }

        public float unitConversion
        {
            get
            {
                return UnitConversion.Result(Unit, UnitInUse);
            }
        }

        public int PointNumbers { get; set; }

        public Trajectory(string source = "", string wellName = "", string trajectoryName = "", DistanceUnit distanceUnit = DistanceUnit.Meter)
        {
            polyLineNodes = new List<Vector3>();
            SourceFile = source;
            WellName = wellName;
            TrajectoryName = trajectoryName;
            Unit = distanceUnit;
            UnitInUse = Unit;
            PointNumbers = 0;
        }

        public void AddNode(Vector3 point)
        {
            polyLineNodes.Add(point);
            PointNumbers += 1;
        }

        public void AddNode(Single x, Single y, Single z)
        {
            Vector3 newNode = new Vector3(x, y, z);
            polyLineNodes.Add(newNode);
            PointNumbers += 1;
        }

        public void AddNode(float[] coordinates)
        {
            if (coordinates.Length == 3)
            {
                Vector3 newNode = new Vector3(coordinates[0], coordinates[1], coordinates[2]);
                polyLineNodes.Add(newNode);
                PointNumbers += 1;
            }
        }

        public Vector3 this[int index]
        {
            get
            {
                if (index < PolyLineNodes.Count)
                {
                    return PolyLineNodes[index];
                }
                else
                {
                    throw new ArgumentOutOfRangeException("index");
                }
            }
        }

        public Trajectory ConvertTo(DistanceUnit distanceUnit)
        {
            UnitInUse = distanceUnit;
            return this;
        }
    }
}