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
            _TrackProgress.Value = (int)((float)seconds / (float)_TotalSeconds);
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

        public void OnQueueChanged(string zgid)
        {
            if (zgid != _ZoneGroup)
                return;

            PopulateQueue(); // TODO: Invoke...
        }
        public void OnTrackChanged(string zgid)
        {
            if (zgid != _ZoneGroup)
                return;
            PopulateNowPlaying();// TODO: Invoke...
        }

        public void OnTrackProgress(string zgid, int seconds)
        {
            if (zgid != _ZoneGroup)
                return;

            // TODO: If InvokeRequired ...
            _TrackProgress.Value = (int)((float)seconds / (float)_TotalSeconds);
        }

        public void OnVolumeChanged(string zgid, int level)
        {
            if (zgid != _ZoneGroup)
                return;

            UpdateControls();
        }

        public void OnPlayStateChanged(string zgid, bool playing)
        {
            if (zgid != _ZoneGroup)
                return;

            _Play.Text = playing ? "Pause" : "Play";

        }

        public void OnMuteChanged(string zgid, bool muted)
        {
            if (zgid != _ZoneGroup)
                return;

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
    }
}
