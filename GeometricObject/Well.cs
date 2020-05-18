using System.Collections.Generic;

namespace GeometricObject
{
    public class Well
    {
        public List<string> Sources { get; set; }

        public List<Trajectory> Trajectories
        {
            get; set;
        }

        public int TrajectoryCount
        {
            get; set;
        }

        public string WellName
        {
            get; set;
        }

        public Well(string wellName = "")
        {
            Sources = new List<string>();
            Trajectories = new List<Trajectory>();
            WellName = wellName;
            TrajectoryCount = 0;
        }

        public void AddTrajectory(Trajectory trajectory)
        {
            Trajectories.Add(trajectory);
            Sources.Add(trajectory.SourceFile);
            TrajectoryCount = TrajectoryCount + 1;
        }
    }
}