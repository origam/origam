using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Origam.Git
{
    public class WebGitXmlParser
    {
        // "https://api.bitbucket.org/2.0/repositories/cistic/?fields=values.name,values.links.clone,values.links.avatar
        private readonly string baseUrl = "https://api.bitbucket.org/2.0/repositories/";
        private readonly string readmeSubLink = "/filehistory/master/README.md?fields=values.path,values.commit.date,values.links.self";
        private readonly string fieldsLink = "/?fields=values.name,values.links.clone,values.links.avatar";
        private readonly string gitUserLink = "cistic";
        private static readonly List<WebGitData> RepositoryList = new List<WebGitData>();

        public WebGitXmlParser()
        {

        }

        public List<WebGitData> GetList()
        {
            if (RepositoryList.Count == 0)
            {
                try
                {
                    string url = BuildUrl(null);
                    XmlDocument templatesJson = (XmlDocument)GetData(url);
                    XmlNodeList nodeList = templatesJson.SelectNodes("/ROOT/values");
                    foreach (XmlNode node in nodeList)
                    {
                        string name = node.FirstChild.InnerText;
                        XmlNode cloneNode = node.SelectSingleNode("(links/clone)[1]");
                        XmlNode imageNode = node.SelectSingleNode("links/avatar");
                        Image avatar = null;
                        using (var ms = new MemoryStream((byte[])GetData(imageNode.FirstChild.InnerText)))
                        {
                            avatar = Image.FromStream(ms);
                        }
                        string link = cloneNode.FirstChild.InnerText;
                        string urlReadme = BuildUrl(name);
                        XmlDocument xmlReadme = (XmlDocument)GetData(urlReadme);
                        string readmeLink = xmlReadme.SelectSingleNode("(/ROOT/values)[1]/links/self").FirstChild.InnerText;
                        string readme = (string)GetData(readmeLink);
                        WebGitData gitData = new WebGitData(avatar, name, link, readme);
                        RepositoryList.Add(gitData);
                    }
                }
                catch (Exception)
                {

                }
            }
            IsLoaded = true;
            return RepositoryList;
        }

        public Boolean IsLoaded { get; set; } = false;

        private object GetData(string url)
        {
            return HttpTools.SendRequest(url, method: "get");
        }

        private string BuildUrl(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                //readme Link
                return string.Format("{0}{1}/{2}{3}", baseUrl, gitUserLink, name, readmeSubLink);
            }
            //list of repository
            return string.Format("{0}{1}{2}", baseUrl, gitUserLink, fieldsLink);
        }
    }
}
