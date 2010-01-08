using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Sonar
{
    public class SocialItem
    {
        public string User;
        public Image Image;
        public string Message;
        public string Artist;
        public string Track;
        public string Service;
        public DateTime PostTime;
        public string Url; // playable url of track
        public string Key 
        { 
            get 
            { 
                // last.fm hack:
                if (string.IsNullOrEmpty(Message))
                    Message = Track + " by " + Artist;
                return User + ": " + Message; 
            } 
        }
        public Resolver.Result Source;

        //public override string ToString() { return User + ": " + Track + " by " + Artist; }
        public override string ToString() { return Key; }

        public ListViewItem ToListItem()
        {
            if (string.IsNullOrEmpty(Message)) // Assuming last.fm for now
                Message = Track + " by " + Artist;

            ListViewItem i = new ListViewItem(Key);
            i.ToolTipText = Message + " [" + PostTime.ToString() + "]";
            i.Tag = this;

            // TODO: Figure out if this is actually playable, instead of just parseable.
            if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Track))
                i.ForeColor = Color.DodgerBlue;  

            // TODO: Deal with image (need an index, which we can't get yet);

            return i;

        }
        public Image GetImage(string url)
        {
            WebClient wc = new WebClient();
            byte[] data = wc.DownloadData(url);
            MemoryStream ms = new MemoryStream(data);
            return new Bitmap(ms);
        }

        /// <summary>
        ///  Only works on Sonos Twitter Autofill posts. Don't go getting delusions.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>true if it was able to fill them</returns>
        public bool try_populate_artist_track(string input)
        {            
            // Possible strings: 
            // 1. "Enjoying %s by %s all over the house on my #Sonos"
            // 2. "Listening to %s by %s"
            // "Love hearing %s playing on my #Sonos" (Artist)
            // "%s is playing all over my house" (Artist)
            
            Regex r1 = new Regex(@"Enjoying (?<title>.*) by (?<artist>.*) all over the house on my #Sonos", RegexOptions.Compiled);           
            Match m = r1.Match(input);
            if (m.Success)
            {                
                Artist = m.Groups["artist"].Value;
                Track = m.Groups["title"].Value;
                return true;
            }

            Regex r2 = new Regex(@"Listening to (?<title>.*) by (?<artist>.*)", RegexOptions.Compiled);
            m = r2.Match(input);
            if (m.Success)
            {
                Artist = m.Groups["artist"].Value;
                Track = m.Groups["title"].Value;
                return true;
            }

            return false;
        }
        /// <summary>
        ///  Populate artist and track using a specified regular expression
        /// </summary>
        /// <param name="input"></param>
        /// <param name="regex"></param>
        /// <returns>true if it was able to fill them</returns>
        public bool try_populate_artist_track(string input, string regex)
        {

            Regex r = new Regex(regex, RegexOptions.Compiled);
            Match m = r.Match(input);
            if (m.Success)
            {
                Artist = m.Groups["artist"].Value;
                Track = m.Groups["title"].Value;
                return true;
            }
            return false;
        }
    }

    public class DisplayInfo
    {
        public DisplayInfo(Image i, Color c)
        {
            Color = c;
            Logo = i;
        }
        public Color Color { get; private set; }
        public Image Logo { get; private set; }
    }

    public static class DisplayInfoFactory
    {
        static Image GetResource(string name)
        {
            Image i = null;
            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                // string[] names = a.GetManifestResourceNames(); // for debugging
                Stream s = a.GetManifestResourceStream("Sonar.images." + name);
                i = Image.FromStream(s);
            }
            catch { };
            return i;
        }

        static Image _twitterLogo;
        static Image _lastfmLogo;

        static DisplayInfoFactory()
        {
            _twitterLogo = GetResource("twitter-logo-large.png");
            _lastfmLogo = GetResource("lastfm-logo-small.png");
        }

        public static DisplayInfo Get(SocialItem i)
        {
            switch (i.Service)
            {
                case "twitter":
                    return new DisplayInfo(_twitterLogo, Color.LightBlue);
                case "twitter-sonos":
                    return new DisplayInfo(_twitterLogo, Color.DarkBlue);
                case "lastfm":
                    return new DisplayInfo(_lastfmLogo, Color.Red);
                default:
                    return new DisplayInfo(null, Color.Black);
            }
        }

    }
}
