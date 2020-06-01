
namespace Well_Trajectory_Visualization
{
    public class DisplayChoice
    {
        public bool AddAnnotation
        {
            get; set;
        }

        public bool AddSharpestPoint
        {
            get; set;
        }
        
        public bool ChooseRegion
        {
            get; set;
        }

        public bool Synchronize
        {
            get; set;
        }

        public DisplayChoice()
        {
            AddAnnotation = false;
            AddSharpestPoint = false;
            Synchronize = false;
            ChooseRegion = false;
        }
    }
}
