using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Globalization;
using Yedda;
using LastFmLib.API20;
using LastFmLib.API20.Types;
using LastFmLib.General;

namespace Sonar
{
    public interface ISocialDataSource
    {
        List<SocialItem> Init();
        List<SocialItem> Update();
    }

    public class LastFmFriendsLoved : ISocialDataSource
    {
        // TODO: is this repeated code disgusting?
        AuthData make_credentials()
        {
            MD5Hash key = new MD5Hash(Credentials.LastFmKey, true, System.Text.Encoding.ASCII);
            MD5Hash secret = new MD5Hash(Credentials.LastFmSecret, true, System.Text.Encoding.ASCII);

            return new AuthData(key, secret);
        }

        #region ISocialDataSource Members

        public List<SocialItem> Init()
        {
            AuthData credentials = make_credentials();
            LastFmLib.API20.Settings20.AuthData = credentials;
            LastFmClient c = LastFmClient.Create(credentials);

            List<SocialItem> items = new List<SocialItem>();
            List<FriendUser> friends = c.User.GetFriends(Credentials.LastFmUser, false, 50);// arbitrary number of friends
            friends.Add(new FriendUser(Credentials.LastFmUser, null)); // I want to see my own favorites, too.
            string time_format = "d MMM yyyy, HH:mm";
            foreach (FriendUser f in friends)
            {
                string name = f.Name;
                Bitmap image = f.DownloadImage(modEnums.ImageSize.Smallest); // TODO: Check HasAnyImage first?
                try
                {
                    List<Track> tracks = c.User.GetLovedTracks(name);
                    foreach (Track track in tracks)
                    {
                        try
                        {
                            SocialItem i = new SocialItem();
                            i.User = name;
                            i.Artist = track.ArtistName;
                            i.Track = track.Title;
                            i.Image = track.DownloadImage(modEnums.ImageSize.Small);
                            if (i.Image == null)
                                i.Image = image; // set this to user profile image.

                            // Would be nice to go back and hack up LastFMLibNet to return an actual DateTime, but easier to do it in C# for now.                            
                            i.PostTime = DateTime.ParseExact(track.Time, time_format, CultureInfo.InvariantCulture);// 30 Oct 2009, 21:13
                            
                            items.Add(i);
                        }
                        catch (Exception) { }
                    }
                }
                catch (Exception) { }
            }

            return items;
        }

        public List<SocialItem> Update()
        {
            return Init();
        }

        #endregion
    }

    public class LastFmFriends : ISocialDataSource
    {
        AuthData make_credentials()
        {
            MD5Hash key = new MD5Hash(Credentials.LastFmKey, true, System.Text.Encoding.ASCII);
            MD5Hash secret = new MD5Hash(Credentials.LastFmSecret, true, System.Text.Encoding.ASCII);

            return new AuthData(key, secret);
        }

        #region ISocialDataSource Members

        public List<SocialItem> Init()
        {
            AuthData credentials = make_credentials();
            LastFmLib.API20.Settings20.AuthData = credentials;
            LastFmClient c = LastFmClient.Create(credentials);

            List<SocialItem> items = new List<SocialItem>();
            List<FriendUser> friends = c.User.GetFriends(Credentials.LastFmUser, false, 50);// arbitrary number of friends
            foreach (FriendUser f in friends)
            {
                string name = f.Name;
                Bitmap image = f.DownloadImage(modEnums.ImageSize.Smallest); // TODO: Check HasAnyImage first?
                try
                {
                    List<RecentTrack> tracks = c.User.GetRecentTracks(name, 5);// Could adjust this. arbitrary for now.
                    foreach (RecentTrack track in tracks)
                    {
                        try
                        {
                            SocialItem i = new SocialItem();
                            i.User = name;
                            i.Artist = track.ArtistName;
                            i.Track = track.Title;
                            i.Image = track.DownloadImage(modEnums.ImageSize.Small);
                            if (i.Image == null)
                                i.Image = image; // set this to user profile image.

                            i.PostTime = track.StartDate; // TODO: Deal with time zones?

                            items.Add(i);
                        }
                        catch (Exception) { }
                    }
                }
                catch(Exception){}
            }

            return items;
        }

        public List<SocialItem> Update()
        {
            return Init();
        }

        #endregion
    }

    public class TwitterSonos : ISocialDataSource
    {
        #region ISocialDataSource Members

        // Result looks like:
        /*
        <entry>
            <id>tag:search.twitter.com,2005:5570205072</id>
            <published>2009-11-09T21:57:19Z</published>
            <link type="text/html" href="http://twitter.com/Ricardo5599/statuses/5570205072" rel="alternate" />
            <title>Enjoying Close To You by JLS all over the house on my #Sonos</title>
            <content type="html">Enjoying Close To You &lt;b&gt;by&lt;/b&gt; JLS all over the house on my &lt;a href="http://search.twitter.com/search?q=%23Sonos"&gt;#Sonos&lt;/a&gt;</content>
            <updated>2009-11-09T21:57:19Z</updated>
            <link type="image/png" href="http://a1.twimg.com/profile_images/509073086/WeeMee_16002855_for_richard.suttonpgs_normal.jpg" rel="image"/>
            <twitter:geo></twitter:geo>
            <twitter:source>&lt;a href="http://www.sonos.com/tweetfeed" rel="nofollow"&gt;Sonos&lt;/a&gt;</twitter:source>
            <twitter:lang>en</twitter:lang>
            <author>
                <name>Ricardo5599 (RichardSutton)</name>
                <uri>http://twitter.com/Ricardo5599</uri>
            </author>
        </entry>*/         
        public List<SocialItem> Init()
        {
            Twitter t = new Twitter();
            List<SocialItem> items = new List<SocialItem>();
            XmlDocument timeline = t.Search("by+source:sonos");
            XmlNodeList statuses = timeline.GetElementsByTagName("title");
            XmlNodeList screen_names = timeline.GetElementsByTagName("author");
            XmlNodeList images = timeline.GetElementsByTagName("link"); // this matches too much stuff!
            XmlNodeList post_times = timeline.GetElementsByTagName("published");
            int s = statuses.Count;
            int n = screen_names.Count;
            System.Diagnostics.Debug.Assert(statuses.Count-1 == screen_names.Count);
            System.Diagnostics.Debug.Assert(images.Count - 5 == 2 * screen_names.Count);
            
                                
            string time_format = "yyyy-MM-ddTHH:mm:ssZ";//2009-11-09T21:57:19Z
 
            // Loop for the <title>, <link>, <description> and all the other tags
            for (int i = 0; i < screen_names.Count; i++)
            {
                try
                {
                    string status = statuses[i + 1].InnerText; // "title" is returned in the header, so we add one here to compensate for it.
                    SocialItem item = new SocialItem();
                    if (item.try_populate_artist_track(status))
                    {
                        item.Message = status;
                        string user = screen_names[i].InnerText;
                        item.User = user.Substring(0, user.IndexOf(' '));// looks like this jsundram (jason Sundram)https:..sdfasdfasdjfh


                        item.Image = item.GetImage(images[6 + 2 * i].Attributes[1].Value);
                        item.PostTime = DateTime.ParseExact(post_times[i].InnerText, time_format, CultureInfo.InvariantCulture);

                        items.Add(item);
                    }
                }
                catch (Exception) { }
            }

            return items;
        }

        public List<SocialItem> Update()
        {
            return Init();
        }

        #endregion
    }

    public class TwitterFriends : ISocialDataSource
    {
        #region ISocialDataSource Members

        public List<SocialItem> Init()
        {            
            Twitter t = new Twitter();
            List<SocialItem> items = new List<SocialItem>();

            // Get Data
            XmlDocument timeline = t.GetFriendsTimelineAsXML(Credentials.TwitterUser, Credentials.TwitterPass);
            //timeline.Save("twitter_friends.xml");

            // Get important pieces.
            XmlNodeList statuses = timeline.GetElementsByTagName("text");
            XmlNodeList screen_names = timeline.GetElementsByTagName("screen_name");
            XmlNodeList clients = timeline.GetElementsByTagName("source");
            XmlNodeList images = timeline.GetElementsByTagName("profile_image_url");
            // XmlNodeList utc_offsets = timeline.GetElementsByTagName("utc_offset"); // necessary?
            XmlNodeList post_times = timeline.GetElementsByTagName("created_at");
            System.Diagnostics.Debug.Assert(statuses.Count == clients.Count);

            string time_format = "ddd MMM dd HH:mm:ss zzz yyyy"; // determined experimentally from a response: "Tue Nov 10 21:01:23 +0000 2009";

            // Loop for the <title>, <link>, <description> and all the other tags
            for (int i = 0; i < statuses.Count; i++)
            {
                string status = statuses[i].InnerText;
                string client = clients[i].InnerText;

                if ((!string.IsNullOrEmpty(status) && status.ToLower().Contains("#sonos")) ||
                    (client == "<a href=\"http://www.sonos.com/tweetfeed\" rel=\"nofollow\">Sonos</a>"))
                {
                    try
                    {
                        SocialItem item = new SocialItem();
                        item.User = screen_names[i].InnerText;
                        item.Message = status;
                        item.try_populate_artist_track(status);
                        item.Image = item.GetImage(images[i].InnerText);

                        item.PostTime = DateTime.ParseExact(post_times[i].InnerText, time_format, CultureInfo.InvariantCulture); // e.g. Tue Nov 10 21:01:23 +0000 2009

                        items.Add(item);
                    }
                    catch (Exception) { }
                }
            }

            return items;
        }

        public List<SocialItem> Update()
        {
            return Init(); // TODO: Be smarter about updates
        }

        #endregion
    }

}
