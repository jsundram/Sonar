using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sonar
{
    public partial class NowPlayingPanel : UserControl
    {
        List<Image> _Images = null;
        int _Index = 0;

        SonosClient _Sonos = null;
        string _ZoneGroup;
        int _TotalSeconds;

        public NowPlayingPanel(SonosClient sonos, string zgid)
        {
            InitializeComponent();
            _Sonos = sonos;
            _ZoneGroup = zgid;
            Init();
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

            // TODO: Something with np.AlbumArtUri;
            _Artist.Text = np.Artist;
            _Album.Text = np.Album;
            _Track.Text = np.Track;
            _TotalSeconds = np.PlayTime;

            // Update Scrubber:
            int seconds = _Sonos.GetTrackTime(_ZoneGroup);
            if (_TotalSeconds != 0)
                _TrackProgress.Value = (int)(100 * (float)seconds / (float)_TotalSeconds);

            // Start up album art threads

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

        private void _Artist_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Show form with awesome echo-nesty data here.
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
