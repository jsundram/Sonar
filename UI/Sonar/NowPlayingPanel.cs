using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace Sonar
{
    public partial class NowPlayingPanel : UserControl
    {
        List<Image> _Images = new List<Image>();
        int _Index = 0;

        SonosClient _Sonos = null;
        string _ZoneGroup;
        int _TotalSeconds;
        ContextMenuStrip _PlayMenu = new ContextMenuStrip();

        public NowPlayingPanel(SonosClient sonos, string zgid)
        {
            InitializeComponent();
            _Sonos = sonos;
            _ZoneGroup = zgid;
            Init();

            _PlayMenu.Items.Add("Get Info");
            _PlayMenu.ItemClicked += new ToolStripItemClickedEventHandler(_PlayMenu_ItemClicked);

            _Queue.ContextMenuStrip = _PlayMenu;
            _Queue.MouseDown += new MouseEventHandler(_PlayMenu_MouseDown);

        }


        void _PlayMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int curr = _Queue.SelectedIndex;
            if (curr != -1)
            {
                SonosClient.Metadata r = _Queue.Items[curr] as SonosClient.Metadata;
                if (r == null)
                    return;

                if (e.ClickedItem.Text == "Get Info")
                {
                    ArtistInspector a = new ArtistInspector(r.Artist, r.Album);
                    a.Show(); // TODO, cache this.
                }
            }
        }

        void _PlayMenu_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //select the item under the mouse pointer
                _Queue.SelectedIndex = _Queue.IndexFromPoint(e.Location);
                if (_Queue.SelectedIndex != -1)
                    _PlayMenu.Show();
            }
        }


        public void Init()
        {
            // Register handlers
            _Sonos.OnQueueChanged += this.OnQueueChanged;
            _Sonos.OnTrackChanged += this.OnTrackChanged;
            _Sonos.OnTrackProgress += this.OnTrackProgress;
            _Sonos.OnPlayStateChanged += this.OnPlayStateChanged;
            _Sonos.OnMuteChanged += this.OnMuteChanged;
            _Sonos.OnVolumeChanged += this.OnVolumeChanged;

            PopulateQueue();
            PopulateNowPlaying();
            UpdateControls();
        }

        public void Destroy()
        {
            // Code that needs to be called when destroying this panel.
            _Sonos.OnQueueChanged -= this.OnQueueChanged;
            _Sonos.OnTrackChanged -= this.OnTrackChanged;
            _Sonos.OnTrackProgress -= this.OnTrackProgress;
            _Sonos.OnPlayStateChanged -= this.OnPlayStateChanged;
            _Sonos.OnMuteChanged -= this.OnMuteChanged;
            _Sonos.OnVolumeChanged -= this.OnVolumeChanged;
        }

        void PopulateQueue()
        {
            List<SonosClient.Metadata> q = _Sonos.GetQueue(_ZoneGroup);
            _Queue.Items.AddRange(q.ToArray()); // Probably want something else.
        }

        void PopulateNowPlaying()
        {
            SonosClient.Metadata np = _Sonos.GetTrackMetadata(_ZoneGroup);

            _Artist.Text = np.Artist;
            _Album.Text = np.Album;
            _Track.Text = np.Track;
            _TotalSeconds = np.PlayTime;

            // Update Scrubber:
            int seconds = _Sonos.GetTrackTime(_ZoneGroup);
            if (_TotalSeconds != 0)
                _TrackProgress.Value = (int)(100 * (float)seconds / (float)_TotalSeconds);

            // Start up album art threads
            if (!string.IsNullOrEmpty(np.AlbumArtUri))
            {
                AlbumArtWorker w = new AlbumArtWorker();
                w.Start(this, new List<string>(new string[] { np.AlbumArtUri }), OnAlbumArtRetrieved);
            }

            GetAmazonAlbumArt(np);
        }

        void GetAmazonAlbumArt(SonosClient.Metadata np)
        {
            if (np != null && !string.IsNullOrEmpty(np.Artist) && !string.IsNullOrEmpty(np.Album))
            {
                string keywords = np.Artist + ": " + np.Album;
                
                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = false;
                bw.WorkerSupportsCancellation = false;
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.RunWorkerAsync(keywords);
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = AmazonGateway.SearchAlbumArtUris((string)e.Argument);
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<string> urls = e.Result as List<string>;
            if (urls != null && urls.Count > 0)
            {
                AlbumArtWorker w = new AlbumArtWorker();
                w.Start(this, urls, OnAlbumArtRetrieved);
            }
        }

        public void AddImages(Dictionary<string, Image> images)
        {
            _Images.AddRange(images.Values);
            if (_AlbumArt.Image == null)
                _AlbumArt.Image = _Images[0];
            
            MainForm.Trace("Added " + images.Count.ToString() + " images for album");
        }

        public static void OnAlbumArtRetrieved(object panel, Dictionary<string, Image> images)
        {
            NowPlayingPanel p = panel as NowPlayingPanel;
            if (p == null)
                return;

            if (p.InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                p.BeginInvoke(new AlbumArtWorker.OnAlbumArtRetrieved(OnAlbumArtRetrieved), new object[] { panel, images });
            }
            
            p.AddImages(images);
        }

        void UpdateControls()
        {
            bool playing = _Sonos.IsPlaying(_ZoneGroup);
            _Play.Text = playing ? "Pause" : "Play";

            bool muted = _Sonos.IsMuted(_ZoneGroup);
            // TODO: Change state of mute button.
            _Volume.Enabled = !muted;

            int volume = _Sonos.GetVolume(_ZoneGroup);
            _Volume.Value = volume;
        }

        delegate void ActionDelegate();
        public void OnQueueChanged(string zgid)
        {
            if (zgid != _ZoneGroup)
                return;

            if (InvokeRequired)
                BeginInvoke(new ActionDelegate(PopulateQueue));
            else
                PopulateQueue(); 
        }

        public void OnTrackChanged(string zgid)
        {
            if (zgid != _ZoneGroup)
                return;

            if (InvokeRequired)
                BeginInvoke(new ActionDelegate(PopulateNowPlaying));
            else
                PopulateQueue(); 
        }

        public void UpdateTrackProgress(int seconds)
        {
            if (_TotalSeconds != 0)
                _TrackProgress.Value = (int)(100.0 * (float)seconds / (float)_TotalSeconds);

            TimeSpan t = TimeSpan.FromSeconds(seconds);
            TimeSpan total = TimeSpan.FromSeconds(_TotalSeconds);    
            _TrackTime.Text = string.Format("{0:D2}:{1:D2} / {2:D2}:{3:D2}", t.Minutes, t.Seconds, total.Minutes, total.Seconds);
        }

        delegate void TrackProgressDelegate(int seconds);
        public void OnTrackProgress(string zgid, int seconds)
        {
            if (zgid != _ZoneGroup)
                return;

            if (InvokeRequired)
                BeginInvoke(new TrackProgressDelegate(UpdateTrackProgress), new object[]{seconds}); //BeginInvoke();
            else
                UpdateTrackProgress(seconds); 
        }

        public void OnVolumeChanged(string zgid, int level)
        {
            if (zgid != _ZoneGroup)
                return;

            if (InvokeRequired)
                BeginInvoke(new ActionDelegate(UpdateControls));
            else
                UpdateControls(); 
        }

        public void OnPlayStateChanged(string zgid, bool playing)
        {
            if (zgid != _ZoneGroup)
                return;

            if (InvokeRequired)
                BeginInvoke(new ActionDelegate(delegate { _Play.Text = playing ? "Pause" : "Play"; }));
            else
                _Play.Text = playing ? "Pause" : "Play"; 
        }

        public void OnMuteChanged(string zgid, bool muted)
        {
            if (zgid != _ZoneGroup)
                return;

            if (InvokeRequired)
                BeginInvoke(new ActionDelegate(UpdateControls));
            else
                UpdateControls();
        }

        Dictionary<string, ArtistInspector> _inspectors = new Dictionary<string, ArtistInspector>();
        ArtistInspector GetArtistInspector(string artist, string album)
        {
            string key = artist + album;
            if (_inspectors.ContainsKey(key))
            {
                ArtistInspector a = _inspectors[key];
                if (a.LoadedSuccessfully())
                    return a;
            }

            ArtistInspector b = new ArtistInspector(artist, album);
            _inspectors[key] = b;
            return b;
        }

        void _Artist_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Show form with awesome echo-nesty data here.
            ArtistInspector a = new ArtistInspector(_Artist.Text, _Album.Text);
            if (_Artist.Text == "Your Mom")
                a = GetArtistInspector("Tori Amos", "Boys for Pele");

            a.Show();
        }

        void _AlbumArt_DoubleClick(object sender, EventArgs e)
        {
            _Index = (_Index + 1) % _Images.Count;
            _AlbumArt.Image = _Images[_Index];
        }

        void _Play_Click(object sender, EventArgs e)
        {
            bool play = (_Play.Text == "Play");

            if (play)
                _Sonos.Play(_ZoneGroup, -1);
            else
                _Sonos.Pause(_ZoneGroup);
            
            _Play.Text = play ? "Pause" : "Play";
        }

        void _Back_Click(object sender, EventArgs e)
        {
            _Sonos.Back(_ZoneGroup);
        }

        void _Next_Click(object sender, EventArgs e)
        {
            _Sonos.Next(_ZoneGroup);
        }

        void _Mute_Click(object sender, EventArgs e)
        {
            bool muted = !_Volume.Enabled;
            _Sonos.SetMute(_ZoneGroup, !muted);

            _Volume.Enabled = !muted;
        }

        void _Volume_MouseUp(object sender, MouseEventArgs e)
        {
            _Sonos.SetVolume(_ZoneGroup, _Volume.Value);
            MainForm.Trace("Setting Volume to " + _Volume.Value.ToString());
        }

    }
}
