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

namespace Well_Trajectory_Visualization
{
    public partial class MainForm : Form
    {

        TrajectoryDataReader trajectoryDataReader;
        Projection projection;
        WellViewSaver wellViewSaver;

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
            string tabPageText = $"{wellName} - {trajectoryName}";
            foreach (TabPage tabpage in tabControl.TabPages)
            {
                if (tabpage.Text == tabPageText)
                {
                    tabControl.SelectedTab = tabpage;
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
 
            TableLayoutPanel tableLayoutPanel = SetTableLayoutPanelForTabPage();
            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Add(DrawTopViewOfTrajectory(wellName, trajectoryName), 0, 0);
            Graphics g = tableLayoutPanel.CreateGraphics();
            Pen pen = new Pen(Color.Green);
            g.DrawLine(pen, 0, 0, 15, 15);
            tableLayoutPanel.ResumeLayout();
            
            tabPage.Controls.Add(tableLayoutPanel);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;
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
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            return tableLayoutPanel;
            //scroll?
        }

        // Tab Page

        private Panel DrawTopViewOfTrajectory(string wellName, string trajectoryName)
        {
            Trajectory trajectory = wells.Find(x => x.WellName == wellName).Trajectories.Find(x => x.TrajectoryName == trajectoryName);
            Vector3 normalVector = Vector3.UnitZ;
            List<PointIn2D> projectionInTopView = projection.GetProjectionInPlane(trajectory.PolyLineNodes, normalVector);
            Panel panelTopView = new Panel
            {
                Dock = DockStyle.Fill,
            };
            Graphics g = panelTopView.CreateGraphics();
            Pen skyBluePen = new Pen(Brushes.DeepSkyBlue);
            skyBluePen.Width = 2.0F;
            g.DrawLine(skyBluePen, 20, 10, 300, 100);
            return panelTopView;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        // Menu Bar
        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTrajectoryDataFromFile();
        }


    }
}
