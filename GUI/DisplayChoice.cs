
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

        public DisplayChoice()
        {
            AddAnnotation = false;
            AddSharpestPoint = false;
        }
    }
}
