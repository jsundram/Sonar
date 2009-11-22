using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel; // Requires .NET 3.0
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Sonar.Amazon;

// Mostly lifted from http://www.seesharpdot.net/?p=157
// Requires .NET 3.0
namespace Sonar
{

    public static class AmazonGateway
    {
        // This sucks, but can be fixed later.
        static string CleanString(string s)
        {
            string clean = string.Empty;           
            foreach (char c in s)
                clean += char.IsLetterOrDigit(c) ? c : ' ';

            return clean.Trim();
        }

        public static List<string> SearchAlbumArtUris(string keywords)
        {
            try
            {
                keywords = CleanString(keywords);

                ItemSearchRequest itemRequest = new ItemSearchRequest();
                itemRequest.Keywords = keywords;
                // http://docs.amazonwebservices.com/AWSECommerceService/2009-02-01/DG/index.html?ItemLookup.html
                itemRequest.SearchIndex = "Music"; // Other possibly valid choices:Classical, DigitalMusic, Mp3Downloads, Music, MusicTracks, 
                itemRequest.ResponseGroup = new string[] { "Images" }; // images only

                ItemSearch request = new ItemSearch();
                request.AWSAccessKeyId = Credentials.AmazonAccessKeyId;
                request.Request = new ItemSearchRequest[] { itemRequest };

                BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                binding.MaxReceivedMessageSize = int.MaxValue;

                AWSECommerceServicePortTypeClient client = new AWSECommerceServicePortTypeClient(binding, new EndpointAddress("https://webservices.amazon.com/onca/soap?Service=AWSECommerceService"));
                client.ChannelFactory.Endpoint.Behaviors.Add(new AmazonSigningEndpointBehavior(Credentials.AmazonAccessKeyId, Credentials.AmazonSecretKey));
                ItemSearchResponse response = client.ItemSearch(request);

                // Determine if item was found
                bool itemFound = ((response.Items[0].Item != null) && (response.Items[0].Item.Length > 0));

                if (itemFound)
                {
                    List<string> images = new List<string>();
                    foreach (Item currItem in response.Items[0].Item)
                    {
                        try
                        {
                            if (currItem != null && currItem.LargeImage != null)
                                images.Add(currItem.LargeImage.URL);
                        }
                        catch { }
                    }

                    return images;
                }
            }
            catch (Exception) {}

            return null;
        }


        public static List<System.Drawing.Image> SearchAlbumArt(string keywords)
        {
            try
            {
                keywords = CleanString(keywords);

                ItemSearchRequest itemRequest = new ItemSearchRequest();
                itemRequest.Keywords = keywords;
                // http://docs.amazonwebservices.com/AWSECommerceService/2009-02-01/DG/index.html?ItemLookup.html
                itemRequest.SearchIndex = "Music"; // Other possibly valid choices:Classical, DigitalMusic, Mp3Downloads, Music, MusicTracks, 
                itemRequest.ResponseGroup = new string[] { "Images" }; // images only

                ItemSearch request = new ItemSearch();
                request.AWSAccessKeyId = Credentials.AmazonAccessKeyId;
                request.Request = new ItemSearchRequest[] { itemRequest };

                BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
                binding.MaxReceivedMessageSize = int.MaxValue;

                AWSECommerceServicePortTypeClient client = new AWSECommerceServicePortTypeClient(binding, new EndpointAddress("https://webservices.amazon.com/onca/soap?Service=AWSECommerceService"));
                client.ChannelFactory.Endpoint.Behaviors.Add(new AmazonSigningEndpointBehavior(Credentials.AmazonAccessKeyId, Credentials.AmazonSecretKey));
                ItemSearchResponse response = client.ItemSearch(request);

                // Determine if book was found
                bool itemFound = ((response.Items[0].Item != null) && (response.Items[0].Item.Length > 0));

                if (itemFound)
                {
                    List<System.Drawing.Image> images = new List<System.Drawing.Image>();

                    foreach (Item currItem in response.Items[0].Item)
                    {
                        try
                        {
                            if (currItem != null && currItem.LargeImage != null)
                                images.Add(ConvertByteArrayToImage(GetBytesFromUrl(currItem.LargeImage.URL)));
                        }
                        catch { }
                    }

                    return images;
                }
            }
            catch (Exception) {}

            return null;
        }



        public static System.Drawing.Image ConvertByteArrayToImage(byte[] byteArray)
        {
            try
            {
                if (byteArray != null)
                {
                    MemoryStream ms = new MemoryStream(byteArray, 0, byteArray.Length);
                    ms.Write(byteArray, 0, byteArray.Length);
                    return System.Drawing.Image.FromStream(ms, true);
                }
            }
            catch { }

            return null;
        }

        static public byte[] GetBytesFromUrl(string url)
        {
            byte[] b;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            WebResponse myResp = myReq.GetResponse();
            Stream stream = myResp.GetResponseStream();

            using (BinaryReader br = new BinaryReader(stream))
            {
                //int i = (int)(stream.Length);

                b = br.ReadBytes(500000);

                br.Close();
            }

            myResp.Close();

            return b;
        }
    }



    public class AmazonSigningEndpointBehavior : IEndpointBehavior
    {
        private string accessKeyId = "";
        private string secretKey = "";

        public AmazonSigningEndpointBehavior(string accessKeyId, string secretKey)
        {
            this.accessKeyId = accessKeyId;
            this.secretKey = secretKey;
        }

        public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new AmazonSigningMessageInspector(accessKeyId, secretKey));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher) { return; }
        public void Validate(ServiceEndpoint serviceEndpoint) { return; }
        public void AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters) { return; }
    }

    public class AmazonSigningMessageInspector : IClientMessageInspector
    {
        private string accessKeyId = "";
        private string secretKey = "";

        public AmazonSigningMessageInspector(string accessKeyId, string secretKey)
        {
            this.accessKeyId = accessKeyId;
            this.secretKey = secretKey;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            // prepare the data to sign
            string operation = Regex.Match(request.Headers.Action, "[^/]+$").ToString();
            DateTime now = DateTime.UtcNow;
            string timestamp = now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string signMe = operation + timestamp;
            byte[] bytesToSign = Encoding.UTF8.GetBytes(signMe);

            // sign the data
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            HMAC hmacSha256 = new HMACSHA256(secretKeyBytes);
            byte[] hashBytes = hmacSha256.ComputeHash(bytesToSign);
            string signature = Convert.ToBase64String(hashBytes);

            // add the signature information to the request headers
            request.Headers.Add(new AmazonHeader("AWSAccessKeyId", accessKeyId));
            request.Headers.Add(new AmazonHeader("Timestamp", timestamp));
            request.Headers.Add(new AmazonHeader("Signature", signature));

            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }
    }

    public class AmazonHeader : MessageHeader
    {
        private string name;
        private string value;

        public AmazonHeader(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public override string Name { get { return name; } }
        public override string Namespace { get { return "http://security.amazonaws.com/doc/2007-01-01/"; } }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter xmlDictionaryWriter, MessageVersion messageVersion)
        {
            xmlDictionaryWriter.WriteString(value);
        }
    }
}
