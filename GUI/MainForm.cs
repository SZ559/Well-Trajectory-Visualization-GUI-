using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GeometricObject;
using FileHandler;
using System.Numerics;
using System.ComponentModel;

namespace Well_Trajectory_Visualization
{
    public partial class MainForm : Form
    {

        TrajectoryDataReader trajectoryDataReader;
        Projection projection;
        WellViewSaver wellViewSaver;
        Trajectory CurrentTrajectory
        {
            get
            {
                Trajectory currentTrajectory;
                if (tabControl.SelectedTab != null)
                {
                    string wellName = tabControl.SelectedTab.Name.Split('-')[0];
                    string trajectoryName = tabControl.SelectedTab.Name.Split('-')[1];
                    currentTrajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
                    return currentTrajectory;
                }
                else
                {
                    return null;
                }
            }
        }

        Single zoomXY;
        Single zoomZ;
        Single zoomInAxisParameter;
        List<Well> wells;

        readonly int leftPadding;
        readonly int rightPadding;
        readonly int middleMargin;
        readonly int verticalPaddingForHeaderName;
        readonly int verticalPaddingForCloseIcon;

        readonly int numberOfDataInAxisX;
        readonly int numberOfDataInAxisY;
        readonly int spaceHeightForViewName;
        readonly int segementLength;
        readonly int paddingX;
        readonly int paddingY;
        readonly int marginAxis;
        readonly int widthOfCoordinate;
        readonly int heightOfCoordinate;

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


        public MainForm()
        {
            InitializeComponent();

            leftPadding = 5;
            rightPadding = 5;
            middleMargin = 5;
            verticalPaddingForHeaderName = 5;
            verticalPaddingForCloseIcon = 9;

            numberOfDataInAxisX = 5;
            numberOfDataInAxisY = 10;
            spaceHeightForViewName = 25;
            segementLength = 5;
            marginAxis = 5;
            widthOfCoordinate = 40;
            heightOfCoordinate = 18;
            paddingX = segementLength * 3 / 2 + marginAxis + widthOfCoordinate + 10;
            paddingY = segementLength * 3 / 2 + marginAxis + heightOfCoordinate + 10;



            trajectoryDataReader = new TrajectoryDataReader();
            wellViewSaver = new WellViewSaver();
            projection = new Projection();
            tabControl.Padding = new Point(18, 5);
            wells = new List<Well>();
            isDoubleClick = false;
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
                ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
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
            Panel mainViewPanel = InitializePanelForProjection("Main View");
            Panel leftViewPanel = InitializePanelForProjection("Left View");
            Panel topViewPanel = InitializePanelForProjection("Top View");
            mainViewPanel.Paint += new PaintEventHandler(PaintPanel);
            leftViewPanel.Paint += new PaintEventHandler(PaintPanel);
            topViewPanel.Paint += new PaintEventHandler(PaintPanel);

            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Add(mainViewPanel, 0, 0);
            tableLayoutPanel.Controls.Add(leftViewPanel, 1, 0);
            tableLayoutPanel.Controls.Add(topViewPanel, 2, 0);
            tableLayoutPanel.ResumeLayout();
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

        private void PaintPanel(object sender, PaintEventArgs e)
        {
            SetZoomForThreeViews();
            DrawViewPanel((Panel)sender, e.Graphics);
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

        // tab header: https://github.com/SZ559/Well-Trajectory-Visualization-GUI-/wiki/Drawing-Tab-Page
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

            int sizeOfCloseIcon = tabHeaderArea.Height - 2 * verticalPaddingForCloseIcon;
            //draw filename
            tabHeaderArea.Offset(leftPadding, verticalPaddingForHeaderName);
            tabHeaderArea.Width = tabHeaderArea.Width - leftPadding - middleMargin - rightPadding - sizeOfCloseIcon;
            tabHeaderArea.Height = tabHeaderArea.Height - 2 * verticalPaddingForHeaderName;
            graphic.DrawString(tabHeaderText,
                new Font(tabControl.Font, fontStyle),
                SystemBrushes.ControlText,
                tabHeaderArea);

            // draw close icon
            using (Pen closeIconPen = new Pen(Color.FromArgb(240, 128, 128), 2f))
            {
                tabHeaderArea.Offset(tabHeaderArea.Width + middleMargin, -tabHeaderArea.Y + verticalPaddingForCloseIcon);
                tabHeaderArea.Width = sizeOfCloseIcon;
                tabHeaderArea.Height = sizeOfCloseIcon;
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
                    int sizeOfCloseIcon = tabCloseIconArea.Height - 2 * verticalPaddingForCloseIcon;
                    tabCloseIconArea.Offset(tabCloseIconArea.Width - (sizeOfCloseIcon + rightPadding), verticalPaddingForCloseIcon);
                    tabCloseIconArea.Width = sizeOfCloseIcon;
                    tabCloseIconArea.Height = sizeOfCloseIcon;
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
                case "Top View":
                default:
                    axisXCaption = "x";
                    axisYCaption = "y";
                    break;
            }
            return new string[] { axisXCaption, axisYCaption };
        }


        ///////////// Paint Panels ////////////////////

        private void SetZoomForThreeViews()
        {
            Single maxX = CurrentTrajectory.PolyLineNodes.Select(x => x.X).Max();
            Single maxY = CurrentTrajectory.PolyLineNodes.Select(x => x.Y).Max();
            Single maxZ = CurrentTrajectory.PolyLineNodes.Select(x => x.Z).Max();

            Single minX = CurrentTrajectory.PolyLineNodes.Select(x => x.X).Min();
            Single minY = CurrentTrajectory.PolyLineNodes.Select(x => x.Y).Min();
            Single minZ = CurrentTrajectory.PolyLineNodes.Select(x => x.Z).Min();

            zoomXY = Math.Max(maxX - minX, maxY - minY);
            zoomXY = zoomXY > 0 ? zoomXY : 1;
            zoomZ = maxZ - minZ;
            zoomZ = zoomZ > 0 ? zoomZ : 1;
        }

        // use cuboid of control to try to include the whole trajectory
        private void GetZoomInAxisParameter(Control control)
        {
            if (zoomZ * (control.Width - 2 * paddingX) > zoomXY * (control.Height - 2 * paddingY - spaceHeightForViewName))
            {
                zoomInAxisParameter = (control.Height - 2 * paddingY - spaceHeightForViewName) / zoomZ;
            }
            else
            {
                zoomInAxisParameter = (control.Width - 2 * paddingX) / zoomXY;
            }
        }

        private void DrawViewPanel(Panel viewPanel, Graphics graphics)
        {
            Vector3 normalVector = GetNormalVectorForView(viewPanel.Name);
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(CurrentTrajectory.PolyLineNodes, normalVector);
            GetZoomInAxisParameter(viewPanel);
            Single minX = projectionPointIn2D.Select(x => x.X).Min();
            Single minY = projectionPointIn2D.Select(x => x.Y).Min();
            int spaceX = (int)(paddingX - minX * zoomInAxisParameter);
            int spaceY = (int)(paddingY + spaceHeightForViewName - minY * zoomInAxisParameter);

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
                Rectangle rect = new Rectangle(0, 0, viewPanel.Width, spaceHeightForViewName);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                graphics.DrawString(viewPanel.Name, fontForCaption, Brushes.Black, rect, stringFormat);
            }

            //draw axis
            string axisXCaption = GetAxisCaption(viewPanel.Name)[0];
            string axisYCaption = GetAxisCaption(viewPanel.Name)[1];


            PointF upperLeftAxisPoint = new PointF(paddingX - marginAxis, spaceHeightForViewName + paddingY - marginAxis);
            PointF upperRightAxisPoint = new PointF(viewPanel.Width - paddingX + marginAxis, spaceHeightForViewName + paddingY - marginAxis);
            PointF lowerLeftAxisPoint = new PointF(paddingX - marginAxis, viewPanel.Height - paddingY + marginAxis);
            PointF lowerRightAxisPoint = new PointF(viewPanel.Width - paddingX + marginAxis, viewPanel.Height - paddingY + marginAxis);
            Font textFont = viewPanel.Font;
            using (Pen penForAxis = new Pen(Color.Black, 0.3F))
            {
                // draw axis border
                graphics.DrawLine(penForAxis, upperLeftAxisPoint, upperRightAxisPoint);
                graphics.DrawLine(penForAxis, upperRightAxisPoint, lowerRightAxisPoint);
                graphics.DrawLine(penForAxis, lowerRightAxisPoint, lowerLeftAxisPoint);
                graphics.DrawLine(penForAxis, lowerLeftAxisPoint, upperLeftAxisPoint);

                // draw axis name
                graphics.DrawString(axisXCaption, textFont, Brushes.Black, upperRightAxisPoint.X + segementLength, upperRightAxisPoint.Y - heightOfCoordinate / 2);
                graphics.DrawString(axisYCaption, textFont, Brushes.Black, lowerLeftAxisPoint.X - widthOfCoordinate / 2, lowerLeftAxisPoint.Y + segementLength);

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

            graphics.Dispose();
        }

    }
}