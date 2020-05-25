using System.Collections.Generic;
using System;
using System.Numerics;

namespace GeometricObject
{
    public class Trajectory
    {
        public List<Vector3> PolyLineNodes { get; set; }

        public string SourceFile { get; set; }

        public string TrajectoryName { get; set; }

        public string WellName { get; set; }

        public DistanceUnit Unit { get; set; }

        public int PointNumbers { get; set; }

        public Trajectory(string source = "", string wellName = "", string trajectoryName = "", DistanceUnit distanceUnit = DistanceUnit.Meter)
        {
            PolyLineNodes = new List<Vector3>();
            SourceFile = source;
            WellName = wellName;
            TrajectoryName = trajectoryName;
            Unit = distanceUnit;
            PointNumbers = 0;
        }

        public void AddNode(Vector3 point)
        {
            PolyLineNodes.Add(point);
            PointNumbers += 1;
        }

        public void AddNode(Single x, Single y, Single z)
        {
            Vector3 newNode = new Vector3(x, y, z);
            PolyLineNodes.Add(newNode);
            PointNumbers += 1;
        }

        public void AddNode(float[] coordinates)
        {
            if (coordinates.Length == 3)
            {
                Vector3 newNode = new Vector3(coordinates[0], coordinates[1], coordinates[2]);
                PolyLineNodes.Add(newNode);
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
            if (distanceUnit != Unit)
            {
                Trajectory newTrajectory = new Trajectory(this.SourceFile, this.WellName, this.TrajectoryName, distanceUnit);

                if (Unit == DistanceUnit.Feet && distanceUnit == DistanceUnit.Meter)
                {
                    for (int i = 0; i < this.PointNumbers; i++)
                    {
                        float[] coordinates = new float[] { this[i].X, this[i].Y, this[i].Z };
                        newTrajectory.AddNode(UnitConvertor.FeetToMeter(coordinates));
                    }
                }
                else if (Unit == DistanceUnit.Meter && distanceUnit == DistanceUnit.Feet)
                {
                    for (int i = 0; i < this.PointNumbers; i++)
                    {
                        float[] coordinates = new float[] { this[i].X, this[i].Y, this[i].Z };
                        newTrajectory.AddNode(UnitConvertor.MeterToFeet(coordinates));
                    }
                }

                return newTrajectory;
            }
            else
            {
                return this;
            }
        }
    }
}