using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Well_Trajectory_Visualization
{
    public class ZoomInformationOfView
    {
        public float InflateX
        {
            get; set;
        }

        public float InflateY
        {
            get; set;
        }

        public float InflateZ
        {
            get; set;
        }

        public float OffsetX
        {
            get; set;
        }

        public float OffsetY
        {
            get; set;
        }

        public float OffsetZ
        {
            get; set;
        }

        public bool ZoomIsOn
        {
            get; set;
        }

        public ZoomInformationOfView()
        {
            OffsetX = 0;
            OffsetY = 0;
            OffsetZ = 0;
            ZoomIsOn = false;
            InflateX = 1;
            InflateY = 1;
            InflateZ = 1;
        }

        public void ResetZoom()
        {
            OffsetX = 0;
            OffsetY = 0;
            OffsetZ = 0;
            ZoomIsOn = false;
            InflateX = 1;
            InflateY = 1;
            InflateZ = 1;
        }
    }
}
