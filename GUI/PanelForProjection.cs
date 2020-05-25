using GeometricObject;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Drawing.Drawing2D;

namespace Well_Trajectory_Visualization
{
    public class PanelForProjection : Panel
    {
        Projection projection = new Projection();

        readonly int numberOfDataInAxisX;
        readonly int numberOfDataInAxisY;
        readonly int spaceHeightForViewName;
        readonly int segementLength;
        readonly int paddingX;
        readonly int paddingY;
        readonly int marginAxis;
        readonly int widthOfCoordinate;
        readonly int heightOfCoordinate;
        private string axisXCaption;
        private string axisYCaption;

        private Single minX;
        private Single minY;
        private Single zoomXY;
        private Single zoomZ;

        Single zoomInAxisParameter;
        ToolTip toolTipForAnnotation;

        public Trajectory CurrentTrajectory
        {
            get; set;
        }

        public List<PointIn2D> TrajectoryProjectionIn2D
        {
            get;
        }

        public PointF[] TrajectoryProjectionLocationOnPanel
        {
            get;
        }

        public List<int> SharpestPointProjectionIndex
        {
            get; set;
        }

        public bool AddAnnotation
        {
            get; set;
        }
        public bool AddSharpestPoint
        {
            get; set;
        }

        public PanelForProjection(Vector3 normalVector, Trajectory currentTrajectory, Single zoomXY, Single zoomZ, bool addAnnotation)
        {
            //initialize panel property
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;
            Name = projection.GetProjectionView(normalVector);
            TrajectoryProjectionIn2D = projection.GetProjectionInPlane(currentTrajectory.PolyLineNodes, normalVector);
            TrajectoryProjectionLocationOnPanel = new PointF[TrajectoryProjectionIn2D.Count];
            CurrentTrajectory = currentTrajectory;
            
            this.zoomXY = zoomXY;
            this.zoomZ = zoomZ;
            minX = TrajectoryProjectionIn2D.Select(x => x.X).Min();
            minY = TrajectoryProjectionIn2D.Select(x => x.Y).Min();
            AddAnnotation = addAnnotation;
            SetAxisCaption();

            //Initialize drawing property

            numberOfDataInAxisX = 5;
            numberOfDataInAxisY = 10;
            spaceHeightForViewName = 25;
            segementLength = 5;
            marginAxis = 10;
            widthOfCoordinate = 40;
            heightOfCoordinate = 18;
            paddingX = segementLength * 3 / 2 + marginAxis + widthOfCoordinate + 5;
            paddingY = segementLength * 3 / 2 + marginAxis + heightOfCoordinate + 5;

            //tool tip
            toolTipForAnnotation = new ToolTip()
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 100,
                ShowAlways = true,
            };

            this.Paint += new PaintEventHandler(PaintPanel);
            this.MouseMove += new MouseEventHandler(Panel_MouseMove);
        }

        ///////Helper Methods//////////
        private void SetAxisCaption()
        {
            switch (Name)
            {
                case "Main View":
                    axisXCaption = "x/";
                    axisYCaption = "z/";
                    break;
                case "Left View":
                    axisXCaption = "y/";
                    axisYCaption = "z/";
                    break;
                case "Top View":
                default:
                    axisXCaption = "x/";
                    axisYCaption = "y/";
                    break;
            }

            string unit;
            switch (CurrentTrajectory.Unit)
            {
                case DistanceUnit.Meter:
                    unit = "m";
                    break;
                case DistanceUnit.Feet:
                default:
                    unit = "f";
                    break;
            }

            axisXCaption += unit;
            axisYCaption += unit;
        }

        private void GetZoomInAxisParameter()
        {
            if (this.zoomZ * (this.Width - 2 * paddingX) > zoomXY * (this.Height - 2 * paddingY - spaceHeightForViewName))
            {
                zoomInAxisParameter = (this.Height - 2 * paddingY - spaceHeightForViewName) / zoomZ;
            }
            else
            {
                zoomInAxisParameter = (this.Width - 2 * paddingX) / zoomXY;
            }
        }

        /// Paint Panel///
        private void PaintPanel(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            GetZoomInAxisParameter();
            int spaceX = (int)(paddingX - minX * zoomInAxisParameter);
            int spaceY = (int)(paddingY + spaceHeightForViewName - minY * zoomInAxisParameter);

            for (int i = 0; i < TrajectoryProjectionIn2D.Count; i = i + 1)
            {
                float xForPaint = TrajectoryProjectionIn2D[i].X * zoomInAxisParameter + spaceX;
                float yForPaint = TrajectoryProjectionIn2D[i].Y * zoomInAxisParameter + spaceY;
                TrajectoryProjectionLocationOnPanel[i] = new PointF(xForPaint, yForPaint);
            }
            //GraphicsPath pathOfTrajectory = new GraphicsPath();
            //pathOfTrajectory.AddLines(TrajectoryProjectionLocationOnPanel);

            // points limit 8517?
            using (Pen penForLine = new Pen(Color.FromArgb(204, 234, 187), 3.0F))
            {
                //graphics.DrawLines(penForLine, TrajectoryProjectionLocationOnPanel);
                //graphics.DrawPath(penForLine, pathOfTrajectory);
                for (int i = 0; i < TrajectoryProjectionLocationOnPanel.Length - 1; i = i + 1)
                {
                    graphics.DrawLine(penForLine, TrajectoryProjectionLocationOnPanel[i].X, TrajectoryProjectionLocationOnPanel[i].Y, TrajectoryProjectionLocationOnPanel[i + 1].X, TrajectoryProjectionLocationOnPanel[i + 1].Y);
                }
            }

            // highlight data points
            using (SolidBrush brushForPoint = new SolidBrush(Color.FromArgb(63, 63, 68)))
            {
                foreach (var location in TrajectoryProjectionLocationOnPanel)
                {
                    graphics.FillRectangle(brushForPoint, location.X - 1, location.Y - 1, 2, 2);
                }
            }

            // draw caption
            using (Font fontForCaption = new Font("Microsoft YaHei", 11, FontStyle.Regular, GraphicsUnit.Point))
            {
                Rectangle rect = new Rectangle(0, 0, this.Width, spaceHeightForViewName);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                graphics.DrawString(this.Name, fontForCaption, Brushes.Black, rect, stringFormat);
            }
            //draw axis
            graphics = DrawAxis(graphics, spaceX, spaceY);

            //draw annotation
            if (AddAnnotation)
            {
                graphics = DrawAnnotation(graphics, TrajectoryProjectionIn2D, spaceX, spaceY);
            }
          
            
            if (AddSharpestPoint)
            {
                using (SolidBrush brushForPoint = new SolidBrush(Color.Red))
                {
                    foreach (var index in SharpestPointProjectionIndex)
                    {
                        graphics.FillRectangle(brushForPoint, TrajectoryProjectionLocationOnPanel[index].X - 1, TrajectoryProjectionLocationOnPanel[index].Y - 1, 2, 2);
                    }
                }
            }
            graphics.Dispose();

        }

        ////Draw Axis///
        private Graphics DrawAxis(Graphics graphics, int spaceX, int spaceY)
        {
            PointF upperLeftAxisPoint = new PointF(paddingX - marginAxis, spaceHeightForViewName + paddingY - marginAxis);
            PointF upperRightAxisPoint = new PointF(this.Width - paddingX + marginAxis, spaceHeightForViewName + paddingY - marginAxis);
            PointF lowerLeftAxisPoint = new PointF(paddingX - marginAxis, this.Height - paddingY + marginAxis);
            PointF lowerRightAxisPoint = new PointF(this.Width - paddingX + marginAxis, this.Height - paddingY + marginAxis);
            Font textFont = this.Font;

            using (Pen penForAxis = new Pen(Color.Black, 0.3F))
            {
                // draw axis border
                graphics.DrawLine(penForAxis, upperLeftAxisPoint, upperRightAxisPoint);
                graphics.DrawLine(penForAxis, upperRightAxisPoint, lowerRightAxisPoint);
                graphics.DrawLine(penForAxis, lowerRightAxisPoint, lowerLeftAxisPoint);
                graphics.DrawLine(penForAxis, lowerLeftAxisPoint, upperLeftAxisPoint);

                // draw axis name
                Rectangle xAxisCaptionRect = new Rectangle((int)(upperRightAxisPoint.X + segementLength), (int)(upperRightAxisPoint.Y - heightOfCoordinate / 2), widthOfCoordinate, heightOfCoordinate);
                Rectangle yAxisCaptionRect = new Rectangle((int)(lowerLeftAxisPoint.X - widthOfCoordinate / 2), (int)(lowerLeftAxisPoint.Y + segementLength), widthOfCoordinate, heightOfCoordinate);
                StringFormat axisCaptionStringFormat = new StringFormat();
                axisCaptionStringFormat.Alignment = StringAlignment.Center;
                graphics.DrawString(axisXCaption, textFont, Brushes.Black, xAxisCaptionRect, axisCaptionStringFormat);
                graphics.DrawString(axisYCaption, textFont, Brushes.Black, yAxisCaptionRect, axisCaptionStringFormat);

                // draw number in axis
                float widthOfAxis = upperRightAxisPoint.X - upperLeftAxisPoint.X;
                float heightOfAxis = lowerLeftAxisPoint.Y - upperLeftAxisPoint.Y;
                StringFormat xAxisNumberFormat = new StringFormat();
                xAxisNumberFormat.Alignment = StringAlignment.Center;

                float coordinateX = upperLeftAxisPoint.X;
                while (coordinateX <= upperRightAxisPoint.X)
                {
                    float coordinateXInReal = (coordinateX - spaceX) / zoomInAxisParameter;

                    Rectangle rectangleForNumberInAxisX = new Rectangle((int)(coordinateX - widthOfCoordinate / 2), (int)(upperLeftAxisPoint.Y - segementLength * 3 / 2 - heightOfCoordinate), widthOfCoordinate, heightOfCoordinate);
                    graphics.DrawLine(penForAxis, coordinateX, upperLeftAxisPoint.Y, coordinateX, upperLeftAxisPoint.Y - segementLength);
                    graphics.DrawString(((int)coordinateXInReal).ToString(), textFont, Brushes.Black, rectangleForNumberInAxisX, xAxisNumberFormat);

                    coordinateX = coordinateX + widthOfAxis / numberOfDataInAxisX;
                }

                StringFormat yAxisNumberFormat = new StringFormat();
                yAxisNumberFormat.Alignment = StringAlignment.Far;

                float coordinateY = upperLeftAxisPoint.Y;
                while (coordinateY <= lowerLeftAxisPoint.Y)
                {
                    float coordinateYInReal = (coordinateY - spaceY) / zoomInAxisParameter;

                    Rectangle rectangleForNumberInAxisY = new Rectangle((int)(upperLeftAxisPoint.X - segementLength * 3 / 2 - widthOfCoordinate), (int)(coordinateY - heightOfCoordinate / 2), widthOfCoordinate, heightOfCoordinate);
                    graphics.DrawLine(penForAxis, upperLeftAxisPoint.X, coordinateY, upperLeftAxisPoint.X - segementLength, coordinateY);
                    graphics.DrawString(((int)coordinateYInReal).ToString(), textFont, Brushes.Black, rectangleForNumberInAxisY, yAxisNumberFormat);

                    coordinateY = coordinateY + heightOfAxis / numberOfDataInAxisY;
                }
            }
            return graphics;
        }

        /// Draw Annotation////
        private Graphics DrawAnnotation(Graphics graphics, List<PointIn2D> projectionPointIn2D, int spaceX, int spaceY)
        {
            using (Font textFont = new Font("Microsoft YaHei", 6, FontStyle.Regular, GraphicsUnit.Point))
            {
                Dictionary<String, int> pointAnnotationDictionary = new Dictionary<String, int>();
                foreach (var point in projectionPointIn2D)
                {
                    float xForPaint = point.X * zoomInAxisParameter + spaceX;
                    float yForPaint = point.Y * zoomInAxisParameter + spaceY;
                    PointF locationOfPoint = new PointF(xForPaint + 4, yForPaint - 4);
                    String pointAnnotation = $"({Math.Round(point.X, 1)}, {Math.Round(point.Y, 1)})";
                    if (!pointAnnotationDictionary.ContainsKey(pointAnnotation))
                    {
                        graphics.DrawString(pointAnnotation, textFont, Brushes.Black, locationOfPoint);
                        pointAnnotationDictionary.Add(pointAnnotation, 1);
                    }
                }
            }
            return graphics;
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            PanelForProjection panel = (PanelForProjection)sender;
            string tip = "";
            int radius = 4;
            int index = 0;
            foreach (var location in TrajectoryProjectionLocationOnPanel)
            {
                if ((Math.Abs(e.X - location.X) < radius) &&
                    (Math.Abs(e.Y - location.Y) < radius))
                {
                    tip = $"({Math.Round(panel.TrajectoryProjectionIn2D[index].X, 1)}, {Math.Round(panel.TrajectoryProjectionIn2D[index].Y, 1)})";
                    break;
                }
                index = index + 1;
            }
            if (toolTipForAnnotation.GetToolTip(panel) != tip)
            {
                toolTipForAnnotation.SetToolTip(panel, tip);
            }
        }
    }
}




//public List<Vector3> LargestInflectionPoint
//{
//    get; set;
//}

//public Single ZoomXY
//{
//    get; set;
//}

//public Single ZoomZ
//{
//    get; set;
//}

//private void AddAnnotationUpdate(object sender, EventArgs e)
//{
//    AddAnnotation = (bool) sender;
//}

