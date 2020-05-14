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
        Single zoom;
        List<Well> wells;

        bool hasPreviewTab;
        private bool isDoubleClick;

        public MainForm()
        {
            InitializeComponent();

            trajectoryDataReader = new TrajectoryDataReader();
            wellViewSaver = new WellViewSaver();
            projection = new Projection();

            wells = new List<Well>();
            hasPreviewTab = false;
            isDoubleClick = false;
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
                OpenNewTabPage(wellName, trajectoryName);
            }
        }

        // tab page
        public bool IfTabPageOpened(string tabPageText)
        {
            defaultPagePanel.Visible = false;
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page.Text == tabPageText)
                {
                    if (isDoubleClick && tabControl.TabPages.IndexOf(page) == tabControl.TabCount - 1)
                    {
                        page.Tag = true;
                        hasPreviewTab = false;
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
                Text = $"{wellName}-{trajectoryName}"
            };

            trajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
            SetZoom();

            TableLayoutPanel tableLayoutPanel = SetTableLayoutPanelForTabPage();

            tabPage.Controls.Add(tableLayoutPanel);
            tabPage.Tag = isDoubleClick; // opened or preview : true means opened
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;

            hasPreviewTab = !isDoubleClick;
        }

        //private void UpdateSelectedTrajectory(Object sender, EventArgs e)
        //{
        //    if (tabControl.SelectedTab != null)
        //    {
        //        string wellName = tabControl.SelectedTab.Text.Split('-')[0];
        //        string trajectoryName = tabControl.SelectedTab.Text.Split('-')[1];
        //        trajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
        //        SetZoom();
        //    }
        //}

        private void SetZoom()
        {
            Single maxX = trajectory.PolyLineNodes.Select(x => x.X).Max();
            Single maxY = trajectory.PolyLineNodes.Select(x => x.Y).Max();
            Single maxZ = trajectory.PolyLineNodes.Select(x => x.Z).Max();
            zoom = Math.Max(Math.Max(maxX, maxY), maxZ);
        }

        // Tab Page
        private void CloseTheCurrentTabPageToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                if ((bool)tabControl.SelectedTab.Tag == false)
                {
                    hasPreviewTab = false;
                }
                tabControl.TabPages.Remove(tabControl.SelectedTab);
                //tabControl.SelectedIndex = tabControl.TabCount - 1;
                if (tabControl.SelectedIndex == -1)
                {
                    defaultPagePanel.Visible = true;
                }
            }
        }

        private TableLayoutPanel SetTableLayoutPanelForTabPage()
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

            Panel mainViewPanel = InitializePanelForProjection("Top View");
            Panel topViewPanel = InitializePanelForProjection("Main View");
            Panel leftViewPanel = InitializePanelForProjection("Left View");
            tableLayoutPanel.Controls.Add(mainViewPanel, 0, 0);
            tableLayoutPanel.Controls.Add(topViewPanel, 1, 0);
            tableLayoutPanel.Controls.Add(leftViewPanel, 2, 0);

            PaintPanel(topViewPanel);
            PaintPanel(mainViewPanel);
            PaintPanel(leftViewPanel);
            tableLayoutPanel.ResumeLayout();
            return tableLayoutPanel;
        }

        private Panel InitializePanelForProjection(string text)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Name = text
            };
            return panel;
        }

        private void PaintPanel(Panel panel)
        {
            panel.Paint += new PaintEventHandler(View_Paint);

            Point locationOfPanelLabel = new Point(panel.Width - 10, 5);
            panel.Controls.Add(new Label { Text = panel.Name, Location = locationOfPanelLabel });
        }

        private void View_Paint(object sender, PaintEventArgs pe)
        {
            Graphics graphic = pe.Graphics;
            Pen skyBluePen = new Pen(Brushes.DeepSkyBlue);
            skyBluePen.Width = 1.0F;

            Panel panel = (Panel)sender;
            Vector3 normalVector;
            switch (panel.Name)
            {
                case "Top View":
                    normalVector = Vector3.UnitZ;
                    break;
                case "Main View":
                    normalVector = Vector3.UnitY;
                    break;
                case "Left View":
                default:
                    normalVector = Vector3.UnitZ;
                    break;
            }
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, normalVector);
            Single zoomInXAxisParameter = panel.Width / zoom;
            Single zoomInYAxisParameter = panel.Height / zoom;

            for (int i = 0; i < projectionPointIn2D.Count - 1; i = i + 1)
            {
                graphic.DrawLine(skyBluePen, projectionPointIn2D[i].X * zoomInXAxisParameter, projectionPointIn2D[i].Y * zoomInYAxisParameter, projectionPointIn2D[i + 1].X * zoomInXAxisParameter, projectionPointIn2D[i + 1].Y * zoomInYAxisParameter);
            }
            graphic.Dispose();
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

        // TODO: ????
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                string wellName = tabControl.SelectedTab.Text.Split('-')[0];
                string trajectoryName = tabControl.SelectedTab.Text.Split('-')[1];
                trajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
                SetZoom();

                FontStyle fontStyle = (bool)tabControl.SelectedTab.Tag ? FontStyle.Regular : FontStyle.Italic;

                Graphics g = tabControl.CreateGraphics();
                Rectangle rect = new Rectangle(tabControl.SelectedIndex * tabControl.ItemSize.Width + 2, 2, tabControl.ItemSize.Width - 2, tabControl.ItemSize.Height - 2);
                g.FillRectangle(Brushes.LightBlue, rect);
                g.DrawString(tabControl.SelectedTab.Text, new Font(tabControl.SelectedTab.Font, fontStyle), Brushes.Black, rect);
            }
        }
    }
}