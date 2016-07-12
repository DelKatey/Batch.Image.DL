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
using Ookii.Dialogs;

namespace Batch_Image_DL_Lite
{
    public partial class MainWin : Form
    {
        Image imgStore;
        readonly CookieContainer cookiecontainer = new CookieContainer();
        private ImageFormat imgF = ImageFormat.Jpeg;
        private int initialValue;
        private bool SaveFile = false, Batched = false, PreviewNotDownload = true;
        private string strDirectory = System.Environment.SpecialFolder.MyPictures.ToString();

        public MainWin()
        {
            InitializeComponent();
        }

        #region Builder code //http://stackoverflow.com/a/24590419/3472690
        private HttpWebResponse Builder(string url, string host, NameValueCollection cookies)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8");
            request.Headers.Set(HttpRequestHeader.CacheControl, "max-age=0");

            request.Host = host;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.146 Safari/537.36";

            request.CookieContainer = cookiecontainer;

            if (cookies != null)
            {
                foreach (var cookiekey in cookies.AllKeys)
                {
                    request.CookieContainer.Add(
                        new Cookie(
                            cookiekey,
                            cookies[cookiekey],
                            @"/",
                            host));
                }
            }
            return (HttpWebResponse)request.GetResponse();
        }

        private static string[] Parse(Stream _stream, string encoding)
        {
            const string setCookieCall = "setCookie('";

            // copy html as string
            using (var ms = new MemoryStream())
            {
                _stream.CopyTo(ms);
                var html = Encoding.GetEncoding(encoding).GetString(ms.ToArray());

                // find setCookie call
                var findFirst = html.IndexOf(
                    setCookieCall,
                    StringComparison.InvariantCultureIgnoreCase) + setCookieCall.Length;
                var last = html.IndexOf(");", findFirst, StringComparison.InvariantCulture);

                var setCookieStatmentCall = html.Substring(findFirst, last - findFirst);
                // take the parameters
                var parameters = setCookieStatmentCall.Split(new[] { ',' });
                for (int x = 0; x < parameters.Length; x++)
                {
                    // cleanup
                    parameters[x] = parameters[x].Replace("'", "").Trim();
                }
                return parameters;
            }
        }
        #endregion

        #region Preparation Methods
        private string PrepValue(int iValue)
        {
            return PrepValue(iValue.ToString());
        }

        private string PrepValue(string sValue)
        {
            return (sValue.Length == 3) ? sValue : (new String('0', 3 - sValue.Length) + sValue);
        }

        private string PrepValue(TextBox sTextBox)
        {
            return PrepValue(sTextBox.Text);
        }

        private string PrepExt(string sValue)
        {
            return sValue.Contains(".") ? sValue : "." + sValue;
        }

        private string PrepExt(TextBox sTextBox)
        {
            return PrepExt(sTextBox.Text);
        }

        private string PrepExt(ComboBox sComboBox)
        {
            return PrepExt(sComboBox.Text);
        }
        #endregion

        private void ToggleInterfaces()
        {
            siteGroupBox.Enabled = !siteGroupBox.Enabled;
            previewButton.Enabled = !previewButton.Enabled;
            dlButton.Enabled = !dlButton.Enabled;

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

        private void ProcessImages(bool batchCheck)
        {
            bool NetworkAvailability = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            if (NetworkAvailability)
            {
                Cursor.Current = Cursors.WaitCursor;

                if (!batchCheck)
                {
                    string tempURL = UrlParser(urlTextBox.Text, extComboBox) + (PrepValue(range1TextBox.Text)) + (PrepExt(extComboBox.Text));
                    
                    var cookies = new NameValueCollection();

                    try
                    {
                        for (int tries = 0; tries < 2; tries++)
                        {
                            using (var response = Builder(tempURL, new Uri(tempURL).Host, cookies))
                            {
                                using (var stream = response.GetResponseStream())
                                {
                                    string contentType = response.ContentType.ToLowerInvariant();
                                    if (contentType.StartsWith("text/html"))
                                    {
                                        var parameters = Parse(stream, response.CharacterSet);
                                        cookies.Add(parameters[0], parameters[1]);
                                    }
                                    if (contentType.StartsWith("image"))
                                    {
                                        imgStore = Image.FromStream(stream);
                                        if (imgStore.Width > 10 && imgStore.Height > 10)
                                        {
                                            imgPictureBox.Image = imgStore;
                                            filenameTextBox.Text = tempURL.Substring(tempURL.LastIndexOf('/') + 1);
                                            if (SaveFile)
                                            {
                                                try
                                                { SaveImage(imgStore, filenameTextBox); }
                                                catch
                                                { }
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            tempURL = UrlParser(urlTextBox.Text, extComboBox) + ((PrepValue(range1TextBox.Text)) + "_" + (PrepValue(int.Parse(range1TextBox.Text) + 1))) + (PrepExt(extComboBox.Text));
                                            cookies = new NameValueCollection();

                                            using (var response2 = Builder(tempURL, new Uri(tempURL).Host, cookies))
                                            {
                                                using (var stream2 = response2.GetResponseStream())
                                                {
                                                    if (contentType.StartsWith("text/html"))
                                                    {
                                                        var parameters = Parse(stream2, response2.CharacterSet);
                                                        cookies.Add(parameters[0], parameters[1]);
                                                    }
                                                    if (contentType.StartsWith("image"))
                                                    {
                                                        imgStore = Image.FromStream(stream2);
                                                        if (imgStore.Width > 10 && imgStore.Height > 10)
                                                        {
                                                            imgPictureBox.Image = imgStore;
                                                            filenameTextBox.Text = tempURL.Substring(tempURL.LastIndexOf('/') + 1);
                                                            if (SaveFile)
                                                            {
                                                                try
                                                                { SaveImage(imgStore, filenameTextBox); }
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
                        MessageBox.Show("Image does not exist!");
                    }
                    Cursor.Current = Cursors.Default;

                }
                else
                {
                    initialValue = int.Parse(range1TextBox.Text);
                    batchTimer.Start();
                }
            }
            else
                MessageBox.Show("You are not connected to the Internet!");
        }

        private bool IsEqualBlank(TextBox sTextBox)
        {
            return String.IsNullOrWhiteSpace(sTextBox.Text);
        }

        private bool IsEqualBlank(ComboBox sComboBox)
        {
            return String.IsNullOrWhiteSpace(sComboBox.Text);
        }

        private bool CheckForBlanks()
        {
            if (!Batched)
            {
                if (IsEqualBlank(urlTextBox) || IsEqualBlank(extComboBox) || IsEqualBlank(range1TextBox))
                {
                    MessageBox.Show("The " + ((IsEqualBlank(urlTextBox)) ? "URL " : (IsEqualBlank(extComboBox) ? "extension " : "first page number ")) + "field must be filled!", "Missing Parameters!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (IsEqualBlank(urlTextBox) || IsEqualBlank(extComboBox) || IsEqualBlank(range1TextBox) || IsEqualBlank(range2TextBox))
                {
                    MessageBox.Show("The " + ((IsEqualBlank(urlTextBox)) ? "URL " : (IsEqualBlank(extComboBox) ? "extension " : (IsEqualBlank(range1TextBox) ? "first page number " : "last page number "))) + "field must be filled!", "Missing Parameters!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else
                    return false;
            }
        }

        internal static bool TryParse(string s)
        {
            int result = 0;
            return int.TryParse(s, out result); 
        }

        private bool CheckForInvalidity()
        {
            if (!Batched)
            {
                if (!Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute) || !TryParse(range1TextBox.Text))
                {
                    MessageBox.Show("The " + ((!Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute)) ? "URL is invalid!" : "first page number field contains invalid characters! Namely, non-numeric ones."), "Invalid Parameters", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (!Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute) || !TryParse(range1TextBox.Text) || !TryParse(range2TextBox.Text))
                {
                    MessageBox.Show("The " + ((!Uri.IsWellFormedUriString(urlTextBox.Text, UriKind.Absolute)) ? "URL is invalid!" : (!TryParse(range1TextBox.Text) ? "first page number" : "last page number") + " field contains invalid characters! Namely, non-numeric ones."), "Invalid Parameters", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else
                    return false;
            }
        }

        private string UrlParser(string input, out string sExt)
        {
            string part1Url = input.Substring(0, input.LastIndexOf('/') + 1);
            string part2Url = input.Substring(input.LastIndexOf('/') + 1);

            if (mfRadioButton.Checked)
            { /* For future possible additions */ }
            else if (mhRadioButton.Checked)
                part2Url = part2Url.Substring(0, part2Url.LastIndexOf('?'));

            sExt = part2Url.Substring(part2Url.LastIndexOf('.'));
            //http://stackoverflow.com/a/3732864/3472690
            part2Url = part2Url.Substring(0, part2Url.IndexOfAny("0123456789".ToCharArray()));

            return part1Url + part2Url;
        }

        private string UrlParser(string input, ComboBox inputCBox)
        {
            string otherresult = "";
            string result = UrlParser(input, out otherresult);
            inputCBox.Text = otherresult;
            return result;
        }

        private void previewButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            SaveFile = false;
            Batched = !(PreviewNotDownload = true);

            if (CheckForBlanks() || CheckForInvalidity())
                return;

            ToggleInterfaces();
            parametersGroupBox.Enabled = false;
            ProcessImages(false);
            parametersGroupBox.Enabled = true;
            ToggleInterfaces();
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
            ProcessImages(true);
        }

        private ImageFormat DetectFormat(string fileName)
        {
            return ((fileName.Contains(".jpg") || fileName.Contains(".jpeg")) ? ImageFormat.Jpeg : ((fileName.Contains(".png") ? ImageFormat.Png : ((fileName.Contains(".gif") ? ImageFormat.Gif : ImageFormat.Bmp)))));
        }

        private ImageFormat DetectFormat(TextBox fTextBox)
        {
            return DetectFormat(fTextBox.Text);
        }

        private void SaveImage(Image iInput, string sFilename)
        {
            iInput.Save((Uri.IsWellFormedUriString(strDirectory + sFilename, UriKind.Absolute)) ? strDirectory + sFilename : strDirectory + "\\" + sFilename, DetectFormat(sFilename));
        }

        private void SaveImage(Image iInput, TextBox sTextbox)
        {
            SaveImage(iInput, sTextbox.Text);
        }

        private void batchTimer_Tick(object sender, EventArgs e)
        {
            if (initialValue < int.Parse(range2TextBox.Text) + 1)
            {
                string tempURL = UrlParser(urlTextBox.Text, extComboBox) + (PrepValue(initialValue)) + (PrepExt(extComboBox.Text));

                var cookies = new NameValueCollection();

                try
                {
                    for (int tries = 0; tries < 2; tries++)
                    {
                        using (var response = Builder(tempURL, new Uri(tempURL).Host, cookies))
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                string contentType = response.ContentType.ToLowerInvariant();
                                if (contentType.StartsWith("text/html"))
                                {
                                    var parameters = Parse(stream, response.CharacterSet);
                                    cookies.Add(parameters[0], parameters[1]);
                                }
                                if (contentType.StartsWith("image"))
                                {
                                    imgStore = Image.FromStream(stream);
                                    if (imgStore.Width > 10 && imgStore.Height > 10)
                                    {
                                        imgPictureBox.Image = imgStore;
                                        filenameTextBox.Text = tempURL.Substring(tempURL.LastIndexOf('/') + 1);
                                        if (SaveFile)
                                        {
                                            try
                                            { SaveImage(imgStore, filenameTextBox); }
                                            catch
                                            { }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        tempURL = UrlParser(urlTextBox.Text, extComboBox) + (PrepValue(initialValue)) + "_" + (PrepValue(initialValue + 1)) + PrepExt(extComboBox);
                                        cookies = new NameValueCollection();

                                        using (var response2 = Builder(tempURL, new Uri(tempURL).Host, cookies))
                                        {
                                            using (var stream2 = response2.GetResponseStream())
                                            {
                                                if (contentType.StartsWith("text/html"))
                                                {
                                                    var parameters = Parse(stream2, response2.CharacterSet);
                                                    cookies.Add(parameters[0], parameters[1]);
                                                }
                                                if (contentType.StartsWith("image"))
                                                {
                                                    imgStore = Image.FromStream(stream2);
                                                    if (imgStore.Width > 10 && imgStore.Height > 10)
                                                    {
                                                        imgPictureBox.Image = imgStore;
                                                        filenameTextBox.Text = tempURL.Substring(tempURL.LastIndexOf('/') + 1);
                                                        if (SaveFile)
                                                        {
                                                            try
                                                            { SaveImage(imgStore, filenameTextBox); }
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
                { /*Silent fail for in batch processes*/ }

                Cursor.Current = Cursors.Default;
                initialValue++;
            }
            else
            {
                batchTimer.Stop();
                parametersGroupBox.Enabled = true;
                ToggleInterfaces();
                this.Cursor = Cursors.Default;
                MessageBox.Show("Process completed!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void stopBatchPreviewButton_Click(object sender, EventArgs e)
        {
            stopBatchPreviewButton.Enabled = false;
            for (int iii = 0; iii < 3; iii++)
                batchTimer.Stop();
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
            ProcessImages(false);
            parametersGroupBox.Enabled = true;
            ToggleInterfaces();
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
            ProcessImages(true);
        }

        private void stopDlBatchButton_Click(object sender, EventArgs e)
        {
            SaveFile = false;
            stopDlBatchButton.Enabled = false;
            for (int iii = 0; iii < 3; iii++)
                batchTimer.Stop();
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

            batchTimer.Interval = intStartingValue;
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
    }
}
