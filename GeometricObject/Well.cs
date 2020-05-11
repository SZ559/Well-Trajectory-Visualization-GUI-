using System.Collections.Generic;

namespace GeometricObject
{
    public class Well
    {
        public List<Trajectory> Trajectories
        {
            get; set;
        }

        public string WellName
        {
            get; set;
        }

        public void AddTrajectory(Trajectory trajectory)
        {
            Trajectories.Add(trajectory);
        }
    }
}