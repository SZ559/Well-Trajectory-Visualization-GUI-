using System.Collections.Generic;
using System;
using System.Numerics;

namespace GeometricObject
{
    public class Trajectory
    {
        public List<Vector3> PolyLineNodes
        {
            get; set;
        }

        public string SourceFile
        {
            get; set;
        }

        public string TrajectoryName { get; set; }

        public string WellName { get; set; }

        public Enum Unit { get; set; }

        public Trajectory(string source = "", string wellName = "", string trajectoryName = "", DistanceUnit distanceUnit = DistanceUnit.Meter)
        {
            PolyLineNodes = new List<Vector3>();
            SourceFile = source;
            WellName = wellName;
            TrajectoryName = trajectoryName;
            Unit = distanceUnit;
        }

        public void AddNode(Vector3 point)
        {
            PolyLineNodes.Add(point);
        }

        public void AddNode(Single x, Single y, Single z)
        {
            Vector3 newNode = new Vector3(x, y, z);
            PolyLineNodes.Add(newNode);
        }
    }
}