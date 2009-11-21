using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Xml;

namespace Sonar
{
    // See docs here: http://developer.echonest.com/pages/overview
    public static class EchoNest
    {
        static string BaseUrl = @"http://developer.echonest.com/api";
        static string _ApiKey = Credentials.EchoNestApiKey;

        #region Helper Classes
        public class Document
        {
            public Document(XmlNode n)
            {
                foreach (XmlNode child in n.ChildNodes)
                {
                    string value = child.InnerText;
                    string varname = child.Name;

                    switch (varname)
                    {
                        case "url": url = value; break;
                        case "name": name = value; break;
                        default: break;
                    }
                }
            }
            
            public string url { get; set; }
            public string name { get; set; }

            public static List<Document> ReadListFromXml(XmlNodeList x)
            {
                List<Document> docs = new List<Document>();
                foreach (XmlNode document in x)
                    docs.Add(new Document(document));

                return docs;
            }
        }

        public class Artist
        {
            public Artist(XmlNode n)
            {
                foreach (XmlNode child in n.ChildNodes)
                {
                    string value = child.InnerText;
                    string varname = child.Name;

                    switch (varname)    
                    {
                        case "id":
                            id = value; break;
                        case "name":
                            name = value; break;
                        case "hotttnesss":
                            hotttness = Convert.ToSingle(value); break;
                        case "familiarity":
                            familiarity = Convert.ToSingle(value); break;
                        case "rank":
                            rank = Convert.ToInt32(value); break; 
                        default:
                            MainForm.Trace("Don't know what to do with field: " + varname);
                            break; // pass
                    }
                }
            }

            public string id { get; set; }
            public string name { get; set; }
            public float hotttness { get; set; }
            public float familiarity { get; set; }
            public int rank { get; set; }

            public static List<Artist> ReadListFromXml(XmlNodeList x)
            {
                List<Artist> artists = new List<Artist>();
                foreach (XmlNode artist in x)
                    artists.Add(new Artist(artist));                 

                return artists;
            }
        }

        public class Parameter
        {
            public Parameter(string name, string value)
            {
                Name = name;
                Value = value;                
            }
                
            public Parameter(XmlNode n)
            {
                Name = n.Attributes[0].Value;
                Value = n.InnerText;
            }
            public string Name { get; set; }
            public string Value { get; set; }

            public static List<Parameter> ReadListFromXml(XmlNode n )
            {
                List<Parameter> parameters = new List<Parameter>();
                foreach (XmlNode param in n.ChildNodes)
                {
                    parameters.Add(new Parameter(param));
                }

                return parameters;
            }
        }
        public class Query
        {
            public Query(XmlNodeList x)
            {
                Parameters = Parameter.ReadListFromXml(x[0]);
            }

            public List<Parameter> Parameters {get; set;}
        }
        public class Status
        {
            public Status(XmlNodeList x)
            {
                XmlNode n = x[0];
                Code = Convert.ToInt32(n.FirstChild.Value);
                Message = n.FirstChild.NextSibling.InnerText;
            }
            public int Code { get; set; }
            public string Message { get; set; }
        }

        public class Response
        {
            public Response(string xml)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                version = doc.DocumentElement.Attributes[0].Value;
                query = new Query(doc.GetElementsByTagName("query"));
                artists = Artist.ReadListFromXml(doc.GetElementsByTagName("artist"));
                status = new Status(doc.GetElementsByTagName("status"));
                documents = Document.ReadListFromXml(doc.GetElementsByTagName("doc"));
            }

            // returns first artist on list, or null if there are none.
            public Artist GetArtist()
            {
                if (artists != null && artists.Count != 0)
                    return artists[0];

                return null;
            }

            public string version {get; set;}
            public Status status {get; set;}
            public Query query { get; set; }
            public List<Artist> artists { get; set; }
            public List<Document> documents { get; set; }
        }
        #endregion

        #region Privates
        static string MakeQuery(string function, string argname, string arg)
        {
            return string.Format("{0}/{1}?api_key={2}&{3}={4}&version=3", BaseUrl, function, _ApiKey, argname, arg);
        }

        static string GetArtistID(string id)
        {
            if (id.StartsWith("music://id.echonest.com/"))
                return id;
            // else

            Response r = SearchArtist(id);
            return r.GetArtist().id;
        }

        /// <summary>
        /// Executes an HTTP GET command and retrives the information.		
        /// </summary>
        /// <param Name="url">The URL to perform the GET operation</param>
        /// <returns>The response of the request, or null if we got 404 or nothing.</returns>
        static string ExecuteGetCommand(string url)
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

            //return null; // silence the "unreachable Code detected"
        }
        #endregion

        public static Image GetImage(string url)
        {
            WebClient wc = new WebClient();
            byte[] data = wc.DownloadData(url);
            MemoryStream ms = new MemoryStream(data);
            return new Bitmap(ms);
        }

        public static void Test()
        {
            Response r = SearchArtist("Radiohead");
            Artist a = r.GetArtist();
            float familiarity = GetFamiliarity(a.id).GetArtist().familiarity;
            float hotttnesss = GetHotttnesss(a.id).GetArtist().hotttness;
            List<Artist> similars = GetSimilar(a.id).artists;

            // Download all the images (this may be best expressed on more than one line)
            List<Image> images = GetImages(a.id).documents.ConvertAll<Image>(delegate(Document d) { return GetImage(d.url); });
            
        }

        public static Response GetImages(string id)
        {
            string url = MakeQuery("get_images", "id", GetArtistID(id));
            return new Response(ExecuteGetCommand(url));
        }

        public static Response GetSimilar(string id)
        {
            string url = MakeQuery("get_similar", "id", GetArtistID(id));
            return new Response(ExecuteGetCommand(url));
        }

        public static Response GetFamiliarity(string id)
        {
            string url = MakeQuery("get_familiarity", "id", GetArtistID(id));
            return new Response(ExecuteGetCommand(url));
        }

        public static Response GetHotttnesss(string id)
        {
            string url = MakeQuery("get_hotttnesss", "id", GetArtistID(id));
            return new Response(ExecuteGetCommand(url));
        }

        public static Response SearchArtist(string artist)
        {
            // Response looks like this:
            /*
            <?xml version="1.0" encoding="UTF-8"?>
            <response version="3">
	            <status>
		            <Code>0</Code>
		            <Message>Success</Message>
	            </status>
	            <query>
		            <parameter Name="api_key">HSHR3EZROVIQJYY43</parameter>
		            <parameter Name="query">evanessence</parameter>
		            <parameter Name="exact">N</parameter>
		            <parameter Name="sounds_like">Y</parameter>
		            <parameter Name="type">Name</parameter>
		            <parameter Name="rows">15</parameter>
	            </query>
	            <artists>
		            <artist>
			            <id>music://id.echonest.com/~/AR/ARVXU2X1187B9AE6D8</id>
			            <Name>Evanescence</Name></artist>
		            <artist>
			            <id>music://id.echonest.com/~/AR/AR0LH2G1187FB5C893</id>
			            <Name>Afonsinhos do Condado</Name>
		            </artist>
	            </artists>
            </response>  */
            string url = MakeQuery("search_artists", "query", artist);            
            return new Response(ExecuteGetCommand(url));
        }
    }
}
