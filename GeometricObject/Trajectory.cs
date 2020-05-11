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

        public string Well
        {
            get; set;
        }

        public Trajectory()
        {
            PolyLineNodes = new List<Vector3>();
        }

        public void AddNodes(Vector3 point)
        {
            PolyLineNodes.Add(point);
        }

        public void AddNodes(Single x, Single y, Single z)
        {
            Vector3 newNode = new Vector3(x, y, z);
            PolyLineNodes.Add(newNode);
        }
    }
}