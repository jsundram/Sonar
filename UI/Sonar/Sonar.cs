using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using CookComputing.XmlRpc;


namespace Sonar
{
    public partial class Sonar : Form
    {
        public Sonar()
        {
            InitializeComponent();
            //PopulateSocial();
            // string playable_url = Resolver.Resolve("The Beatles", "Anna"); // I know I have this one.

            // Playing with XML-RPC
            IStateName proxy = XmlRpcProxyGen.Create<IStateName>();
            string stateName = proxy.GetStateName(41);
        }

        [XmlRpcUrl("http://betty.userland.com/RPC2")]
        public interface IStateName : IXmlRpcProxy
        {
            [XmlRpcMethod("examples.getStateName")]
            string GetStateName(int stateNumber);
        }

        string server = "http://192.168.2.7:8000";
        private void PopulateSocial()
        {
            _social.AddDataSource(new TwitterFriends());
            _social.AddDataSource(new LastFmFriendsLoved());
            _social.AddDataSource(new TwitterSonos());
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
    }



}