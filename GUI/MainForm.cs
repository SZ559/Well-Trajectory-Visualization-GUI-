using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeometricObject;
using FileHandler;
using System.Numerics;
using System.Drawing.Configuration;

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

        public MainForm()
        {
            InitializeComponent();

            trajectoryDataReader = new TrajectoryDataReader();
            wellViewSaver = new WellViewSaver();
            projection = new Projection();

            wells = new List<Well>();
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

        private void WellsTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent == null || e.Node.Parent.Text == "Wells")
            {
                return;
            }
            else
            {
                string wellName = e.Node.Parent.Text;
                string trajectoryName = e.Node.Text;
                VisualizeWellTrajectryInThreeViews(wellName, trajectoryName);
            }
        }

        private void VisualizeWellTrajectryInThreeViews(string wellName, string trajectoryName)
        {
            string tabPageText = $"{wellName}-{trajectoryName}";
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page.Text == tabPageText)
                {
                    tabControl.SelectedTab = page;
                    return;
                }
            }

            if (tabControl.TabCount >= 10)
            {
                MessageBox.Show("Only 10 pages can be opened. Please close a page before opening a new one.");
                return;
            }

            TabPage tabPage = new TabPage
            {
                Text = tabPageText
            };

            trajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
            SetZoom();
            TableLayoutPanel tableLayoutPanel = SetTableLayoutPanelForTabPage();            
            tabPage.Controls.Add(tableLayoutPanel);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;
        }

        private void UpdateSelectedTrajectory(Object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                string wellName = tabControl.SelectedTab.Text.Split('-')[0];
                string trajectoryName = tabControl.SelectedTab.Text.Split('-')[1];
                trajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
                SetZoom();
            }
        }

        private void SetZoom()
        {
            Single maxX = trajectory.PolyLineNodes.Select(x => x.X).Max();
            Single maxY = trajectory.PolyLineNodes.Select(x => x.Y).Max();
            Single maxZ = trajectory.PolyLineNodes.Select(x => x.Z).Max();
            zoom = Math.Max(maxX, maxY);
            zoom = Math.Max(zoom, maxZ);
        }
        // Tab Page
        private void CloseTheCurrentTabPageToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                tabControl.TabPages.Remove(tabControl.SelectedTab);
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
            
            Panel mainViewPanel = InitializePanelForProjection();
            Panel topViewPanel = InitializePanelForProjection();
            Panel leftViewPanel = InitializePanelForProjection();
            tableLayoutPanel.Controls.Add(mainViewPanel, 0, 0);
            tableLayoutPanel.Controls.Add(topViewPanel, 1, 0);
            tableLayoutPanel.Controls.Add(leftViewPanel, 2, 0);

            PaintPanel(topViewPanel, "Top View");
            PaintPanel(mainViewPanel, "Main View");
            PaintPanel(leftViewPanel, "Left View");
            tableLayoutPanel.ResumeLayout();
            return tableLayoutPanel;
        }

        private Panel InitializePanelForProjection()
        {
            Panel panelTopView = new Panel
            {
                Dock = DockStyle.Fill,
            };
            return panelTopView;
        }

        private Panel PaintPanel(Panel panel, string option)
        {
            switch (option)
            {
                case "Top View":
                    panel.Paint += new PaintEventHandler(TopView_Paint);
                    break;
                case "Left View":
                    panel.Paint += new PaintEventHandler(LeftView_Paint);
                    break;
                case "Main View":
                    panel.Paint += new PaintEventHandler(MainView_Paint);
                    break;
            }
            Point locationOfPanelLabel = new Point(panel.Width - 10, 5);
            panel.Controls.Add(new Label { Text = option, Location = locationOfPanelLabel });
            return panel;
        }


        private void TopView_Paint(object sender, PaintEventArgs pe)
        {
            Graphics graphic = pe.Graphics;
            Pen skyBluePen = new Pen(Brushes.DeepSkyBlue);
            skyBluePen.Width = 1.0F;

            Panel panel = (Panel)sender;
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, Vector3.UnitZ);
            Single zoomInXAxisParameter = panel.Width / zoom;
            Single zoomInYAxisParameter = panel.Height / zoom;

            for (int i = 0; i < projectionPointIn2D.Count - 1; i = i + 1)
            {
                graphic.DrawLine(skyBluePen, projectionPointIn2D[i].X * zoomInXAxisParameter, projectionPointIn2D[i].Y * zoomInYAxisParameter, projectionPointIn2D[i + 1].X * zoomInXAxisParameter, projectionPointIn2D[i + 1].Y * zoomInYAxisParameter);
            }
            graphic.Dispose();
        }

        private void MainView_Paint(object sender, PaintEventArgs pe)
        {
            Graphics graphic = pe.Graphics;
            Pen skyBluePen = new Pen(Brushes.DeepSkyBlue);
            skyBluePen.Width = 1.0F;

            Panel panel = (Panel)sender;
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, Vector3.UnitY);
            Single zoomInXAxisParameter = panel.Width / zoom;
            Single zoomInYAxisParameter = panel.Height / zoom;

            for (int i = 0; i < projectionPointIn2D.Count - 1; i = i + 1)
            {
                graphic.DrawLine(skyBluePen, projectionPointIn2D[i].X * zoomInXAxisParameter, projectionPointIn2D[i].Y * zoomInYAxisParameter, projectionPointIn2D[i + 1].X * zoomInXAxisParameter, projectionPointIn2D[i + 1].Y * zoomInYAxisParameter);
            }
            graphic.Dispose();
        }

        private void LeftView_Paint(object sender, PaintEventArgs pe)
        {
            Graphics graphic = pe.Graphics;
            Pen skyBluePen = new Pen(Brushes.DeepSkyBlue);
            skyBluePen.Width = 1.0F;

            Panel panel = (Panel)sender;
            List<PointIn2D> projectionPointIn2D = projection.GetProjectionInPlane(trajectory.PolyLineNodes, Vector3.UnitX);
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


    }
}
