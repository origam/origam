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

using System.Linq;
using System.Xml.Linq;

namespace Origam.Extensions;

public static class XExtensions
{
    public static void RenameAttribute(
        this XElement element,
        string oldLocalName,
        string newLocalName
    )
    {
        var attribute = element
            .Attributes()
            .FirstOrDefault(attr => attr.Name.LocalName == oldLocalName);
        if (attribute == null)
        {
            return;
        }

        XNamespace nameSpace = attribute.Name.Namespace;
        string value = attribute.Value;
        attribute.Remove();

        element.SetAttributeValue(nameSpace.GetName(newLocalName), value);
    }
}
