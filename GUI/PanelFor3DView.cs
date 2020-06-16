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
        List<Vector3> axis;
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

        bool isDrag = false;
        Point beginDragLocation;
        double angleX;
        double angleZ;
        ToolTip toolTipForAnnotation;
        float zoom;
        float offsetX;
        float offsetY;
        public PanelFor3DView(CurrentTrajectory currentTrajectory, DisplayChoice displayChoice)
        {
            paddingForAxis = Math.Max((int)(currentTrajectory.Radius / 10), 5);
            paddingForAxisNotation = Math.Max((int)(currentTrajectory.Radius / 20), 2);
            paddingX = 20;
            paddingY = 10;

            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;

            zoom = 0;
            offsetX = 0;
            offsetY = 0;
            angleX = 0;
            //- 10 * Math.PI / 180;
            angleZ = 0;
                //- 10 * Math.PI / 180;

            this.currentTrajectory = currentTrajectory;
            this.displayChoice = displayChoice;
            intermediareNodes = currentTrajectory.Nodes.Select(p => (Vector3)p).ToList();
            InitializeAxis();
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

        private PointF[] GetProjectionFrom3DTo2D(List<Vector3> coordinatesInObject)
        {
            var coordinatesInWorld = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(coordinatesInObject, angleX, angleZ);
            var coordinatesInImage = Projection.GetParallelCoordinatesInImageCoordinatesSystem(coordinatesInWorld);
            
            var sizeOfCanvas = Math.Min(WidthOfCanvas, HeightOfCanvas);
            var coordinatesInCanvas = Projection.GetRasterCoordinateInCanvasCoordiantesSystem(coordinatesInImage, sizeOfScreen, sizeOfCanvas, sizeOfCanvas, zoom, offsetX, offsetY);
            return coordinatesInCanvas.Select(p => new PointF(p[0] + paddingX + Math.Max((WidthOfCanvas - HeightOfCanvas) / 2, 0), p[1] + paddingY + Math.Max((HeightOfCanvas - WidthOfCanvas) / 2, 0))).ToArray();
        }

        private void InitializeAxis()
        {
            axis = (new Vector3[] { new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MinY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MaxZ),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY, currentTrajectory.MaxZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MinY, currentTrajectory.MaxZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MaxY, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MaxX, currentTrajectory.MaxY, currentTrajectory.MaxZ),


            }).ToList();
            axis = Projection.FromObjectCoordinateToWorldCoordinate(axis, currentTrajectory.CenterOfTrajectory);
            axis = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(axis, angleX, angleZ);

        }

        private void InitializeXYZAxis()
        {
            float heightOfTheTriangle = paddingForAxisNotation / 2;
            float widthOfTheTriangle = paddingForAxisNotation;

            xAxis = (new Vector3[] { 
                //Axis end point
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY, currentTrajectory.MinZ),
                
                //Axis notation
                new Vector3(currentTrajectory.MaxX + paddingForAxis + paddingForAxisNotation, currentTrajectory.MinY, currentTrajectory.MinZ),

                //Axis triangle
                new Vector3(currentTrajectory.MaxX + paddingForAxis + heightOfTheTriangle, currentTrajectory.MinY, currentTrajectory.MinZ),

                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY + widthOfTheTriangle / 2, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY - widthOfTheTriangle / 2, currentTrajectory.MinZ),

                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY, currentTrajectory.MinZ + widthOfTheTriangle / 2),
                new Vector3(currentTrajectory.MaxX + paddingForAxis, currentTrajectory.MinY, currentTrajectory.MinZ - widthOfTheTriangle / 2),


            }).ToList();


            yAxis = (new Vector3[] {
                //Axis end point
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ),
                
                //Axis notation
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis + paddingForAxisNotation, currentTrajectory.MinZ),
                
                //Axis triangle
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis + heightOfTheTriangle, currentTrajectory.MinZ),

                new Vector3(currentTrajectory.MinX + widthOfTheTriangle / 2, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ),
                new Vector3(currentTrajectory.MinX - widthOfTheTriangle / 2, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ),

                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ + widthOfTheTriangle / 2),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MaxY + paddingForAxis, currentTrajectory.MinZ - widthOfTheTriangle / 2),

            }).ToList();


            zAxis = (new Vector3[] {
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis),
                
                //Axis notation
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis + paddingForAxisNotation),
                             
                //Axis triangle
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis +  heightOfTheTriangle),


                new Vector3(currentTrajectory.MinX + widthOfTheTriangle / 2, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MinX - widthOfTheTriangle / 2, currentTrajectory.MinY, currentTrajectory.MaxZ + paddingForAxis),


                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY + widthOfTheTriangle / 2, currentTrajectory.MaxZ + paddingForAxis),
                new Vector3(currentTrajectory.MinX, currentTrajectory.MinY - widthOfTheTriangle / 2, currentTrajectory.MaxZ + paddingForAxis),

            }).ToList();

            xAxis = Projection.FromObjectCoordinateToWorldCoordinate(xAxis, currentTrajectory.CenterOfTrajectory);
            yAxis = Projection.FromObjectCoordinateToWorldCoordinate(yAxis, currentTrajectory.CenterOfTrajectory);
            zAxis = Projection.FromObjectCoordinateToWorldCoordinate(zAxis, currentTrajectory.CenterOfTrajectory);

            xAxis = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(xAxis, angleX, angleZ);
            yAxis = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(yAxis, angleX, angleZ);
            zAxis = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(zAxis, angleX, angleZ);

        }

        private void InitializeIntermediareNodes()
        {
            intermediareNodes = Projection.FromObjectCoordinateToWorldCoordinate(intermediareNodes, currentTrajectory.CenterOfTrajectory);
            intermediareNodes = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(intermediareNodes, angleX, angleZ);

        }


        private void PaintPanel(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            pointsOnCanvas = GetProjectionFrom3DTo2D(intermediareNodes);
            DrawTrajectory(graphics);
            HighlightDataPoint(graphics); 
            DrawAxis(graphics);
            DrawAnnotation(graphics);
            DrawSharpestPoint(graphics);
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
                    graphics.FillRectangle(brushForPoint, point.X - 2, point.Y - 2, 4, 4);
                }
            }
        }

        private void DrawAxis(Graphics graphics)
        {
            var axisOnCanvas = GetProjectionFrom3DTo2D(axis);
            using (Pen penForLine = new Pen(Color.FromArgb(63, 63, 68), 1.0F))
            {
                //graphics.DrawLine(penForLine, axisOnCanvas[0], axisOnCanvas[1]);
                //graphics.DrawLine(penForLine, axisOnCanvas[0], axisOnCanvas[2]);
                //graphics.DrawLine(penForLine, axisOnCanvas[0], axisOnCanvas[3]);

                graphics.DrawLine(penForLine, axisOnCanvas[7], axisOnCanvas[4]);
                graphics.DrawLine(penForLine, axisOnCanvas[7], axisOnCanvas[5]);
                graphics.DrawLine(penForLine, axisOnCanvas[7], axisOnCanvas[6]);

                graphics.DrawLine(penForLine, axisOnCanvas[1], axisOnCanvas[6]);
                graphics.DrawLine(penForLine, axisOnCanvas[2], axisOnCanvas[6]);
                graphics.DrawLine(penForLine, axisOnCanvas[1], axisOnCanvas[5]);
                graphics.DrawLine(penForLine, axisOnCanvas[3], axisOnCanvas[5]);
                graphics.DrawLine(penForLine, axisOnCanvas[3], axisOnCanvas[4]);
                graphics.DrawLine(penForLine, axisOnCanvas[2], axisOnCanvas[4]);
            }

            var origin = axisOnCanvas[0];

            using (Pen penForXAxis = new Pen(Color.Red, 1.0F))
            {
                var xAxisOnCanvas = GetProjectionFrom3DTo2D(xAxis);
                var xAxisEndPoint = xAxisOnCanvas[0];
                graphics.DrawLine(penForXAxis, origin, xAxisEndPoint);

                var xAxisNotationOnCanvas = xAxisOnCanvas[1];
                graphics.DrawString("X", Control.DefaultFont, Brushes.Red, xAxisNotationOnCanvas.X, xAxisNotationOnCanvas.Y);

                var xAxisTriangle1 = new PointF[] { xAxisOnCanvas[2], xAxisOnCanvas[3], xAxisOnCanvas[4] };
                var xAxisTriangle2 = new PointF[] { xAxisOnCanvas[2], xAxisOnCanvas[5], xAxisOnCanvas[6] };
                graphics.FillPolygon(Brushes.Red, xAxisTriangle1);
                graphics.FillPolygon(Brushes.Red, xAxisTriangle2);
            }

            using (Pen penForYAxis = new Pen(Color.Blue, 1.0F))
            {
                var yAxisOnCanvas = GetProjectionFrom3DTo2D(yAxis);
                var yAxisEndPoint = yAxisOnCanvas[0];
                graphics.DrawLine(penForYAxis, origin, yAxisEndPoint);

                var yAxisNotationOnCanvas = yAxisOnCanvas[1];
                graphics.DrawString("Y", Control.DefaultFont, Brushes.Blue, yAxisNotationOnCanvas.X, yAxisNotationOnCanvas.Y);

                var yAxisTriangle1 = new PointF[] { yAxisOnCanvas[2], yAxisOnCanvas[3], yAxisOnCanvas[4] };
                var yAxisTriangle2 = new PointF[] { yAxisOnCanvas[2], yAxisOnCanvas[5], yAxisOnCanvas[6] };
                graphics.FillPolygon(Brushes.Blue, yAxisTriangle1);
                graphics.FillPolygon(Brushes.Blue, yAxisTriangle2);
            }

            using (Pen penForZAxis = new Pen(Color.Green, 1.0F))
            {
                var zAxisOnCanvas = GetProjectionFrom3DTo2D(zAxis);
                var zAxisEndPoint = zAxisOnCanvas[0];
                graphics.DrawLine(penForZAxis, origin, zAxisEndPoint);

                var zAxisNotationOnCanvas = zAxisOnCanvas[1];
                graphics.DrawString("Z", Control.DefaultFont, Brushes.Green, zAxisNotationOnCanvas.X, zAxisNotationOnCanvas.Y);

                var zAxisTriangle1 = new PointF[] { zAxisOnCanvas[2], zAxisOnCanvas[3], zAxisOnCanvas[4] };
                var zAxisTriangle2 = new PointF[] { zAxisOnCanvas[2], zAxisOnCanvas[5], zAxisOnCanvas[6] };
                graphics.FillPolygon(Brushes.Green, zAxisTriangle1);
                graphics.FillPolygon(Brushes.Green, zAxisTriangle2);
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

                intermediareNodes = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(intermediareNodes, angleX, angleZ);
                axis = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(axis, angleX, angleZ);
                xAxis = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(xAxis, angleX, angleZ);
                yAxis = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(yAxis, angleX, angleZ);
                zAxis = Projection.GetCoordinatesInWorldCoordinatesSystemAfterRotation(zAxis, angleX, angleZ);
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
                offsetX = (float) (pointOnCanvas.X / WidthOfCanvas - 0.5) * zoom;
                offsetY = (float) ((HeightOfCanvas - pointOnCanvas.Y) / HeightOfCanvas - 0.5) * zoom;
                Refresh();
            }
        }
 
        private void Reset()
        {
            offsetX = 0;
            offsetY = 0;
            zoom = 0;
            angleX = 0;
            //- 10 * Math.PI / 180;
            angleZ = 0;
                //10 * Math.PI / 180;
            Refresh();
        }
    }
}
