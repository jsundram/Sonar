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
        private List<ISocialDataSource> _DataSources = new List<ISocialDataSource>();

        System.Threading.Timer _timer = null;

        public SocialPanel()
        {
            InitializeComponent();
            _Feed.View = View.List;
            _Feed.Scrollable = false; // "solves" the horizontal scroll problem. TODO: fix for real.

            _timer = new System.Threading.Timer(new TimerCallback(Update), _DataSources, Timeout.Infinite, 60 * 1000);
        }

        public void AddDataSource(ISocialDataSource i)
        {
            _DataSources.Add(i);
        }

        // Initial Population
        public void Populate()
        {
            _timer.Change(0, 60 * 1000);
        }

        delegate void SocialItemListDelegate(List<SocialItem> value);

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
            // Stuff data into the listview:

            _Feed.Clear(); // TODO: Could add instead of killing and repopulating.

            ImageList images = new ImageList();
            int counter = 0;
            foreach (SocialItem i in sorted_data)
            {
                ListViewItem item = i.ToListItem();

                if (i.Image != null)
                {
                    images.Images.Add(i.Image);
                    item.ImageIndex = counter;
                    counter += 1;
                }

                _Feed.Items.Add(item);
            }

            _Feed.LargeImageList = images;
            _Feed.SmallImageList = images;
        }

        void Update(object state)
        {            
            List<ISocialDataSource> dataSources = (List<ISocialDataSource>)state;
            Sonar.Trace(string.Format("In Update(), have {0} data sources", dataSources.Count));

            List<SocialItem> data = new List<SocialItem>();
            foreach (ISocialDataSource i in dataSources)
            {
                data.AddRange(i.Update());
            }

            data.Sort(delegate(SocialItem i1, SocialItem i2)
            {
                return i2.PostTime.CompareTo(i1.PostTime); // descending post time.
            });

            //foreach (SocialItem i in data) Sonar.Trace("post time " + i.PostTime.ToString());

            Sonar.Trace(string.Format("Calling PopulateFeed() with {0} entries", data.Count));
            PopulateFeed(data);
        }

        public void SocialPanel_Enter(object sender, EventArgs e)
        {
            Sonar.Trace("Entering SocialPanel()");
            _timer.Change(0, 60 * 1000);
        }

        private void SocialPanel_Leave(object sender, EventArgs e)
        {
            Sonar.Trace("Leaving SocialPanel()");
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
            SocialPanel p = (SocialPanel)panel;
            Sonar.Trace(string.Format("In Social Panel, have Social Item with url {0}", i.Url));
            // here's the part where we figure out where that sucker is in the view, and update it's state.
        }
    }
}
