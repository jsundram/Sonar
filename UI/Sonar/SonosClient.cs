using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using CookComputing.XmlRpc;


namespace Sonar
{
    
    public class SonosClient
    {
        const string SonosServerUrl = "http://192.168.2.7:8000/RPC2";

        #region Helper Classes
        public class Metadata
        {
            public Metadata(string artist, string album, string track, string uri, string aa_uri, int seconds)
            {
                Artist = artist;
                Album = album;
                Track = track;
                Uri = uri;
                AlbumArtUri = aa_uri;
                PlayTime = seconds;
            }

            public Metadata(XmlRpcStruct md)
            {
                if (md.ContainsKey("Artist"))
                    Artist = (string)md["Artist"];
                if (md.ContainsKey("Album"))
                    Album = (string)md["Album"];
                if (md.ContainsKey("Title"))
                    Track = (string)md["Title"];
                if (md.ContainsKey("Uri"))
                    Uri = (string)md["Uri"];
                if (md.ContainsKey("AlbumArtUri"))
                    AlbumArtUri = (string)md["AlbumArtUri"];
                if (md.ContainsKey("PlayTime"))
                    PlayTime = (int)md["PlayTime"];
            }

            public string Artist { get; set; }
            public string Album { get; set; }
            public string Track { get; set; }
            public string Uri { get; set; }
            public string AlbumArtUri { get; set; }
            public int PlayTime { get; set; }

            public XmlRpcStruct ToXmlRpc()
            {
                XmlRpcStruct metadata = new XmlRpcStruct();
                metadata.Add("Artist", Artist);
                metadata.Add("Album", Album);
                metadata.Add("Title", Track);
                metadata.Add("Uri", Uri);
                metadata.Add("AlbumArtUri", AlbumArtUri);
                metadata.Add("PlayTime", PlayTime);
                return metadata;
            }
        }

        public class Event
        {
            public Event(XmlRpcStruct x)
            {
                if (x.ContainsKey("Name"))
                    Name = (string)x["Name"];
                Args = new List<object>();
                if (x.ContainsKey("Args"))
                {
                    object o = x["Args"];                    
                    object[] array = (object[])o;
                    Args.AddRange(array);
                }
            }
            public List<object> Args { get; set;  }
            public string Name { get; set; }
            public void Dispatch(SonosClient c)
            {
                switch (Name)
                {
                    case "OnMuteChanged":
                        c.OnMuteChanged((string)Args[0], (bool)Args[1]); break;
                    case "OnPlayStateChanged":
                        c.OnPlayStateChanged((string)Args[0], (bool)Args[1]); break;
                    case "OnQueueChanged":
                        c.OnQueueChanged((string)Args[0]); break;
                    case "OnTick":
                        c.OnTrackProgress((string)Args[0], (int)Args[1]); break;
                    case "OnTrackChanged":
                        c.OnTrackChanged((string)Args[0]); break;
                    case "OnVolumeChanged":
                        c.OnVolumeChanged((string)Args[0], (int)Args[1]); break;
                    case "OnZoneGroupsChanged":
                        c.OnZoneGroupsChanged(); break;
                    default:
                        break;/*do nothing*/
                }
            }

        }
        #endregion


        // returns, e.g. "mp3"
        public static string MimeTypeToExt(string mime)
        {
            switch (mime)
            {
                case "audio/mpeg": return "mp3";
                case "audio/wav": return "wav";
                case "audio/x-ms-wma": return "wma";
                case "audio/mp4": return "m4a"; // mp4?
                case "audio/aiff": return "aiff";
                case "audio/flac": return "flac";
                case "application/ogg": return "ogg";
                default:
                    return "mp3"; // as good a guess as any.
            }
        }
        // If you subscribe to these events and do stuff with the UI, be sure to do it via Invoke.
        public delegate void ZoneChange(string zgid);
        public delegate void Tick(string zgid, int n);
        public delegate void WorldChanged();
        public delegate void StateChanged(string zgid, bool b);

        ISonosClient _Proxy = XmlRpcProxyGen.Create<ISonosClient>();
        volatile bool _Stop = false;
        Thread _EventLoop;

        public SonosClient(string preferredHH)
        {
            try
            {
                string[] hhids = _Proxy.SearchForHHIDs();
                bool subscribed = false;
                if (0 < hhids.Length)
                    subscribed = _Proxy.SubscribeToHH(hhids[0]); // first = best.
                // if this fails, we should probably throw?

                _EventLoop = new Thread(new ThreadStart(PollForEvents));
                _EventLoop.Start();
                this.OnQueueChanged += delegate (string z){ MainForm.Trace("On QueueChanged: " + z); };
                this.OnTrackChanged += delegate (string z){ MainForm.Trace("On OnTrackChanged" + z); };
                this.OnTrackProgress += delegate (string z, int i) { MainForm.Trace("On OnTrackProgress: " + z + " " + i.ToString() ); };
                this.OnZoneGroupsChanged += delegate { MainForm.Trace("OnZoneGroupsChanged"); };
            }
            catch(XmlRpcFaultException fex) 
            {
                MainForm.Trace("Fault Response: " + fex.FaultCode + " " + fex.FaultString);
            }
        }

        public ZoneChange OnQueueChanged { get; set; }
        public ZoneChange OnTrackChanged { get; set; }
        public Tick OnTrackProgress { get; set; }
        public WorldChanged OnZoneGroupsChanged { get; set; }
        public Tick OnVolumeChanged { get; set; }
        public StateChanged OnPlayStateChanged { get; set; }
        public StateChanged OnMuteChanged { get; set; }

        public void StopListening()
        {
            _Stop = true;
            _EventLoop.Join();
        }

        // TODO: If this is slow, maintain and update a list.
        public List<Metadata> GetQueue(string zgid)
        {
            XmlRpcStruct[] items = _Proxy.GetQueue(zgid);
            List<Metadata> queue_items = new List<Metadata>();
            foreach (XmlRpcStruct x in items)
                queue_items.Add(new Metadata(x));
            return queue_items;
        }

        public List<string> GetZoneGroups()
        {
            return new List<string>(_Proxy.GetAllZoneGroups());
        }
        // TODO: We need to mangle the Uri to have the format http://playdar/sid/track.mp3?sid=blah
        // returns position the queue.
        public int Enqueue(Metadata m)
        {
            return _Proxy.EnqueueTrack(m.ToXmlRpc());
        }

        public int GetTrackTime(string zgid)
        {
            return _Proxy.GetTrackTime(zgid);
        }

        public Metadata GetTrackMetadata(string zgid)
        {
            return new Metadata(_Proxy.GetTrackMetadata(zgid));
        }

        public bool Next(string zgid) { return _Proxy.Next(zgid); }

        public bool Back(string zgid) { return _Proxy.Back(zgid); }

        public bool Play(string zgid, int index) { return _Proxy.Play(zgid, index); }

        public bool IsPlaying(string zgid) { return _Proxy.IsPlaying(zgid); }

        public void SetVolume(string zgid, int volume) { _Proxy.SetVolume(zgid, volume); } // volume [0,100]

        public int GetVolume(string zgid) { return _Proxy.GetVolume(zgid); }

        public bool SetMute(string zgid, bool mute) { return _Proxy.SetMute(zgid, mute); }

        public bool IsMuted(string zgid) { return _Proxy.IsMuted(zgid); }

        List<Event> _PollForEvents(int timeout)
        {
            XmlRpcStruct[] xmlEvents = _Proxy.PollForEvents(timeout);
            List<Event> events = new List<Event>();
            foreach (XmlRpcStruct x in xmlEvents)
                events.Add(new Event(x));
            return events;
        }

        void PollForEvents()
        {
            while (!_Stop)
            {
                List<Event> events = _PollForEvents(5);
                foreach (Event e in events)
                {
                    e.Dispatch(this);
                }
            }
        }

        [XmlRpcUrl(SonosServerUrl)]
        public interface ISonosClient : IXmlRpcProxy
        {
            [XmlRpcMethod("SearchForHHIDs")]
            string[] SearchForHHIDs();

            [XmlRpcMethod("SubscribeToHH")]
            bool SubscribeToHH(string hhid);

            [XmlRpcMethod("GetAllZoneGroups")]
            string[] GetAllZoneGroups();

            [XmlRpcMethod("GetQueue")]
            XmlRpcStruct[] GetQueue(string zgid);

            [XmlRpcMethod("GetCurrentTrackTime")]
            int GetTrackTime(string zgid);
            
            [XmlRpcMethod("GetCurrentTrackMD")]
            XmlRpcStruct GetTrackMetadata(string zgid);

            [XmlRpcMethod("EnqueueTrack")]
            int EnqueueTrack(XmlRpcStruct metadata);

            [XmlRpcMethod("Next")]
            bool Next(string zgid);

            [XmlRpcMethod("Back")]
            bool Back(string zgid);

            // True on success, False if nIxToPlay is out of range, or if szZGID is unknown. 
            // NB: This will return True if the currently playing track is already playing, and you pass in -1. 
            // It will restart the currently playing track if you pass in the index thereof. 
            [XmlRpcMethod("Play")]
            bool Play(string zgid, int queue_index);

            [XmlRpcMethod("IsPlaying")]
            bool IsPlaying(string zgid);

            [XmlRpcMethod("SetVolume")]
            void SetVolume(string zgid, int level);//0-100

            [XmlRpcMethod("GetVolume")]
            int GetVolume(string zgid);//0-100

            [XmlRpcMethod("SetMute")]
            bool SetMute(string zgid, bool mute);

            [XmlRpcMethod("IsMuted")]
            bool IsMuted(string zgid);

            [XmlRpcMethod("PollForEvents")]
            XmlRpcStruct[] PollForEvents(int timeout);
        }

    }
}
