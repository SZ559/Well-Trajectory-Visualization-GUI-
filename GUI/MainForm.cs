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
        Trajectory trajectory;
        Single zoomX;
        Single zoomY;

        List<Well> wells;

        bool hasPreviewTab;
        private bool isDoubleClick = false;


        public MainForm()
        {
            InitializeComponent();

            trajectoryDataReader = new TrajectoryDataReader();
            wellViewSaver = new WellViewSaver();
            projection = new Projection();

            wells = new List<Well>();
            hasPreviewTab = false;
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
            TreeViewSelection(e.Node, isDoubleClick);
        }

        private void WellsTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            isDoubleClick = true;
            TreeViewSelection(e.Node, isDoubleClick);
            ChangeTabPageHeaderFontStyle(tabControl.SelectedTab, FontStyle.Regular);
        }

        private void TreeViewSelection(TreeNode node, bool isDoubleClick)
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
                OpenNewTabPage(wellName, trajectoryName);
            }
        }

        public bool IfTabPageOpened(string tabPageText)
        {
            defaultPagePanel.Visible = false;
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page.Text == tabPageText)
                {
                    tabControl.SelectedTab = page;
                    if (isDoubleClick && tabControl.TabPages.IndexOf(page) == tabControl.TabCount - 1)
                    {
                        ChangeTabPageHeaderFontStyle(page, FontStyle.Regular);
                        hasPreviewTab = false;
                    }
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
            }
        }

        private void OpenNewTabPage(string wellName, string trajectoryName)
        {
            defaultPagePanel.Visible = false;

            if (tabControl.TabCount >= 10)
            {
                MessageBox.Show("Only 10 pages can be opened. Please close a page before opening a new one.");
                return;
            }

            TabPage tabPage = new TabPage
            {
                Text = $"{wellName}-{trajectoryName}",

            };

            trajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
            SetZoomForThreeViews();

            TableLayoutPanel tableLayoutPanel = InitializeTableLayoutPanelForTabPage();
            tabPage.Controls.Add(tableLayoutPanel);
            tabControl.TabPages.Add(tabPage);
            SetTableLayoutPanel(tableLayoutPanel);
            tabControl.SelectedTab = tabPage;

            hasPreviewTab = !isDoubleClick;
            if (isDoubleClick)
            {
                ChangeTabPageHeaderFontStyle(tabControl.SelectedTab, FontStyle.Regular);
            }
            else
            {
                ChangeTabPageHeaderFontStyle(tabControl.SelectedTab, FontStyle.Italic);
            }
        }

        private void ChangeTabPageHeaderFontStyle(TabPage tabpage, FontStyle fontStyle)
        {
            Graphics g = tabControl.CreateGraphics();
            Rectangle rect = new Rectangle(tabControl.TabPages.IndexOf(tabpage) * tabControl.ItemSize.Width + 2, 2, tabControl.ItemSize.Width - 2, tabControl.ItemSize.Height - 2);
            g.FillRectangle(Brushes.LightBlue, rect);
            g.DrawString(tabpage.Text, new Font(tabControl.Font, fontStyle), Brushes.Black, rect);
        }

        private void SetZoomForThreeViews()
        {
            Single maxX = trajectory.PolyLineNodes.Select(x => x.X).Max();
            Single maxY = trajectory.PolyLineNodes.Select(x => x.Y).Max();
            Single maxZ = trajectory.PolyLineNodes.Select(x => x.Z).Max();

            Single minX = trajectory.PolyLineNodes.Select(x => x.X).Min();
            Single minY = trajectory.PolyLineNodes.Select(x => x.Y).Min();
            Single minZ = trajectory.PolyLineNodes.Select(x => x.Z).Min();

            Single max_XAxis = Math.Max(maxX, maxY);
            Single min_XAxis = Math.Min(minX, minY);

            Single max_YAxis = Math.Max(maxZ, maxY);
            Single min_YAxis = Math.Min(minZ, minY);
            zoomX = max_XAxis - min_XAxis;
            zoomY = max_YAxis - min_YAxis;
        }

        // Tab Page
        private void CloseTheCurrentTabPageToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                if (tabControl.SelectedIndex == tabControl.TabCount - 1 && hasPreviewTab == true)
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
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset,
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
            PictureBox mainViewPictureBox = InitializePictureBoxForProjection();
            PictureBox topViewPictureBox = InitializePictureBoxForProjection();
            PictureBox leftViewPictureBox = InitializePictureBoxForProjection();

            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Add(mainViewPictureBox, 0, 0);
            tableLayoutPanel.Controls.Add(topViewPictureBox, 1, 0);
            tableLayoutPanel.Controls.Add(leftViewPictureBox, 2, 0);
            tableLayoutPanel.ResumeLayout();

            PaintPictureBox(topViewPictureBox, "Top View");
            PaintPictureBox(mainViewPictureBox, "Main View");
            PaintPictureBox(leftViewPictureBox, "Left View");
        }


        private PictureBox InitializePictureBoxForProjection()
        {
            PictureBox pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.Fixed3D,
            };
            return pictureBox;
        }

        private PictureBox PaintPictureBox(PictureBox pictureBox, string option)
        {
            switch (option)
            {
                case "Top View":
                    TopView_LoadPicture(pictureBox);
                    break;
                case "Left View":
                    LeftView_LoadPicture(pictureBox);
                    break;
                case "Main View":
                    MainView_LoadPicture(pictureBox);
                    break;
            }
            return pictureBox;
        }

        private void LeftView_LoadPicture(PictureBox pictureBox)
        {
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, Vector3.UnitX);
            Bitmap bitMap = LoadPicture("Left View", pictureBox.Width, pictureBox.Height, projectionPointIn2D);
            pictureBox.Image = bitMap;
        }

        private void MainView_LoadPicture(PictureBox pictureBox)
        {
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, Vector3.UnitY);
            Bitmap bitMap = LoadPicture("Main View", pictureBox.Width, pictureBox.Height, projectionPointIn2D);
            pictureBox.Image = bitMap;
        }

        private void TopView_LoadPicture(PictureBox pictureBox)
        {
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, Vector3.UnitZ);
            Bitmap bitMap = LoadPicture("Top View", pictureBox.Width, pictureBox.Height, projectionPointIn2D);
            pictureBox.Image = bitMap;
        }

        private Bitmap LoadPicture(string text, int pictureBoxWidth, int pictureBoxHeight, List<PointIn2D> projectionPointIn2D)
        {
            int spaceX = 20;
            int spaceY = 25;
            int textPositionY = 5;
            Single zoomInXAxisParameter = (pictureBoxWidth - spaceX * 2) / zoomX;
            Single zoomInYAxisParameter = (pictureBoxHeight - spaceY * 2) / zoomY;
            Single minX = projectionPointIn2D.Select(x => x.X).Min();
            Single minY = projectionPointIn2D.Select(x => x.Y).Min();
            spaceX = SetInitialPointToPaint(minX * zoomInXAxisParameter, spaceX);
            spaceY = SetInitialPointToPaint(minY * zoomInYAxisParameter, spaceY);

            Pen skyBluePen = new Pen(Color.DeepSkyBlue);
            skyBluePen.Width = 2.0F;
            Pen darkBluePen = new Pen(Color.DarkBlue);
            SolidBrush darkBlueBrush = new SolidBrush(Color.DarkBlue);
            Bitmap bitMap = new Bitmap(pictureBoxWidth, pictureBoxHeight);
            Graphics graphic = Graphics.FromImage(bitMap);
            int radius = 3;

            for (int i = 0; i < projectionPointIn2D.Count - 1; i = i + 1)
            {
                float xForPaint = projectionPointIn2D[i].X * zoomInXAxisParameter + spaceX;
                float yForPaint = projectionPointIn2D[i].Y * zoomInYAxisParameter + spaceY;
                float x2ForPaint = projectionPointIn2D[i + 1].X * zoomInXAxisParameter + spaceX;
                float y2ForPaint = projectionPointIn2D[i + 1].Y * zoomInYAxisParameter + spaceY;
                graphic.DrawLine(skyBluePen, xForPaint, yForPaint, x2ForPaint, y2ForPaint);
                graphic.FillEllipse(darkBlueBrush, xForPaint, yForPaint, radius, radius);
            }

            graphic.FillEllipse(darkBlueBrush, projectionPointIn2D[projectionPointIn2D.Count - 1].X * zoomInXAxisParameter + spaceX, projectionPointIn2D[projectionPointIn2D.Count - 1].Y * zoomInYAxisParameter + spaceY, radius, radius);
            graphic.DrawString(text, new Font(Label.DefaultFont, FontStyle.Regular), Brushes.Black, new PointF(pictureBoxWidth - 80, textPositionY));
            skyBluePen.Dispose();
            darkBlueBrush.Dispose();
            graphic.Dispose();
            return bitMap;
        }

        private int SetInitialPointToPaint(Single minValue, int space)
        {
            if (minValue < 0)
            {
                space = (int)(space - minValue);
            }
            return space;
        }

        // Menu Bar
        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTrajectoryDataFromFile();
        }

        private void viewSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // HACK
            System.Diagnostics.Process.Start("https://github.com/SZ559/Well-Trajectory-Visualization-GUI-");
        }

        private void referenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://commons.wikimedia.org/wiki/File:Third_angle_projecting.svg");

        }

        //// TODO: ????
        //private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    Graphics g = tabControl.CreateGraphics();
        //    Rectangle rect = new Rectangle(tabControl.SelectedIndex * tabControl.ItemSize.Width + 2, 2, tabControl.ItemSize.Width - 2, tabControl.ItemSize.Height - 2);
        //    g.FillRectangle(Brushes.LightBlue, rect);
        //    g.DrawString(tabControl.SelectedTab.Text, new Font(tabControl.SelectedTab.Font, FontStyle.Italic), Brushes.Black, rect);
        //}
    }
}