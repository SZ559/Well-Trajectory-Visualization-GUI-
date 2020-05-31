using GeometricObject;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.Numerics;
using System.ComponentModel;

namespace Well_Trajectory_Visualization
{
    public class CurrentTrajectoryInformation
    {
        private Trajectory currentTrajectory;

        public event PropertyChangedEventHandler PropertyChanged;

        public DisplayChoice DisplayChoice
        {
            get; set;
        }

        public Trajectory CurrentTrajectory
        {
            get
            {
                return currentTrajectory;
            }
            set
            {
                currentTrajectory = value;
                OnPropertyChanged();
            }
        }

        public DistanceUnit Unit
        {
            get
            {
                return CurrentTrajectory.UnitInUse;
            }
            set
            {
                CurrentTrajectory = CurrentTrajectory.ConvertTo(value);
            }
        }

        public List<Vector3> SharpestPoint 
        { 
            get; private set; 
        }

        public float MaxXYOfTrajectory 
        { 
            get; private set; 
        }

        public float MaxZOfTrajectory 
        { 
            get; private set; 
        }

        public string UnitForCaption
        {
            get
            {
                string unitForCaption;
                switch (CurrentTrajectory.UnitInUse)
                {
                    case DistanceUnit.Meter:
                        unitForCaption = "m";
                        break;
                    case DistanceUnit.Feet:
                    default:
                        unitForCaption = "ft";
                        break;
                }
                return unitForCaption;
            }
        }

        public CurrentTrajectoryInformation(Trajectory currentTrajectory, DisplayChoice displayChoice)
        {
            this.currentTrajectory = currentTrajectory;
            DisplayChoice = displayChoice;
            SharpestPoint = GetSharpestPointIndex(CurrentTrajectory);
            MaxXYOfTrajectory = GetMaxXYOfTrajectory();
            MaxZOfTrajectory = GetMaxZOfTrajectory();
        }

        private void OnPropertyChanged()
        {
            SharpestPoint = GetSharpestPointIndex(CurrentTrajectory);
            MaxXYOfTrajectory = GetMaxXYOfTrajectory();
            MaxZOfTrajectory = GetMaxZOfTrajectory();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        private List<Vector3> GetSharpestPointIndex(Trajectory currentTrajectory)
        {
            double lengthOfVector1, lengthOfVector2, dotProduct;
            Vector3 vector1, vector2;
            double maxCurvature = Math.PI;
            double radian;
            int indexPreventOverlaping_Vector1 = 0;

            List<Vector3> maxCurvaturePointIndex = new List<Vector3>();
            for (int i = 1; i < currentTrajectory.PolyLineNodes.Count - 1; i = i + 1)
            {

                vector1 = Vector3.Subtract(currentTrajectory[i - indexPreventOverlaping_Vector1 - 1], currentTrajectory[i]);
                vector2 = Vector3.Subtract(currentTrajectory[i + 1], currentTrajectory[i]);

                lengthOfVector1 = vector1.Length();
                lengthOfVector2 = vector2.Length();

                if (lengthOfVector1 == 0)
                {
                    continue;
                }

                if (lengthOfVector2 != 0)
                {
                    dotProduct = Vector3.Dot(vector1, vector2);
                    radian = Math.Round(Math.Acos(dotProduct / (lengthOfVector1 * lengthOfVector2)),4);

                    if (radian > Math.PI)
                    {
                        radian = 2 * Math.PI - radian;
                    }

                    if (radian < maxCurvature)
                    {
                        maxCurvature = radian;
                        maxCurvaturePointIndex.Clear();
                        maxCurvaturePointIndex.Add(currentTrajectory[i]);
                    }
                    else if (maxCurvature == radian)
                    {
                        maxCurvaturePointIndex.Add(currentTrajectory[i]);
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

        private Single GetMaxXYOfTrajectory()
        {
            Single zoomXY;
            Single maxX = CurrentTrajectory.PolyLineNodes.Select(x => x.X).Max();
            Single maxY = CurrentTrajectory.PolyLineNodes.Select(x => x.Y).Max();
            Single minX = CurrentTrajectory.PolyLineNodes.Select(x => x.X).Min();
            Single minY = CurrentTrajectory.PolyLineNodes.Select(x => x.Y).Min();

            zoomXY = Math.Max(maxX - minX, maxY - minY);
            return zoomXY > 0 ? zoomXY : 1;
        }

        private Single GetMaxZOfTrajectory()
        {
            Single zoomZ;
            Single maxZ = CurrentTrajectory.PolyLineNodes.Select(x => x.Z).Max();
            Single minZ = CurrentTrajectory.PolyLineNodes.Select(x => x.Z).Min();
            zoomZ = maxZ - minZ;
            return zoomZ > 0 ? zoomZ : 1;
        }
    }
}
