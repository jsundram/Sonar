using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LastFmLib.General;
using LastFmLib.API20;
using LastFmLib.API20.Types;


namespace Sonar
{
    public partial class ArtistInspector : Form
    {
        EchoNest.Artist _EnArtist;
        string _album;
        string _artist;
        int _Index = 0;
        List<Image> _Images = new List<Image>();
        List<string> _SimilarArtists = new List<string>();
        AlbumInfo _AlbumInfo;
        public ArtistInspector(string artist, string album)
        {
            _artist = artist;
            _album = album;
            InitializeComponent();
            EchoNest.Response r = EchoNest.SearchArtist(artist);
            _EnArtist = r.GetArtist();

            if (_EnArtist != null)
            {
                AlbumArtWorker w = new AlbumArtWorker();
                List<string> urls = _EnArtist.images.ConvertAll<string>(delegate(EchoNest.Document d) { return d.url; });
                w.Start(this, urls, OnAlbumArtRetrieved);
                List<EchoNest.Artist> similars = EchoNest.GetSimilar(_EnArtist.id).artists;
                foreach (EchoNest.Artist a in similars)
                    _SimilarArtists.Add(a.name);
            }
            else
                _Artist.Text = "EchoNest API error";
        }

        public bool LoadedSuccessfully()
        {
            return _EnArtist != null;
        }

        void ArtistInspector_Load(object sender, EventArgs e)
        {
            if (_EnArtist != null)
            {
                _Artist.Text = _EnArtist.name;
                _Hotness.Value = (int)(100.0 * _EnArtist.hotttness);
                _Familiarity.Value = (int)(100.0 * _EnArtist.familiarity);
                string text = "Similar Artists: ";
                foreach (string a in _SimilarArtists)
                    text += a + ", "; // TODO: Extra comma.
                _ArtistInfo.Text = text;
            }
            _AlbumInfo = FindAlbumOnLastFm();

            string info = "Couldn't find album on last.fm";
            if (_AlbumInfo != null)
            {
                info = string.Format("{0}:\r\n\r\n{1:d} listeners\r\n{2:d} plays\r\nreleased: {3}\r\n\r\n", _AlbumInfo.AlbumName, _AlbumInfo.NumListeners, _AlbumInfo.PlayCount, _AlbumInfo.ReleaseDate.ToShortDateString());
                info += "Tags: ";
                if (_AlbumInfo.Tags != null)
                    foreach (string key in _AlbumInfo.Tags.Keys)
                        info += key + ", ";
            }
            _LastFmInfo.Text = info;
        }

        bool fuzzy_match(string s1, string s2)
        {
            return s1.ToLower().Equals(s2.ToLower());
        }

        AlbumInfo FindAlbumOnLastFm()
        {
            LastFmClient c = LastFm.get_client();
            return c.Album.GetInfo(_artist, _album, false,  "en");
        }
        public void AddImages(Dictionary<string, Image> images)
        {
            _Images.AddRange(images.Values);
            if (_Art.Image == null)
                _Art.Image = _Images[0];
     
            MainForm.Trace("Added " + images.Count.ToString() + " images for album");
        }

        public static void OnAlbumArtRetrieved(object form, Dictionary<string, Image> images)
        {
            ArtistInspector f = form as ArtistInspector;
            if (f == null)
                return;

            if (f.InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                f.BeginInvoke(new AlbumArtWorker.OnAlbumArtRetrieved(OnAlbumArtRetrieved), new object[] { f, images });
            }

            f.AddImages(images);
        }

        void _Art_DoubleClick(object sender, EventArgs e)
        {
            if (_Images.Count != 0)
            {
                _Index = (_Index + 1) % _Images.Count;
                _Art.Image = _Images[_Index];
            }
        }

    }
}
