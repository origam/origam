using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Origam.Git
{
    public class XmlParser
    {
        private readonly string baseUrl = "https://api.bitbucket.org/2.0/repositories/";
        private static readonly List<object[]> RepositoryList = new List<object[]>() ;
        private readonly string[] GitTemplateUrl = new string[] 
        {   "cistic",
            "/?fields=values.name,values.links.clone,values.links.avatar",
        "/filehistory/master/README.md?fields=values.path,values.commit.date,values.links.self"};
        // "https://api.bitbucket.org/2.0/repositories/opensoftgitrepo/?fields=values.name,values.links.clone,values.links.avatar
        public XmlParser()
        {
        
        }

        public List<object[]> GetList()
        {
            if (RepositoryList.Count==0)
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
                            avatar= Image.FromStream(ms);
                        }
                        string link = cloneNode.FirstChild.InnerText;
                        string urlReadme = BuildUrl(name);
                        XmlDocument xmlReadme = (XmlDocument)GetData(urlReadme);
                        string readmeLink = xmlReadme.SelectSingleNode("(/ROOT/values)[1]/links/self").FirstChild.InnerText;
                        string readme = (string)GetData(readmeLink);
                        RepositoryList.Add(new object[] { avatar, name, link, readme });
                    }
                }
                catch (Exception)
                {
                    
                }
                IsLoaded = true;
            }
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
                return string.Format("{0}{1}/{2}{3}", baseUrl, GitTemplateUrl[0], name, GitTemplateUrl[2]);
            }
            return string.Format("{0}{1}{2}", baseUrl, GitTemplateUrl[0], GitTemplateUrl[1]);
        }
    }
}
