using BLLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ValueObject;

namespace Well_Trajectory_Visualization
{
    public class PanelFor3DView : Panel
    {
        CurrentTrajectory currentTrajectory;
        Vector3 positionOfCamera;
        float distanceBetweenCameraAndImage;
        float sizeOfScreen;

        List<Vector3> intermediareNodes;
        List<Vector3> axis;

        int paddingX;
        int paddingY;

        int WidthOfCanvas
        {
            get
            {
                return Width - 2 * paddingX;
            }
        }

        int HeightOfCanvas
        {
            get
            {
                return Height - 2 * paddingY;
            }
        }

        bool isDrag = false;
        Point beginDragLocation;
        double angleX = 0;
        double angleZ = 0;


        public PanelFor3DView(CurrentTrajectory currentTrajectory)
        {
            this.currentTrajectory = currentTrajectory;
            intermediareNodes = currentTrajectory.Nodes.Select(p => (Vector3)p).ToList();
            InitAxis();

            SetCamera();

            paddingX = 20;
            paddingY = 10;
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;

            Paint += new PaintEventHandler(PaintPanel);
            MouseMove += new MouseEventHandler(Panel_MouseMove);
            MouseDown += new MouseEventHandler(Panel_MouseDown);
            MouseUp += new MouseEventHandler(Panel_MouseUp);
        }

        private void SetCamera()
        {
            if (currentTrajectory != null)
            {
                positionOfCamera.X = (currentTrajectory.MaxX + currentTrajectory.MinX) / 2;
                positionOfCamera.Y = 2 * currentTrajectory.Radius;
                positionOfCamera.Z = (currentTrajectory.MaxZ + currentTrajectory.MinZ) / 2;

                distanceBetweenCameraAndImage = currentTrajectory.Radius;
                sizeOfScreen = 2 * currentTrajectory.Radius;
            }
        }

        private PointF[] GetProjectionFrom3DTo2D(ref List<Vector3> coordinatesInObject)
        {
            coordinatesInObject = Projection.GetCoordinatesInWorldCoordinatesSystem(coordinatesInObject, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
            var coordinatesInCamera = Projection.GetCoordinatesInCameraCoordinatesSystem(coordinatesInObject, positionOfCamera);
            var coordinatesInImage = Projection.GetParallelCoordinatesInImageCoordinatesSystem(coordinatesInCamera);
            var sizeOfCanvas = Math.Min(WidthOfCanvas, HeightOfCanvas);
            var coordinatesInCanvas = Projection.GetRasterCoordinateInCanvasCoordiantesSystem(coordinatesInImage, sizeOfScreen, sizeOfCanvas, sizeOfCanvas);
            return coordinatesInCanvas.Select(p => new PointF(p[0], p[1])).ToArray();
        }

        private void InitAxis()
        {
            axis = (new Vector3[] { new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MinY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MaxZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY, currentTrajectory.MaxZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MinY, currentTrajectory.MaxZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MaxY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MaxY, currentTrajectory.MaxZ) }).ToList();
        }

        private void PaintPanel(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            var points = GetProjectionFrom3DTo2D(ref intermediareNodes);
            using (Pen penForLine = new Pen(Color.FromArgb(204, 234, 187), 3.0F))
            {
                for (int i = 0; i < points.Length - 1; i = i + 1)
                {
                    graphics.DrawLine(penForLine, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
                }
            }

            using (SolidBrush brushForPoint = new SolidBrush(Color.Red))
            {
                foreach (var point in points)
                {
                    graphics.FillRectangle(brushForPoint, point.X - 1, point.Y - 1, 2, 2);
                }
            }


            var axisInCanvas = GetProjectionFrom3DTo2D(ref axis);
            using (Pen penForLine = new Pen(Color.FromArgb(63, 63, 68), 2.0F))
            {
                graphics.DrawLine(penForLine, axisInCanvas[0], axisInCanvas[1]);
                graphics.DrawLine(penForLine, axisInCanvas[0], axisInCanvas[2]);
                graphics.DrawLine(penForLine, axisInCanvas[0], axisInCanvas[3]);

                graphics.DrawLine(penForLine, axisInCanvas[7], axisInCanvas[4]);
                graphics.DrawLine(penForLine, axisInCanvas[7], axisInCanvas[5]);
                graphics.DrawLine(penForLine, axisInCanvas[7], axisInCanvas[6]);

                graphics.DrawLine(penForLine, axisInCanvas[1], axisInCanvas[6]);
                graphics.DrawLine(penForLine, axisInCanvas[2], axisInCanvas[6]);
                graphics.DrawLine(penForLine, axisInCanvas[1], axisInCanvas[5]);
                graphics.DrawLine(penForLine, axisInCanvas[3], axisInCanvas[5]);
                graphics.DrawLine(penForLine, axisInCanvas[3], axisInCanvas[4]);
                graphics.DrawLine(penForLine, axisInCanvas[2], axisInCanvas[4]);
            }


            graphics.Dispose();
        }

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                beginDragLocation = e.Location;
                isDrag = true;
            }
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                angleZ = Math.Atan2((e.X - beginDragLocation.X), currentTrajectory.Radius);
                angleX = Math.Atan2((beginDragLocation.Y - e.Y), currentTrajectory.Radius);
                var panel = (PanelFor3DView)sender;
                panel.Refresh();
            }
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                isDrag = false;
            }
        }
    }
}
