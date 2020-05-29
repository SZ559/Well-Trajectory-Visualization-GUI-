using GeometricObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;


namespace Well_Trajectory_Visualization
{
    public class PanelForProjection : Panel
    {
        Projection projection = new Projection();
        CurrentTrajectoryInformation currentTrajectoryInformation;

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

        private Vector3 normalVector;

        public string AxisXCaption
        {
            get
            {
                string caption;
                switch (Name)
                {
                    case "Main":
                        caption = "x";
                        break;
                    case "Left":
                        caption = "y";
                        break;
                    case "Top":
                    default:
                        caption = "x";
                        break;
                }

                return caption + "(" + currentTrajectoryInformation.UnitForCaption + ")";
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
                        caption = "z";
                        break;
                    case "Left":
                        caption = "z";
                        break;
                    case "Top":
                    default:
                        caption = "y";
                        break;
                }

                return caption + "(" + currentTrajectoryInformation.UnitForCaption + ")";
            }
        }

        private Single minX;
        private Single minY;
        private List<PointIn2D> sharpestPointsProjectionIn2D;

        private SizeF inflateSize;
        private float offsetX;
        private float offsetY;
        public Rectangle GraphicDrawingArea
        {
            get
            {
                return new Rectangle(leftPaddingX, paddingY + spaceHeightForViewName, Width - leftPaddingX - rightPaddingX, Height - paddingY * 2 - spaceHeightForViewName);
            }
        }

        public Rectangle MouseRectangleArea
        {
            get
            {
                return Rectangle.Inflate(GraphicDrawingArea, 8, 8);
            }
        }

        bool isChoosingRegion = false;
        Rectangle MouseRectangle = Rectangle.Empty;
        bool zoomIsOn;
        float zoomInXAxisParameter;
        float zoomInYAxisParameter;
        bool isDrag;
        Point beginDragLocation;

        Single ZoomInAxisParameter
        {
            get
            {
                float zoomInAxisParameter;
                if (currentTrajectoryInformation.MaxZOfTrajectory * (Width - leftPaddingX - rightPaddingX) > currentTrajectoryInformation.MaxXYOfTrajectory * (Height - 2 * paddingY - spaceHeightForViewName))
                {
                    zoomInAxisParameter = (Height - 2 * paddingY - spaceHeightForViewName) / currentTrajectoryInformation.MaxZOfTrajectory;
                }
                else
                {
                    zoomInAxisParameter = (Width - leftPaddingX - rightPaddingX) / currentTrajectoryInformation.MaxXYOfTrajectory;
                }
                return zoomInAxisParameter;
            }
        }

        ToolTip toolTipForAnnotation;


        public List<PointIn2D> TrajectoryProjectionIn2D
        {
            get; set;
        }

        public PointF[] TrajectoryProjectionLocationOnPanel
        {
            get; set;
        }

        public PanelForProjection(Vector3 normalVector, CurrentTrajectoryInformation currentTrajectoryInformation)
        {
            //initialize panel property
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;

            this.currentTrajectoryInformation = currentTrajectoryInformation;
            this.normalVector = normalVector;
            TrajectoryProjectionIn2D = projection.GetProjectionInPlane(this.currentTrajectoryInformation.CurrentTrajectory.PolyLineNodes, this.normalVector);
            sharpestPointsProjectionIn2D = projection.GetProjectionInPlane(this.currentTrajectoryInformation.SharpestPoint, this.normalVector);
            this.currentTrajectoryInformation.PropertyChanged += UpdateParameters;

            Name = projection.GetProjectionView(this.normalVector);
            TrajectoryProjectionLocationOnPanel = new PointF[TrajectoryProjectionIn2D.Count];
            minX = TrajectoryProjectionIn2D.Select(x => x.X).Min();
            minY = TrajectoryProjectionIn2D.Select(x => x.Y).Min();

            //Initialize drawing property
            numberOfDataInAxisX = 5;
            numberOfDataInAxisY = 10;
            spaceHeightForViewName = 25;
            segementLength = 5;
            marginAxis = 10;
            widthOfCoordinate = 50;
            heightOfCoordinate = 18;
            widthOfAxisName = 35;
            heightOfAxisName = 18;
            leftPaddingX = segementLength * 3 / 2 + marginAxis + widthOfCoordinate + 5;
            rightPaddingX = segementLength / 2 + marginAxis + widthOfAxisName + 5;
            paddingY = segementLength * 3 / 2 + marginAxis + Math.Max(heightOfAxisName, heightOfCoordinate) + 5;

            //zoom and drag
            offsetX = 0;
            offsetY = 0;
            zoomIsOn = false;
            inflateSize = new SizeF(1, 1);
            isDrag = false;

            //tool tip
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

        private void SetZoomInAixsPrameter()
        {
            zoomInXAxisParameter = ZoomInAxisParameter;
            zoomInYAxisParameter = ZoomInAxisParameter;
            if (zoomIsOn)
            {
                zoomInXAxisParameter = ZoomInAxisParameter * inflateSize.Width;
                zoomInYAxisParameter = ZoomInAxisParameter * inflateSize.Height;
            }
        }

        private void UpdateParameters(object sender, PropertyChangedEventArgs e)
        {
            TrajectoryProjectionIn2D = projection.GetProjectionInPlane(currentTrajectoryInformation.CurrentTrajectory.PolyLineNodes, normalVector);
            TrajectoryProjectionLocationOnPanel = new PointF[TrajectoryProjectionIn2D.Count];
            minX = TrajectoryProjectionIn2D.Select(x => x.X).Min();
            minY = TrajectoryProjectionIn2D.Select(x => x.Y).Min();
            sharpestPointsProjectionIn2D = projection.GetProjectionInPlane(currentTrajectoryInformation.SharpestPoint, normalVector);
        }

        /// Paint Panel ///
        private void PaintPanel(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;

            SetZoomInAixsPrameter();
            int spaceX = (int)(leftPaddingX - minX * zoomInXAxisParameter);
            int spaceY = (int)(paddingY + spaceHeightForViewName - minY * zoomInYAxisParameter);

            for (int i = 0; i < TrajectoryProjectionIn2D.Count; i = i + 1)
            {
                float xForPaint = TrajectoryProjectionIn2D[i].X * zoomInXAxisParameter + spaceX - offsetX;
                float yForPaint = TrajectoryProjectionIn2D[i].Y * zoomInYAxisParameter + spaceY - offsetY;
                TrajectoryProjectionLocationOnPanel[i] = new PointF(xForPaint, yForPaint);

            }
            graphics.SetClip(GraphicDrawingArea);

            DrawPoints(graphics, TrajectoryProjectionLocationOnPanel);

            HighlightPoints(graphics);

            DrawAnnotation(graphics);

            DrawSharpestPoint(graphics, spaceX, spaceY);

            graphics.ResetClip();

            DrawCaption(graphics);

            DrawAxis(graphics, spaceX, spaceY);

            graphics.Dispose();
        }

        private void DrawPoints(Graphics graphics, PointF[] points)
        {
            using (Pen penForLine = new Pen(Color.FromArgb(204, 234, 187), 3.0F))
            {
                for (int i = 0; i < points.Length - 1; i = i + 1)
                {
                    graphics.DrawLine(penForLine, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
                }
            }
        }
        private void HighlightPoints(Graphics graphics)
        {
            using (SolidBrush brushForPoint = new SolidBrush(Color.FromArgb(63, 63, 68)))
            {
                foreach (var location in TrajectoryProjectionLocationOnPanel)
                {
                    graphics.FillRectangle(brushForPoint, location.X - 1, location.Y - 1, 2, 2);
                }
            }
        }

        private void DrawAxis(Graphics graphics, int spaceX, int spaceY)
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

                //int powerOfScientificNotionForXAxis = Math.Round(Math.Max(Math.Abs((upperLeftAxisPoint.X - spaceX) / zoomInAxisParameter), Math.Abs((upperRightAxisPoint.X - spaceX) / zoomInAxisParameter))).ToString().Length - 1;
                //double divisorX = powerOfScientificNotionForXAxis >= 4 ? Math.Pow(10, powerOfScientificNotionForXAxis) : 1;
                //string notionForXAxis = "";
                //if (divisorX != 1)
                //{
                //    notionForXAxis += "E+" + powerOfScientificNotionForXAxis.ToString();
                //}

                float coordinateX = upperLeftAxisPoint.X;
                while (coordinateX <= upperRightAxisPoint.X)
                {
                    float coordinateXInReal = (coordinateX - spaceX + offsetX) / zoomInXAxisParameter;

                    Rectangle rectangleForNumberInAxisX = new Rectangle((int)(coordinateX - widthOfCoordinate / 2), (int)(upperLeftAxisPoint.Y - segementLength * 3 / 2 - heightOfCoordinate), widthOfCoordinate, heightOfCoordinate);
                    graphics.DrawLine(penForAxis, coordinateX, upperLeftAxisPoint.Y, coordinateX, upperLeftAxisPoint.Y - segementLength);
                    //graphics.DrawString((Math.Round(coordinateXInReal / divisorX, 1)).ToString() + notionForXAxis, textFont, Brushes.Black, rectangleForNumberInAxisX, xAxisNumberFormat);
                    graphics.DrawString((Math.Round(coordinateXInReal, 0)).ToString(), textFont, Brushes.Black, rectangleForNumberInAxisX, xAxisNumberFormat);

                    coordinateX = coordinateX + widthOfAxis / numberOfDataInAxisX;
                }

                // y-axis
                StringFormat yAxisNumberFormat = new StringFormat();
                yAxisNumberFormat.Alignment = StringAlignment.Far;

                //int powerOfScientificNotionForYAxis = Math.Round(Math.Max(Math.Abs((upperLeftAxisPoint.Y - spaceY) / zoomInAxisParameter), Math.Abs((lowerLeftAxisPoint.Y - spaceY) / zoomInAxisParameter))).ToString().Length - 1;
                //double divisorY = powerOfScientificNotionForYAxis >= 4 ? Math.Pow(10, powerOfScientificNotionForYAxis) : 1;
                //string notionForYAxis = "";
                //if (divisorY != 1)
                //{
                //    notionForYAxis += "E+" + powerOfScientificNotionForYAxis.ToString();
                //}

                float coordinateY = upperLeftAxisPoint.Y;
                while (coordinateY <= lowerLeftAxisPoint.Y)
                {
                    float coordinateYInReal = (coordinateY - spaceY + offsetY) / zoomInYAxisParameter;

                    Rectangle rectangleForNumberInAxisY = new Rectangle((int)(upperLeftAxisPoint.X - segementLength * 3 / 2 - widthOfCoordinate), (int)(coordinateY - heightOfCoordinate / 2), widthOfCoordinate, heightOfCoordinate);
                    graphics.DrawLine(penForAxis, upperLeftAxisPoint.X, coordinateY, upperLeftAxisPoint.X - segementLength, coordinateY);
                    //graphics.DrawString((Math.Round(coordinateYInReal / divisorY, 1)).ToString() + notionForYAxis, textFont, Brushes.Black, rectangleForNumberInAxisY, yAxisNumberFormat);
                    graphics.DrawString((Math.Round(coordinateYInReal, 0)).ToString(), textFont, Brushes.Black, rectangleForNumberInAxisY, yAxisNumberFormat);

                    coordinateY = coordinateY + heightOfAxis / numberOfDataInAxisY;
                }

            }

        }

        /// Draw Annotation////
        private void DrawAnnotation(Graphics graphics)
        {
            if (currentTrajectoryInformation.DisplayChoice.AddAnnotation)
            {
                using (Font textFont = new Font("Microsoft YaHei", 6, FontStyle.Regular, GraphicsUnit.Point))
                {
                    Dictionary<String, int> pointAnnotationDictionary = new Dictionary<String, int>();
                    for (int i = 0; i < TrajectoryProjectionIn2D.Count; i = i + 1)
                    {
                        String pointAnnotation = $"({Math.Round(TrajectoryProjectionIn2D[i].X, 1)}, {Math.Round(TrajectoryProjectionIn2D[i].Y, 1)})";
                        if (!pointAnnotationDictionary.ContainsKey(pointAnnotation))
                        {
                            graphics.DrawString(pointAnnotation, textFont, Brushes.Black, TrajectoryProjectionLocationOnPanel[i]);
                            pointAnnotationDictionary.Add(pointAnnotation, 1);
                        }
                    }
                }
            }
        }

        private void DrawSharpestPoint(Graphics graphics, int spaceX, int spaceY)
        {
            if (currentTrajectoryInformation.DisplayChoice.AddSharpestPoint)
            {
                using (SolidBrush brushForPoint = new SolidBrush(Color.Red))
                {
                    foreach (var sharpestPoint in sharpestPointsProjectionIn2D)
                    {
                        float xForPaint = sharpestPoint.X * zoomInXAxisParameter + spaceX - offsetX;
                        float yForPaint = sharpestPoint.Y * zoomInYAxisParameter + spaceY - offsetY;
                        graphics.FillRectangle(brushForPoint, xForPaint - 1, yForPaint - 1, 2, 2);
                    }
                }
            }
        }

        private void DrawCaption(Graphics graphics)
        {
            using (Font fontForCaption = new Font("Microsoft YaHei", 11, FontStyle.Regular, GraphicsUnit.Point))
            {
                Rectangle rect = new Rectangle(leftPaddingX, 0, Width - leftPaddingX - rightPaddingX, spaceHeightForViewName);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                graphics.DrawString(Name, fontForCaption, Brushes.Black, rect, stringFormat);
            }
        }

        //Zoom//
        private void Reset()
        {
            offsetX = 0;
            offsetY = 0;
            zoomIsOn = false;
            inflateSize = new SizeF(1, 1);
        }

        private void ResizeToRectangle(Point p)
        {
            DrawRectangle();
            MouseRectangle.Width = p.X - MouseRectangle.Left;
            MouseRectangle.Height = p.Y - MouseRectangle.Top;
            DrawRectangle();
        }

        private void DrawRectangle()
        {
            Rectangle rect = this.RectangleToScreen(MouseRectangle);
            Color color = Color.Green;
            double aspectRatioOfMouseRectangle = Math.Abs(Math.Round(((float)MouseRectangle.Height / (float)MouseRectangle.Width), 1));
            double aspectRatioOfGraphicDrawingArea = Math.Round(((float)GraphicDrawingArea.Height / (float)GraphicDrawingArea.Width), 1);
            if (MouseRectangle.Width != 0 && aspectRatioOfMouseRectangle == aspectRatioOfGraphicDrawingArea)
            {
                color = Color.Brown;
            }
            ControlPaint.DrawReversibleFrame(rect, color, FrameStyle.Thick);
        }

        private void DrawStart(Point StartPoint)
        {
            Capture = true;
            Cursor.Clip = RectangleToScreen(MouseRectangleArea);
            MouseRectangle = new Rectangle(StartPoint.X, StartPoint.Y, 0, 0);
        }

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {

            if (currentTrajectoryInformation.DisplayChoice.ChooseRegion)
            {
                if (MouseRectangleArea.Contains(e.Location))
                {
                    isChoosingRegion = true;
                    DrawRectangle();
                    DrawStart(e.Location);
                }
            }
            else
            {
                beginDragLocation = e.Location;
                isDrag = true;
            }
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                offsetX = offsetX + (beginDragLocation.X - e.X);
                offsetY = offsetY + (beginDragLocation.Y - e.Y);
                beginDragLocation.X = e.X;
                beginDragLocation.Y = e.Y;
                Invalidate();
            }
            else if (isChoosingRegion)
            {
                ResizeToRectangle(e.Location);
            }
            else
            {
                string tip = "";
                int radius = 4, index = 0;
                foreach (var location in TrajectoryProjectionLocationOnPanel)
                {
                    if ((Math.Abs(e.X - location.X) < radius) &&
                        (Math.Abs(e.Y - location.Y) < radius))
                    {
                        tip = $"({Math.Round(TrajectoryProjectionIn2D[index].X, 1)}, {Math.Round(TrajectoryProjectionIn2D[index].Y, 1)})";
                        break;
                    }
                    index = index + 1;
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
            }
            else if (isChoosingRegion)
            {
                if (MouseRectangle.Width != 0 && MouseRectangle.Height != 0)
                {
                    DrawRectangle();
                    int radius = 8;
                    if (Math.Abs(MouseRectangleArea.Width - Math.Abs(MouseRectangle.Width)) < radius && Math.Abs(MouseRectangleArea.Height - Math.Abs(MouseRectangle.Height)) < radius)
                    {
                        Reset();
                        Refresh();
                    }
                    else
                    {
                        UpdateOffsetAndInflateAfterChoosingZoomRegion();
                    }
                }
                Capture = false;
                Cursor.Clip = Rectangle.Empty;
                isChoosingRegion = false;
                MouseRectangle = Rectangle.Empty;
            }
        }

        private void UpdateOffsetAndInflateAfterChoosingZoomRegion()
        {
            zoomIsOn = true;
            float inflateX = ((float)GraphicDrawingArea.Width / ((float)Math.Abs(MouseRectangle.Width)));
            float inflateY = ((float)GraphicDrawingArea.Height / ((float)Math.Abs(MouseRectangle.Height)));
            float realMouseRectangleUpperLeftCornerX = MouseRectangle.X;
            float realMouseRectangleUpperLeftCornerY = MouseRectangle.Y;

            if (MouseRectangle.Width < 0)
            {
                realMouseRectangleUpperLeftCornerX = MouseRectangle.X + MouseRectangle.Width;
            }
            if (MouseRectangle.Height < 0)
            {

                realMouseRectangleUpperLeftCornerY = MouseRectangle.Y + MouseRectangle.Height;
            }
            inflateSize.Width = Math.Abs(inflateSize.Width) * inflateX;
            offsetX = (realMouseRectangleUpperLeftCornerX - GraphicDrawingArea.X + offsetX) * inflateX;
            inflateSize.Height = Math.Abs(inflateSize.Height) * inflateY;
            offsetY = (realMouseRectangleUpperLeftCornerY - GraphicDrawingArea.Y + offsetY) * inflateY;
            Invalidate();
        }

        //need to improve
        private bool ValidInflateWidth(float inflateX)
        {
            return inflateSize.Width * inflateX >= 1;
        }
        //need to improve
        private bool ValidInflateHeight(float inflateY)
        {
            return inflateSize.Height * inflateY >= 1;
        }


        private void Panel_MouseWheel(object sender, MouseEventArgs e)
        {
            float zoomDeltaOfWheel = (float)(e.Delta / 120 * 0.01);
            float inflateX = zoomDeltaOfWheel + 1;
            float inflateY = zoomDeltaOfWheel + 1;
            if (inflateX * inflateSize.Width >= 0 && inflateY * inflateSize.Height >= 0)
            {
                if (ValidInflateWidth(inflateX) || ValidInflateHeight(inflateY))
                {
                    zoomIsOn = true;
                    offsetX = inflateX * (e.X + offsetX - leftPaddingX) + leftPaddingX - e.X;
                    inflateSize.Width = inflateSize.Width * inflateX;
                    offsetY = inflateY * (e.Y + offsetY - (paddingY + spaceHeightForViewName)) + (paddingY + spaceHeightForViewName) - e.Y;
                    inflateSize.Height = inflateSize.Height * inflateY;
                    Invalidate();
                }
            }
        }
    }
}

//private void Panel_MouseHover(object sender, EventArgs e)
//{
//    string tip = "";
//    int radius = ;
//    int index = 0;
//    PointF position = PointToClient(Cursor.Position);
//    foreach (var location in TrajectoryProjectionLocationOnPanel)
//    {
//        if ((Math.Abs(position.X - location.X) < radius) &&
//            (Math.Abs(position.Y - location.Y) < radius))
//        {
//            tip = $"({Math.Round(TrajectoryProjectionIn2D[index].X, 1)}, {Math.Round(TrajectoryProjectionIn2D[index].Y, 1)})";
//            break;
//        }
//        index = index + 1;
//    }
//    if (toolTipForAnnotation.GetToolTip(this) != tip)
//    {
//        toolTipForAnnotation.SetToolTip(this, tip);
//    }
//}
//private bool DisplayAllDataPoints()
//{
//    int spaceX = (int)(leftPaddingX - minX * zoomInXAxisParameter);
//    int spaceY = (int)(paddingY + spaceHeightForViewName - minY * zoomInYAxisParameter);
//    int minXLocation = (int)(minX * zoomInXAxisParameter + spaceX - offsetX);
//    int minYLocation = (int)(minY * zoomInYAxisParameter + spaceY - offsetY);
//    int maxXLocation = (int)(maxX * zoomInXAxisParameter + spaceX - offsetX);
//    int maxYLocation = (int)(maxY * zoomInYAxisParameter + spaceY - offsetY);
//    Point upperLeftCornerPoint = new Point(minXLocation, minYLocation);
//    Point lowerRightCornerPoint = new Point(maxXLocation, maxYLocation);

//    return GraphicDrawingArea.Contains(upperLeftCornerPoint) && GraphicDrawingArea.Contains(lowerRightCornerPoint);
//}
//zoomIsOn = true;
//offsetX = ((float)(inflateSize.Width + zoomDeltaOfWheel) / (float) (inflateSize.Width)) * ((float)(offsetX));
//inflateSize.Width = inflateSize.Width + zoomDeltaOfWheel;
//offsetY = ((float)(inflateSize.Height + zoomDeltaOfWheel) / (float) (inflateSize.Height)) * ((float)(offsetY));
//inflateSize.Height = inflateSize.Height + zoomDeltaOfWheel;
//Invalidate();

