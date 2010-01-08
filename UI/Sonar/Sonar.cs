using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;

namespace Sonar
{
    public partial class MainForm : Form
    {
        SonosClient _Sonos = new SonosClient("");
        ContextMenuStrip _PlayMenu = new ContextMenuStrip();
        public MainForm()
        {
            InitializeComponent();

            UpdateNowPlaying();
            _Sonos.OnZoneGroupsChanged += UpdateNowPlaying;

            PopulateSocial();

            _PlayMenu.Items.Add("Enqueue");
            _PlayMenu.Items.Add("Get Info");
            _PlayMenu.ItemClicked += new ToolStripItemClickedEventHandler(_PlayMenu_ItemClicked);

            _SearchResults.ContextMenuStrip = _PlayMenu;
            _SearchResults.MouseDown += new MouseEventHandler(_SearchResults_MouseDown);
            _social._Sonos = _Sonos;
            /*
            
            _Images = AmazonGateway.SearchAlbumArt("Please Please Me");
            // PlayMe.AlbumResponse r = PlayMe.GetAlbum("Please Please Me"); // This returns 9 albums, none of which are the right one.
            // PlayMe.Album a = PlayMe.GetAlbum("The Beatles", "Please Please Me");
            _AlbumArt.Image = _Images[_Index];
            _AlbumArt.MouseDoubleClick += new MouseEventHandler(_AlbumArt_MouseDoubleClick);
            EchoNest.Response r = EchoNest.SearchArtist("The Beatles");
            EchoNest.Artist a = r.GetArtist();
            float familiarity = EchoNest.GetFamiliarity(a.id).GetArtist().familiarity;
            float hotttnesss = EchoNest.GetHotttnesss(a.id).GetArtist().hotttness;
            _Familiarity.Value = (int)(familiarity * 100.0);
            _Hotness.Value = (int)(hotttnesss * 100.0);
            
            // string playable_url = Resolver.Resolve("The Beatles", "Anna"); // I know I have this one.
             */

        }

        void  _PlayMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int curr = _SearchResults.SelectedIndex;
            if (curr != -1)
            {
                Resolver.Result r = _SearchResults.Items[curr] as Resolver.Result;
                if (r == null)
                    return;

                if (e.ClickedItem.Text == "Enqueue")
                    _Sonos.Enqueue(GetCurrentZoneGroup(), r);
                else if (e.ClickedItem.Text == "Get Info")
                {
                    ArtistInspector a = new ArtistInspector(r.artist, r.album, r.track);
                    a.Show(); // TODO, cache this.
                }
            }
        }

        void _SearchResults_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //select the item under the mouse pointer
                _SearchResults.SelectedIndex = _SearchResults.IndexFromPoint(e.Location);
                if (_SearchResults.SelectedIndex != -1)
                    _PlayMenu.Show();
            }
        }

        TabPage MakeZoneTab(string zgid, string friendly_name)
        {
            TabPage t = new TabPage(friendly_name);
            t.Name = zgid;
            NowPlayingPanel p = new NowPlayingPanel(_Sonos, zgid);
            p.Dock = DockStyle.Fill;
            t.Controls.Add(p);
            return t;
        }

        public void UpdateNowPlaying()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SonosClient.WorldChanged(UpdateNowPlaying));
                return;
            }

            List<string> zgids = _Sonos.GetZoneGroups();
            
            foreach (string zgid in zgids)
                if (!now_playing_tabs.TabPages.ContainsKey(zgid))
                    now_playing_tabs.TabPages.Add(MakeZoneTab(zgid, zgid));

            foreach (TabPage t in now_playing_tabs.TabPages)
                if (!zgids.Contains(t.Name))
                    now_playing_tabs.TabPages.Remove(t);
        }

        public string GetCurrentZoneGroup()
        {
            return now_playing_tabs.SelectedTab.Text;
        }

        public static void WriteToFile(string filename, object data)
        {
            TextWriter tw = new StreamWriter(filename);
            tw.Write(data);
            tw.Close();
        }


        private void PopulateSocial()
        {
            //_social.AddDataSource(new TwitterFriends());
            _social.AddDataSource(new LastFmFriendsLoved());
            
            _social.AddDataSource(new TwitterSonos());

            // Add The Hype Machine (hypem.com)
            _social.AddDataSource(new TwitterSearch("from%3Ahypem", @"(?<title>.*) by (?<artist>.*) reached"));

            // Add Hunted (wearehunted.com)
            _social.AddDataSource(new TwitterSearch("%23wearehunted", @".* - (?<title>.*) / (?<artist>.*)"));

            _social.AddDataSource(new LastFmFriends());
            
            _social.Populate();
        }

        public static void Trace(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        void discover_tabs_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == _social.Parent)
                _social.SocialPanel_Enter(sender, e); // Hack...
        }

        private void _Artist_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                DoSearch(_Artist.Text, _Track.Text);
            }
           
            base.OnKeyPress(e);
        }

        private void _Track_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                DoSearch(_Artist.Text, _Track.Text);
            }

            base.OnKeyPress(e);
        }

        public void OnResolveProgress(List<Resolver.Result> results, bool complete)
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new Resolver.OnResolveProgress(OnResolveProgress), new object[] { results, complete});
                return;
            }

            _SearchResults.Items.Clear();
            results.Sort();
            _SearchResults.Items.AddRange(results.ToArray());

            if (!complete && results.Count == 0)
                _SearchResults.Items.Add("Searching ...");

            if (complete)
            {
                _SearchResults.ForeColor = Color.DodgerBlue;
                if (results.Count == 0)
                    _SearchResults.Items.Add("No results found.");
            }
            _SearchResults.Update();
        }

        void DoSearch(string artist, string track)
        {
            if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(track))
                return;

            _SearchResults.Items.Clear();
            //else, we are good to go. 
            Resolver.Resolve(artist, track, OnResolveProgress);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _Sonos.StopListening();
        }

    }



}