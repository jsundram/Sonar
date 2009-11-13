using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Sonar
{
    public partial class SocialPanel : UserControl
    {
        private List<ISocialDataSource> _DataSources = new List<ISocialDataSource>();

        public SocialPanel()
        {
            InitializeComponent();
            _Feed.View = View.List;
            _Feed.Scrollable = false; // "solves" the horizontal scroll problem. TODO: fix for real.            
        }

        public void AddDataSource(ISocialDataSource i)
        {
            _DataSources.Add(i);
        }

        public void Populate()
        {
            // TODO: This should be threaded.
            List<SocialItem> data = new List<SocialItem>();
            foreach (ISocialDataSource i in _DataSources)
            {
                data.AddRange(i.Init());
            }

            data.Sort(delegate(SocialItem i1, SocialItem i2)
            {
                return i2.PostTime.CompareTo(i1.PostTime); // descending post time.
            });

            // Stuff data into the listview:
            // TODO: Could add instead of killing and repopulating.
            _Feed.Clear(); 

            ImageList images = new ImageList();
            int counter = 0;
            foreach (SocialItem i in data)
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

        private void SocialPanel_Enter(object sender, EventArgs e)
        {

        }
    }
}
