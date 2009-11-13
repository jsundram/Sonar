using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;

using System.IO;

using System.Text;

using System.Windows.Forms;
using System.Xml;
using Yedda;


namespace Sonar
{
    public partial class Sonar : Form
    {
        public Sonar()
        {
            InitializeComponent();
            _social.AddDataSource(new TwitterFriends());
            _social.AddDataSource(new LastFmFriendsLoved());
            _social.AddDataSource(new TwitterSonos());
            _social.AddDataSource(new LastFmFriends());
            _social.Populate();
        }


        private void twitter_Enter(object sender, EventArgs e)
        {
            // populate_twitter_friends();
        }


        private void populate_sonos_twitter()
        {
            Twitter t = new Twitter();
            

        }
    }
}