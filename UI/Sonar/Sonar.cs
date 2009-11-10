using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Yedda;

namespace SonarGUI
{
    public partial class Sonar : Form
    {
        public Sonar()
        {
            InitializeComponent();
            populate_twitter_friends();
            populate_sonos_twitter();
        }


        private Dictionary<string, string> try_get_artist_title(string input)
        {
            // Possible strings: 
            // 1. "Enjoying %s by %s all over the house on my #Sonos"
            // 2. "Listening to %s by %s"
            // "Love hearing %s playing on my #Sonos" (Artist)
            // "%s is playing all over my house" (Artist)
            Dictionary<string, string> results = new Dictionary<string, string>();
            Regex r1 = new Regex(@"Enjoying (?<title>.*) by (?<artist>.*) all over the house on my #Sonos", RegexOptions.Compiled);           
            Match m = r1.Match(input);
            if (m.Success)
            {
                results["artist"] = m.Groups["artist"].Value;
                results["title"] = m.Groups["title"].Value;
                return results;
            }

            Regex r2 = new Regex(@"Listening to (?<title>.*) by (?<artist>.*)", RegexOptions.Compiled);
            m = r2.Match(input);
            if (m.Success)
            {
                results["artist"] = m.Groups["artist"].Value;
                results["title"] = m.Groups["title"].Value;
                return results;
            }
            return null;
        }

        private void twitter_Enter(object sender, EventArgs e)
        {
            // populate_twitter_friends();
        }

        private void sonos_Enter(object sender, EventArgs e)
        {
            // populate_sonos_twitter();
        }

        private void populate_sonos_twitter()
        {
            Twitter t = new Twitter();
            XmlDocument results = t.Search("by+source:sonos");

        }

        
        private Image get_image(string url)
        {
            WebClient wc = new WebClient();
            byte[] data = wc.DownloadData(url);
            MemoryStream ms = new MemoryStream(data);
            return new Bitmap(ms);
        }

        private void populate_twitter_friends()
        {
            // Get Data
            Twitter t = new Twitter();
            string user = ConfigurationSettings.AppSettings["twitter_user"]; // if these don't exist you need to setup your app settings.
            string pass = ConfigurationSettings.AppSettings["twitter_pass"];
            XmlDocument timeline = t.GetFriendsTimelineAsXML(user, pass);
            XmlNodeList statuses = timeline.GetElementsByTagName("text");
            XmlNodeList screen_names = timeline.GetElementsByTagName("screen_name");
            XmlNodeList clients = timeline.GetElementsByTagName("source");
            XmlNodeList images = timeline.GetElementsByTagName("profile_image_url");
            System.Diagnostics.Debug.Assert(statuses.Count == clients.Count);

            ImageList profile_images = new ImageList();
            ListViewItem[] items = new ListViewItem[statuses.Count];
            // Loop for the <title>, <link>, <description> and all the other tags
            for (int i = 0; i < statuses.Count; i++)
            {
                string status = statuses[i].InnerText;
                string client = clients[i].InnerText;

                // could also look for sender.
                if ((!string.IsNullOrEmpty(status) && status.ToLower().Contains("#sonos")) ||
                    (client == "<a href=\"http://www.sonos.com/tweetfeed\" rel=\"nofollow\">Sonos</a>")) 
                {
                    ListViewItem item = new ListViewItem(screen_names[i].InnerText + ": " + status);
                    item.Tag = try_get_artist_title(status);
                    if (item.Tag != null)
                        item.ForeColor = Color.DodgerBlue; // Distinguish playable items.
                    item.ToolTipText = status;
                    
                    
                    Image profile_image = get_image(images[i].InnerText);
                    profile_images.Images.Add(profile_image);
                    item.ImageIndex = profile_images.Images.Count - 1;
                    
                    items[i] = item;
                }
            }

            // Update Screen
            twitter_view.Clear();
            for (int i = 0; i < items.Length; i++)
                if (items[i] != null)
                    twitter_view.Items.Add(items[i]);
            twitter_view.LargeImageList = profile_images;
            twitter_view.SmallImageList = profile_images;
        }


    }
}