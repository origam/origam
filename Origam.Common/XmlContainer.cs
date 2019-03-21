#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
﻿using System.Net.Mime;
using System.Xml;

namespace Origam
{
    public class XmlContainer : IXmlContainer
    {
        public XmlContainer()
        {
            Xml = new XmlDocument();
        }

        public XmlContainer(XmlDocument xmlDocument)
        {
            Xml = xmlDocument;
        }

        public XmlContainer(string xmlString)
        {
            Xml = new XmlDocument();
            if (!string.IsNullOrEmpty(xmlString))
            {
                Xml.LoadXml(xmlString);
            }
        }

        public XmlDocument Xml { get; }

        public void Load(XmlReader xmlReader)
        {
            Xml.Load(xmlReader);
        }

        public void LoadXml(string xmlString)
        {
            Xml.LoadXml(xmlString);
        }

        public object Clone()
        {
            return Xml.Clone();
        }
    }
}