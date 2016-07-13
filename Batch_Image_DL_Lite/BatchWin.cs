using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Batch_Image_DL_Lite
{
    public partial class BatchWin : Form
    {
        private static int SelectedIndex = 0, initialValue = 0, BatchStart = 1, BatchEnd = 1, TotalPages = 0;
        private static string BatchUrl = "", BatchDest = "";
        private bool Processing = false;

        private bool blnDebug = false;

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
                        if (c.Name != "modifyButton")
                            c.Enabled = false;
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
                                if (!String.IsNullOrWhiteSpace(dirTextBox.Text) && Uri.IsWellFormedUriString(dirTextBox.Text, UriKind.Absolute))
                                {
                                    entriesListView.Items[SelectedIndex].SubItems[0].Text = urlTextBox.Text.Trim();
                                    entriesListView.Items[SelectedIndex].SubItems[1].Text = startTextBox.Text.Trim();
                                    entriesListView.Items[SelectedIndex].SubItems[2].Text = endTextBox.Text.Trim();
                                    entriesListView.Items[SelectedIndex].SubItems[3].Text = dirTextBox.Text.Trim();
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
                foreach (Control c in controlsGroupBox.Controls)
                {
                    c.Enabled = true;
                }
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
                upButton.Enabled = downButton.Enabled = state;

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
                        if (!String.IsNullOrWhiteSpace(lvi.SubItems[2].Text) && MainWin.TryParse(lvi.SubItems[2].Text))
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

        internal string UrlParser(string input, out string sExt)
        {

            string part1Url = (!(input.Contains("xkcd") && !input.Contains("wiki") && !input.Contains("explain"))) ? input.Substring(0, input.LastIndexOf('/') + 1) : "http://xkcd.com/";
            string part2Url = (!(input.Contains("xkcd") && !input.Contains("wiki") && !input.Contains("explain"))) ? input.Substring(input.LastIndexOf('/') + 1) : String.Empty;

            if (input.Contains("mangafox"))
            { /* For future possible additions */ }
            else if (input.Contains("mangahere"))
            {
                try
                { part2Url = part2Url.Substring(0, part2Url.LastIndexOf('?')); }
                catch { /* Silent Failure Here */ }
            }
            else if (input.Contains("xkcd") && !input.Contains("wiki") && !input.Contains("explain"))
            { /* For future possible additions */ }

            if (input.Contains("mangahere"))
            {
                try
                { sExt = part2Url.Substring(part2Url.LastIndexOf('.'), part2Url.LastIndexOf('?')); }
                catch
                {
                    try
                    { sExt = part2Url.Substring(part2Url.LastIndexOf('.')); }
                    catch
                    { sExt = ".html"; }
                }
            }
            else if (input.Contains("xkcd") && !input.Contains("wiki") && !input.Contains("explain"))
                sExt = "";
            else
                sExt = part2Url.Substring(part2Url.LastIndexOf('.'));

            //http://stackoverflow.com/a/3732864/3472690
            try
            { part2Url = part2Url.Substring(0, part2Url.IndexOfAny("0123456789".ToCharArray())); }
            catch
            { part2Url = ""; }

            return part1Url + part2Url;
        }

        private string UrlParser(string input, TextBox oTextBox)//ComboBox inputCBox)
        {
            string otherresult = "";
            string result = UrlParser(input, out otherresult);
            oTextBox.Tag = otherresult;
            return result;
        }

        private List<Uri> FetchLinksFromSource(string htmlSource)
        {
            //http://stackoverflow.com/questions/138839/how-do-you-parse-an-html-string-for-image-tags-to-get-at-the-src-information
            List<Uri> links = new List<Uri>();
            string regexImgSrc = "src=[\"'](.+?)[\"'].*?>";
            MatchCollection matchesImgSrc = Regex.Matches(htmlSource, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in matchesImgSrc)
            {
                string href = m.Groups[1].Value;
                foreach (string x in new string[] { ".jpg", ".jpeg", ".png" })
                {
                    if (href.Contains(x))
                    {//http://stackoverflow.com/questions/2912476/using-c-sharp-to-check-if-string-contains-a-string-in-string-array
                        if ((href.Contains("z.mfcdn.net/store")) || (href.Contains("a.mhcdn.net/store")))
                        {
                            if (href.Contains("compressed"))
                            {
                                links.Add(new Uri(href.Substring(0, href.LastIndexOf(x) + x.Length)));
                                break;
                            }
                        }
                        else if (href.Contains("xkcd") && href.Contains("comics"))
                        {
                            links.Add(new Uri(href.Substring(0, href.LastIndexOf(x) + x.Length)));
                            break;
                        }
                    }
                }
            }
            return links;
        }

        private void PathBrancher(string url, string start, string end, string directory)
        {
            bool NetworkAvailability = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            if (NetworkAvailability)
            {
                string tag = "";
                Processing = true;

                if (!String.IsNullOrWhiteSpace(end) && MainWin.TryParse(end))
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

                    string tempURL = UrlParser(url, out tag) + int.Parse(start) + (MainWin.PrepExt(tag));

                    var cookies = new NameValueCollection();

                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tempURL);
                        request.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                            {
                                List<Uri> links = FetchLinksFromSource(sr.ReadToEnd());

                                if (links.Count == 0)
                                    return;
                                else
                                {
                                    tempURL = links[0].ToString().Replace("file://", "http://");

                                    try
                                    {
                                        for (int tries = 0; tries < 2; tries++)
                                        {
                                            using (response = MainWin.Builder(tempURL, new Uri(tempURL).Host, cookies))
                                            {
                                                using (var stream = response.GetResponseStream())
                                                {
                                                    string contentType = response.ContentType.ToLowerInvariant();
                                                    if (contentType.StartsWith("text/html"))
                                                    {
                                                        var parameters = MainWin.Parse(stream, response.CharacterSet);
                                                        cookies.Add(parameters[0], parameters[1]);
                                                    }
                                                    if (contentType.StartsWith("image"))
                                                    {
                                                        MainWin.imgStore = Image.FromStream(stream);
                                                        if (MainWin.imgStore.Width > 10 && MainWin.imgStore.Height > 10)
                                                        {
                                                            string strFilename = tempURL.Substring(tempURL.LastIndexOf('/') + 1);
                                                            if (!blnDebug)
                                                            {
                                                                try
                                                                { 
                                                                    MainWin.SaveImage(MainWin.imgStore, strFilename, directory);

                                                                    try
                                                                    { toolStripProgressBar.Value++; }
                                                                    catch { /* Silent Failure */ }

                                                                    Processing = false;
                                                                }
                                                                catch
                                                                { }
                                                            }
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            cookies = new NameValueCollection();

                                                            using (var response2 = MainWin.Builder(tempURL, new Uri(tempURL).Host, cookies))
                                                            {
                                                                using (var stream2 = response2.GetResponseStream())
                                                                {
                                                                    if (contentType.StartsWith("text/html"))
                                                                    {
                                                                        var parameters = MainWin.Parse(stream2, response2.CharacterSet);
                                                                        cookies.Add(parameters[0], parameters[1]);
                                                                    }
                                                                    if (contentType.StartsWith("image"))
                                                                    {
                                                                        MainWin.imgStore = Image.FromStream(stream2);
                                                                        if (MainWin.imgStore.Width > 10 && MainWin.imgStore.Height > 10)
                                                                        {
                                                                            string strFilename = tempURL.Substring(tempURL.LastIndexOf('/') + 1);

                                                                            if (!blnDebug)
                                                                            {
                                                                                try
                                                                                { 
                                                                                    MainWin.SaveImage(MainWin.imgStore, strFilename, directory);

                                                                                    try
                                                                                    { toolStripProgressBar.Value++; }
                                                                                    catch { /* Silent Failure */ }

                                                                                    Processing = false;
                                                                                }
                                                                                catch
                                                                                { }
                                                                            }
                                                                        }
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    { 
                                        /* Silent Failure */
                                        Processing = false;
                                    }
                                }
                            }
                        }

                        actionToolStripStatusLabel.Text = "1 out of 1" + " page processed.";
                    }
                    catch 
                    { 
                        /* Silent Failure */
                        Processing = false;
                    }
                }
            }
            else
                MessageBox.Show("You are not connected to the Internet!");
        }

        private void PathBrancher(ListViewItem lvi)
        {
            PathBrancher(lvi.SubItems[0].Text, lvi.SubItems[1].Text, lvi.SubItems[2].Text, lvi.SubItems[3].Text); 
        }
        #endregion

        private void batchTimer_Tick(object sender, EventArgs e)
        {
            if (BatchStart < BatchEnd + 1)
            {
                string tag = "";
                string tempURL = UrlParser(BatchUrl, out tag) + BatchStart + (MainWin.PrepExt(tag));
                statusToolStripStatusLabel.Text = "Running...";

                var cookies = new NameValueCollection();

                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tempURL);
                    request.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            List<Uri> links = FetchLinksFromSource(sr.ReadToEnd());

                            if (links.Count == 0)
                                return;
                            else
                            {
                                tempURL = links[0].ToString().Replace("file://", "http://");

                                try
                                {
                                    for (int tries = 0; tries < 2; tries++)
                                    {
                                        using (response = MainWin.Builder(tempURL, new Uri(tempURL).Host, cookies))
                                        {
                                            using (var stream = response.GetResponseStream())
                                            {
                                                string contentType = response.ContentType.ToLowerInvariant();
                                                if (contentType.StartsWith("text/html"))
                                                {
                                                    var parameters = MainWin.Parse(stream, response.CharacterSet);
                                                    cookies.Add(parameters[0], parameters[1]);
                                                }
                                                if (contentType.StartsWith("image"))
                                                {
                                                    MainWin.imgStore = Image.FromStream(stream);
                                                    if (MainWin.imgStore.Width > 10 && MainWin.imgStore.Height > 10)
                                                    {
                                                        string strFilename = tempURL.Substring(tempURL.LastIndexOf('/') + 1);
                                                        if (!blnDebug)
                                                        {
                                                            try
                                                            { 
                                                                MainWin.SaveImage(MainWin.imgStore, strFilename, BatchDest);
                                                                try
                                                                { toolStripProgressBar.Value++; }
                                                                catch { /* Silent Failure */ }
                                                            }
                                                            catch
                                                            { }
                                                        }
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        cookies = new NameValueCollection();

                                                        using (var response2 = MainWin.Builder(tempURL, new Uri(tempURL).Host, cookies))
                                                        {
                                                            using (var stream2 = response2.GetResponseStream())
                                                            {
                                                                if (contentType.StartsWith("text/html"))
                                                                {
                                                                    var parameters = MainWin.Parse(stream2, response2.CharacterSet);
                                                                    cookies.Add(parameters[0], parameters[1]);
                                                                }
                                                                if (contentType.StartsWith("image"))
                                                                {
                                                                    MainWin.imgStore = Image.FromStream(stream2);
                                                                    if (MainWin.imgStore.Width > 10 && MainWin.imgStore.Height > 10)
                                                                    {
                                                                        string strFilename = tempURL.Substring(tempURL.LastIndexOf('/') + 1);
                                                                        if (!blnDebug)
                                                                        {
                                                                            try
                                                                            { 
                                                                                MainWin.SaveImage(MainWin.imgStore, strFilename, BatchDest);
                                                                                try
                                                                                { toolStripProgressBar.Value++; }
                                                                                catch { /* Silent Failure */ }
                                                                            }
                                                                            catch
                                                                            { }
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch
                                { MessageBox.Show("Image does not exist!"); }
                            }
                        }
                    }
                }
                catch
                { /* Silent Failure */ }

                //Cursor.Current = Cursors.Default;

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
    }
}
