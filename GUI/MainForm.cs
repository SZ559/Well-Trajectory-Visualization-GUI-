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

        bool hasPreviewTab;

        List<Well> wells;

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
                VisualizeWellTrajectoryInThreeViews(wellName, trajectoryName, 2);
            }
        }

        private void wellsTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent == null || e.Node.Parent.Text == "Wells")
            {
                return;
            }
            else
            {
                string wellName = e.Node.Parent.Text;
                string trajectoryName = e.Node.Text;
                VisualizeWellTrajectoryInThreeViews(wellName, trajectoryName, 1);
            }
        }

        private void ChangeTabPageHeaderFontStyle(TabPage tabpage, FontStyle fontStyle)
        {
            Graphics g = tabControl.CreateGraphics();
            Rectangle rect = new Rectangle(tabControl.TabPages.IndexOf(tabpage) * tabControl.ItemSize.Width + 2, 2, tabControl.ItemSize.Width - 2, tabControl.ItemSize.Height - 2);
            g.FillRectangle(Brushes.LightBlue, rect);
            g.DrawString(tabpage.Text, new Font(tabControl.Font, fontStyle), Brushes.Black, rect);
        }


        private void VisualizeWellTrajectoryInThreeViews(string wellName, string trajectoryName, int clickMode)
        {
            defaultPagePanel.Visible = false;

            string tabPageText = $"{wellName} - {trajectoryName}";

            foreach (TabPage tabpage in tabControl.TabPages)
            {
                if (tabpage.Text == tabPageText)
                {
                    tabControl.SelectedTab = tabpage;
                    if (clickMode == 2 && tabControl.TabPages.IndexOf(tabpage) == tabControl.TabCount - 1)
                    {
                        ChangeTabPageHeaderFontStyle(tabpage, FontStyle.Regular);
                        hasPreviewTab = false;
                    }
                    return;
                }
            }

            if (hasPreviewTab == false && tabControl.TabCount >= 10)
            {
                MessageBox.Show("Only 10 pages can be opened. Please close a page before opening a new one.");
                return;
            }

            TabPage tabPage = new TabPage
            {
                Text = tabPageText,
            };
            

            TableLayoutPanel tableLayoutPanel = SetTableLayoutPanelForTabPage();
            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Add(DrawTopViewOfTrajectory(wellName, trajectoryName), 0, 0);
            Graphics g = tableLayoutPanel.CreateGraphics();
            Pen pen = new Pen(Color.Green);
            g.DrawLine(pen, 0, 0, 15, 15);
            tableLayoutPanel.ResumeLayout();

            tabPage.Controls.Add(tableLayoutPanel);

            if (hasPreviewTab)
            {
                tabControl.TabPages.RemoveAt(tabControl.TabCount - 1);
            }

            tabControl.TabPages.Add(tabPage);
            if(clickMode == 1)
            {
                ChangeTabPageHeaderFontStyle(tabPage, FontStyle.Italic);
            }else if (clickMode == 2)
            {
                ChangeTabPageHeaderFontStyle(tabPage, FontStyle.Regular);
            }
            
            tabControl.SelectedTab = tabPage;


            if(clickMode == 1)
            {
                hasPreviewTab = true;
            }
            else if (clickMode == 2)
            {
                hasPreviewTab = false;
            }
        }

        // Tab Page
        private void CloseTheCurrentTabPageToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                if(tabControl.SelectedIndex == tabControl.TabCount-1  && hasPreviewTab == true)
                {
                    hasPreviewTab = false;
                }
                tabControl.TabPages.Remove(tabControl.SelectedTab);
                tabControl.SelectedIndex = tabControl.TabCount - 1;
                if(tabControl.SelectedIndex == -1)
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
