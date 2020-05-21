using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GeometricObject;
using FileHandler;
using System.Numerics;

namespace Well_Trajectory_Visualization
{
    public partial class MainForm : Form
    {

        TrajectoryDataReader trajectoryDataReader;
        Projection projection;
        WellViewSaver wellViewSaver;
        Trajectory currentTrajectory;

        Single zoomXY;
        Single zoomZ;
        Single zoomInAxisParameter;
        List<Well> wells;
        readonly int widthOfCloseIcon;
        readonly int paddingYForTabHeaderRectangle;
        readonly int leftMarginXForTabHeaderRectangle;
        readonly int rightMarginXForTabHeaderRectangle;
        readonly int paddingXForCloseIcon;
        readonly int paddingYForCloseIcon;


        int PreviewTabIndex
        {
            get
            {
                foreach (TabPage tabpage in tabControl.TabPages)
                {
                    if ((bool)tabpage.Tag == false)
                    {
                        return tabControl.TabPages.IndexOf(tabpage);
                    }
                }
                return -1;
            }
        }
        private bool isDoubleClick;

        bool addAnnotation;
        ToolTip toolTipForAnnotation;

        public MainForm()
        {
            InitializeComponent();
            widthOfCloseIcon = 18;
            paddingYForTabHeaderRectangle = 3;
            leftMarginXForTabHeaderRectangle = 5;
            rightMarginXForTabHeaderRectangle = 1;
            paddingXForCloseIcon = 5;
            paddingYForCloseIcon = 6;

            trajectoryDataReader = new TrajectoryDataReader();
            wellViewSaver = new WellViewSaver();
            projection = new Projection();
            tabControl.Padding = new Point(widthOfCloseIcon, 5);
            wells = new List<Well>();
            isDoubleClick = false;
            toolTipForAnnotation = new ToolTip()
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 100,
                ShowAlways = true,
            };
        }

        ///////////// Menu Bar /////////////////

        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTrajectoryDataFromFile();
        }

        private void ViewSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // HACK
            System.Diagnostics.Process.Start("https://github.com/SZ559/Well-Trajectory-Visualization-GUI-");
        }

        private void ReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://commons.wikimedia.org/wiki/File:Third_angle_projecting.svg");
        }

        /////////////// Save Views ///////////////////

        private void SaveViewToFigure(Control control)
        {
            saveFileDialog.FileName = tabControl.SelectedTab.Name + "-" + control.Name;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string errorMessage;
                string filePath = saveFileDialog.FileName;
                Bitmap bitmap = new Bitmap(control.Width, control.Height);
                control.DrawToBitmap(bitmap, new Rectangle(0, 0, control.Width, control.Height));
                wellViewSaver.SaveView(filePath, bitmap, out errorMessage);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show($"Saving view to {filePath} succeed.", "Saving View", MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show($"Saving view to {filePath} failed.\n{errorMessage}", "Saving View", MessageBoxButtons.OK);
                }
            }
        }

        // save all views
        private void SaveViewOnTabPage(object sender, EventArgs e)
        {    
            if (tabControl.SelectedIndex != -1)
            {
                TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)tabControl.SelectedTab.Controls[0];
                foreach (Control control in tableLayoutPanel.Controls)
                {
                    SaveViewToFigure(control);
                }
            }
        }

        // save single view
        private void ViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem) sender;
                Control controlForView = tabControl.SelectedTab.Controls.Find(toolStripMenuItem.Text, true).First();
                SaveViewToFigure(controlForView);
            }
        }


        /////////////// Load trajectory from file ///////////////////

        private void LoadTrajectoryDataFromFile()
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    if (wells.Select(x => x.TrajectoryCount).Sum() >= 30)
                    {
                        MessageBox.Show($"Loading {filePath} failed. Reach the well trajectory loading limit.", "Loading Well Trajectory", MessageBoxButtons.OK);
                        continue;
                    }

                    if (wells.SelectMany(x => x.Sources).Contains(filePath))
                    {
                        MessageBox.Show($"Trajectory has been already loaded for {filePath}!", "Loading Well Trajectory", MessageBoxButtons.OK);
                        continue;
                    }

                    string errorMessage;
                    Trajectory newTrajectory = trajectoryDataReader.ReadFile(filePath, out errorMessage);
                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        if (!wells.Select(x => x.WellName).Contains(newTrajectory.WellName))
                        {
                            Well newWell = new Well(newTrajectory.WellName);
                            newWell.AddTrajectory(newTrajectory);
                            wells.Add(newWell);
                        }
                        else
                        {
                            wells.Find(x => x.WellName == newTrajectory.WellName).AddTrajectory(newTrajectory);
                        }

                        UpdateTreeView();
                        MessageBox.Show($"New Well loading from {filePath} succeed.", "Loading Well Trajectory", MessageBoxButtons.OK);
                    }
                    else
                    {
                        MessageBox.Show($"Loading {filePath} failed.\n{errorMessage}", "Loading Well Trajectory", MessageBoxButtons.OK);
                    }
                }
            }
        }

        //////////////// Tree View //////////////////////////

        private void UpdateTreeView()
        {
            wellsTreeView.BeginUpdate();
            wellsTreeView.Nodes[0].Nodes.Clear();

            int i = 0;
            foreach (var well in wells)
            {
                wellsTreeView.Nodes[0].Nodes.Add(well.WellName);
                wellsTreeView.Nodes[0].Nodes[i].Name = well.WellName;
                wellsTreeView.Nodes[0].Nodes[i].Tag = well;
                int j = 0;
                foreach (var trajectory in well.Trajectories)
                {
                    wellsTreeView.Nodes[0].Nodes[i].Nodes.Add(trajectory.TrajectoryName);
                    wellsTreeView.Nodes[0].Nodes[i].Nodes[j].Name = trajectory.WellName + "-" + trajectory.TrajectoryName;
                    wellsTreeView.Nodes[0].Nodes[i].Nodes[j].Tag = trajectory;
                    j = j + 1;
                }
                i = i + 1;
            }

            wellsTreeView.EndUpdate();
            wellsTreeView.ExpandAll();
        }

        private void WellsTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            isDoubleClick = false;
            TreeViewSelection(e.Node);
        }

        private void WellsTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            isDoubleClick = true;
            TreeViewSelection(e.Node);
        }

        private void TreeViewSelection(TreeNode node)
        {
            if (node.Parent == null || node.Parent.Text == "Wells")
            {
                return;
            }
            else
            {
                string wellName = node.Parent.Text;
                string trajectoryName = node.Text;
                string tabPageName = $"{wellName}-{trajectoryName}";
                defaultPagePanel.Visible = false;
                if (HasSameTabPage(tabPageName))
                {
                    if (isDoubleClick)
                    {
                        tabControl.SelectedTab.Tag = true;
                        tabControl.SelectedTab = null; // force drawitem method
                    }
                    tabControl.SelectedIndex = tabControl.TabPages.IndexOfKey(tabPageName);
                    return;
                }
                else
                {
                    RemovePreviewTab();

                    if (tabControl.TabCount >= 10)
                    {
                        MessageBox.Show("Only 10 pages can be opened. Please close a page before opening a new one.");
                        return;
                    }

                    OpenNewTabPage(wellName, trajectoryName);
                }
            }
        }


        /////////////// Tab Page ////////////////////

        private bool HasSameTabPage(string tabPageName)
        {
            foreach (TabPage tabpage in tabControl.TabPages)
            {
                if (tabpage.Name == tabPageName)
                {
                    return true;
                }
            }
            return false;
        }

        private void RemovePreviewTab()
        {
            int previewTabIndex = PreviewTabIndex;
            if (previewTabIndex != -1)
            {
                tabControl.TabPages.RemoveAt(previewTabIndex);
            }
        }

        private void OpenNewTabPage(string wellName, string trajectoryName)
        {
            TabPage tabPage = new TabPage
            {
                Text = GetHeaderTextForTabPage(wellName, trajectoryName),
                Name = $"{wellName}-{trajectoryName}",
                Font = tabControl.Font,
                BorderStyle = BorderStyle.None,
                Tag = isDoubleClick // opened or preview : true means opened
            };

            TableLayoutPanel tableLayoutPanel = InitializeTableLayoutPanelForTabPage();
            tabPage.Controls.Add(tableLayoutPanel);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;
        }

        ///////////// Panels inside Tab Page //////////////////

        private TableLayoutPanel InitializeTableLayoutPanelForTabPage()
        {
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                BorderStyle = BorderStyle.None,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                RowCount = 1,
                ColumnCount = 3,
                AutoScroll = true,
                Dock = DockStyle.Fill,
            };
            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tableLayoutPanel.ResumeLayout();
            AddViewPanelForTableLayoutPanel(tableLayoutPanel);
            return tableLayoutPanel;
        }

        private void AddViewPanelForTableLayoutPanel(TableLayoutPanel tableLayoutPanel)
        {
            NewPanel mainViewPanel = InitializePanelForProjection("Main View");
            NewPanel leftViewPanel = InitializePanelForProjection("Left View");
            NewPanel topViewPanel = InitializePanelForProjection("Top View");


            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Add(mainViewPanel, 0, 0);
            tableLayoutPanel.Controls.Add(leftViewPanel, 1, 0);
            tableLayoutPanel.Controls.Add(topViewPanel, 2, 0);
            tableLayoutPanel.ResumeLayout();
        }

        private NewPanel InitializePanelForProjection(string viewName)
        {
            NewPanel panel = new NewPanel()
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Name = viewName,
            };
            panel.Paint += new PaintEventHandler(PaintPanel);
            panel.MouseMove += new MouseEventHandler(Panel_MouseMove);
            return panel;
        }

        private void PaintPanel(object sender, PaintEventArgs e)
        {
            UpdateParametersOfCurrentTrajectory();
            DrawViewPanel((NewPanel) sender, e.Graphics);
        }


        /////////// Draw Tab Page Header //////////////////

        private string GetHeaderTextForTabPage(string wellName, string trajectoryName)
        {
            if (wellName.Length + trajectoryName.Length > 30)
            {
                if (wellName.Length > 15)
                {
                    if (trajectoryName.Length > 15)
                    {
                        wellName = wellName.Remove(13) + "...";
                        trajectoryName = trajectoryName.Remove(13) + "...";
                    }
                    else
                    {
                        wellName = wellName.Remove(28 - trajectoryName.Length) + "...";
                    }
                }
                else
                {
                    trajectoryName = trajectoryName.Remove(28 - wellName.Length) + "...";
                }
            }
            return $"{wellName}-{trajectoryName}";
        }

        // tab header: 
        // x: leftMarginX + filename + leftMarginX + rightMarginX + spaceForCloseIcon + rightMarginX
        // y: paddingY + text + paddingY
        // spaceForCloseIcon: 
        // - x: paddingXForCloseIcon + clsoeIcon + paddingXForCloseIcon
        // - y: paddingYForCloseIcon + clsoeIcon + paddingYForCloseIcon
        private void DrawTabHeaderText_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics graphic = e.Graphics;
            Rectangle tabHeaderArea = tabControl.GetTabRect(e.Index);

            // draw background
            using (Brush brushForHeaderBackground = new SolidBrush(Color.FromArgb(240, 255, 255))) // color.Azure / color.mint-cream
            {
                if (tabControl.SelectedTab != null && e.Index == tabControl.SelectedIndex)
                {
                    graphic.FillRectangle(brushForHeaderBackground, tabHeaderArea);
                }
            }

            string tabHeaderText = tabControl.TabPages[e.Index].Text;
            FontStyle fontStyle = (bool)tabControl.TabPages[e.Index].Tag == true ? FontStyle.Regular : FontStyle.Italic;

            //draw filename
            tabHeaderArea.Offset(leftMarginXForTabHeaderRectangle, paddingYForTabHeaderRectangle);
            tabHeaderArea.Width = tabHeaderArea.Width - leftMarginXForTabHeaderRectangle - rightMarginXForTabHeaderRectangle - widthOfCloseIcon;
            tabHeaderArea.Height = tabHeaderArea.Height - 2 * paddingYForTabHeaderRectangle;
            graphic.DrawString(tabHeaderText,
                new Font(tabControl.Font, fontStyle),
                SystemBrushes.ControlText,
                tabHeaderArea);

            // draw close icon
            using (Pen closeIconPen = new Pen(Color.FromArgb(240, 128, 128), 2f))
            {
                tabHeaderArea.Offset(tabHeaderArea.Width + paddingXForCloseIcon, paddingYForCloseIcon);
                tabHeaderArea.Width = widthOfCloseIcon - 2 * paddingXForCloseIcon;
                tabHeaderArea.Height = tabHeaderArea.Height - 2 * paddingYForCloseIcon;
                Point p1 = new Point(tabHeaderArea.X, tabHeaderArea.Y);
                Point p2 = new Point(tabHeaderArea.X + tabHeaderArea.Width, tabHeaderArea.Y + tabHeaderArea.Height);
                graphic.DrawLine(closeIconPen, p1, p2);
                Point p3 = new Point(tabHeaderArea.X, tabHeaderArea.Y + tabHeaderArea.Height);
                Point p4 = new Point(tabHeaderArea.X + tabHeaderArea.Width, tabHeaderArea.Y);
                graphic.DrawLine(closeIconPen, p3, p4);
            }
            e.Graphics.Dispose();
        }

        private void DrawCloseIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Rectangle tabCloseIconArea = tabControl.GetTabRect(tabControl.SelectedIndex);
                    tabCloseIconArea.Offset(tabCloseIconArea.Width - (widthOfCloseIcon + rightMarginXForTabHeaderRectangle) + paddingXForCloseIcon, paddingYForTabHeaderRectangle + paddingYForCloseIcon);
                    tabCloseIconArea.Width = widthOfCloseIcon - 2 * paddingXForCloseIcon;
                    tabCloseIconArea.Height = tabCloseIconArea.Height - 2 * paddingYForTabHeaderRectangle;
                    if (tabCloseIconArea.Contains(e.Location))
                    {
                        CloseSelectedTab();
                    }
                }
            }
        }

        private void CloseSelectedTab()
        {
            int selectedTabIndex = tabControl.SelectedIndex;
            if (selectedTabIndex != -1)
            {
                tabControl.TabPages.RemoveAt(selectedTabIndex);
                tabControl.SelectedIndex = (selectedTabIndex - 1 == -1) ? (tabControl.TabCount - 1) : (selectedTabIndex - 1);
            }
            SwitchDefaultPage();
        }

        private void SwitchDefaultPage()
        {
            defaultPagePanel.Visible = (tabControl.TabPages.Count == 0) ? true : false;
        }


        /////////////// Helper functions //////////////

        private Vector3 GetNormalVectorForView(string viewName)
        {
            Vector3 normalVector;
            switch (viewName)
            {
                case "Top View":
                    normalVector = Vector3.UnitZ;
                    break;
                case "Left View":
                    normalVector = Vector3.UnitX;
                    break;
                case "Main View":
                default:
                    normalVector = Vector3.UnitY;
                    break;
            }
            return normalVector;
        }

        private string[] GetAxisCaption(string viewPanelName)
        {
            string axisXCaption;
            string axisYCaption;
            switch (viewPanelName)
            {
                case "Main View":
                    axisXCaption = "x";
                    axisYCaption = "z";
                    break;
                case "Left View":
                    axisXCaption = "y";
                    axisYCaption = "z";
                    break;
                default:
                    axisXCaption = "x";
                    axisYCaption = "y";
                    break;
            }
            return new string[] { axisXCaption, axisYCaption };
        }


        ///////////// Paint Panels ////////////////////
        private void UpdateParametersOfCurrentTrajectory()
        {
            SetCurrentTrajectory();
            SetZoomForThreeViews();
        }

        private void SetZoomForThreeViews()
        {
            if (currentTrajectory != null)
            {
                Single maxX = currentTrajectory.PolyLineNodes.Select(x => x.X).Max();
                Single maxY = currentTrajectory.PolyLineNodes.Select(x => x.Y).Max();
                Single maxZ = currentTrajectory.PolyLineNodes.Select(x => x.Z).Max();

                Single minX = currentTrajectory.PolyLineNodes.Select(x => x.X).Min();
                Single minY = currentTrajectory.PolyLineNodes.Select(x => x.Y).Min();
                Single minZ = currentTrajectory.PolyLineNodes.Select(x => x.Z).Min();

                zoomXY = Math.Max(maxX - minX, maxY - minY);
                zoomXY = zoomXY > 0 ? zoomXY : 1;
                zoomZ = maxZ - minZ;
                zoomZ = zoomZ > 0 ? zoomZ : 1;
            }
        }

        private void SetCurrentTrajectory()
        {
            if (tabControl.SelectedTab != null)
            {
                string wellName = tabControl.SelectedTab.Name.Split('-')[0];
                string trajectoryName = tabControl.SelectedTab.Name.Split('-')[1];
                currentTrajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
            }
            else
            {
                currentTrajectory = null;
            }
        }

        // use cuboid of control to try to include the whole trajectory
        private void GetZoomInAxisParameter(Control control, int paddingX, int paddingY)
        {
            if (zoomZ * (control.Width - 2 * paddingX) > zoomXY * (control.Height - 2 * paddingY))
            {
                zoomInAxisParameter = (control.Height - 2 * paddingY) / zoomZ;
            }
            else
            {
                zoomInAxisParameter = (control.Width - 2 * paddingX) / zoomXY;
            }
        }


        private void DrawViewPanel(NewPanel viewPanel, Graphics graphics)
        {
            int paddingX = 50;
            int paddingY = 55;
            Vector3 normalVector = GetNormalVectorForView(viewPanel.Name);
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(currentTrajectory.PolyLineNodes, normalVector);
            GetZoomInAxisParameter(viewPanel, paddingX, paddingY);
            Single minX = projectionPointIn2D.Select(x => x.X).Min();
            Single minY = projectionPointIn2D.Select(x => x.Y).Min();
            int spaceX = (int)(paddingX - minX * zoomInAxisParameter);
            int spaceY = (int)(paddingY - minY * zoomInAxisParameter);

            PointF[] locationOfProjectionPointIn2DOnPanel = new PointF[projectionPointIn2D.Count];
            PointF[] dataPointsProjection = new PointF[projectionPointIn2D.Count];
            List<PointF[]> dataPointProjectionIn2DAndLocationOnPanel = new List<PointF[]> ();
            //Dictionary<PointIn2D, PointF> dataPointProjectionIn2DAndLocationOnPanel = new Dictionary<PointIn2D, PointF>();
            for (int i = 0; i < projectionPointIn2D.Count; i = i + 1)
            {
                float xForPaint = projectionPointIn2D[i].X * zoomInAxisParameter + spaceX;
                float yForPaint = projectionPointIn2D[i].Y * zoomInAxisParameter + spaceY;
                //dataPointProjectionIn2DAndLocationOnPanel.Add(projectionPointIn2D[i], new PointF(xForPaint, yForPaint));
                locationOfProjectionPointIn2DOnPanel[i] = new PointF(xForPaint, yForPaint);
                dataPointsProjection[i] = new PointF(projectionPointIn2D[i].X, projectionPointIn2D[i].Y);
            }
            dataPointProjectionIn2DAndLocationOnPanel.Add(dataPointsProjection);
            dataPointProjectionIn2DAndLocationOnPanel.Add(locationOfProjectionPointIn2DOnPanel);

            viewPanel.Tag = dataPointProjectionIn2DAndLocationOnPanel;
            //viewPanel.Tag = dataPointProjectionIn2DAndLocationOnPanel;
            // draw line; 
            //PointF[] locationOfProjectionPointIn2DOnPanel = dataPointProjectionIn2DAndLocationOnPanel.Values.ToArray();
            using (Pen penForLine = new Pen(Color.FromArgb(204, 234, 187), 3.0F))
            {
                graphics.DrawLines(penForLine, locationOfProjectionPointIn2DOnPanel);
            }

            // highlight data points
            using (SolidBrush brushForPoint = new SolidBrush(Color.FromArgb(63, 63, 68)))
            {
                foreach (var location in locationOfProjectionPointIn2DOnPanel)
                {
                    graphics.FillRectangle(brushForPoint, location.X - 1, location.Y - 1, 2, 2);
                }
            }

            // draw caption
            using (Font fontForCaption = new Font("Microsoft YaHei", 11, FontStyle.Regular, GraphicsUnit.Point))
            {
                Rectangle rect = new Rectangle(0, 0, viewPanel.Width, paddingY - 30);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                graphics.DrawString(viewPanel.Name, fontForCaption, Brushes.Black, rect, stringFormat);
            }


            //draw axis
            string axisXCaption = GetAxisCaption(viewPanel.Name)[0];
            string axisYCaption = GetAxisCaption(viewPanel.Name)[1];

            int lineLength = 5;
            int spaceForTextInXDirection = 40;
            int spaceForTextInYDirection = 20;
            int spaceForTextAlignment = 20;

            PointF xAxisStartPoint = new PointF(paddingX - 2 * lineLength, paddingY - lineLength);
            PointF yAxisStartPoint = new PointF(paddingX - lineLength, paddingY - 2 * lineLength);
            PointF xAxisEndPoint = new PointF(viewPanel.Width - paddingX + lineLength, paddingY - lineLength);
            PointF yAxisEndPoint = new PointF(paddingX - lineLength, viewPanel.Height - paddingY + lineLength);
            PointF[] xAxisArrowhead = new PointF[] { new PointF(xAxisEndPoint.X, xAxisEndPoint.Y - lineLength), new PointF(xAxisEndPoint.X, xAxisEndPoint.Y + lineLength), new PointF(xAxisEndPoint.X + lineLength, xAxisEndPoint.Y) };
            PointF[] yAxisArrowhead = new PointF[] { new PointF(yAxisEndPoint.X - lineLength, yAxisEndPoint.Y), new PointF(yAxisEndPoint.X + lineLength, yAxisEndPoint.Y), new PointF(yAxisEndPoint.X, yAxisEndPoint.Y + lineLength) };
            Font textFont = viewPanel.Font;
            using (Pen penForAxis = new Pen(Color.Black, 0.3F))
            {
                graphics.DrawLine(penForAxis, xAxisStartPoint, xAxisEndPoint);
                graphics.DrawLine(penForAxis, yAxisStartPoint, yAxisEndPoint);
                graphics.DrawString(axisXCaption, textFont, Brushes.Black, xAxisEndPoint.X - spaceForTextAlignment / 2, xAxisEndPoint.Y - spaceForTextInYDirection);
                graphics.DrawString(axisYCaption, textFont, Brushes.Black, yAxisEndPoint.X - spaceForTextInXDirection / 2, yAxisEndPoint.Y - spaceForTextAlignment / 2);
                //graphics.DrawPolygon(penForAxis, xAxisArrowhead);
                graphics.FillPolygon(Brushes.Black, xAxisArrowhead);
                //graphics.DrawPolygon(penForAxis, yAxisArrowhead);
                graphics.FillPolygon(Brushes.Black, yAxisArrowhead);

                int scale = (int)(1 / zoomInAxisParameter) * 60;
                int coordinateX = (int)minX;
                int coordinateY = (int)minY;
                float coordinateXLocationX = coordinateX * zoomInAxisParameter + spaceX;
                float coordinateXLocationY = xAxisStartPoint.Y - spaceForTextInYDirection;
                StringFormat stringFormatRightAlignment = new StringFormat();
                stringFormatRightAlignment.Alignment = StringAlignment.Far;
                StringFormat stringFormatCenterAlignment = new StringFormat();
                stringFormatCenterAlignment.Alignment = StringAlignment.Center;
                stringFormatCenterAlignment.LineAlignment = StringAlignment.Center;

                while (coordinateXLocationX <= xAxisEndPoint.X - 20)
                {
                    Rectangle rectangle = new Rectangle((int)(coordinateXLocationX - spaceForTextAlignment), (int)coordinateXLocationY, spaceForTextAlignment * 2, (int)(spaceForTextInYDirection - lineLength));
                    graphics.DrawLine(penForAxis, coordinateXLocationX, xAxisStartPoint.Y, coordinateXLocationX, xAxisStartPoint.Y - lineLength);
                    graphics.DrawString(coordinateX.ToString(), textFont, Brushes.Black, rectangle, stringFormatCenterAlignment);
                    coordinateX = coordinateX + scale;

                    coordinateXLocationX = coordinateX * zoomInAxisParameter + spaceX;
                }
                float coordinateYLocationX = yAxisStartPoint.X - spaceForTextInXDirection;
                float coordinateYLocationY = (int)coordinateY * zoomInAxisParameter + spaceY;
                while (coordinateYLocationY <= yAxisEndPoint.Y - 20)
                {
                    graphics.DrawLine(penForAxis, yAxisStartPoint.X, coordinateYLocationY, yAxisStartPoint.X - lineLength, coordinateYLocationY);
                    Rectangle rectangle = new Rectangle((int)coordinateYLocationX, (int)(coordinateYLocationY - spaceForTextInYDirection / 2), (int)(spaceForTextInXDirection - lineLength), spaceForTextInYDirection);
                    graphics.DrawString(coordinateY.ToString(), textFont, Brushes.Black, rectangle, stringFormatRightAlignment);
                    coordinateY = coordinateY + scale;
                    coordinateYLocationY = coordinateY * zoomInAxisParameter + spaceY;
                }
            }

            if (addAnnotation)
            {
                graphics = DrawAnnotation(graphics, projectionPointIn2D, spaceX, spaceY);
            }
            graphics.Dispose();
        }

        private Graphics DrawAnnotation(Graphics graphics, List<PointIn2D> projectionPointIn2D, int spaceX, int spaceY)
        {
            using (Font textFont = new Font("Microsoft YaHei", 6, FontStyle.Regular, GraphicsUnit.Point))
            {
                Dictionary<String, int> pointAnnotationDictionary = new Dictionary<String, int>();
                foreach (var point in projectionPointIn2D)
                {                
                    float xForPaint = point.X * zoomInAxisParameter + spaceX;
                    float yForPaint = point.Y * zoomInAxisParameter + spaceY;
                    PointF locationOfPoint = new PointF(xForPaint + 3, yForPaint - 4);
                    String pointAnnotation = point.ToString();
                    if (!pointAnnotationDictionary.ContainsKey(pointAnnotation))
                    {
                        graphics.DrawString(pointAnnotation, textFont, Brushes.Black, locationOfPoint);
                        pointAnnotationDictionary.Add(pointAnnotation, 1);
                    }
                }
            }
            return graphics;
        }


        private void AnnnotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addAnnotation = annnotationToolStripMenuItem.Checked;
            if (tabControl.SelectedTab != null)
            {
                //tabControl.SelectedTab.Invalidate();
                //tabControl.SelectedTab.Update();
                tabControl.SelectedTab.Refresh();
            }
        }


        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            NewPanel panel = (NewPanel)sender;
            if (panel.Tag == null)
            {
                return;
            }
            string tip = "";
            int radius = 4;
            //Rectangle tabCloseIconArea
            //if (tabCloseIconArea.Contains(e.Location))

            int index = 0;
            foreach (var location in ((List<PointF[]>)panel.Tag)[1])      
            //if((List<PointF[]>)panel.Tag).ContainsKey())
            {
                if ((Math.Abs(e.X - location.X) < radius) &&
                    (Math.Abs(e.Y - location.Y) < radius))
                {
                    tip = $"({((List<PointF[]>) panel.Tag)[0][index].X}, {((List<PointF[]>)panel.Tag)[0][index].Y})";
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