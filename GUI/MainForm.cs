using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ValueObject;
using BLLayer;
using MongoDB.Bson;
using System.Collections.Generic;

namespace Well_Trajectory_Visualization
{
    public partial class MainForm : Form
    {
        WellViewSaver wellViewSaver;
        DisplayChoice displayChoice;
        TrajectoryOperator trajectoryOperator;

        readonly int leftPadding;
        readonly int rightPadding;
        readonly int middleMargin;
        readonly int verticalPaddingForHeaderName;
        readonly int verticalPaddingForCloseIcon;

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

            wellViewSaver = new WellViewSaver();
            displayChoice = new DisplayChoice();

            trajectoryOperator = new TrajectoryOperator();
            BuildWholeTreeView();

            tabControl.Padding = new Point(18, 5);
            isDoubleClick = false;

            KeyPreview = true;
            KeyDown += Panel_KeyDown;
            KeyUp += Panel_KeyUp;
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
                    if (trajectoryOperator.HasBeenLoaded(filePath))
                    {
                        MessageBox.Show($"Trajectory has been already loaded for {filePath}!", "Loading Well Trajectory", MessageBoxButtons.OK);
                        continue;
                    }

                    string errorMessage;
                    Trajectory newTrajectory = trajectoryOperator.ImportTrajectoryFromFile(filePath, out errorMessage);
                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        trajectoryOperator.LoadTrajectoryToDatabase(newTrajectory);
                        BuildWholeTreeView();
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

        private void BuildWholeTreeView()
        {
            Dictionary<string, List<Trajectory>> treeviewDict = trajectoryOperator.ConstructTreeViewDictionary(trajectoryOperator.GetAllTrajectories());
            BuildTreeViewFromDict(treeviewDict);
        }

        private void BuildTreeViewFromDict(Dictionary<string, List<Trajectory>> treeviewDict)
        {
            wellsTreeView.BeginUpdate();
            wellsTreeView.Nodes[0].Nodes.Clear();

            int i = 0;
            foreach (var well in treeviewDict.Keys)
            {
                wellsTreeView.Nodes[0].Nodes.Add(well);
                wellsTreeView.Nodes[0].Nodes[i].Name = well;
                //wellsTreeView.Nodes[0].Nodes[i].Tag = well;
                int j = 0;
                foreach (var trajectory in treeviewDict[well])
                {
                    wellsTreeView.Nodes[0].Nodes[i].Nodes.Add(trajectory.TrajectoryName);
                    wellsTreeView.Nodes[0].Nodes[i].Nodes[j].Name = trajectory.WellName + "-" + trajectory.TrajectoryName;
                    wellsTreeView.Nodes[0].Nodes[i].Nodes[j].Tag = trajectory.MongoDbId;
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
                    OpenNewTabPage((ObjectId)node.Tag);
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
                TabPage tabPage = tabControl.TabPages[previewTabIndex];
                tabControl.TabPages.RemoveAt(previewTabIndex);
                tabPage.Dispose();
            }
        }

        private void OpenNewTabPage(ObjectId id)
        {
            Trajectory trajectory = trajectoryOperator.GetTrajectoryByTrajectoryNode(id);

            TabPage tabPage = new TabPage
            {
                Text = GetHeaderTextForTabPage(trajectory.WellName, trajectory.TrajectoryName),
                Name = $"{trajectory.WellName}-{trajectory.TrajectoryName}",
                Font = tabControl.Font,
                BorderStyle = BorderStyle.None,
                Tag = isDoubleClick // opened or preview : true means opened
            };

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Panel2Collapsed = true
            };
            splitContainer.Panel1.Controls.Add(new TableLayoutPanelForProjection(new CurrentTrajectory(trajectory), displayChoice));
            splitContainer.Panel2.Controls.Add(new PanelFor3DView(new CurrentTrajectory(trajectory), displayChoice));
            tabPage.Controls.Add(splitContainer);

            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;
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
                TabPage tabPage = tabControl.TabPages[selectedTabIndex];
                tabControl.TabPages.RemoveAt(selectedTabIndex);
                tabControl.SelectedIndex = (selectedTabIndex - 1 == -1) ? (tabControl.TabCount - 1) : (selectedTabIndex - 1);
                tabPage.Dispose();
            }
            SwitchDefaultPage();
        }

        private void SwitchDefaultPage()
        {
            defaultPagePanel.Visible = (tabControl.TabPages.Count == 0) ? true : false;
        }

        private void AnnnotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayChoice.IfShowAnnotation = annnotationToolStripMenuItem.Checked;
            if (tabControl.SelectedTab != null)
            {
                tabControl.SelectedTab.Refresh();
            }
        }

        private void MeterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                TableLayoutPanelForProjection tableLayoutPanel = (TableLayoutPanelForProjection)((SplitContainer)tabControl.SelectedTab.Controls[0]).Panel1.Controls[0];
                tableLayoutPanel.CurrentTrajectory.UnitInUse = DistanceUnit.Meter;
                tabControl.SelectedTab.Refresh();
            }
        }

        private void FeetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                TableLayoutPanelForProjection tableLayoutPanel = (TableLayoutPanelForProjection)((SplitContainer)tabControl.SelectedTab.Controls[0]).Panel1.Controls[0];
                tableLayoutPanel.CurrentTrajectory.UnitInUse = DistanceUnit.Feet;
                tabControl.SelectedTab.Refresh();
            }
        }

        private void SharpestPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayChoice.IfShowSharpestPoint = sharpestPointToolStripMenuItem.Checked;
            if (tabControl.SelectedTab != null)
            {
                tabControl.SelectedTab.Refresh();
            }
        }

        private void Panel_KeyDown(object sender, KeyEventArgs e)
        {
            displayChoice.IfUseRegionChoosing = e.KeyCode == Keys.ControlKey;

        }

        private void Panel_KeyUp(object sender, KeyEventArgs e)
        {
            displayChoice.IfUseRegionChoosing = false;
        }

        private void ResetToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                ((TableLayoutPanelForProjection)((SplitContainer)tabControl.SelectedTab.Controls[0]).Panel1.Controls[0]).ResetZoom();
                ((PanelFor3DView)((SplitContainer)tabControl.SelectedTab.Controls[0]).Panel2.Controls[0]).Reset();
                tabControl.SelectedTab.Refresh();
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateTreeView();
        }

        private void UpdateTreeView()
        {
            if (!string.IsNullOrWhiteSpace(searchTextbox.Text))
            {
                var searchResult = trajectoryOperator.GetTrajectoriesBySearchFunction(searchTextbox.Text);
                if (searchResult.Count != 0)
                {
                    searchTextbox.BackColor = Color.White;
                    var treeviewDict = trajectoryOperator.ConstructTreeViewDictionary(searchResult);
                    BuildTreeViewFromDict(treeviewDict);
                }
                else
                {
                    searchTextbox.BackColor = Color.Red;
                    BuildWholeTreeView();
                }
            }
            else
            {
                searchTextbox.BackColor = Color.White;
                BuildWholeTreeView();
            }
        }

        private void DeleteNodeToolStripButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete the(se) trajectory(/ies)?", "Deleting trajectory...", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (wellsTreeView.SelectedNode != null && wellsTreeView.SelectedNode.Parent != null)
                {
                    if (wellsTreeView.SelectedNode.GetNodeCount(true) != 0)
                    {
                        foreach (TreeNode node in wellsTreeView.SelectedNode.Nodes)
                        {
                            trajectoryOperator.DeleteTrajectoryByTrajectoryNode((ObjectId)node.Tag);
                        }
                    }
                    else
                    {
                        trajectoryOperator.DeleteTrajectoryByTrajectoryNode((ObjectId)wellsTreeView.SelectedNode.Tag);
                    }
                    UpdateTreeView();
                }
            }
        }

        private void Show3DViewToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                ((SplitContainer)tabControl.SelectedTab.Controls[0]).Panel1Collapsed = true;
                ((SplitContainer)tabControl.SelectedTab.Controls[0]).Panel2Collapsed = false;
            }
        }

        private void ShowThreeViewToolStripButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                ((SplitContainer)tabControl.SelectedTab.Controls[0]).Panel1Collapsed = false;
                ((SplitContainer)tabControl.SelectedTab.Controls[0]).Panel2Collapsed = true;
            }
        }
    }
}