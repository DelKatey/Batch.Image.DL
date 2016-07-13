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
using Ookii.Dialogs;

namespace Batch_Image_DL_Lite
{
    public partial class MainWin : Form
    {
        Image imgStore;
        internal readonly CookieContainer cookiecontainer = new CookieContainer();
        internal static string[] imgExt = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private int initialValue;
        private bool SaveFile = false, Batched = false, PreviewNotDownload = true;
        private string strDirectory = System.Environment.SpecialFolder.MyPictures.ToString();
        private static bool MangaMode = true;
        private int intStartPage = 0, intRanCount = 0;
        //internal static readonly BatcherWin objBatch = new BatcherWin();

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
                    string tempURL = UrlParser(urlTextBox.Text, extComboBox) + int.Parse(range1TextBox.Text) + (PrepExt(extComboBox.Text));

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
                                    return false;
                                else
                                {
                                    tempURL = links[0].ToString().Replace("file://", "http://");

                                    try
                                    {
                                        for (int tries = 0; tries < 2; tries++)
                                        {
                                            using (response = Builder(tempURL, new Uri(tempURL).Host, cookies))
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
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    catch 
                    { 
                        if (xkcdRadioButton.Checked && int.Parse(range1TextBox.Text) == 404)
                            filenameTextBox.Text = "ERROR 404!";
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
                if (!xkcdRadioButton.Checked && (IsEqualBlank(urlTextBox) || IsEqualBlank(extComboBox) || IsEqualBlank(range1TextBox)))
                {
                    MessageBox.Show("The " + ((IsEqualBlank(urlTextBox)) ? "URL " : (IsEqualBlank(extComboBox) ? "extension " : "first page number ")) + "field must be filled!", "Missing Parameters!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else if (xkcdRadioButton.Checked && (IsEqualBlank(urlTextBox)  || IsEqualBlank(range1TextBox)))
                {
                    MessageBox.Show("The " + ((IsEqualBlank(urlTextBox) ? "URL " : "first page number ")) + "field must be filled!", "Missing Parameters!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (!xkcdRadioButton.Checked && (IsEqualBlank(urlTextBox) || IsEqualBlank(extComboBox) || IsEqualBlank(range1TextBox) || IsEqualBlank(range2TextBox)))
                {
                    MessageBox.Show("The " + ((IsEqualBlank(urlTextBox)) ? "URL " : (IsEqualBlank(extComboBox) ? "extension " : (IsEqualBlank(range1TextBox) ? "first page number " : "last page number "))) + "field must be filled!", "Missing Parameters!", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            string part1Url = (!xkcdRadioButton.Checked) ? input.Substring(0, input.LastIndexOf('/') + 1) : "http://xkcd.com/";
            string part2Url = (!xkcdRadioButton.Checked) ? input.Substring(input.LastIndexOf('/') + 1) : String.Empty;

            if (mfRadioButton.Checked)
            { /* For future possible additions */ }
            else if (mhRadioButton.Checked)
            {
                try
                { part2Url = part2Url.Substring(0, part2Url.LastIndexOf('?')); }
                catch { /* Silent Failure Here */ }
            }
            else if (xkcdRadioButton.Checked)
            { /* For future possible additions */ }

            if (mhRadioButton.Checked)
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
            else if (xkcdRadioButton.Checked)
                sExt = "";
            else
                sExt = part2Url.Substring(part2Url.LastIndexOf('.'));

            //http://stackoverflow.com/a/3732864/3472690
            try
            { part2Url = part2Url.Substring(0, part2Url.IndexOfAny("0123456789".ToCharArray())); }
            catch
            { part2Url = "";  }

            return part1Url + part2Url;
        }

        public List<Uri> FetchLinksFromSource(string htmlSource)
        {
            //http://stackoverflow.com/questions/138839/how-do-you-parse-an-html-string-for-image-tags-to-get-at-the-src-information
            List<Uri> links = new List<Uri>();
            string regexImgSrc = "src=[\"'](.+?)[\"'].*?>";
            MatchCollection matchesImgSrc = Regex.Matches(htmlSource, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in matchesImgSrc)
            {
                string href = m.Groups[1].Value;
                foreach (string x in imgExt)
                {
                    if (href.Contains(x))
                    {//http://stackoverflow.com/questions/2912476/using-c-sharp-to-check-if-string-contains-a-string-in-string-array
                        if ((mfRadioButton.Checked && href.Contains("z.mfcdn.net/store")) || (mhRadioButton.Checked && href.Contains("a.mhcdn.net/store")))
                        {
                            if (href.Contains("compressed"))
                            {
                                links.Add(new Uri(href.Substring(0, href.LastIndexOf(x) + x.Length)));
                                break;
                            }
                        }
                        else if (xkcdRadioButton.Checked && href.Contains("comics"))
                        {
                            links.Add(new Uri(href.Substring(0, href.LastIndexOf(x) + x.Length)));
                            break;
                        }
                    }
                }
            }
            return links;
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
            if (initialValue < int.Parse(range2TextBox.Text) + 1)
            {
                string tempURL = UrlParser(urlTextBox.Text, extComboBox) + initialValue + (PrepExt(extComboBox.Text));

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
                                        using (response = Builder(tempURL, new Uri(tempURL).Host, cookies))
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
                            }
                        }
                    }
                }
                catch 
                { 
                    /* Silent Failure */
                    if (xkcdRadioButton.Checked && int.Parse(range1TextBox.Text) == 404)
                        filenameTextBox.Text = "ERROR 404!";
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

        private void mfRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            extComboBox.Text = ".jpg";
        }

        private void mhRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            extComboBox.Text = ".jpg";
        }

        private void xkcdRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            extComboBox.Text = "";
        }

        private void extComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }
    }
}
