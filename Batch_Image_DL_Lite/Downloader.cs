using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Downloading
{
    internal static class Downloading
    {
        internal static readonly CookieContainer cookiecontainer = new CookieContainer();

        #region Builder code //http://stackoverflow.com/a/24590419/3472690
        internal static HttpWebResponse Builder(string url, string host, NameValueCollection cookies)
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

        internal static string[] Parse(Stream _stream, string encoding)
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
        internal static string PrepValue(int value)
        {
            return PrepValue(value.ToString());
        }

        internal static string PrepValue(string value)
        {
            return (value.Length == 3) ? value : (new String('0', 3 - value.Length) + value);
        }

        internal static string PrepExt(string value)
        {
            return value.Contains(".") ? value : "." + value;
        }
        #endregion

        internal static bool TryParse(string s)
        {
            int result = 0;
            return int.TryParse(s, out result);
        }

        internal static void SaveImage(Image input, string filename, string directory)
        {
            input.Save((Uri.IsWellFormedUriString(directory + filename, UriKind.Absolute)) ? directory + filename : directory + "\\" + filename, DetectFormat(filename));
        }

        internal static ImageFormat DetectFormat(string fileName)
        {
            return ((fileName.Contains(".jpg") || fileName.Contains(".jpeg")) ? ImageFormat.Jpeg : ((fileName.Contains(".png") ? ImageFormat.Png : ((fileName.Contains(".gif") ? ImageFormat.Gif : ImageFormat.Bmp)))));
        }

        internal static List<Uri> FetchLinksFromSource(string htmlSource)
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

        internal static string UrlParser(string input, out string ext)
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
                { ext = part2Url.Substring(part2Url.LastIndexOf('.'), part2Url.LastIndexOf('?')); }
                catch
                {
                    try
                    { ext = part2Url.Substring(part2Url.LastIndexOf('.')); }
                    catch
                    { ext = ".html"; }
                }
            }
            else if (input.Contains("xkcd") && !input.Contains("wiki") && !input.Contains("explain"))
                ext = "";
            else
                ext = part2Url.Substring(part2Url.LastIndexOf('.'));

            //http://stackoverflow.com/a/3732864/3472690
            try
            { part2Url = part2Url.Substring(0, part2Url.IndexOfAny("0123456789".ToCharArray())); }
            catch
            { part2Url = ""; }

            return part1Url + part2Url;
        }

        internal static bool AttemptDownload(string url, string destination, bool save)
        {
            Image iGarbage;
            string sGarbage;
            return AttemptDownload(url, destination, save, out iGarbage, out sGarbage);
        }

        internal static bool AttemptDownload(string url, string destination, bool save, out Image img, out string filename)
        {
            bool KyugenNyugenThisIsReturnedIfAllElseFailsForSomeUnknownReasonArtixKriegerJWittzButHopefullyEverythingWontGoToHellOverItButWhoKnowsPepepepepepepepepepepeLastFailsafe = false;
            NameValueCollection cookies = new NameValueCollection();
            img = null;
            filename = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Credentials = System.Net.CredentialCache.DefaultCredentials;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        List<Uri> links = FetchLinksFromSource(sr.ReadToEnd());

                        if (links.Count == 0)
                        {
                            System.Windows.Forms.MessageBox.Show("Debug Error Code: " + "no links");
                            return false;
                        }
                        else
                        {
                            url = links[0].ToString().Replace("file://", "http://");
                            try
                            {
                                for (int _tries = 0; _tries < 2; _tries++)
                                {
                                    using (response = Builder(url, new Uri(url).Host, cookies))
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
                                                img = Image.FromStream(stream);
                                                if (img.Width > 10 && img.Height > 10)
                                                {
                                                    filename = url.Substring(url.LastIndexOf('/') + 1);
                                                    if (save)
                                                    {
                                                        try
                                                        {
                                                            SaveImage(img, filename, destination);
                                                            return true;
                                                        }
                                                        catch
                                                        {
                                                            System.Windows.Forms.MessageBox.Show("Debug Error Code: " + "1ws2f4swr01");
                                                            return false;
                                                        }
                                                    }
                                                    else
                                                        return true;
                                                    //break;
                                                }
                                                else
                                                {
                                                    cookies = new NameValueCollection();

                                                    using (var newresponse = Builder(url, new Uri(url).Host, cookies))
                                                    {
                                                        using (var newstream = newresponse.GetResponseStream())
                                                        {
                                                            if (contentType.StartsWith("text/html"))
                                                            {
                                                                var parameters = Parse(newstream, newresponse.CharacterSet);
                                                                cookies.Add(parameters[0], parameters[1]);
                                                            }
                                                            if (contentType.StartsWith("image"))
                                                            {
                                                                img = Image.FromStream(newstream);
                                                                if (img.Width > 10 && img.Height > 10)
                                                                {
                                                                    filename = url.Substring(url.LastIndexOf('/') + 1);
                                                                    if (save)
                                                                    {
                                                                        try
                                                                        {
                                                                            SaveImage(img, filename, destination);
                                                                            return true;
                                                                        }
                                                                        catch
                                                                        {
                                                                            System.Windows.Forms.MessageBox.Show("Debug Error Code: " + "2ws2f4swr02");
                                                                            return false;
                                                                        }
                                                                    }
                                                                    else
                                                                        return true;
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
                                System.Windows.Forms.MessageBox.Show("Debug Error Code: " + "3nrrfsrorwg202");
                                return false;
                            }
                        }
                    }
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Debug Error Code: " + "4ftnlnrnrnaa04");
                return false;
            }

            System.Windows.Forms.MessageBox.Show("Debug Error Code: " + "default error");
            return KyugenNyugenThisIsReturnedIfAllElseFailsForSomeUnknownReasonArtixKriegerJWittzButHopefullyEverythingWontGoToHellOverItButWhoKnowsPepepepepepepepepepepeLastFailsafe;
        }
    }

    internal class Downloader
    {

    }
}
