using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using BLLayer;

namespace Well_Trajectory_Visualization
{
    public partial class TableLayoutPanelForProjection : TableLayoutPanel
    {
        public CurrentTrajectory CurrentTrajectory
        {
            get; set;
        }
        private DisplayChoice displayChoice;
        private PanelForProjection topViewPanel;
        private PanelForProjection leftViewPanel;
        private PanelForProjection mainViewPanel;
        private ZoomInformationOfView zoomInformation;

        public float ZoomInXAxis
        {
            get
            {
                return Math.Min(topViewPanel.GraphicDrawingArea.Width, mainViewPanel.GraphicDrawingArea.Width) / CurrentTrajectory.DifferenceInXOfTrajectory;
            }
        }

        public float ZoomInZAxis
        {
            get
            {
                return Math.Min(leftViewPanel.GraphicDrawingArea.Height, mainViewPanel.GraphicDrawingArea.Height) / CurrentTrajectory.DifferenceInZOfTrajectory;
            }
        }

        public float ZoomInYAxis
        {
            get
            {
                return Math.Min(leftViewPanel.GraphicDrawingArea.Width, topViewPanel.GraphicDrawingArea.Height) / CurrentTrajectory.DifferenceInYOfTrajectory;
            }
        }

        public TableLayoutPanelForProjection(CurrentTrajectory currentTrajectory, DisplayChoice displayChoice)
        {
            InitializeComponent();
            BorderStyle = BorderStyle.None;
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            RowCount = 1;
            AutoScroll = true;
            Dock = DockStyle.Fill;

            this.CurrentTrajectory = currentTrajectory;
            zoomInformation = new ZoomInformationOfView();
            this.displayChoice = displayChoice;

            ColumnCount = 3;
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            this.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

            mainViewPanel = new PanelForProjection(Vector3.UnitY, CurrentTrajectory, zoomInformation, displayChoice);
            leftViewPanel = new PanelForProjection(Vector3.UnitX, CurrentTrajectory, zoomInformation, displayChoice);
            topViewPanel = new PanelForProjection(Vector3.UnitZ, CurrentTrajectory, zoomInformation, displayChoice);

            this.SuspendLayout();
            this.Controls.Add(mainViewPanel, 0, 0);
            this.Controls.Add(leftViewPanel, 1, 0);
            this.Controls.Add(topViewPanel, 2, 0);
            this.ResumeLayout();
        }

        public void ResetZoom()
        {
            zoomInformation.ResetZoom();
        }
    }
}
