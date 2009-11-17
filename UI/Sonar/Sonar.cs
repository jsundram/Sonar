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

using CookComputing.XmlRpc;


namespace Sonar
{
    public partial class Sonar : Form
    {
        public Sonar()
        {
            InitializeComponent();
            // PopulateSocial();
            EchoNest.Test();
            


            // string playable_url = Resolver.Resolve("The Beatles", "Anna"); // I know I have this one.

            // Playing with XML-RPC
            //IStateName proxy = XmlRpcProxyGen.Create<IStateName>();
            //string stateName = proxy.GetStateName(41);S
            /*
            IQueueTrack proxy = XmlRpcProxyGen.Create<IQueueTrack>();
            XmlRpcStruct metadata = new XmlRpcStruct();
            metadata.Add("Artist", "The Beatles");
            metadata.Add("Track", "Anna");
            string result = proxy.EnqueueTrack("http://192.168.2.12:60210/sid/430FB444-DBC8-4BBE-8160-48CB491B6063", metadata);
            Sonar.Trace(result);*/
        }

        public static void WriteToFile(string filename, object data)
        {
            TextWriter tw = new StreamWriter(filename);
            tw.Write(data);
            tw.Close();
        }

        [XmlRpcUrl("http://192.168.2.7:8000/RPC2")]
        public interface IQueueTrack : IXmlRpcProxy
        {
            [XmlRpcMethod("EnqueueTrack")]
            string EnqueueTrack(string url, XmlRpcStruct metadata);
        }

        private void PopulateSocial()
        {
            // _social.AddDataSource(new TwitterFriends());
            _social.AddDataSource(new LastFmFriendsLoved());
            // _social.AddDataSource(new TwitterSonos());
            // _social.AddDataSource(new LastFmFriends());
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
    }



}