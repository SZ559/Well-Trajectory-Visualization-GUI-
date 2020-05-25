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
        WellViewSaver wellViewSaver;

        List<Well> wells;

        readonly int leftPadding;
        readonly int rightPadding;
        readonly int middleMargin;
        readonly int verticalPaddingForHeaderName;
        readonly int verticalPaddingForCloseIcon;
        //internal event EventHandler AddAnnotation;

        int PreviewTabIndex
        {
            get
            {
                foreach (TabPage tabpage in tabControl.TabPages)
                {
                    //tabpage tag
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

            trajectoryDataReader = new TrajectoryDataReader();
            wellViewSaver = new WellViewSaver();
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
                    OpenNewTabPage(wellName, trajectoryName, (Trajectory) node.Tag);
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

        private void OpenNewTabPage(string wellName, string trajectoryName, Trajectory trajectory)
        {
            TabPage tabPage = new TabPage
            {
                Text = GetHeaderTextForTabPage(wellName, trajectoryName),
                Name = $"{wellName}-{trajectoryName}",
                Font = tabControl.Font,
                BorderStyle = BorderStyle.None,
                Tag = isDoubleClick // opened or preview : true means opened
            };

            TableLayoutPanel tableLayoutPanel = InitializeTableLayoutPanelForTabPage(trajectory);
            tabPage.Controls.Add(tableLayoutPanel);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;
        }

        ///////////// Panels inside Tab Page //////////////////

        private TableLayoutPanel InitializeTableLayoutPanelForTabPage(Trajectory trajectory)
        {
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                BorderStyle = BorderStyle.None,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                RowCount = 1,
                ColumnCount = 3,
                AutoScroll = true,
                Dock = DockStyle.Fill,
                Tag = trajectory,
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
            Single zoomXY = GetZoomXYForThreeViews((Trajectory)tableLayoutPanel.Tag);
            Single zoomZ = GetZoomZForThreeViews((Trajectory)tableLayoutPanel.Tag);
            List<int> largestInflectionPointIndex= GetLargestInflectionPointIndex(((Trajectory)tableLayoutPanel.Tag).PolyLineNodes);
            PanelForProjection mainViewPanel = new PanelForProjection(Vector3.UnitY, (Trajectory)tableLayoutPanel.Tag, zoomXY, zoomZ, annnotationToolStripMenuItem.Checked);
            PanelForProjection leftViewPanel = new PanelForProjection(Vector3.UnitX, (Trajectory)tableLayoutPanel.Tag, zoomXY, zoomZ, annnotationToolStripMenuItem.Checked);
            PanelForProjection topViewPanel = new PanelForProjection(Vector3.UnitZ, (Trajectory)tableLayoutPanel.Tag, zoomXY, zoomZ, annnotationToolStripMenuItem.Checked);

            mainViewPanel.LargestInflectionPointProjectionIndex = largestInflectionPointIndex;
            leftViewPanel.LargestInflectionPointProjectionIndex = largestInflectionPointIndex;
            topViewPanel.LargestInflectionPointProjectionIndex = largestInflectionPointIndex;

            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Add(mainViewPanel, 0, 0);
            tableLayoutPanel.Controls.Add(leftViewPanel, 1, 0);
            tableLayoutPanel.Controls.Add(topViewPanel, 2, 0);
            tableLayoutPanel.ResumeLayout();
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
        //public static List<Vector3> GetLargestInflectionPoint(List<Vector3> currentTrajectory)

        public static List<int> GetLargestInflectionPointIndex(List<Vector3> currentTrajectory)
        {
            float lengthOfVector1, lengthOfVector2, dotProduct;
            Vector3 vector1, vector2;
            double maxCurvature = Math.PI;
            double radian;
            int indexPreventOverlaping = 0;
            List<int> maxCurvaturePointIndex = new List<int>();
            for (int i = 1; i < currentTrajectory.Count - 1; i = i + 1)
            {
                vector1 = Vector3.Subtract(currentTrajectory[i - indexPreventOverlaping - 1], currentTrajectory[i]);
                vector2 = Vector3.Subtract(currentTrajectory[i + 1], currentTrajectory[i]);

                lengthOfVector1 = vector1.Length();
                lengthOfVector2 = vector2.Length();

                if (lengthOfVector1 == 0)
                {
                    indexPreventOverlaping = indexPreventOverlaping + 1;
                    continue;
                }

                if (lengthOfVector2 != 0)
                {
                    dotProduct = Vector3.Dot(vector1, vector2);
                    radian = Math.Acos(dotProduct / (lengthOfVector1 * lengthOfVector2));

                    if (radian > Math.PI)
                    {
                        radian = 2 * Math.PI - radian;
                    }

                    if (radian < maxCurvature)
                    {
                        maxCurvature = radian;
                        maxCurvaturePointIndex.Clear();
                        maxCurvaturePointIndex.Add(i);
                    }
                    else if (maxCurvature == radian)
                    {
                        maxCurvaturePointIndex.Add(i);
                    }
                    indexPreventOverlaping = 0;
                }
            }
            return maxCurvaturePointIndex;
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

        private void AnnnotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TabPage tabPage in tabControl.Controls)
            {
                TableLayoutPanel tableLayoutPanel = (TableLayoutPanel) tabPage.Controls[0];
                foreach (PanelForProjection panel in tableLayoutPanel.Controls)
                {
                    panel.AddAnnotation = annnotationToolStripMenuItem.Checked;
                }
            }

            //AddAnnotation?.Invoke(annnotationToolStripMenuItem.Checked, FormClosedEventArgs.Empty);
            if (tabControl.SelectedTab != null)
            {
                tabControl.SelectedTab.Refresh();
            }
        }
    }
}