using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Sonar
{

    public partial class SocialPanel : UserControl
    {
        static int RefreshDelayMs = 5 * 60 * 1000; // 5 minutes.
        private List<ISocialDataSource> _DataSources = new List<ISocialDataSource>();
        DateTime _LastUpdate = DateTime.MinValue;

        System.Threading.Timer _timer = null;
        ContextMenuStrip _PlayMenu = new ContextMenuStrip();
        public SonosClient _Sonos;
        public SocialPanel()
        {
            InitializeComponent();

            _PlayMenu.Items.Add("Enqueue");
            _PlayMenu.Items.Add("Get Info");
            _PlayMenu.ItemClicked += new ToolStripItemClickedEventHandler(_PlayMenu_ItemClicked);

            _Feed.ContextMenuStrip = _PlayMenu;
            _Feed.MouseDown += new MouseEventHandler(_Feed_MouseDown);

            _timer = new System.Threading.Timer(new TimerCallback(Update), _DataSources, Timeout.Infinite, RefreshDelayMs);            
        }

        void _PlayMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int curr = _Feed.SelectedIndex;
            if (curr != -1)
            {
                SocialItem i = _Feed.Items[curr] as SocialItem;

                if (e.ClickedItem.Text == "Get Info")
                {
                    ArtistInspector a = new ArtistInspector(i.Artist, "");
                    a.Show(); // TODO, cache this.
                }

                if (i.Source == null)
                    return;

                if (e.ClickedItem.Text == "Enqueue")
                    _Sonos.Enqueue(i.Source);
            }
        }

        void _Feed_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //select the item under the mouse pointer
                _Feed.SelectedIndex = _Feed.IndexFromPoint(e.Location);
                if (_Feed.SelectedIndex != -1)
                {
                    SocialItem i = _Feed.Items[_Feed.SelectedIndex] as SocialItem;
                    _PlayMenu.Items[0].Enabled = (i != null && i.Source != null);
                    _PlayMenu.Show();
                }
            }
        }


        public void AddDataSource(ISocialDataSource i)
        {
            _DataSources.Add(i);
        }

        // Initial Population
        public void Populate()
        {
            _timer.Change(0, RefreshDelayMs);
        }

        delegate void SocialItemListDelegate(List<SocialItem> value);
        delegate void SocialItemDelegate(SocialItem value);

        void PopulateFeed(List<SocialItem> sorted_data)
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SocialItemListDelegate(PopulateFeed), new object[] { sorted_data });
                return;
            }

            Resolve(sorted_data); // kick off the resolvers

            // Must be on the UI thread if we've got this far
            _Feed.Items.Clear(); // TODO: Could add instead of killing and repopulating.

            foreach (SocialItem i in sorted_data)
            {
                _Feed.Items.Add(i);
            }
        }

        void Update(object state)
        {
            if (DateTime.Now - _LastUpdate < TimeSpan.FromMilliseconds(RefreshDelayMs))
                return;
            _LastUpdate = DateTime.Now;
            List<ISocialDataSource> dataSources = (List<ISocialDataSource>)state;
            MainForm.Trace(string.Format("In Update(), have {0} data sources", dataSources.Count));

            List<SocialItem> data = new List<SocialItem>();
            foreach (ISocialDataSource i in dataSources)
            {
                List<SocialItem> l = i.Update();
                if (l != null)
                    data.AddRange(l);
            }

            if (data.Count == 0)
            {
                MainForm.Trace("Ditching, no data returned");
                return;
            }

            data.Sort(delegate(SocialItem i1, SocialItem i2)
            {
                return i2.PostTime.CompareTo(i1.PostTime); // descending post time.
            });

            //foreach (SocialItem i in data) MainForm.Trace("post time " + i.PostTime.ToString());

            MainForm.Trace(string.Format("Calling PopulateFeed() with {0} entries", data.Count));
            _LastUpdate = DateTime.Now;
            PopulateFeed(data);
        }

        public void SocialPanel_Enter(object sender, EventArgs e)
        {
            MainForm.Trace("Entering SocialPanel()");
            _timer.Change(0, RefreshDelayMs);
        }

        private void SocialPanel_Leave(object sender, EventArgs e)
        {
            MainForm.Trace("Leaving SocialPanel()");
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        void Resolve(List<SocialItem> sorted_data)
        {
            ResolveWorker.OnResolveCompleted d = OnResolutionSuccess;
            // kick off a bunch of jobs
            foreach (SocialItem i in sorted_data)
            {
                ResolveWorker w = new ResolveWorker();
                w.Start(this, i, d); // might be nice to keep w around so it can be cancelled, but ... let's not for now.
            }
        }

        static void OnResolutionSuccess(object panel, SocialItem i)
        {
            SocialPanel p = panel as SocialPanel;
            if (p == null)
                return;

            if (p.InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                p.BeginInvoke(new ResolveWorker.OnResolveCompleted(OnResolutionSuccess), new object[] { panel, i });
                return;
            }
            
            // TODO: Update item corresponding to i with status
            
        }
    }
}
