using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using Jayrock.Json.Conversion;
using System.Drawing;

namespace Sonar
{
    // http://lab.playme.com/api_overview
    public class PlayMe
    {
        #region JSON Classes
        public class Artist
        {
            public string artistCode { get; set; }
            public string name { get; set; }
            public override string ToString()
            {
                return name;
            }
        }

        public static Image GetImage(string url)
        {
            WebClient wc = new WebClient();
            byte[] data = wc.DownloadData(url);
            MemoryStream ms = new MemoryStream(data);
            return new Bitmap(ms);
        }

        public class Album
        {
            public string albumCode { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string numTracks { get; set; }
            public string publicationYear { get; set; }
            public IDictionary images { get; set; }
            public Artist artist { get; set; }
            public List<Image> RetrieveImages()
            {
                List<Image> i = new List<Image>();
                foreach (string img_url in this.images.Values)
                    i.Add(GetImage(img_url));
                return i;
            }
            public override string ToString()
            {
                return name;
            }
        }

        public class AlbumResponse
        {
            public Album[] albums { get; set; }
            public string available { get; set; }
            public string page { get; set; }
        }
        public class ArtistResponse
        {
            public Artist[] artists { get; set; }
            public string available { get; set; }
            public string page { get; set; }
            public Artist GetArtist(string name)
            {
                foreach (Artist a in artists)
                {
                    if (a.name == name)
                        return a;
                }
                return null;
            }
        }
        public class AlbumResponseWrapper
        {
            public AlbumResponse response { get; set; }
        }
        public class ArtistResponseWrapper
        {
            public ArtistResponse response { get; set; }
        }

        #endregion
        static string BaseUrl = @"http://api.playme.com/";
        
        public static AlbumResponse SearchAlbum(string keywords)
        {
            string url = string.Format("{0}album.search?query={1}&country=us&format=json&apikey={2}", BaseUrl, UrlEncode(keywords), Credentials.PlayMeApiKey);
            string result = ExecuteGetCommand(url);

            AlbumResponseWrapper r =  (AlbumResponseWrapper)JsonConvert.Import(typeof(AlbumResponseWrapper), result);
            return r.response;
        }

        public static AlbumResponse GetAlbum(string name)
        {
            string url = string.Format("{0}album.searchByName?query={1}&sort=desc&country=us&format=json&apikey={2}", BaseUrl, UrlEncode(name), Credentials.PlayMeApiKey);
            string result = ExecuteGetCommand(url);

            AlbumResponseWrapper r = (AlbumResponseWrapper)JsonConvert.Import(typeof(AlbumResponseWrapper), result);
            return r.response;
        }

        public static ArtistResponse SearchArtist(string name)
        {
            string url = string.Format("{0}artist.searchByName?query={1}&sort=desc&country=us&format=json&apikey={2}", BaseUrl, UrlEncode(name), Credentials.PlayMeApiKey);
            string result = ExecuteGetCommand(url);

            ArtistResponseWrapper r = (ArtistResponseWrapper)JsonConvert.Import(typeof(ArtistResponseWrapper), result);
            return r.response;
        }

        public static Album GetAlbum(string artist, string name)
        {
            AlbumResponse albums = GetAlbumsForArtist(artist);
            foreach (Album a in albums.albums)
                if (a.name == name)
                    return a;
            return null;
        }
        public static AlbumResponse GetAlbumsForArtist(string name)
        {
            ArtistResponse ar = SearchArtist(name);
            Artist a = ar.GetArtist(name);
            if (a == null)
                return null;

            return GetAlbumsForArtist(a);
        }
        public static AlbumResponse GetAlbumsForArtist(Artist a)
        {
            string url = string.Format("{0}artist.getAlbums?artistCode={1}&sort=desc&country=us&format=json&apikey={2}", BaseUrl, a.artistCode, Credentials.PlayMeApiKey);
            string result = ExecuteGetCommand(url);

            AlbumResponseWrapper r = (AlbumResponseWrapper)JsonConvert.Import(typeof(AlbumResponseWrapper), result);
            return r.response;
        }

        #region Utility (duplicated)
        public static string UrlEncode(string s)
        {
            return HttpUtility.UrlPathEncode(s);
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
        #endregion
    }
}
