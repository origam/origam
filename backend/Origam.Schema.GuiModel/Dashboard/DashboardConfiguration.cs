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
using System.Xml;
using System.Xml.Serialization;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Summary description for DashboardConfiguration.
/// </summary>
[Serializable()]
[XmlRoot(elementName: "configuration")]
public class DashboardConfiguration
{
    private DashboardConfigurationItem[] _items;

    public DashboardConfiguration() { }

    public static DashboardConfiguration Deserialize(XmlDocument doc)
    {
        if (doc == null)
        {
            return new DashboardConfiguration();
        }

        XmlSerializer ser = new XmlSerializer(type: typeof(DashboardConfiguration));
        XmlNodeReader reader = new XmlNodeReader(node: doc);
        return (DashboardConfiguration)ser.Deserialize(xmlReader: reader);
    }

    [XmlElement(elementName: "item", type: typeof(DashboardConfigurationItem))]
    public DashboardConfigurationItem[] Items
    {
        get { return _items; }
        set { _items = value; }
    }
}
