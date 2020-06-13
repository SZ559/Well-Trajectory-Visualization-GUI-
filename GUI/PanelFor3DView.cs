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
        int paddingForAxis;

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
            paddingX = 20;
            paddingY = 10;
            paddingForAxis = (int)(currentTrajectory.Radius / 10);
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;

            this.currentTrajectory = currentTrajectory;
            intermediareNodes = currentTrajectory.Nodes.Select(p => (Vector3)p).ToList();
            InitAxis();
            SetCamera();

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
                sizeOfScreen = (float)(2 * (currentTrajectory.Radius + Math.Sqrt(3) * paddingForAxis));
            }
        }

        private PointF[] GetProjectionFrom3DTo2D(List<Vector3> coordinatesInObject)
        {
            var coordinatesInWorld = Projection.GetCoordinatesInWorldCoordinatesSystem(coordinatesInObject, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
            var coordinatesInCamera = Projection.GetCoordinatesInCameraCoordinatesSystem(coordinatesInWorld, positionOfCamera);
            var coordinatesInImage = Projection.GetParallelCoordinatesInImageCoordinatesSystem(coordinatesInCamera);
            var sizeOfCanvas = Math.Min(WidthOfCanvas, HeightOfCanvas);
            var coordinatesInCanvas = Projection.GetRasterCoordinateInCanvasCoordiantesSystem(coordinatesInImage, sizeOfScreen, sizeOfCanvas, sizeOfCanvas);
            return coordinatesInCanvas.Select(p => new PointF(p[0] + paddingX + Math.Max((WidthOfCanvas - HeightOfCanvas) / 2, 0), p[1] + paddingY + Math.Max((HeightOfCanvas - WidthOfCanvas) / 2, 0))).ToArray();
        }

        private void InitAxis()
        {
            axis = (new Vector3[] { new Vector3(currentTrajectory.MinX - paddingForAxis, currentTrajectory.MinY - paddingForAxis, currentTrajectory.MinZ - paddingForAxis),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY - paddingForAxis, currentTrajectory.MinZ - paddingForAxis),
                new Vector3(currentTrajectory.MinX - paddingForAxis, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ - paddingForAxis),
                new Vector3(currentTrajectory.MinX - paddingForAxis, currentTrajectory.MinY - paddingForAxis, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MinX - paddingForAxis, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY - paddingForAxis, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ - paddingForAxis),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MaxZ + paddingForAxis) }).ToList();
        }

        private void PaintPanel(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            var points = GetProjectionFrom3DTo2D(intermediareNodes);
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


            var axisInCanvas = GetProjectionFrom3DTo2D(axis);
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
                angleZ =
                //(e.X - beginDragLocation.X) * Math.PI / 200;
                //Math.Tanh(e.X - beginDragLocation.X) * Math.PI / 2;
                Math.Atan2((e.X - beginDragLocation.X), Math.Max(Width, Height));
                angleX =
                //(beginDragLocation.Y - e.Y) * Math.PI / 200;
                //Math.Tanh(beginDragLocation.Y - e.Y) * Math.PI / 2;
                Math.Atan2((beginDragLocation.Y - e.Y), Math.Max(Width, Height));
                var panel = (PanelFor3DView)sender;
                panel.Refresh();
            }
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                isDrag = false;

                intermediareNodes = Projection.GetCoordinatesInWorldCoordinatesSystem(intermediareNodes, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
                axis = Projection.GetCoordinatesInWorldCoordinatesSystem(axis, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
            }
        }
    }
}
