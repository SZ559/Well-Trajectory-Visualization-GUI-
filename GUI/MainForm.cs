using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GeometricObject;
using FileHandler;
using System.Numerics;
using System.Text;
using System.Drawing.Text;

namespace Well_Trajectory_Visualization
{
    public partial class MainForm : Form
    {

        TrajectoryDataReader trajectoryDataReader;
        Projection projection;
        WellViewSaver wellViewSaver;
        Trajectory trajectory;

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

        bool hasPreviewTab;
        private bool isDoubleClick;


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
            hasPreviewTab = false;
            isDoubleClick = false;
        }

        private void SaveViewToFigure(Control control, string viewName, Trajectory trajectory)
        {
            saveFileDialog.FileName = tabControl.SelectedTab.Text + "-" + viewName;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string errorMessage;
                string filePath = saveFileDialog.FileName;
                Vector3 normalVector = GetNormalVectorForView(viewName);
                List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, normalVector);
                Bitmap bitmap = PaintView(control, projectionPointIn2D);
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



        // TreeView
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
                    wellsTreeView.Nodes[0].Nodes[i].Nodes[j].Name = trajectory.WellName + " - " + trajectory.TrajectoryName;
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
                string tabPageText = $"{wellName}-{trajectoryName}";
                if (IfTabPageOpened(tabPageText))
                {
                    return;
                }
                RemovePreviewTab();

                if (tabControl.TabCount >= 10)
                {
                    MessageBox.Show("Only 10 pages can be opened. Please close a page before opening a new one.");
                    return;
                }

                OpenNewTabPage(wellName, trajectoryName);
            }
        }


        // tab page
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
        private void DrawOnTab(object sender, DrawItemEventArgs e)
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

            string text = tabControl.TabPages[e.Index].Text;
            FontStyle fontStyle = (bool)tabControl.TabPages[e.Index].Tag == true ? FontStyle.Regular : FontStyle.Italic;

            //draw filename
            tabHeaderArea.Offset(leftMarginXForTabHeaderRectangle, paddingYForTabHeaderRectangle);
            tabHeaderArea.Width = tabHeaderArea.Width - leftMarginXForTabHeaderRectangle - rightMarginXForTabHeaderRectangle - widthOfCloseIcon;
            tabHeaderArea.Height = tabHeaderArea.Height - 2 * paddingYForTabHeaderRectangle;
            graphic.DrawString(text,
                tabControl.Font,
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

        private void TabControl_MouseDown(object sender, MouseEventArgs e)
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
                        CloseTabPage();
                    }
                }
            }
        }

        public bool IfTabPageOpened(string tabPageText)
        {
            defaultPagePanel.Visible = false;
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page.Name == tabPageText)
                {
                    if (isDoubleClick && tabControl.TabPages.IndexOf(page) == tabControl.TabCount - 1)
                    {
                        page.Tag = true;
                        hasPreviewTab = false;
                        tabControl.SelectedTab = null;
                    }
                    tabControl.SelectedTab = page;
                    return true;
                }
            }
            return false;
        }

        private void RemovePreviewTab()
        {
            if (hasPreviewTab == true)
            {
                tabControl.TabPages.RemoveAt(tabControl.TabCount - 1);
                hasPreviewTab = false;
            }
        }

        private void OpenNewTabPage(string wellName, string trajectoryName)
        {
            defaultPagePanel.Visible = false;

            TabPage tabPage = new TabPage
            {
                Text = GetHeaderTextForTabPage(wellName, trajectoryName),
                Name = $"{wellName}-{trajectoryName}",
                Font = tabControl.Font,
                BorderStyle = BorderStyle.None,
                Name = $"{wellName}-{trajectoryName}"
            };
            tabPage.Tag = isDoubleClick; // opened or preview : true means opened
            trajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);

            SetZoomForThreeViews();
            TableLayoutPanel tableLayoutPanel = InitializeTableLayoutPanelForTabPage();
            tabPage.Controls.Add(tableLayoutPanel);
            tabControl.TabPages.Add(tabPage);
            SetTableLayoutPanel(tableLayoutPanel);
            tabControl.SelectedTab = tabPage;

            hasPreviewTab = !isDoubleClick;
        }

        private void SetZoomForThreeViews()
        {
            Single maxX = trajectory.PolyLineNodes.Select(x => x.X).Max();
            Single maxY = trajectory.PolyLineNodes.Select(x => x.Y).Max();
            Single maxZ = trajectory.PolyLineNodes.Select(x => x.Z).Max();

            Single minX = trajectory.PolyLineNodes.Select(x => x.X).Min();
            Single minY = trajectory.PolyLineNodes.Select(x => x.Y).Min();
            Single minZ = trajectory.PolyLineNodes.Select(x => x.Z).Min();

            zoomXY = Math.Max(maxX - minX, maxY - minY);
            zoomXY = zoomXY > 0 ? zoomXY : 1;
            zoomZ = maxZ - minZ;
            zoomZ = zoomZ > 0 ? zoomZ : 1;
        }


        // Tab Page
        private void CloseTheCurrentTabPageToolStripButton_Click(object sender, EventArgs e)
        {
            CloseTabPage();
        }

        private void CloseTabPage()
        {
            if (tabControl.SelectedTab != null)
            {
                if ((bool)tabControl.SelectedTab.Tag == false)
                {
                    hasPreviewTab = false;
                }
                tabControl.TabPages.Remove(tabControl.SelectedTab);
                tabControl.SelectedIndex = tabControl.TabCount - 1;
                if (tabControl.SelectedIndex == -1)
                {
                    defaultPagePanel.Visible = true;
                }
            }
        }

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
            return tableLayoutPanel;
        }

        private void SetTableLayoutPanel(TableLayoutPanel tableLayoutPanel)
        {
            //PictureBox mainViewPictureBox = InitializePictureBoxForProjection("Main View");
            //PictureBox leftViewPictureBox = InitializePictureBoxForProjection("Left View");
            //PictureBox topViewPictureBox = InitializePictureBoxForProjection("Top View");
            Panel mainViewPanel = InitializePanelForProjection("Main View");
            Panel leftViewPanel = InitializePanelForProjection("Left View");
            Panel topViewPanel = InitializePanelForProjection("Top View");

            tableLayoutPanel.SuspendLayout();
            //tableLayoutPanel.Controls.Add(mainViewPictureBox, 0, 0);
            //tableLayoutPanel.Controls.Add(leftViewPictureBox, 1, 0);
            //tableLayoutPanel.Controls.Add(topViewPictureBox, 2, 0);
            tableLayoutPanel.Controls.Add(mainViewPanel, 0, 0);
            tableLayoutPanel.Controls.Add(leftViewPanel, 1, 0);
            tableLayoutPanel.Controls.Add(topViewPanel, 2, 0);

            tableLayoutPanel.ResumeLayout();

            //PaintPictureBox(mainViewPictureBox);
            //PaintPictureBox(leftViewPictureBox);
            //PaintPictureBox(topViewPictureBox);
        }

        private Panel InitializePanelForProjection(string viewName)
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Name = viewName
            };
        }

        //private PictureBox InitializePictureBoxForProjection(string viewName)
        //{
        //    PictureBox pictureBox = new PictureBox
        //    {
        //        Dock = DockStyle.Fill,
        //        BorderStyle = BorderStyle.None,
        //        Name = viewName
        //    };
        //    return pictureBox;
        //}

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

        private void PaintSelectedTab(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                string wellName = tabControl.SelectedTab.Name.Split('-')[0];
                string trajectoryName = tabControl.SelectedTab.Name.Split('-')[1];
                trajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
                SetZoomForThreeViews();
                TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)tabControl.SelectedTab.Controls[0];

                Panel mainViewPanel = (Panel)tableLayoutPanel.Controls.Find("Main View", true).First();
                Panel leftViewPanel = (Panel)tableLayoutPanel.Controls.Find("Left View", true).First();
                Panel topViewPanel = (Panel)tableLayoutPanel.Controls.Find("Top View", true).First();

                DrawViewPanel(mainViewPanel);
                DrawViewPanel(leftViewPanel);
                DrawViewPanel(topViewPanel);
            }
        }

        private void PaintPictureBox(PictureBox pictureBox)
        {
            Vector3 normalVector = GetNormalVectorForView(pictureBox.Name);
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, normalVector);
            Bitmap bitMap = PaintView(pictureBox, projectionPointIn2D);
            pictureBox.Image = bitMap;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        }

        // use cuboid of control to try to include the whole trajectory
        private void GetZoomInAxixParameter(Control control, int paddingX, int paddingY)
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

        private Bitmap PaintView(Control control, List<PointIn2D> projectionPointIn2D)
        {
            int paddingX = 30;
            int paddingY = 35;
            GetZoomInAxixParameter(control, paddingX, paddingY);
            Single minX = projectionPointIn2D.Select(x => x.X).Min();
            Single minY = projectionPointIn2D.Select(x => x.Y).Min();
            int spaceX = (int)(paddingX - minX * zoomInAxisParameter);
            int spaceY = (int)(paddingY - minY * zoomInAxisParameter);

            Bitmap bitMap = new Bitmap(control.Width, control.Height);
            Graphics graphics = Graphics.FromImage(bitMap);

            // draw line
            using (Pen penForLine = new Pen(Color.FromArgb(204, 234, 187), 3.0F))
            {
                for (int i = 0; i < projectionPointIn2D.Count - 1; i = i + 1)
                {
                    float xForPaint = projectionPointIn2D[i].X * zoomInAxisParameter + spaceX;
                    float yForPaint = projectionPointIn2D[i].Y * zoomInAxisParameter + spaceY;
                    float x2ForPaint = projectionPointIn2D[i + 1].X * zoomInAxisParameter + spaceX;
                    float y2ForPaint = projectionPointIn2D[i + 1].Y * zoomInAxisParameter + spaceY;
                    graphics.DrawLine(penForLine, xForPaint, yForPaint, x2ForPaint, y2ForPaint);
                }
            }

            // highlight data points
            using (SolidBrush brushForPoint = new SolidBrush(Color.FromArgb(63, 63, 68)))
            {
                foreach (var point in projectionPointIn2D)
                {
                    graphics.FillRectangle(brushForPoint, point.X * zoomInAxisParameter + spaceX - 1, point.Y * zoomInAxisParameter + spaceY - 1, 2, 2);
                    //graphics.FillEllipse(brushForPoint, point.X * zoomInXAxisParameter + spaceX - radius / 2, point.Y * zoomInYAxisParameter + spaceY - radius / 2, radius, radius);
                }
            }

            StringFormat stringFormatCenterAlignment = new StringFormat();
            stringFormatCenterAlignment.Alignment = StringAlignment.Center;
            stringFormatCenterAlignment.LineAlignment = StringAlignment.Center;
            // draw caption
            using (Font fontForCaption = new Font("Microsoft YaHei Light", 10, FontStyle.Regular, GraphicsUnit.Point))
            {
                Rectangle rect = new Rectangle(0, 0, control.Width, paddingY - 20);
                graphics.DrawString(control.Name, fontForCaption, Brushes.Black, rect, stringFormatCenterAlignment);
            }

            //draw axis
            string axisXCaption;
            string axisYCaption;
            switch (control.Name)
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

            int lineLength = 5;
            int spaceForTextInXDirection = 35;
            int spaceForTextInYDirection = 20;
            int spaceForTextAlignment = 20;

            PointF xAxisStartPoint = new PointF(paddingX - lineLength, paddingY);
            PointF yAxisStartPoint = new PointF(paddingX, paddingY - lineLength);
            PointF xAxisEndPoint = new PointF(control.Width - paddingX, paddingY);
            PointF yAxisEndPoint = new PointF(paddingX, control.Height - paddingY);
            PointF[] xAxisArrowhead = new PointF[] { new PointF(xAxisEndPoint.X, xAxisEndPoint.Y - lineLength), new PointF(xAxisEndPoint.X, xAxisEndPoint.Y + lineLength), new PointF(xAxisEndPoint.X + lineLength, xAxisEndPoint.Y) };
            PointF[] yAxisArrowhead = new PointF[] { new PointF(yAxisEndPoint.X - lineLength, yAxisEndPoint.Y), new PointF(yAxisEndPoint.X + lineLength, yAxisEndPoint.Y), new PointF(yAxisEndPoint.X, yAxisEndPoint.Y + lineLength) };
            Font textFont = control.Font;
            using (Pen penForAxis = new Pen(Color.Black, 0.3F))
            {
                graphics.DrawLine(penForAxis, xAxisStartPoint, xAxisEndPoint);
                graphics.DrawLine(penForAxis, yAxisStartPoint, yAxisEndPoint);
                graphics.DrawString(axisXCaption, textFont, Brushes.Black, xAxisEndPoint.X - spaceForTextAlignment / 2, xAxisEndPoint.Y - spaceForTextInYDirection);
                graphics.DrawString(axisYCaption, textFont, Brushes.Black, yAxisEndPoint.X - spaceForTextInXDirection / 2, yAxisEndPoint.Y - spaceForTextAlignment / 2);
                graphics.DrawPolygon(penForAxis, xAxisArrowhead);
                graphics.FillPolygon(Brushes.Black, xAxisArrowhead);
                graphics.DrawPolygon(penForAxis, yAxisArrowhead);
                graphics.FillPolygon(Brushes.Black, yAxisArrowhead);

                int scale = (int)(1 / zoomInAxisParameter) * 60;
                int coordinateX = (int) minX;
                int coordinateY = (int) minY;
                float coordinateXLocationX = coordinateX * zoomInAxisParameter + spaceX;
                float coordinateXLocationY = xAxisStartPoint.Y - spaceForTextInYDirection;
                StringFormat stringFormatRightAlignment = new StringFormat();
                stringFormatRightAlignment.Alignment = StringAlignment.Far;
                while (coordinateXLocationX <= xAxisEndPoint.X - scale / 3)
                {
                    Rectangle rectangle = new Rectangle((int)(coordinateXLocationX - spaceForTextAlignment), (int)coordinateXLocationY, spaceForTextAlignment * 2, (int)(spaceForTextInYDirection - lineLength));
                    graphics.DrawLine(penForAxis, coordinateXLocationX, xAxisStartPoint.Y, coordinateXLocationX, xAxisStartPoint.Y - lineLength);
                    graphics.DrawString(coordinateX.ToString(), textFont, Brushes.Black, rectangle, stringFormatCenterAlignment);
                    //graphics.DrawString(coordinateX.ToString(), textFont, Brushes.Black, coordinateXLocationX - spaceForTextAlignment, coordinateXLocationY);
                    coordinateX = coordinateX + scale;
                    coordinateXLocationX = coordinateX * zoomInAxisParameter + spaceX;
                }
                float coordinateYLocationX = yAxisStartPoint.X - spaceForTextInXDirection;
                float coordinateYLocationY = (int) coordinateY * zoomInAxisParameter + spaceY;
                while (coordinateYLocationY <= yAxisEndPoint.Y - scale / 3)
                {
                    graphics.DrawLine(penForAxis, yAxisStartPoint.X, coordinateYLocationY, yAxisStartPoint.X - lineLength, coordinateYLocationY);
                    Rectangle rectangle = new Rectangle((int) coordinateYLocationX, (int) (coordinateYLocationY - 8), (int) (spaceForTextInXDirection - lineLength), spaceForTextInYDirection);
                    graphics.DrawString(coordinateY.ToString(), textFont, Brushes.Black, rectangle, stringFormatRightAlignment);
                    //graphics.DrawString(coordinateY.ToString(), textFont, Brushes.Black, coordinateYLocationX, coordinateYLocationY - spaceForTextAlignment / 2);
                    coordinateY = coordinateY + scale;
                    coordinateYLocationY = coordinateY * zoomInAxisParameter + spaceY;
                }
            }
            
            
            graphics.Dispose();

            return bitMap;
        }


        // Menu Bar
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



        private void SaveToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)tabControl.SelectedTab.Controls[0];
                PictureBox mainViewPictureBox = (PictureBox)tableLayoutPanel.Controls.Find("Main View", true).First();
                PictureBox leftViewPictureBox = (PictureBox)tableLayoutPanel.Controls.Find("Left View", true).First();
                PictureBox topViewPictureBox = (PictureBox)tableLayoutPanel.Controls.Find("Top View", true).First();

                Panel newPanel = new Panel();
                newPanel.Size = mainViewPictureBox.Size;
                newPanel.Name = "Main View";
                SaveViewToFigure(newPanel, "Main View", trajectory);
                newPanel.Name = "Left View";
                SaveViewToFigure(newPanel, "Left View", trajectory);
                newPanel.Name = "Top View";
                SaveViewToFigure(newPanel, "Top View", trajectory);
            }
        }

        private void MainViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)tabControl.SelectedTab.Controls[0];
                PictureBox mainViewPictureBox = (PictureBox)tableLayoutPanel.Controls.Find("Main View", true).First();

                Panel newPanel = new Panel();
                newPanel.Size = mainViewPictureBox.Size;
                newPanel.Name = "Main View";
                SaveViewToFigure(newPanel, "Main View", trajectory);
            }
        }

        private void LeftViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)tabControl.SelectedTab.Controls[0];
                PictureBox mainViewPictureBox = (PictureBox)tableLayoutPanel.Controls.Find("Main View", true).First();

                Panel newPanel = new Panel();
                newPanel.Size = mainViewPictureBox.Size;
                newPanel.Name = "Left View";
                SaveViewToFigure(newPanel, "Left View", trajectory);
            }
        }

        private void TopViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)tabControl.SelectedTab.Controls[0];
                PictureBox mainViewPictureBox = (PictureBox)tableLayoutPanel.Controls.Find("Main View", true).First();

                Panel newPanel = new Panel();
                newPanel.Size = mainViewPictureBox.Size;
                newPanel.Name = "Top View";
                SaveViewToFigure(newPanel, "Top View", trajectory);
            }
        }

        private void ThreeViewsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex != -1)
            {
                TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)tabControl.SelectedTab.Controls[0];
                PictureBox mainViewPictureBox = (PictureBox)tableLayoutPanel.Controls.Find("Main View", true).First();

                Panel newPanel = new Panel
                {
                    Size = mainViewPictureBox.Size,
                    Name = "Main View",
                };
                SaveViewToFigure(newPanel, "Main View", trajectory);
                newPanel.Name = "Left View";
                SaveViewToFigure(newPanel, "Left View", trajectory);
                newPanel.Name = "Top View";
                SaveViewToFigure(newPanel, "Top View", trajectory);
            }
        }

        private void DrawViewPanel(Panel viewPanel)
        {
            int paddingX = 20;
            int paddingY = 25;

            Vector3 normalVector = GetNormalVectorForView(viewPanel.Name);
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, normalVector);
            GetZoomInAxixParameter(viewPanel, paddingX, paddingY);
            Single minX = projectionPointIn2D.Select(x => x.X).Min();
            Single minY = projectionPointIn2D.Select(x => x.Y).Min();
            int spaceX = (int)(paddingX - minX * zoomInAxisParameter);
            int spaceY = (int)(paddingY - minY * zoomInAxisParameter);

            Graphics graphics = viewPanel.CreateGraphics();

            // draw line
            using (Pen penForLine = new Pen(Color.FromArgb(204, 234, 187), 3.0F))
            {
                for (int i = 0; i < projectionPointIn2D.Count - 1; i = i + 1)
                {
                    float xForPaint = projectionPointIn2D[i].X * zoomInAxisParameter + spaceX;
                    float yForPaint = projectionPointIn2D[i].Y * zoomInAxisParameter + spaceY;
                    float x2ForPaint = projectionPointIn2D[i + 1].X * zoomInAxisParameter + spaceX;
                    float y2ForPaint = projectionPointIn2D[i + 1].Y * zoomInAxisParameter + spaceY;
                    graphics.DrawLine(penForLine, xForPaint, yForPaint, x2ForPaint, y2ForPaint);
                }
            }

            // highlight data points
            using (SolidBrush brushForPoint = new SolidBrush(Color.FromArgb(63, 63, 68)))
            {
                foreach (var point in projectionPointIn2D)
                {
                    graphics.FillRectangle(brushForPoint, point.X * zoomInAxisParameter + spaceX - 1, point.Y * zoomInAxisParameter + spaceY - 1, 2, 2);
                }
            }

            // draw caption
            using (Font fontForCaption = new Font("Microsoft YaHei", 11, FontStyle.Regular, GraphicsUnit.Point))
            {
                Rectangle rect = new Rectangle(0, 0, viewPanel.Width, paddingY - 5);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                graphics.DrawString(viewPanel.Name, fontForCaption, Brushes.Black, rect, stringFormat);
            }
        }
    }
}