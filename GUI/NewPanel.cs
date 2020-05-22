using GeometricObject;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace Well_Trajectory_Visualization
{
    public class NewPanel : Panel
    {
        public List<PointIn2D> TrajectoryProjectionIn2D
        {
            get; set;
        }
        public string ViewName
        {
            get; set;
        }

        public PointF[] TrajectoryProjectionLocationOnPanel
        {
            get; set;
        }

        public NewPanel()
        {

        }
    }
}

