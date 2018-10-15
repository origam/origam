using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;

namespace Origam.Sharepoint.ListsWebService
{
    // mock - remove when import sharepoint asmx web service successfully
    public class Lists
    {
        public string Url { get; set; }
        public CredentialCache Credentials { get; set; }

        public XmlNode GetListItems(string a, string b, object c, object d, string e, object f)
        {
            return null;
        }
        public XmlNode UpdateListItems (object a, object b)
        {
            return null;
        }

        public XmlNode GetList(object a)
        {
            return null;
        }
    }
}
