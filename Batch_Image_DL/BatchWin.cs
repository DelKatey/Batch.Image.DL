using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Downloading;

namespace Batch_Image_DL
{
    public partial class BatchWin : Form
    {
        private static int SelectedIndex = 0, initialValue = 0, BatchStart = 1, BatchEnd = 1, TotalPages = 0;
        private static string BatchUrl = "", BatchDest = "";
        private bool Processing = false;

        public BatchWin()
        {
            InitializeComponent();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute))
            {
                if ((urlTextBox.Text.Contains("mangahere")) || (urlTextBox.Text.Contains("mangafox")) || (urlTextBox.Text.Contains("xkcd") && !urlTextBox.Text.Contains("explain") && !urlTextBox.Text.Contains("wiki")))
                {
                     if (!String.IsNullOrWhiteSpace(startTextBox.Text))
                     {
                         if (!String.IsNullOrWhiteSpace(dirTextBox.Text) && Path.IsPathRooted(dirTextBox.Text))
                         {
                             ListViewItem lvi = new ListViewItem(urlTextBox.Text.Trim());
                             lvi.SubItems.Add(startTextBox.Text);

                             if ((!String.IsNullOrWhiteSpace(endTextBox.Text)) && (int.Parse(endTextBox.Text) > int.Parse(startTextBox.Text)))
                                lvi.SubItems.Add(endTextBox.Text);
                             else if ((!String.IsNullOrWhiteSpace(endTextBox.Text)) && (int.Parse(endTextBox.Text) <= int.Parse(startTextBox.Text)))
                             {
                                 MessageBox.Show("Sorry, the last page specified cannot be less than the first page specified!", "Invalid Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                 return;
                             }

                             lvi.SubItems.Add(dirTextBox.Text);

                             entriesListView.Items.Add(lvi);
                             ClearFields();
                         }
                         else
                             MessageBox.Show("Sorry, the provided destination path is invalid.", "Missing Destination", MessageBoxButtons.OK, MessageBoxIcon.Error);
                     }
                     else
                         MessageBox.Show("Sorry, you cannot leave the starting page field blank, not even if you're <<B L A N K>>!", "Missing Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("Sorry, the only links that are currently being accepted are from: MangaHere, MangaFox, or Xkcd.", "Invalid Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                MessageBox.Show("Sorry, it looks like the URL that you provided is not valid! This program only accepts absolute links.", "Invalid Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (entriesListView.SelectedIndices.Count == 1)
            {
                if (MessageBox.Show("Are you sure that you wish to remove the selected entry?", "Awaiting Confirmation...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    entriesListView.Items[entriesListView.SelectedIndices[0]].Remove();
                    ClearFields();
                }
            }
        }

        private void modifyButton_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Edit Entry")
            {
                if (entriesListView.SelectedIndices.Count == 1)
                {
                    SelectedIndex = entriesListView.SelectedIndices[0];

                    urlTextBox.Text = entriesListView.Items[SelectedIndex].SubItems[0].Text;
                    startTextBox.Text = entriesListView.Items[SelectedIndex].SubItems[1].Text;
                    endTextBox.Text = entriesListView.Items[SelectedIndex].SubItems[2].Text;
                    dirTextBox.Text = entriesListView.Items[SelectedIndex].SubItems[3].Text;

                    ((Button)sender).Text = "Apply Edit";
                    foreach (Control c in controlsGroupBox.Controls)
                    {
                        foreach (Control cc in c.Controls)
                            if (cc.Name.ToLower().Contains("button") && !cc.Name.Contains("modify"))
                                cc.Enabled = false;
                    }
                }
            }
            else
            {
                if ((0 >= SelectedIndex && SelectedIndex < entriesListView.Items.Count) && (MessageBox.Show("Are you sure that you wish to apply changes to the selected entry?", "Awaiting Confirmation...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
                {
                    if (Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute))
                    {
                        if ((urlTextBox.Text.Contains("mangahere")) || (urlTextBox.Text.Contains("mangafox")) || (urlTextBox.Text.Contains("xkcd") && !urlTextBox.Text.Contains("explain") && !urlTextBox.Text.Contains("wiki")))
                        {
                            if (!String.IsNullOrWhiteSpace(startTextBox.Text))
                            {
                                if (!String.IsNullOrWhiteSpace(dirTextBox.Text) && Path.IsPathRooted(dirTextBox.Text))
                                {
                                    entriesListView.Items[SelectedIndex].SubItems[0].Text = urlTextBox.Text.Trim();
                                    entriesListView.Items[SelectedIndex].SubItems[1].Text = startTextBox.Text.Trim();
                                    entriesListView.Items[SelectedIndex].SubItems[2].Text = endTextBox.Text.Trim();
                                    entriesListView.Items[SelectedIndex].SubItems[3].Text = dirTextBox.Text.Trim();

                                    foreach (Control c in controlsGroupBox.Controls)
                                    {
                                        foreach (Control cc in c.Controls)
                                            cc.Enabled = true;
                                    }
                                }
                                else
                                    MessageBox.Show("Sorry, the provided destination path is invalid.", "Missing Destination", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                                MessageBox.Show("Sorry, you cannot leave the starting page field blank, not even if you're <<B L A N K>>!", "Missing Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                            MessageBox.Show("Sorry, the only links that are currently being accepted are from: MangaHere, MangaFox, or Xkcd.", "Invalid Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                        MessageBox.Show("Sorry, it looks like the URL that you provided is not valid! This program only accepts absolute links.", "Wrong Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (!(0 >= SelectedIndex && SelectedIndex < entriesListView.Items.Count))
                    MessageBox.Show("Sorry, the previously selected entry does not seem to exist anymore!", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Error);

                ClearFields();
                ((Button)sender).Text = "Edit Entry";
            }
            //else
            //    ((Button)sender).Text = "Edit Entry";
        }

        private void numbersOnlyTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void BatcherWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void moveButton_Click(object sender, EventArgs e)
        {
            MoveListViewItems(entriesListView, (((Button)sender).Name == "upButton") ?  MoveDirection.Up : MoveDirection.Down);
        }

        #region Move Items Up or Down in List
        //http://stackoverflow.com/questions/11623893/moving-listviewitems-up-down
        private enum MoveDirection { Up = -1, Down = 1 };

        private static void MoveListViewItems(ListView sender, MoveDirection direction)
        {
            if (sender.SelectedIndices.Count >= 1)
            {
                int dir = (int)direction;

                bool valid = sender.SelectedItems.Count > 0;

                if (valid)
                {
                    //http://stackoverflow.com/questions/14046120/cast-listview-selectedindices-to-listint
                    int[] iIndices = sender.SelectedIndices.Cast<int>().ToArray();
                    int iIndexCount = 0;

                    foreach (ListViewItem item in sender.SelectedItems)
                    {
                        int index = item.Index + dir;
                        if (index >= 0 && index < sender.Items.Count)
                        {
                            sender.Items.RemoveAt(item.Index);
                            sender.Items.Insert(index, item);

                            iIndices[iIndexCount] = index;

                            iIndexCount++;
                        }
                    }

                    sender.SelectedIndices.Clear();

                    foreach (int item in iIndices)
                        sender.SelectedIndices.Add(item);
                }
            }
            else
                MessageBox.Show("Sorry, you can't just like, decide to move nothingness, even if you had The Force.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

        #region Processing Code
        private void ToggleEnable(bool state)
        {
            urlTextBox.Enabled = startTextBox.Enabled = endTextBox.Enabled = dirTextBox.Enabled =
                addButton.Enabled = modifyButton.Enabled = removeButton.Enabled =
                upButton.Enabled = downButton.Enabled = browseButton.Enabled = state;

            listviewCoverPanel.Visible = !state;
            if (state)
                listviewCoverPanel.Dock = DockStyle.None;
            else
                listviewCoverPanel.Dock = DockStyle.Fill;
        }

        private void ClearFields()
        {
            urlTextBox.Clear();
            startTextBox.Clear();
            endTextBox.Clear();
            dirTextBox.Clear();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Start Processing")
            {

                if (entriesListView.Items.Count != 0)
                {
                    ((Button)sender).Text = "Stop Processing";
                    toolStripProgressBar.Value = TotalPages = 0;
                    statusToolStripStatusLabel.Text = "Running...";
                    actionToolStripStatusLabel.Text = "";

                    foreach (ListViewItem lvi in entriesListView.Items)
                    {
                        if (!String.IsNullOrWhiteSpace(lvi.SubItems[2].Text) && Downloading.TryParse(lvi.SubItems[2].Text))
                        {
                            TotalPages += ((int.Parse(lvi.SubItems[2].Text) - int.Parse(lvi.SubItems[1].Text)) + 1);
                        }
                        else
                            TotalPages++;
                    }
                    toolStripProgressBar.Maximum = TotalPages;

                    initialValue = 0;
                    ToggleEnable(false);
                    queueTimer.Start();
                }
                else
                    MessageBox.Show("Sorry, there is nothing queued for processing!", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ((Button)sender).Text = "Start Processing";

                for (int iii = 0; iii < 3; iii++)
                {
                    queueTimer.Stop();
                    batchTimer.Stop();
                }
                initialValue = 0;

                statusToolStripStatusLabel.Text = "Idle";

                BatchStart = BatchEnd = 1;
                ToggleEnable(true);
            }
        }

        private string UrlParser(string input, TextBox oTextBox)//ComboBox inputCBox)
        {
            string otherresult = "";
            string result = Downloading.UrlParser(input, out otherresult);
            oTextBox.Tag = otherresult;
            return result;
        }

        private void PathBrancher(string url, string start, string end, string directory)
        {
            bool NetworkAvailability = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            if (NetworkAvailability)
            {
                string tag = "";
                Processing = true;

                if (!String.IsNullOrWhiteSpace(end) && Downloading.TryParse(end))
                {
                    BatchUrl = url;
                    BatchDest = directory;
                    BatchStart = int.Parse(start);
                    BatchEnd = int.Parse(end);
                    batchTimer.Start();
                }
                else
                {
                    Cursor.Current = Cursors.WaitCursor;

                    //string[] imgExt = new String[] { ".jpg", ".jpeg", ".png" };
                    if (!url.Contains("mangafox") && !url.Contains("mangahere") && !(urlTextBox.Text.Contains("xkcd") || !urlTextBox.Text.Contains("explain") || !urlTextBox.Text.Contains("wiki")))
                    {
                        MessageBox.Show("The program can currently only accept MangaHere, MangaFox or Xkcd links!", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Cursor = Cursors.Default;
                        return;
                    }

                    string tempURL = Downloading.UrlParser(url, out tag) + int.Parse(start) + (Downloading.PrepExt(tag));

                    if (Downloading.AttemptDownload(tempURL, directory, true))
                    {
                        toolStripProgressBar.Value++;
                        pbPercentage(toolStripProgressBar);
                    }

                    actionToolStripStatusLabel.Text = "1 out of 1" + " page processed.";
                }
            }
            else
                MessageBox.Show("You are not connected to the Internet!");
        }

        private void PathBrancher(ListViewItem lvi)
        {
            PathBrancher(lvi.SubItems[0].Text, lvi.SubItems[1].Text, lvi.SubItems[2].Text, lvi.SubItems[3].Text); 
        }

        private void pbPercentage(ToolStripProgressBar pb)
        {
            //http://stackoverflow.com/questions/7708904/adding-text-to-the-tool-strip-progress-bar
            int percent = (int)(((double)(pb.Value - pb.Minimum) /
            (double)(pb.Maximum - pb.Minimum)) * 100);

            using (Graphics gr = pb.ProgressBar.CreateGraphics())
            {
                //Switch to Antialiased drawing for better (smoother) graphic results
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.DrawString(percent.ToString() + "%",
                    SystemFonts.DefaultFont,
                    Brushes.Black,
                    new PointF(pb.Width / 2 - (gr.MeasureString(percent.ToString() + "%",
                        SystemFonts.DefaultFont).Width / 2.0F),
                    pb.Height / 2 - (gr.MeasureString(percent.ToString() + "%",
                        SystemFonts.DefaultFont).Height / 2.0F)));
            }
        }
        #endregion

        private void batchTimer_Tick(object sender, EventArgs e)
        {
            if (BatchStart < BatchEnd + 1)
            {
                string tag = "";
                string tempURL = Downloading.UrlParser(BatchUrl, out tag) + BatchStart + (Downloading.PrepExt(tag));
                statusToolStripStatusLabel.Text = "Running...";

                if (Downloading.AttemptDownload(tempURL, BatchDest, true))
                {
                    toolStripProgressBar.Value++;
                    pbPercentage(toolStripProgressBar);
                }

                actionToolStripStatusLabel.Text = BatchStart + " out of " + BatchEnd + " pages processed.";

                BatchStart++;
            }
            else
            {
                batchTimer.Stop();
                Processing = false;
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            var objOpen = new Ookii.Dialogs.VistaFolderBrowserDialog();

            if (objOpen.ShowDialog() == DialogResult.OK)
                dirTextBox.Text = objOpen.SelectedPath;
        }

        private void queueTimer_Tick(object sender, EventArgs e)
        {
            if (initialValue < entriesListView.Items.Count)
            {
                if (!Processing)
                {
                    entriesListView.SelectedIndices.Clear();
                    entriesListView.SelectedIndices.Add(initialValue);

                    PathBrancher(entriesListView.Items[initialValue]);
                    initialValue++;
                }
            }
            else
            {
                if (toolStripProgressBar.Value == toolStripProgressBar.Maximum)
                {
                    queueTimer.Stop();
                    ToggleEnable(true);

                    entriesListView.Items.Clear();

                    statusToolStripStatusLabel.Text = "Idle";
                    actionToolStripStatusLabel.Text = "Queued tasks completed.";

                    startButton.Text = "Start Processing";
                    this.Cursor = Cursors.Default;
                }
            }
        }

        

        private void BatchWin_Load(object sender, EventArgs e)
        {
            actionToolStripStatusLabel.Text = "";
        }

        private void downloadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

        }
    }
}
