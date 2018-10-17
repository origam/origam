using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace Origam.Server.Handlers
{
    public class GeoLocationHandler : IHttpHandler
    {
        public void ProcessRequest (HttpContext context) 
        {
            if (context.Request.Form.Get("searchInput") == null)
            {
                throw new Exception("No request string received.");
            }
            HttpResponse response = context.Response;
            response.ContentType = "text/xml";
            WriteResponseHeader(response);
            using (WebClient searchClient = new WebClient())
            {
                searchClient.Encoding = System.Text.Encoding.UTF8;
                String searchResponse; 
                try
                {
                    String serviceUrl 
                        = ((NameValueCollection)System.Configuration.ConfigurationManager
                        .GetSection("geoLocationSettings")).Get("serviceUrl");
                    String searchInput = context.Request.Form.Get("searchInput");
                    StringBuilder urlBuilder = new StringBuilder(serviceUrl);
                    urlBuilder.Append("?q=");
                    urlBuilder.Append(searchInput.Replace(" ", "+"));
                    urlBuilder.Append("&format=xml");
                    searchResponse = searchClient.DownloadString(urlBuilder.ToString());
                    WriteResults(response, searchResponse);
                }
                catch (Exception exception)
                {
                    WriteError(response, exception);
                    WriteResponseFooter(response);
                    return;
                }
            }
            WriteResponseFooter(response);
        }

        private void WriteResults(HttpResponse response, String results)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(results)))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "place")
                            {
                                response.Write("<place boundingBox=\"");
                                response.Write(reader.GetAttribute("boundingbox"));
                                response.Write("\" lat=\"");
                                response.Write(reader.GetAttribute("lat"));
                                response.Write("\" lon=\"");
                                response.Write(reader.GetAttribute("lon"));
                                response.Write("\" displayName=\"");
                                response.Write(reader.GetAttribute("display_name"));
                                response.Write("\"/>");
                            }
                            break;
                    }
                }
            }
        }

        private void WriteError(HttpResponse response, Exception exception)
        {
            response.Write("<error>"); 
            response.Write(exception.Message); 
            response.Write("</error>");
        }

        private void WriteResponseHeader (HttpResponse response)
        {
            response.Write("<?xml version=\"1.0\"?><results>");
        }

        private void WriteResponseFooter (HttpResponse response)
        {
            response.Write("</results>");
        }
     
        public bool IsReusable 
        {
            get 
            {
                return false;
            }
        }

    }
}
