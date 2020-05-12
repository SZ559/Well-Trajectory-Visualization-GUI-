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

namespace Well_Trajectory_Visualization
{
    public partial class MainForm : Form
    {

        TrajectoryDataReader trajectoryDataReader;
        WellViewSaver wellViewSaver;

        List<Well> wells;

        public MainForm()
        {
            InitializeComponent();

            trajectoryDataReader = new TrajectoryDataReader();
            wellViewSaver = new WellViewSaver();

            wells = new List<Well>();
        }

        private void LoadTrajectoryDataFromFile()
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                if (wells.SelectMany(x => x.Sources).Contains(filePath))
                {
                    MessageBox.Show("Trajectory has been already loaded!", "Loading Well", MessageBoxButtons.OK);
                    return;
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
                    MessageBox.Show($"New Well loading from {filePath} succeed.", "Loading Well", MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show(errorMessage, "Loading Well", MessageBoxButtons.OK);
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
                int j = 0;
                foreach (var trajectory in well.Trajectories)
                {
                    wellsTreeView.Nodes[0].Nodes[i].Nodes.Add(trajectory.TrajectoryName);
                    wellsTreeView.Nodes[0].Nodes[i].Nodes[j].Name = trajectory.WellName + "-" + trajectory.TrajectoryName;
                    j++;
                }
                i++;
            }

            wellsTreeView.EndUpdate();
            wellsTreeView.ExpandAll();
        }

        // Tab Page


        // Menu Bar

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTrajectoryDataFromFile();
        }

    }
}
