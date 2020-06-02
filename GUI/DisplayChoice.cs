
namespace Well_Trajectory_Visualization
{
    public class DisplayChoice
    {
        public bool IfShowAnnotation
        {
            get; set;
        }

        public bool IfShowSharpestPoint
        {
            get; set;
        }
        
        public bool IfUseRegionChoosing
        {
            get; set;
        }

        public bool IfSynchronize
        {
            get; set;
        }

        public DisplayChoice()
        {
            IfShowAnnotation = false;
            IfShowSharpestPoint = false;
            IfSynchronize = false;
            IfUseRegionChoosing = false;
        }
    }
}
