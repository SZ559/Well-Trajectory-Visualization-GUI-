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

        public Well(string wellName = "")
        {
            Trajectories = new List<Trajectory>();
            WellName = wellName;
        }

        public void AddTrajectory(Trajectory trajectory)
        {
            Trajectories.Add(trajectory);
        }
    }
}