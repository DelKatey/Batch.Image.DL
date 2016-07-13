using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Batch_Image_DL_Lite
{
    public partial class BatcherWin : Form
    {
        private static int SelectedIndex = 0;

        public BatcherWin()
        {
            InitializeComponent();
        }

        private void ClearFields()
        {
            urlTextBox.Clear();
            startTextBox.Clear();
            endTextBox.Clear();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute))
            {
                if ((urlTextBox.Text.Contains("mangahere")) || (urlTextBox.Text.Contains("mangafox")) || (urlTextBox.Text.Contains("xkcd") && !urlTextBox.Text.Contains("explain") && !urlTextBox.Text.Contains("wiki")))
                {
                     if (String.IsNullOrWhiteSpace(startTextBox.Text))
                     {
                        ListViewItem lvi = new ListViewItem(urlTextBox.Text.Trim());
                        lvi.SubItems.Add(startTextBox.Text);
                        lvi.SubItems.Add(endTextBox.Text);

                        entriesListView.Items.Add(lvi);
                        ClearFields();
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

                    ((Button)sender).Text = "Apply Edit";
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
                            if (String.IsNullOrWhiteSpace(startTextBox.Text))
                            {
                                entriesListView.Items[SelectedIndex].SubItems[0].Text = urlTextBox.Text.Trim();
                                entriesListView.Items[SelectedIndex].SubItems[1].Text = startTextBox.Text.Trim();
                                if (!String.IsNullOrWhiteSpace(endTextBox.Text))
                                    entriesListView.Items[SelectedIndex].SubItems[2].Text = endTextBox.Text.Trim();

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

        private void startButton(object sender, EventArgs e)
        {
            // Don't push the red button!
        }

        private void upButton(object sender, EventArgs e)
        {
            // Call the elavator for moving up the floors
        }

        private void downButton(object sender, EventArgs e)
        {
            // Call the elevator for heading down
        }

        private void numbersOnlyTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void BatcherWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = true;
            //this.Hide();
        }
    }
}
