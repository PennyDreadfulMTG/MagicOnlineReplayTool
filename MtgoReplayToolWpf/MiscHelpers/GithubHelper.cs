using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MtgoReplayToolWpf.MiscHelpers
{
    internal class GithubHelper
    {
        class Release
        {
            public string url;
            public Asset[] assets;
        }

        public class Asset
        {
            public string name;
            public string browser_download_url;

            public string GetContents()
            {
                var myWebRequest = (HttpWebRequest)WebRequest.Create(this.browser_download_url);
                myWebRequest.UserAgent = "MagicOnlineReplayTool";
                var myWebResponse = myWebRequest.GetResponse();
                var responseStream = myWebResponse.GetResponseStream();

                var sreader = new StreamReader(responseStream);
                var responseString = sreader.ReadToEnd();
                return responseString;
            }

            internal void Download(string deckPath)
            {
                var myWebRequest = (HttpWebRequest)WebRequest.Create(this.browser_download_url);
                myWebRequest.UserAgent = "MagicOnlineReplayTool";
                var myWebResponse = myWebRequest.GetResponse();
                var responseStream = myWebResponse.GetResponseStream();
    
                using (var file = File.OpenWrite(Path.Combine(deckPath, this.name)))
                {
                    responseStream.CopyTo(file);
                }
            }
        }

        public static Asset[] FetchReleaseAssets(string org, string repo)
        {
            var myWebRequest = (HttpWebRequest)WebRequest.Create($"https://api.github.com/repos/{org}/{repo}/releases/latest");
            myWebRequest.UserAgent = "MagicOnlineReplayTool";
            var myWebResponse = myWebRequest.GetResponse();
            var responseStream = myWebResponse.GetResponseStream();

            var sreader = new StreamReader(responseStream);
            var responseString = sreader.ReadToEnd();
            //JObject.Parse(responseString);
            var release = JsonConvert.DeserializeObject<Release>(responseString);
            return release.assets;
        }
    }
}
