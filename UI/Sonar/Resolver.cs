using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Web;
using System.ComponentModel;
using Jayrock.Json;

using Jayrock.Json.Conversion; // based on sample code from here: http://jayrock.googlegroups.com/attach/1ef3b7784efef53d/jayrock-t4428c08b81d43485.cs?view=1&part=2

namespace Sonar
{
    public class Resolver // I hardly know 'er!
    {
        // only working use case is localhost
        static string PlaydarBaseUrl = "http://localhost:60210/api/?"; // append something like method=stat and we're good to go.

        #region Json Classes
        public class Qid
        {
            public string qid { get; set; }
        }

        public class QueryResult
        {
            public string qid { get; set; }
            public int refresh_interval { get; set; }   // ms (Not sure how this is different than poll_interval)
            public int poll_interval { get; set; }      // ms (Not mentioned in docs, so don't use).
            public int poll_limit { get; set; }         // number of retries (max)
            public Query query { get; set; }            // 'query':{'artist':'The Beatles','album':'','track':'Anna'},
            public bool solved { get; set; }            //
            public Result[] results;
            public Result get_answer() { return solved ? results[0] : null; }
        }

        public class Query
        {
            public string artist { get; set; }
            public string album { get; set; }
            public string track { get; set; }
        }

        public class Result : IComparable<Result>
        {
            public string sid { get; set; }         // 'sid':'C3EA5CA7-8683-4D6B-A963-DBB82DD0B837',
            public string artist { get; set; }      // 'artist':'The Beatles',
            public string track { get; set; }       // 'track':'Anna',
            public string album { get; set; }       // 'album':'Please Please Me',
            public string mimetype { get; set; }    // 'mimetype':'audio/mpeg',
            public float score { get; set; }        // 'score':1.0,
            public int duration { get; set; }       // 'duration':180,
            public int bitrate { get; set; }        // 'bitrate':320,
            public int size { get; set; }           // 'size':7223637,
            public string source { get; set; }      // 'source':'WIN-F1DNH76QUS3'
            public string get_play_url() { return "http://localhost:60210/sid/" + sid; }
            public override string ToString()
            {
                return track + " by " + artist + " (" + album + ")";
            }

            #region IComparable<Result> Members

            public int CompareTo(Result other)
            {
                return score.CompareTo(other.score);
            }

            #endregion
        }
        #endregion


        public static string Stat()
        {
            return ExecuteGetCommand(PlaydarBaseUrl + "method=stat");
        }

        public static string UrlEncode(string s)
        {
            return HttpUtility.UrlPathEncode(s);
        }

        public delegate void OnResolveProgress(List<Result> results, bool complete);

        // TODO: rewrite this to use a background worker like ResolveWorker. That way the caller isn't hanging.
        public static string Resolve(string artist, string track, OnResolveProgress callback)
        {
            string qid = _Resolve(artist, track);
            Sonar.Trace("Attempting to resolve " + artist + " " + track + " " + "qid=" + qid);
            System.Threading.Thread.Sleep(1000);

            QueryResult r = GetResults(qid);
            callback(new List<Result>(r.results), r.solved);
            int poll_count = 1;
            while (!r.solved && poll_count < r.poll_limit)
            {
                System.Threading.Thread.Sleep(r.refresh_interval);
                r = GetResults(qid);
                callback(new List<Result>(r.results), r.solved);
                poll_count += 1;
            }

            callback(new List<Result>(r.results), true);

            return qid;
        }
        // For use by ResolveWorker. Seemed to make more sense to have it here.
        // If we don't support cancellation, can probably use Resolve(artist, track) below.
        public static string Resolve(BackgroundWorker w, SocialItem i)
        {
            string qid = _Resolve(i.Artist, i.Track);
            Sonar.Trace("Attempting to resolve " + i.Artist + " " + i.Track + " " + "qid=" + qid);
            System.Threading.Thread.Sleep(1000);

            QueryResult r = GetResults(qid);
            int poll_count = 1;

            while (!w.CancellationPending && r != null && !r.solved && poll_count < r.poll_limit)
            {
                System.Threading.Thread.Sleep(r.refresh_interval);
                r = GetResults(qid);
                poll_count += 1;
            }
            Resolver.Result answer = r != null ? r.get_answer() : null;

            return answer != null ? answer.get_play_url() : "";
        }
        /// <summary>
        /// Returns a playable url for the given input, if possible. 
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        public static string Resolve(string artist, string track)
        {
            string qid = _Resolve(artist, track);
            Sonar.Trace("Attempting to resolve " + artist + " " + track + " " + "qid=" + qid);
            System.Threading.Thread.Sleep(1000);

            QueryResult r = GetResults(qid);
            int poll_count = 1;
            while (!r.solved && poll_count < r.poll_limit)
            {
                System.Threading.Thread.Sleep(r.refresh_interval);
                r = GetResults(qid);
                poll_count += 1;
            }

            Resolver.Result answer = r.get_answer();

            return answer != null ? answer.get_play_url() : "";
        }

        private static string _Resolve(string artist, string track)
        {
            //string method = string.Format("method=resolve&artist={0}&album=&track={1}&qid={2}", UrlEncode(artist), UrlEncode(track), qid);
            string method = string.Format("method=resolve&artist={0}&album=&track={1}", UrlEncode(artist), UrlEncode(track));
            string json_qid = ExecuteGetCommand(PlaydarBaseUrl + method);
            Qid qid = (Qid)JsonConvert.Import(typeof(Qid), json_qid);
            return qid.qid;
        }


        public static QueryResult GetResults(string qid)
        {
            string method = "method=get_results&qid=" + qid;
            try
            {
                string result = ExecuteGetCommand(PlaydarBaseUrl + method);
                // We expect the result to look something like this:
                /*
                @"{
                    'qid':'1',
                    'refresh_interval':1000,
                    'poll_interval':1000,
                    'poll_limit':6,
                    'query':{'artist':'The Beatles','album':'','track':'Anna'},
                    'solved':true,
                    'results':[
                        {
                            'sid':'C3EA5CA7-8683-4D6B-A963-DBB82DD0B837',
                            'artist':'The Beatles',
                            'track':'Anna',
                            'album':'Please Please Me',
                            'mimetype':'audio/mpeg',
                            'score':1.0,
                            'duration':180,
                            'bitrate':320,
                            'size':7223637,
                            'source':'WIN-F1DNH76QUS3'
                        }
                    ]
                }";*/

                return (QueryResult)JsonConvert.Import(typeof(QueryResult), result);
            }
            catch (Exception e)
            {
                Sonar.Trace("Unexpected Exception during Resolution of " + qid + ": " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Executes an HTTP GET command and retrives the information.		
        /// </summary>
        /// <param name="url">The URL to perform the GET operation</param>
        /// <returns>The response of the request, or null if we got 404 or nothing.</returns>
        protected static string ExecuteGetCommand(string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    using (Stream stream = client.OpenRead(url))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                catch (WebException ex)
                {
                    // Handle HTTP 404 errors gracefully and return a null string to indicate there is no content.
                    if (ex.Response is HttpWebResponse)
                    {
                        if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        {
                            return null;
                        }
                    }

                    throw ex;
                }
            }

            //return null; // silence the "unreachable code detected"
        }
    }
}
