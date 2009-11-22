using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;

namespace Sonar
{
    public class AlbumArtWorker
    {
        BackgroundWorker _Worker = new BackgroundWorker();
        public delegate void OnAlbumArtRetrieved(object sender, Dictionary<string, Image> image);
        OnAlbumArtRetrieved _OnProgress;
        object _Caller;

        public AlbumArtWorker()
        {
            _Worker.DoWork += DoWork;
            _Worker.RunWorkerCompleted += WorkCompleted;
        }

        public static Image GetImage(string url)
        {
            try
            {
                WebClient wc = new WebClient();
                byte[] data = wc.DownloadData(url);
                MemoryStream ms = new MemoryStream(data);
                return new Bitmap(ms);
            }
            catch (Exception)
            {

            }
            return null;
        }

        public void Start(object sender, List<string> imageUris, OnAlbumArtRetrieved on_progress)
        {
            _OnProgress = on_progress;
            _Caller = sender;
            _Worker.RunWorkerAsync(imageUris);
        }

        public void Cancel()
        {
            _Worker.CancelAsync();
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // Extract the argument.
            List<string> imageUris = (List<string>)e.Argument;

            Dictionary<string, Image> all = new Dictionary<string, Image>();
            foreach (string url in imageUris)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                if (url == null)
                    continue;

                Dictionary<string, Image> d = new Dictionary<string, Image>();
                d.Add(url, GetImage(url));

                if (_OnProgress != null)
                    this._OnProgress.DynamicInvoke(new object[] { _Caller, d });

                all.Add(url, d[url]);
            }

            e.Result = all;
        }

        void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MainForm.Trace("AlbumArtWorker thread was cancelled.");
            else if (e.Error != null)
                MainForm.Trace(e.Error.Message);
            else
            {
                // do something with e.Result
                Dictionary<string, Image> d = e.Result as Dictionary<string, Image>;
                //this._OnProgress.DynamicInvoke(new object[] { _Caller, d });
            }
        }

    }
}
