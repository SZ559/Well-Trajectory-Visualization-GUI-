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
        private DisplayChoice displayChoice;

        Vector3 positionOfCamera;
        float distanceBetweenCameraAndImage;
        float sizeOfScreen;

        List<Vector3> intermediareNodes;
        List<Vector3> axis;
        PointF[] pointsOnCanvas;

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

        int sideWidthOfXYZAxisDrawingArea;
        List<Vector3> xAxis;
        List<Vector3> yAxis;
        List<Vector3> zAxis;

        public Rectangle XYZAxisDrawingArea
        {
            get
            {
                return new Rectangle(Width - sideWidthOfXYZAxisDrawingArea - paddingX, paddingY, sideWidthOfXYZAxisDrawingArea, sideWidthOfXYZAxisDrawingArea);
            }
        }

        bool isDrag = false;
        Point beginDragLocation;
        double angleX = 0;
        double angleZ = 0;
        ToolTip toolTipForAnnotation;

        public PanelFor3DView(CurrentTrajectory currentTrajectory, DisplayChoice displayChoice)
        {
            paddingX = 20;
            paddingY = 10;
            paddingForAxis = (int)(currentTrajectory.Radius / 10);
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;

            sideWidthOfXYZAxisDrawingArea = 100;

            this.currentTrajectory = currentTrajectory;
            this.displayChoice = displayChoice;
            intermediareNodes = currentTrajectory.Nodes.Select(p => (Vector3)p).ToList();
            InitializeAxis();
            InitializeXYZAxis();
            SetCamera();

            toolTipForAnnotation = new ToolTip()
            {
                AutoPopDelay = 50000,
                InitialDelay = 500,
                ReshowDelay = 100,
                ShowAlways = true,
            };

            Paint += new PaintEventHandler(PaintPanel);
            MouseMove += new MouseEventHandler(Panel_MouseMove);
            MouseDown += new MouseEventHandler(Panel_MouseDown);
            MouseUp += new MouseEventHandler(Panel_MouseUp);
        }

        private void SetCamera()
        {
            if (currentTrajectory != null)
            {
                //positionOfCamera.X = (currentTrajectory.MaxX + currentTrajectory.MinX) / 2;
                //positionOfCamera.Y = 2 * currentTrajectory.Radius;
                //positionOfCamera.Z = (currentTrajectory.MaxZ + currentTrajectory.MinZ) / 2;


                positionOfCamera.X = 0;
                positionOfCamera.Y = 2 * currentTrajectory.Radius;
                positionOfCamera.Z = 0;


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

        private PointF[] GetProjectionFrom3DTo2DForXYZAxis(List<Vector3> coordinatesInObject)
        {

            var coordinatesInWorld = Projection.GetCoordinatesInWorldCoordinatesSystem(coordinatesInObject, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
            var coordinatesInCamera = Projection.GetCoordinatesInCameraCoordinatesSystem(coordinatesInWorld, positionOfCamera);
            var coordinatesInImage = Projection.GetParallelCoordinatesInImageCoordinatesSystem(coordinatesInCamera);

            //var coordinatesInWorld = Projection.GetCoordinatesInWorldCoordinatesSystem(coordinatesInObject, Vector3.Zero, angleX, angleZ);
            //var coordinatesInCamera = Projection.GetCoordinatesInCameraCoordinatesSystem(coordinatesInWorld, Vector3.Multiply(Vector3.Normalize(positionOfCamera), sideWidthOfXYZAxisDrawingArea));
            //var coordinatesInImage = Projection.GetParallelCoordinatesInImageCoordinatesSystem(coordinatesInCamera);
            var coordinatesInCanvas = Projection.GetRasterCoordinateInCanvasCoordiantesSystem(coordinatesInImage, sideWidthOfXYZAxisDrawingArea, sideWidthOfXYZAxisDrawingArea, sideWidthOfXYZAxisDrawingArea);

            //foreach (var coordinate in coordinatesInCanvas)
            //{
            //    MessageBox.Show(coordinate[0].ToString() + ", " + coordinate[1].ToString());
            //}
            return coordinatesInCanvas.Select(p => new PointF(p[0] + XYZAxisDrawingArea.X, p[1] + XYZAxisDrawingArea.Y + sideWidthOfXYZAxisDrawingArea / 2)).ToArray();
        }

        private void InitializeAxis()
        {


            axis = (new Vector3[] { new Vector3(currentTrajectory.MinX - paddingForAxis, currentTrajectory.MinY - paddingForAxis, currentTrajectory.MinZ - paddingForAxis),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY - paddingForAxis, currentTrajectory.MinZ - paddingForAxis),
                new Vector3(currentTrajectory.MinX - paddingForAxis, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ - paddingForAxis),
                new Vector3(currentTrajectory.MinX - paddingForAxis, currentTrajectory.MinY - paddingForAxis, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MinX - paddingForAxis, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY - paddingForAxis, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ - paddingForAxis),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MaxZ + paddingForAxis) }).ToList();
            axis = Projection.Initialize(axis, currentTrajectory.CenterOfTrajectory);
        }

        private void PaintPanel(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            //angleZ = 5 * Math.PI / 180;
            //angleX = 5 * Math.PI / 180;

            pointsOnCanvas = GetProjectionFrom3DTo2D(intermediareNodes);
            
            //DrawTrajectory(graphics);
            //HighlightDataPoint(graphics);
            DrawAxis(graphics, Pens.Black);
            DrawAnnotation(graphics);
            DrawSharpestPoint(graphics);

            angleZ = 15 * Math.PI / 180;
            angleX = 15 * Math.PI / 180;

            pointsOnCanvas = GetProjectionFrom3DTo2D(intermediareNodes);
            //DrawTrajectory(graphics);
            //HighlightDataPoint(graphics);
            DrawAxis(graphics, Pens.Red);
            //DrawXYZAxis(graphics);
            graphics.Dispose();
        }

        private void DrawTrajectory(Graphics graphics)
        {
            using (Pen penForLine = new Pen(Color.FromArgb(204, 234, 187), 3.0F))
            {
                for (int i = 0; i < pointsOnCanvas.Length - 1; i = i + 1)
                {
                    graphics.DrawLine(penForLine, pointsOnCanvas[i].X, pointsOnCanvas[i].Y, pointsOnCanvas[i + 1].X, pointsOnCanvas[i + 1].Y);
                }
            }
        }

        private void HighlightDataPoint(Graphics graphics)
        {
            using (SolidBrush brushForPoint = new SolidBrush(Color.Red))
            {
                foreach (var point in pointsOnCanvas)
                {
                    graphics.FillRectangle(brushForPoint, point.X - 1, point.Y - 1, 2, 2);
                }
            }
        }

        private void DrawAxis(Graphics graphics, Pen penForLine)
        {
            var axisOnCanvas = GetProjectionFrom3DTo2D(axis);
            //using (Pen penForLine = new Pen(Color.FromArgb(63, 63, 68), 2.0F))
            //{
                graphics.DrawLine(penForLine, axisOnCanvas[0], axisOnCanvas[1]);
                graphics.DrawLine(penForLine, axisOnCanvas[0], axisOnCanvas[2]);
                graphics.DrawLine(penForLine, axisOnCanvas[0], axisOnCanvas[3]);

                graphics.DrawLine(penForLine, axisOnCanvas[7], axisOnCanvas[4]);
                graphics.DrawLine(penForLine, axisOnCanvas[7], axisOnCanvas[5]);
                graphics.DrawLine(penForLine, axisOnCanvas[7], axisOnCanvas[6]);

                graphics.DrawLine(penForLine, axisOnCanvas[1], axisOnCanvas[6]);
                graphics.DrawLine(penForLine, axisOnCanvas[2], axisOnCanvas[6]);
                graphics.DrawLine(penForLine, axisOnCanvas[1], axisOnCanvas[5]);
                graphics.DrawLine(penForLine, axisOnCanvas[3], axisOnCanvas[5]);
                graphics.DrawLine(penForLine, axisOnCanvas[3], axisOnCanvas[4]);
                graphics.DrawLine(penForLine, axisOnCanvas[2], axisOnCanvas[4]);
            //}
        }


        private void DrawAnnotation(Graphics graphics)
        {
            if (displayChoice.IfShowAnnotation)
            {
                using (Font textFont = new Font("Microsoft YaHei", 6, FontStyle.Regular, GraphicsUnit.Point))
                {
                    Dictionary<String, int> pointAnnotationDictionary = new Dictionary<String, int>();
                    for (int i = 0; i < currentTrajectory.Nodes.Count; i = i + 1)
                    {
                        if (!pointAnnotationDictionary.ContainsKey(currentTrajectory.Nodes[i].ToString()))
                        {
                            graphics.DrawString(currentTrajectory.Nodes[i].ToString(), textFont, Brushes.Black, pointsOnCanvas[i].X + 2, pointsOnCanvas[i].Y);
                            pointAnnotationDictionary.Add(currentTrajectory.Nodes[i].ToString(), 1);
                        }
                    }
                }
            }
        }

        private void DrawSharpestPoint(Graphics graphics)
        {
            if (displayChoice.IfShowSharpestPoint)
            {
                using (SolidBrush brushForPoint = new SolidBrush(Color.Blue))
                {
                    foreach (var index in currentTrajectory.SharpestPointIndex)
                    {
                        graphics.FillRectangle(brushForPoint, pointsOnCanvas[index].X - 2, pointsOnCanvas[index].Y - 2, 4, 4);
                    }
                }
            }
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
            else
            {
                string tip = "";
                int radius = 4;
                for (int i = 0; i < pointsOnCanvas.Length; i = i + 1)
                {
                    if ((Math.Abs(e.X - pointsOnCanvas[i].X) < radius) &&
                        (Math.Abs(e.Y - pointsOnCanvas[i].Y) < radius))
                    {
                        tip = currentTrajectory.Nodes[i].ToString();
                        break;
                    }
                }
                if (toolTipForAnnotation.GetToolTip(this) != tip)
                {
                    toolTipForAnnotation.SetToolTip(this, tip);
                }
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

        private void InitializeXYZAxis()
        {
            int sideWidthOfAxisPyramid = (int)sideWidthOfXYZAxisDrawingArea / 10;
            int heightOfAxisPyramid = (int)sideWidthOfXYZAxisDrawingArea / 10;
            int lengthOfAxis = sideWidthOfXYZAxisDrawingArea / 4;
            xAxis = (new Vector3[] { new Vector3(0, 0, 0),
                new Vector3(lengthOfAxis, 0, 0),
                new Vector3(lengthOfAxis, 0, - sideWidthOfAxisPyramid / 2),
                new Vector3(lengthOfAxis, 0, sideWidthOfAxisPyramid / 2),
                new Vector3(lengthOfAxis + heightOfAxisPyramid, 0, 0),
                //new Vector3(sideWidthOfXYZAxisDrawingArea / 3, sideWidthOfAxisPyramid / 2, sideWidthOfAxisPyramid / 2),
                //new Vector3(sideWidthOfXYZAxisDrawingArea / 3, - sideWidthOfAxisPyramid / 2, sideWidthOfAxisPyramid / 2),
                //new Vector3(sideWidthOfXYZAxisDrawingArea / 3, sideWidthOfAxisPyramid / 2, - sideWidthOfAxisPyramid / 2),
                //new Vector3(sideWidthOfXYZAxisDrawingArea / 3, - sideWidthOfAxisPyramid / 2, - sideWidthOfAxisPyramid / 2),
                //new Vector3(sideWidthOfXYZAxisDrawingArea / 3 + heightOfAxisPyramid, 0, 0)
            }).ToList();
            yAxis = (new Vector3[] { new Vector3(0, 0, 0),
                new Vector3(0, lengthOfAxis, 0),
                new Vector3(0, lengthOfAxis, - sideWidthOfAxisPyramid / 2),
                new Vector3(0, lengthOfAxis, sideWidthOfAxisPyramid / 2),
                new Vector3(0, lengthOfAxis + heightOfAxisPyramid, 0),
            }).ToList();
            zAxis = (new Vector3[] { new Vector3(0, 0, 0),
                new Vector3(0, 0, lengthOfAxis),
                new Vector3(- sideWidthOfAxisPyramid / 2, 0, lengthOfAxis),
                new Vector3(sideWidthOfAxisPyramid / 2, 0, lengthOfAxis),
                new Vector3(0, 0, lengthOfAxis + heightOfAxisPyramid),
            }).ToList();
        }

        private void DrawXYZAxis(Graphics graphics)
        {
            var xAxisOnCanvas = GetProjectionFrom3DTo2DForXYZAxis(xAxis);

            graphics.DrawLine(Pens.Red, xAxisOnCanvas[0], xAxisOnCanvas[1]);
            var pointsForXAxisTriangle = new PointF[] { xAxisOnCanvas[2], xAxisOnCanvas[3], xAxisOnCanvas[4] };
            graphics.FillPolygon(Brushes.Red, pointsForXAxisTriangle);
            graphics.DrawString("X", Control.DefaultFont, Brushes.Black, xAxisOnCanvas[4].X + 2, xAxisOnCanvas[4].Y + 2);

            var yAxisOnCanvas = GetProjectionFrom3DTo2DForXYZAxis(yAxis);

            graphics.DrawLine(Pens.Blue, yAxisOnCanvas[0], yAxisOnCanvas[1]);
            var pointsForYAxisTriangle = new PointF[] { yAxisOnCanvas[2], yAxisOnCanvas[3], yAxisOnCanvas[4] };
            graphics.FillPolygon(Brushes.Blue, pointsForYAxisTriangle);
            graphics.DrawString("Y", Control.DefaultFont, Brushes.Black, yAxisOnCanvas[4].X + 2, yAxisOnCanvas[4].Y + 2);

            var zAxisOnCanvas = GetProjectionFrom3DTo2DForXYZAxis(zAxis);

            graphics.DrawLine(Pens.Black, zAxisOnCanvas[0], zAxisOnCanvas[1]);
            var pointsForZAxisTriangle = new PointF[] { zAxisOnCanvas[2], zAxisOnCanvas[3], zAxisOnCanvas[4] };
            graphics.FillPolygon(Brushes.Black, pointsForZAxisTriangle);
            graphics.DrawString("Z", Control.DefaultFont, Brushes.Black, zAxisOnCanvas[4].X + 2, zAxisOnCanvas[4].Y + 2);
            //var pointsForPyramidBottomOnCanvas = new PointF[] { xyzAxisOnCanvas[2], xyzAxisOnCanvas[3], xyzAxisOnCanvas[4], xyzAxisOnCanvas[5] };
            //graphics.FillPolygon(Brushes.Red, pointsForPyramidBottomOnCanvas);

            //var pointsForPyramidSide1OnCanvas = new PointF[] { xyzAxisOnCanvas[2], xyzAxisOnCanvas[3], xyzAxisOnCanvas[6] };
            //graphics.FillPolygon(Brushes.Red, pointsForPyramidSide1OnCanvas);

            //var pointsForPyramidSide2OnCanvas = new PointF[] { xyzAxisOnCanvas[2], xyzAxisOnCanvas[4], xyzAxisOnCanvas[6] };
            //graphics.FillPolygon(Brushes.Red, pointsForPyramidSide2OnCanvas);

            //var pointsForPyramidSide3OnCanvas = new PointF[] { xyzAxisOnCanvas[5], xyzAxisOnCanvas[3], xyzAxisOnCanvas[6] };
            //graphics.FillPolygon(Brushes.Red, pointsForPyramidSide3OnCanvas);

            //var pointsForPyramidSide4OnCanvas = new PointF[] { xyzAxisOnCanvas[5], xyzAxisOnCanvas[4], xyzAxisOnCanvas[6] };
            //graphics.FillPolygon(Brushes.Red, pointsForPyramidSide4OnCanvas);

        }
    }
}
