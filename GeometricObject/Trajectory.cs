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

        public string SourceWell
        {
            get; set;
        }

        public Trajectory(string sourceWell = "")
        {
            PolyLineNodes = new List<Vector3>();
            SourceWell = sourceWell;
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