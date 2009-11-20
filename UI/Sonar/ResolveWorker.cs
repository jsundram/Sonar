using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Sonar
{
    public class ResolveWorker
    {
        BackgroundWorker _Worker = new BackgroundWorker();
        public delegate void OnResolveCompleted(object sender, SocialItem i);
        OnResolveCompleted _OnComplete;
        object _Caller;
        public ResolveWorker()
        {
            _Worker.DoWork += DoWork;
            _Worker.RunWorkerCompleted += WorkCompleted;
        }

        public void Start(object sender, SocialItem i, OnResolveCompleted on_complete)
        {
            _OnComplete = on_complete;
            _Caller = sender;
            _Worker.RunWorkerAsync(i);
        }

        public void Cancel()
        {
            _Worker.CancelAsync();
        }

        void DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // Extract the argument.
            SocialItem i = (SocialItem)e.Argument;

            // Start the time-consuming operation.
            i.Url = Resolver.Resolve(worker, i);
            e.Result = i;
            // If the operation was canceled by the user, 
            // set the DoWorkEventArgs.Cancel property to true.
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }

        }

        void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                MainForm.Trace("ResolveWorker thread was cancelled");
            else if (e.Error != null)
                MainForm.Trace(e.Error.Message);
            else
            {
                // do something with e.Result
                SocialItem i = e.Result as SocialItem;
                if (i != null)
                {
                    if (!string.IsNullOrEmpty(i.Url)) 
                        MainForm.Trace(string.Format("Resolved {0} by {1} to {2}", i.Track, i.Artist, i.Url));
                    else 
                       MainForm.Trace(string.Format("Unable to resolve {0} by {1}", i.Track, i.Artist));
                }
                this._OnComplete.DynamicInvoke(new object[]{_Caller, i});
            }

        }

    }
}
