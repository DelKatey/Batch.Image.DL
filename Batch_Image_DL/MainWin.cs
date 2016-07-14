using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Downloading;
using Ookii.Dialogs;

namespace Batch_Image_DL
{
    public partial class MainWin : Form
    {
        internal static string[] imgExt = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private int initialValue;
        private bool SaveFile = false, Batched = false, PreviewNotDownload = true;
        internal static string strDirectory = System.Environment.SpecialFolder.MyPictures.ToString();
        private static bool MangaMode = true;
        private int intStartPage = 0;
        internal static readonly BatchWin objBatch = new BatchWin();

        public MainWin()
        {
            InitializeComponent();
        }



        #region Preparation Methods
        internal static string PrepValue(int iValue)
        {
            return PrepValue(iValue.ToString());
        }

        internal static string PrepValue(string sValue)
        {
            return (sValue.Length == 3) ? sValue : (new String('0', 3 - sValue.Length) + sValue);
        }

        internal static string PrepValue(TextBox sTextBox)
        {
            return PrepValue(sTextBox.Text);
        }

        internal static string PrepExt(string sValue)
        {
            return sValue.Contains(".") ? sValue : "." + sValue;
        }

        internal static string PrepExt(TextBox sTextBox)
        {
            return PrepExt(sTextBox.Text);
        }

        internal static string PrepExt(ComboBox sComboBox)
        {
            return PrepExt(sComboBox.Text);
        }
        #endregion

        private void ToggleInterfaces()
        {
            siteGroupBox.Enabled = !siteGroupBox.Enabled;
            previewButton.Enabled = !previewButton.Enabled;
            dlButton.Enabled = !dlButton.Enabled;
            advButton.Enabled = !advButton.Enabled;

            if ((!Batched && PreviewNotDownload) || (!Batched && !PreviewNotDownload))
            {
                previewBatchButton.Enabled = !previewBatchButton.Enabled;
                dlBatchButton.Enabled = !dlBatchButton.Enabled;
                stopBatchPreviewButton.Enabled = false;
                stopDlBatchButton.Enabled = false;
            }
            else if (Batched && PreviewNotDownload)
            {
                dlBatchButton.Enabled = !dlBatchButton.Enabled;
                stopBatchPreviewButton.Enabled = !(previewBatchButton.Enabled = (previewBatchButton.Enabled ? false : true));
            }
            else if (Batched && !PreviewNotDownload)
            {
                previewBatchButton.Enabled = !previewBatchButton.Enabled;
                stopDlBatchButton.Enabled = !(dlBatchButton.Enabled = (dlBatchButton.Enabled ? false : true));
            }
        }

        private bool ParseImageLinks(bool batchCheck)
        {
            bool result = true;

            bool NetworkAvailability = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            if (NetworkAvailability)
            {
                Cursor.Current = Cursors.WaitCursor;
                if (MangaMode)
                {
                    imgExt = new String[] { ".jpg", ".jpeg", ".png" };
                    if (mfRadioButton.Checked && !urlTextBox.Text.Contains("mangafox"))
                    {
                        MessageBox.Show("The program can currently only accept MangaFox links!", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ToggleInterfaces();
                        parametersGroupBox.Enabled = true;
                        this.Cursor = Cursors.Default;
                        return false;
                    }
                    else if (mhRadioButton.Checked && !urlTextBox.Text.Contains("mangahere"))
                    {
                        MessageBox.Show("The program can currently only accept MangaHere links!", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ToggleInterfaces();
                        parametersGroupBox.Enabled = true;
                        this.Cursor = Cursors.Default;
                        return false;
                    }
                    else if (xkcdRadioButton.Checked && (!urlTextBox.Text.Contains("xkcd") || urlTextBox.Text.Contains("explain") || urlTextBox.Text.Contains("wiki")))
                    {
                        MessageBox.Show("The program can currently only accept Xkcd links!", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ToggleInterfaces();
                        parametersGroupBox.Enabled = true;
                        this.Cursor = Cursors.Default;
                        return false;
                    }
                }

                if (!batchCheck)
                {
                    string tempURL = UrlParser(urlTextBox.Text, urlTextBox) + int.Parse(range1TextBox.Text) + (PrepExt(urlTextBox.Tag.ToString()));
                    Image tempImage;
                    string filename;

                    if (Downloading.AttemptDownload(tempURL, strDirectory, SaveFile, out tempImage, out filename))
                    {
                        if (tempImage != null && !String.IsNullOrWhiteSpace(filename))
                        {
                            imgPictureBox.Image = tempImage;
                            filenameTextBox.Text = filename;
                        }
                    }
                    else
                    {
                        if (xkcdRadioButton.Checked && int.Parse(range1TextBox.Text) == 404)
                            filenameTextBox.Text = "ERROR 404!";
                        else
                            MessageBox.Show("Image does not exist!");
                    }
                }
                else
                {
                    intStartPage = initialValue = int.Parse(range1TextBox.Text);
                    pageRangeBtwnLabel.Text = "of";
                    newBatchTimer.Start();
                }

                return result;
            }
            else
            {
                MessageBox.Show("You are not connected to the Internet!");
                return false;
            }
        }

        #region Methods for Laziness
        private bool IsEqualBlank(TextBox sTextBox)
        {
            return String.IsNullOrWhiteSpace(sTextBox.Text);
        }

        private bool IsEqualBlank(ComboBox sComboBox)
        {
            return String.IsNullOrWhiteSpace(sComboBox.Text);
        }
        #endregion

        private bool CheckForBlanks()
        {
            if (!Batched)
            {
                if (!xkcdRadioButton.Checked && (IsEqualBlank(urlTextBox) || IsEqualBlank(range1TextBox)))
                {
                    MessageBox.Show("The " + ((IsEqualBlank(urlTextBox)) ? "URL " : "first page number ") + "field must be filled!", "Missing Parameters!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else if (xkcdRadioButton.Checked && (IsEqualBlank(urlTextBox) || IsEqualBlank(range1TextBox)))
                {
                    MessageBox.Show("The " + ((IsEqualBlank(urlTextBox) ? "URL " : "first page number ")) + "field must be filled!", "Missing Parameters!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (!xkcdRadioButton.Checked && (IsEqualBlank(urlTextBox) || IsEqualBlank(range1TextBox) || IsEqualBlank(range2TextBox)))
                {
                    MessageBox.Show("The " + ((IsEqualBlank(urlTextBox)) ? "URL " : (IsEqualBlank(range1TextBox) ? "first page number " : "last page number ")) + "field must be filled!", "Missing Parameters!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else if (xkcdRadioButton.Checked && (IsEqualBlank(urlTextBox) || IsEqualBlank(range1TextBox) || IsEqualBlank(range2TextBox)))
                {
                    MessageBox.Show("The " + ((IsEqualBlank(urlTextBox) ? "URL " : (IsEqualBlank(range1TextBox) ? "first page number " : "last page number "))) + "field must be filled!", "Missing Parameters!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else
                    return false;
            }
        }

        private bool CheckForInvalidity()
        {
            if (!Batched)
            {
                if (!Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute) || !Downloading.TryParse(range1TextBox.Text))
                {
                    MessageBox.Show("The " + ((!Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute)) ? "URL is invalid!" : "first page number field contains invalid characters! Namely, non-numeric ones."), "Invalid Parameters", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (!Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute) || !Downloading.TryParse(range1TextBox.Text) || !Downloading.TryParse(range2TextBox.Text))
                {
                    MessageBox.Show("The " + ((!Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute)) ? "URL is invalid!" : (!Downloading.TryParse(range1TextBox.Text) ? "first page number" : "last page number") + " field contains invalid characters! Namely, non-numeric ones."), "Invalid Parameters", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else
                    return false;
            }
        }

        private string UrlParser(string input, TextBox oTextBox)//ComboBox inputCBox)
        {
            string otherresult = "";
            string result = Downloading.UrlParser(input, out otherresult);
            oTextBox.Tag = otherresult;
            return result;
        }

        private void previewButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            SaveFile = false;
            Batched = !(PreviewNotDownload = true);

            if (CheckForBlanks() || CheckForInvalidity())
            {
                this.Cursor = Cursors.Default;
                return;
            }

            ToggleInterfaces();
            parametersGroupBox.Enabled = false;
            if (ParseImageLinks(false))
                ToggleInterfaces();
            parametersGroupBox.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        private void previewBatchButton_Click(object sender, EventArgs e)
        {
            SaveFile = false;
            Batched = (PreviewNotDownload = true);

            if (CheckForBlanks() || CheckForInvalidity())
                return;

            ToggleInterfaces();
            parametersGroupBox.Enabled = false;
            previewBatchButton.Enabled = false;
            stopBatchPreviewButton.Enabled = true;
            this.Cursor = Cursors.WaitCursor;
            ParseImageLinks(true);
        }

        public static ImageFormat DetectFormat(string fileName)
        {
            return ((fileName.Contains(".jpg") || fileName.Contains(".jpeg")) ? ImageFormat.Jpeg : ((fileName.Contains(".png") ? ImageFormat.Png : ((fileName.Contains(".gif") ? ImageFormat.Gif : ImageFormat.Bmp)))));
        }

        private ImageFormat DetectFormat(TextBox fTextBox)
        {
            return DetectFormat(fTextBox.Text);
        }

        internal static void SaveImage(Image iInput, string sFilename)
        {
            iInput.Save((Uri.IsWellFormedUriString(strDirectory + sFilename, UriKind.Absolute)) ? strDirectory + sFilename : strDirectory + "\\" + sFilename, DetectFormat(sFilename));
        }

        internal static void SaveImage(Image iInput, string sFilename, string sDirectory)
        {
            iInput.Save((Uri.IsWellFormedUriString(sDirectory + sFilename, UriKind.Absolute)) ? sDirectory + sFilename : sDirectory + "\\" + sFilename, DetectFormat(sFilename));
        }

        private void SaveImage(Image iInput, TextBox sTextbox)
        {
            SaveImage(iInput, sTextbox.Text);
        }

        private void stopBatchPreviewButton_Click(object sender, EventArgs e)
        {
            stopBatchPreviewButton.Enabled = false;
            for (int iii = 0; iii < 3; iii++)
                newBatchTimer.Stop();
            range1TextBox.Text = intStartPage.ToString();
            pageRangeBtwnLabel.Text = "to";
            parametersGroupBox.Enabled = true;
            ToggleInterfaces();
            this.Cursor = Cursors.Default;
            MessageBox.Show("Batch preview cancelled!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void dlButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            SaveFile = true;
            Batched = (PreviewNotDownload = false);

            if (CheckForBlanks() || CheckForInvalidity())
            {
                this.Cursor = Cursors.Default;
                return;
            }

            ToggleInterfaces();
            parametersGroupBox.Enabled = false;
            using (VistaFolderBrowserDialog objOpen = new Ookii.Dialogs.VistaFolderBrowserDialog())
            {
                if (objOpen.ShowDialog() == DialogResult.OK)
                    strDirectory = objOpen.SelectedPath;
                else
                {
                    parametersGroupBox.Enabled = true;
                    ToggleInterfaces();
                    return;
                }
            }

            if (ParseImageLinks(false))
                ToggleInterfaces();

            parametersGroupBox.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        private void dlBatchButton_Click(object sender, EventArgs e)
        {
            SaveFile = true;
            Batched = !(PreviewNotDownload = false);

            if (CheckForBlanks() || CheckForInvalidity())
                return;

            ToggleInterfaces();
            parametersGroupBox.Enabled = false;
            using (VistaFolderBrowserDialog objOpen = new Ookii.Dialogs.VistaFolderBrowserDialog())
            {
                if (objOpen.ShowDialog() == DialogResult.OK)
                    strDirectory = objOpen.SelectedPath;
                else
                {
                    parametersGroupBox.Enabled = true;
                    ToggleInterfaces();
                    return;
                }
            }
            dlBatchButton.Enabled = false;
            stopDlBatchButton.Enabled = true;
            this.Cursor = Cursors.WaitCursor;
            ParseImageLinks(true);
        }

        private void stopDlBatchButton_Click(object sender, EventArgs e)
        {
            SaveFile = false;
            stopDlBatchButton.Enabled = false;
            for (int iii = 0; iii < 3; iii++)
                newBatchTimer.Stop();
            pageRangeBtwnLabel.Text = "to";
            range1TextBox.Text = intStartPage.ToString();
            parametersGroupBox.Enabled = true;
            ToggleInterfaces();
            this.Cursor = Cursors.Default;
            MessageBox.Show("Batch download cancelled!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void speedTrackBar_Scroll(object sender, EventArgs e)
        {
            int intStartingValue = 10;


            intStartingValue = (speedTrackBar.Value == 0) ? 10 : (speedTrackBar.Value == 1) ? 100 : ((intStartingValue + (speedTrackBar.Value * 200) < 1050) ? intStartingValue + (speedTrackBar.Value * 200) - 110 : intStartingValue + (speedTrackBar.Value * 200) - 210);

            //http://stackoverflow.com/questions/892369/how-can-i-display-a-tooltip-showing-the-value-of-a-trackbar-in-winforms
            infoToolTip.SetToolTip(speedTrackBar, intStartingValue + "ms");

            newBatchTimer.Interval = intStartingValue;
        }

        private void numbersOnlyTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //http://stackoverflow.com/a/463335
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            new AboutWin().ShowDialog();
        }

        private void newBatchTimer_Tick(object sender, EventArgs e)
        {
            if (initialValue <= int.Parse(range2TextBox.Text))
            {
                string tempURL = UrlParser(urlTextBox.Text, urlTextBox) + initialValue + (PrepExt(urlTextBox.Tag.ToString()));

                Image tempImage;
                string filename;

                if (Downloading.AttemptDownload(tempURL, strDirectory, SaveFile, out tempImage, out filename))
                {
                    if (tempImage != null && !String.IsNullOrWhiteSpace(filename))
                    {
                        imgPictureBox.Image = tempImage;
                        filenameTextBox.Text = filename;
                    }
                }
                else
                {
                    if (xkcdRadioButton.Checked && int.Parse(range1TextBox.Text) == 404)
                        filenameTextBox.Text = "ERROR 404!";
                    else
                        MessageBox.Show("Image does not exist!");
                }

                //Cursor.Current = Cursors.Default;
                range1TextBox.Text = initialValue.ToString();
                initialValue++;
            }
            else
            {
                newBatchTimer.Stop();
                pageRangeBtwnLabel.Text = "to";
                range1TextBox.Text = intStartPage.ToString();
                parametersGroupBox.Enabled = true;
                ToggleInterfaces();
                this.Cursor = Cursors.Default;
                MessageBox.Show("Process completed!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void extComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void advButton_Click(object sender, EventArgs e)
        {
            objBatch.ShowDialog();
        }

        private void mfRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            urlTextBox.Tag = ".jpg";
        }

        private void mhRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            urlTextBox.Tag = ".jpg";
        }

        private void xkcdRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            urlTextBox.Text = "";
        }
    }
}
