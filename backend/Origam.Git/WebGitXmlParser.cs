#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using Origam.Service.Core;
using static Origam.NewProjectEnums;

namespace Origam.Git
{
    public class WebGitXmlParser
    {
        // "https://api.bitbucket.org/2.0/repositories/cistic/?fields=values.name,values.links.clone,values.links.avatar
        private readonly string baseUrl = "https://api.bitbucket.org/2.0/repositories/";
        private readonly string readmeSubLink = "/filehistory/master/README.md?fields=values.path,values.commit.date,values.links.self";
        private readonly string fieldsLink = "/?fields=values.name,values.links.clone,values.links.avatar";
        private readonly string gitUserLink = "cistic";
        private List<WebGitData> RepositoryList = new List<WebGitData>();

        public List<WebGitData> GetList()
        {
                string url = BuildUrl(null);
                XmlDocument templatesJson = ((XmlContainer)GetData(url)).Xml;
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
                    XmlDocument xmlReadme = ((XmlContainer)GetData(urlReadme)).Xml;
                    string readmeLink = xmlReadme.SelectSingleNode("(/ROOT/values)[1]/links/self").FirstChild.InnerText;
                    string readme = (string)GetData(readmeLink);
                    WebGitData gitData = new WebGitData(avatar, name, link, readme,TypeTemplate.Template);
                    RepositoryList.Add(gitData);
                }
            return RepositoryList;
        }

        public Boolean IsLoaded { get; set; } = false;

        private object GetData(string url)
        {
            return HttpTools.Instance.SendRequest(
                    new Request(url: url, method: "GET")
                ).Content;
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
