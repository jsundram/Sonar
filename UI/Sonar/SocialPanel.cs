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
        Image twitterLogo = null;                   // TODO: Should be static
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
            try
            {
                twitterLogo = Image.FromFile(@"c:..\..\..\..\images\twitter-logo-large.png");
                //twitterLogo = Image.FromFile(@"c:\work\Sonar\images\twitter.png");  // TODO total hack - fix this!
            }
            catch { };
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

        private void _Feed_DrawItem(object sender, DrawItemEventArgs e)
        {
            int borderWidth = 2;
            int imageWidth = 48;
            int imageHeight = imageWidth;

            SocialItem item = _Feed.Items[e.Index] as SocialItem;
            if (item == null) return;

            // Set the DrawMode property to draw fixed sized items.
            _Feed.DrawMode = DrawMode.OwnerDrawVariable;

            // Draw the background of the ListBox control for each item.
            e.DrawBackground();

            // Draw a border around each cell
            System.Drawing.Pen pen = new System.Drawing.Pen(Color.Black);
            pen.Width = borderWidth;

            // Code to select different color border depending on service
            bool displayTwitterLogo = false;
            switch(item.Service) {
                case "twitter":
                    pen.Color = Color.LightBlue;
                    displayTwitterLogo = true;
                    break;
                case "twitter-sonos":
                    pen.Color = Color.DarkBlue;
                    displayTwitterLogo = true;
                    break;
                case "lastfm":
                    pen.Color = Color.Red;
                    break;
                default:
                    pen.Color = Color.Black;
                    break;
            }

            // Draw border
            Rectangle border = new Rectangle(e.Bounds.X, e.Bounds.Y+1, e.Bounds.Width - borderWidth, e.Bounds.Height - borderWidth-1);
            e.Graphics.DrawRectangle(pen, border);

            // Define the default color of the brush as black.
            Brush textBrush = Brushes.Black;
            if (item.Url != null)
                textBrush = Brushes.Red;

            // Draw the current item text based on the current Font and the custom brush settings.
            Rectangle textRect = new Rectangle(e.Bounds.X + imageWidth + 4, e.Bounds.Y + 1 + 2, 
                                                e.Bounds.Width - borderWidth - imageWidth - 4, e.Bounds.Height - borderWidth - 1 - 2);
            e.Graphics.DrawString(item.ToString(), e.Font, textBrush, textRect, StringFormat.GenericDefault);

            // Draw the image
            if (item.Image != null)
            {
                Rectangle imageRect = new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 2, imageWidth, imageHeight);
                e.Graphics.DrawImage(item.Image, imageRect);
            }

            // Draw the logo
            if (displayTwitterLogo && twitterLogo != null)
            {
                Rectangle imageRect = new Rectangle(e.Bounds.X + e.Bounds.Width - 80, e.Bounds.Y + e.Bounds.Height - 20 , 64, 15);
                e.Graphics.DrawImage(twitterLogo, imageRect);
            }

            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();

            // Dispose what needs to be disposed of TODO!
            pen.Dispose();

        }

        private void _Feed_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 54;
            e.ItemWidth = 447;
        }

    }
}
