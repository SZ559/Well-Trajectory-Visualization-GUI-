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

        float sizeOfScreen;

        List<Vector3> intermediareNodes;
        List<Vector3> boundingBox;
        PointF[] pointsOnCanvas;

        int paddingX;
        int paddingY;
        int paddingForAxis;
        int paddingForAxisNotation;

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

        float LargestAllowedSizeOfScreen
        {
            get
            {
                return (float)(2 * (currentTrajectory.Radius + Math.Sqrt(3) * paddingForAxis));
            }
        }

        List<Vector3> xAxis;
        List<Vector3> yAxis;
        List<Vector3> zAxis;

        ToolTip toolTipForAnnotation;

        bool isDrag = false;
        Point beginDragLocation;
        double angleX;
        double angleZ;

        float zoom;
        float offsetX;
        float offsetY;

        public PanelFor3DView(CurrentTrajectory currentTrajectory, DisplayChoice displayChoice)
        {
            paddingForAxis = Math.Max((int)(currentTrajectory.Radius / 10), 5);
            paddingForAxisNotation = Math.Max((int)(currentTrajectory.Radius / 30), 2);
            paddingX = 20;
            paddingY = 10;

            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;
            //BackColor = Color.FromArgb(149, 163, 166);

            zoom = 0;
            offsetX = 0;
            offsetY = 0;
            angleX = 0;
            //- 10 * Math.PI / 180;
            angleZ = 0;
            //- 10 * Math.PI / 180;

            this.currentTrajectory = currentTrajectory;
            this.displayChoice = displayChoice;
            InitializeIntermediareNodes();
            InitializeBoundingBox();
            InitializeXYZAxis();
            sizeOfScreen = LargestAllowedSizeOfScreen;


            InitializeIntermediareNodes();

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
            MouseWheel += new MouseEventHandler(Panel_MouseWheel);

        }

        private void InitializeBoundingBox()
        {
            boundingBox = (new Vector3[] { new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MinY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MaxZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY, currentTrajectory.MaxZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MinY, currentTrajectory.MaxZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MaxY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MaxY, currentTrajectory.MaxZ),
            }).ToList();

            boundingBox = Projection.GetCoordinatesInWorldCoordinatesSystem(boundingBox, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
        }

        private void InitializeXYZAxis()
        {
            float heightOfTheTriangle = paddingForAxisNotation / 3;
            float widthOfTheTriangle = paddingForAxisNotation / 3;

            xAxis = (new Vector3[] {
                
                // Axis beigin & end point
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY, currentTrajectory.MinZ),
                
                // Axis notation
                new Vector3(currentTrajectory.MaxX + paddingForAxis + paddingForAxisNotation, currentTrajectory.MinY, currentTrajectory.MinZ),

                // Axis triangle
                new Vector3(currentTrajectory.MaxX + paddingForAxis + heightOfTheTriangle, currentTrajectory.MinY, currentTrajectory.MinZ),

                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY + widthOfTheTriangle, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY - widthOfTheTriangle, currentTrajectory.MinZ),

                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY, currentTrajectory.MinZ + widthOfTheTriangle),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY, currentTrajectory.MinZ - widthOfTheTriangle),
            }).ToList();

            yAxis = (new Vector3[] {
                // Axis beigin & end point
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ),
                
                // Axis notation
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis + paddingForAxisNotation, currentTrajectory.MinZ),
                
                // Axis triangle
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis + heightOfTheTriangle, currentTrajectory.MinZ),

                new Vector3(currentTrajectory.MinX + widthOfTheTriangle, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX - widthOfTheTriangle, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ),

                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ + widthOfTheTriangle),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ - widthOfTheTriangle),
            }).ToList();

            zAxis = (new Vector3[] {
                // Axis beigin & end point
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis),
                
                // Axis notation
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis + paddingForAxisNotation),
                             
                // Axis triangle
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis +  heightOfTheTriangle),

                new Vector3(currentTrajectory.MinX + widthOfTheTriangle, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MinX - widthOfTheTriangle, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis),

                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY + widthOfTheTriangle, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY - widthOfTheTriangle, currentTrajectory.MaxZ + paddingForAxis),

            }).ToList();

            xAxis = Projection.GetCoordinatesInWorldCoordinatesSystem(xAxis, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
            yAxis = Projection.GetCoordinatesInWorldCoordinatesSystem(yAxis, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
            zAxis = Projection.GetCoordinatesInWorldCoordinatesSystem(zAxis, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
        }

        private void InitializeIntermediareNodes()
        {
            intermediareNodes = currentTrajectory.Nodes.Select(p => (Vector3)p).ToList();
            intermediareNodes = Projection.GetCoordinatesInWorldCoordinatesSystem(intermediareNodes, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
        }


        private PointF[] GetProjectionFrom3DTo2D(List<Vector3> coordinatesInObject)
        {
            var coordinatesInWorld = Projection.GetCoordinatesInWorldCoordinatesSystem(coordinatesInObject, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
            var coordinatesInImage = Projection.GetParallelCoordinatesInImageCoordinatesSystem(coordinatesInWorld, currentTrajectory.CenterOfTrajectory);

            var sizeOfCanvas = Math.Min(WidthOfCanvas, HeightOfCanvas);
            var coordinatesInCanvas = Projection.GetRasterCoordinateInCanvasCoordiantesSystem(coordinatesInImage, sizeOfScreen, sizeOfCanvas, sizeOfCanvas, zoom, offsetX, offsetY);
            return coordinatesInCanvas.Select(p => new PointF(p[0] + paddingX + Math.Max((WidthOfCanvas - HeightOfCanvas) / 2, 0), p[1] + paddingY + Math.Max((HeightOfCanvas - WidthOfCanvas) / 2, 0))).ToArray();
        }

        private void PaintPanel(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;

            DrawTrajectory(graphics);
            HighlightDataPoint(graphics);

            DrawBoundingBox(graphics);
            DrawAxis(graphics);

            DrawAnnotation(graphics);
            DrawSharpestPoint(graphics);

            graphics.Dispose();
        }

        private void DrawTrajectory(Graphics graphics)
        {
            pointsOnCanvas = GetProjectionFrom3DTo2D(intermediareNodes);
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
            using (SolidBrush brushForPoint = new SolidBrush(Color.FromArgb(63, 63, 68)))
            {
                foreach (var point in pointsOnCanvas)
                {
                    graphics.FillRectangle(brushForPoint, point.X - 1, point.Y - 1, 2, 2);
                }
            }
        }

        private void DrawBoundingBox(Graphics graphics)
        {
            var boundboxOnCanvas = GetProjectionFrom3DTo2D(boundingBox);
            using (Pen penForLine = new Pen(Color.FromArgb(63, 63, 68), 1.0F))
            {
                penForLine.DashPattern = new float[] { 5, 10 };

                //graphics.DrawLine(penForLine, axisOnCanvas[0], axisOnCanvas[1]);
                //graphics.DrawLine(penForLine, axisOnCanvas[0], axisOnCanvas[2]);
                //graphics.DrawLine(penForLine, axisOnCanvas[0], axisOnCanvas[3]);

                graphics.DrawLine(penForLine, boundboxOnCanvas[7], boundboxOnCanvas[4]);
                graphics.DrawLine(penForLine, boundboxOnCanvas[7], boundboxOnCanvas[5]);
                graphics.DrawLine(penForLine, boundboxOnCanvas[7], boundboxOnCanvas[6]);

                graphics.DrawLine(penForLine, boundboxOnCanvas[1], boundboxOnCanvas[6]);
                graphics.DrawLine(penForLine, boundboxOnCanvas[2], boundboxOnCanvas[6]);
                graphics.DrawLine(penForLine, boundboxOnCanvas[1], boundboxOnCanvas[5]);
                graphics.DrawLine(penForLine, boundboxOnCanvas[3], boundboxOnCanvas[5]);
                graphics.DrawLine(penForLine, boundboxOnCanvas[3], boundboxOnCanvas[4]);
                graphics.DrawLine(penForLine, boundboxOnCanvas[2], boundboxOnCanvas[4]);
            }
        }

        private void DrawAxis(Graphics graphics)
        {
            DrawSingleAxis(graphics, xAxis, Color.FromArgb(102, 194, 165), "X");
            DrawSingleAxis(graphics, yAxis, Color.FromArgb(252, 141, 98), "Y");
            DrawSingleAxis(graphics, zAxis, Color.FromArgb(141, 160, 203), "Z");
        }

        private void DrawSingleAxis(Graphics graphics, List<Vector3> axis, Color colorForLine, string notation)
        {
            using (Pen penForAxis = new Pen(colorForLine, 2.0F))
            {
                var axisOnCanvas = GetProjectionFrom3DTo2D(axis);
                var axisBeginPoint = axisOnCanvas[0];
                var axisEndPoint = axisOnCanvas[1];
                graphics.DrawLine(penForAxis, axisBeginPoint, axisEndPoint);

                var axisNotationOnCanvas = axisOnCanvas[2];
                graphics.DrawString(notation, Control.DefaultFont, Brushes.LightSlateGray, axisNotationOnCanvas.X, axisNotationOnCanvas.Y);

                var axisTriangle1 = new PointF[] { axisOnCanvas[3], axisOnCanvas[4], axisOnCanvas[5] };
                var axisTriangle2 = new PointF[] { axisOnCanvas[3], axisOnCanvas[6], axisOnCanvas[7] };
                graphics.FillPolygon(Brushes.LightSlateGray, axisTriangle1);
                graphics.FillPolygon(Brushes.LightSlateGray, axisTriangle2);
            }
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
                using (SolidBrush brushForPoint = new SolidBrush(Color.Red))
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
                Math.Atan2(e.X - beginDragLocation.X, Math.Max(Width, Height));
                angleX =
                //(beginDragLocation.Y - e.Y) * Math.PI / 200;
                //Math.Tanh(beginDragLocation.Y - e.Y) * Math.PI / 2;
                Math.Atan2(beginDragLocation.Y - e.Y, Math.Max(Width, Height));
                Refresh();
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
                boundingBox = Projection.GetCoordinatesInWorldCoordinatesSystem(boundingBox, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
                xAxis = Projection.GetCoordinatesInWorldCoordinatesSystem(xAxis, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
                yAxis = Projection.GetCoordinatesInWorldCoordinatesSystem(yAxis, currentTrajectory.CenterOfTrajectory, angleX, angleZ);
                zAxis = Projection.GetCoordinatesInWorldCoordinatesSystem(zAxis, currentTrajectory.CenterOfTrajectory, angleX, angleZ);

                angleX = 0;
                angleZ = 0;
            }
        }

        private void Panel_MouseWheel(object sender, MouseEventArgs e)
        {
            float zoomDeltaOfWheel = (float)(e.Delta / 120 * 0.2);
            float zoomAfterChange = zoom + zoomDeltaOfWheel;
            PointF pointOnCanvas = new PointF(e.X - paddingX, e.Y - paddingY);

            if (zoomAfterChange >= 0)
            {
                zoom = zoomAfterChange;
                offsetX = (float)(pointOnCanvas.X / WidthOfCanvas - 0.5) * zoom;
                offsetY = (float)((HeightOfCanvas - pointOnCanvas.Y) / HeightOfCanvas - 0.5) * zoom;
                Refresh();
            }
        }

        public void Reset()
        {
            offsetX = 0;
            offsetY = 0;
            zoom = 0;
            angleX = 0;
            //- 10 * Math.PI / 180;
            angleZ = 0;
            //10 * Math.PI / 180;

            InitializeIntermediareNodes();
            InitializeBoundingBox();
            InitializeXYZAxis();
        }
    }
}
