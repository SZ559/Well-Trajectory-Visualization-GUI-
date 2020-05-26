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
        readonly int leftPaddingX;
        readonly int rightPaddingX;
        readonly int paddingY;
        readonly int marginAxis;
        readonly int widthOfCoordinate;
        readonly int heightOfCoordinate;
        readonly int widthOfAxisName;
        readonly int heightOfAxisName;

        public string UnitForCaption
        {
            get
            {
                string unitForCaption;
                switch (CurrentTrajectory.Unit)
                {
                    case DistanceUnit.Meter:
                        unitForCaption = "m";
                        break;
                    case DistanceUnit.Feet:
                    default:
                        unitForCaption = "ft";
                        break;
                }
                return unitForCaption;
            }
        }

        public string AxisXCaption
        {
            get
            {
                string caption;
                switch (Name)
                {
                    case "Main":
                        caption = "x/";
                        break;
                    case "Left":
                        caption = "y/";
                        break;
                    case "Top":
                    default:
                        caption = "x/";
                        break;
                }

                return caption + UnitForCaption;
            }
        }

        public string AxisYCaption
        {
            get
            {
                string caption;
                switch (Name)
                {
                    case "Main":
                        caption = "z/";
                        break;
                    case "Left":
                        caption = "z/";
                        break;
                    case "Top":
                    default:
                        caption = "y/";
                        break;
                }

                return caption + UnitForCaption;
            }
        }

        private Single minX;
        private Single minY;
        private Single zoomXY;
        private Single zoomZ;

        Single zoomInAxisParameter
        {
            get
            {
                float zoomInAxisParameter;
                if (zoomZ * (Width - leftPaddingX - rightPaddingX) > zoomXY * (Height - 2 * paddingY - spaceHeightForViewName))
                {
                    zoomInAxisParameter = (Height - 2 * paddingY - spaceHeightForViewName) / zoomZ;
                }
                else
                {
                    zoomInAxisParameter = (Width - leftPaddingX - rightPaddingX) / zoomXY;
                }
                return zoomInAxisParameter;
            }
        }

        ToolTip toolTipForAnnotation;

        public Vector3 NormalVector
        {
            get; set;
        }

        public Trajectory CurrentTrajectory
        {
            get; set;
        }


        public List<PointIn2D> TrajectoryProjectionIn2D
        {
            get; set;
        }

        public PointF[] TrajectoryProjectionLocationOnPanel
        {
            get; set;
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

        public PanelForProjection(Vector3 normalVector, Trajectory currentTrajectory, bool addAnnotation)
        {
            //initialize panel property
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;

            CurrentTrajectory = currentTrajectory;
            NormalVector = normalVector;

            Name = projection.GetProjectionView(NormalVector);
            TrajectoryProjectionIn2D = projection.GetProjectionInPlane(CurrentTrajectory.PolyLineNodes, NormalVector);
            TrajectoryProjectionLocationOnPanel = new PointF[TrajectoryProjectionIn2D.Count];
            zoomXY = GetZoomXYForThreeViews(CurrentTrajectory);
            zoomZ = GetZoomZForThreeViews(CurrentTrajectory);
            minX = TrajectoryProjectionIn2D.Select(x => x.X).Min();
            minY = TrajectoryProjectionIn2D.Select(x => x.Y).Min();
            AddAnnotation = addAnnotation;

            //Initialize drawing property

            numberOfDataInAxisX = 5;
            numberOfDataInAxisY = 10;
            spaceHeightForViewName = 25;
            segementLength = 5;
            marginAxis = 10;
            widthOfCoordinate = 55;
            heightOfCoordinate = 18;
            widthOfAxisName = 30;
            heightOfAxisName = 18;
            leftPaddingX = segementLength * 3 / 2 + marginAxis + widthOfCoordinate + 5;
            rightPaddingX = segementLength / 2 + marginAxis + widthOfAxisName + 5;
            paddingY = segementLength * 3 / 2 + marginAxis + Math.Max(heightOfAxisName, heightOfCoordinate) + 5;

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

        public void UpdateParameters()
        {
            TrajectoryProjectionIn2D = projection.GetProjectionInPlane(CurrentTrajectory.PolyLineNodes, NormalVector);
            TrajectoryProjectionLocationOnPanel = new PointF[TrajectoryProjectionIn2D.Count];
            zoomXY = GetZoomXYForThreeViews(CurrentTrajectory);
            zoomZ = GetZoomZForThreeViews(CurrentTrajectory);
            minX = TrajectoryProjectionIn2D.Select(x => x.X).Min();
            minY = TrajectoryProjectionIn2D.Select(x => x.Y).Min();
        }


        /////////////// Helper functions //////////////

        private Single GetZoomXYForThreeViews(Trajectory trajectory)
        {
            Single zoomXY;
            Single maxX = trajectory.PolyLineNodes.Select(x => x.X).Max();
            Single maxY = trajectory.PolyLineNodes.Select(x => x.Y).Max();
            Single minX = trajectory.PolyLineNodes.Select(x => x.X).Min();
            Single minY = trajectory.PolyLineNodes.Select(x => x.Y).Min();

            zoomXY = Math.Max(maxX - minX, maxY - minY);
            return zoomXY > 0 ? zoomXY : 1;
        }

        private Single GetZoomZForThreeViews(Trajectory trajectory)
        {
            Single zoomZ;
            Single maxZ = trajectory.PolyLineNodes.Select(x => x.Z).Max();
            Single minZ = trajectory.PolyLineNodes.Select(x => x.Z).Min();
            zoomZ = maxZ - minZ;
            return zoomZ > 0 ? zoomZ : 1;
        }

        /// Paint Panel///
        private void PaintPanel(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            int spaceX = (int)(leftPaddingX - minX * zoomInAxisParameter);
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

            // draw sharpest point
            if (AddSharpestPoint)
            {
                using (SolidBrush brushForPoint = new SolidBrush(Color.Red))
                {
                    foreach (var index in SharpestPointProjectionIndex)
                    {
                        graphics.FillRectangle(brushForPoint, TrajectoryProjectionLocationOnPanel[index].X - 2, TrajectoryProjectionLocationOnPanel[index].Y - 2, 4, 4);
                    }
                }
            }
            graphics.Dispose();

        }

        ////Draw Axis///
        private Graphics DrawAxis(Graphics graphics, int spaceX, int spaceY)
        {
            PointF upperLeftAxisPoint = new PointF(leftPaddingX - marginAxis, spaceHeightForViewName + paddingY - marginAxis);
            PointF upperRightAxisPoint = new PointF(this.Width - rightPaddingX + marginAxis, spaceHeightForViewName + paddingY - marginAxis);
            PointF lowerLeftAxisPoint = new PointF(leftPaddingX - marginAxis, this.Height - paddingY + marginAxis);
            PointF lowerRightAxisPoint = new PointF(this.Width - rightPaddingX + marginAxis, this.Height - paddingY + marginAxis);
            Font textFont = this.Font;

            using (Pen penForAxis = new Pen(Color.Black, 0.3F))
            {
                // draw axis border
                graphics.DrawLine(penForAxis, upperLeftAxisPoint, upperRightAxisPoint);
                graphics.DrawLine(penForAxis, upperRightAxisPoint, lowerRightAxisPoint);
                graphics.DrawLine(penForAxis, lowerRightAxisPoint, lowerLeftAxisPoint);
                graphics.DrawLine(penForAxis, lowerLeftAxisPoint, upperLeftAxisPoint);

                // draw axis name
                Rectangle xAxisCaptionRect = new Rectangle((int)(upperRightAxisPoint.X + segementLength), (int)(upperRightAxisPoint.Y - heightOfAxisName / 2), widthOfAxisName, heightOfAxisName);
                Rectangle yAxisCaptionRect = new Rectangle((int)(lowerLeftAxisPoint.X - widthOfAxisName / 2), (int)(lowerLeftAxisPoint.Y + segementLength), widthOfAxisName, heightOfAxisName);
                StringFormat axisCaptionStringFormat = new StringFormat();
                axisCaptionStringFormat.Alignment = StringAlignment.Center;
                graphics.DrawString(AxisXCaption, textFont, Brushes.Black, xAxisCaptionRect, axisCaptionStringFormat);
                graphics.DrawString(AxisYCaption, textFont, Brushes.Black, yAxisCaptionRect, axisCaptionStringFormat);

                // draw number in axis
                float widthOfAxis = upperRightAxisPoint.X - upperLeftAxisPoint.X;
                float heightOfAxis = lowerLeftAxisPoint.Y - upperLeftAxisPoint.Y;

                // x-axis
                StringFormat xAxisNumberFormat = new StringFormat();
                xAxisNumberFormat.Alignment = StringAlignment.Center;

                int powerOfScientificNotionForXAxis = Math.Round(Math.Max(Math.Abs((upperLeftAxisPoint.X - spaceX) / zoomInAxisParameter), Math.Abs((upperRightAxisPoint.X - spaceX) / zoomInAxisParameter))).ToString().Length - 1;
                double divisorX = powerOfScientificNotionForXAxis >= 4 ? Math.Pow(10, powerOfScientificNotionForXAxis) : 1;
                string notionForXAxis = "";
                if (divisorX != 1)
                {
                    notionForXAxis += "E+" + powerOfScientificNotionForXAxis.ToString();
                }

                float coordinateX = upperLeftAxisPoint.X;
                while (coordinateX <= upperRightAxisPoint.X)
                {
                    float coordinateXInReal = (coordinateX - spaceX) / zoomInAxisParameter;

                    Rectangle rectangleForNumberInAxisX = new Rectangle((int)(coordinateX - widthOfCoordinate / 2), (int)(upperLeftAxisPoint.Y - segementLength * 3 / 2 - heightOfCoordinate), widthOfCoordinate, heightOfCoordinate);
                    graphics.DrawLine(penForAxis, coordinateX, upperLeftAxisPoint.Y, coordinateX, upperLeftAxisPoint.Y - segementLength);
                    graphics.DrawString((Math.Round(coordinateXInReal / divisorX, 1)).ToString() + notionForXAxis, textFont, Brushes.Black, rectangleForNumberInAxisX, xAxisNumberFormat);

                    coordinateX = coordinateX + widthOfAxis / numberOfDataInAxisX;
                }

                // y-axis
                StringFormat yAxisNumberFormat = new StringFormat();
                yAxisNumberFormat.Alignment = StringAlignment.Far;

                int powerOfScientificNotionForYAxis = Math.Round(Math.Max(Math.Abs((upperLeftAxisPoint.Y - spaceY) / zoomInAxisParameter), Math.Abs((lowerLeftAxisPoint.Y - spaceY) / zoomInAxisParameter))).ToString().Length - 1;
                double divisorY = powerOfScientificNotionForYAxis >= 4 ? Math.Pow(10, powerOfScientificNotionForYAxis) : 1;
                string notionForYAxis = "";
                if (divisorY != 1)
                {
                    notionForYAxis += "E+" + powerOfScientificNotionForYAxis.ToString();
                }

                float coordinateY = upperLeftAxisPoint.Y;
                while (coordinateY <= lowerLeftAxisPoint.Y)
                {
                    float coordinateYInReal = (coordinateY - spaceY) / zoomInAxisParameter;

                    Rectangle rectangleForNumberInAxisY = new Rectangle((int)(upperLeftAxisPoint.X - segementLength * 3 / 2 - widthOfCoordinate), (int)(coordinateY - heightOfCoordinate / 2), widthOfCoordinate, heightOfCoordinate);
                    graphics.DrawLine(penForAxis, upperLeftAxisPoint.X, coordinateY, upperLeftAxisPoint.X - segementLength, coordinateY);
                    graphics.DrawString((Math.Round(coordinateYInReal / divisorY, 1)).ToString() + notionForYAxis, textFont, Brushes.Black, rectangleForNumberInAxisY, yAxisNumberFormat);

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