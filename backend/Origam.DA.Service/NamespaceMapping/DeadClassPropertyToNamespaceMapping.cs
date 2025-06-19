#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using System.Linq;
using System.Xml.Linq;
using Origam.DA.Common;
using Origam.Extensions;

namespace Origam.DA.Service.NamespaceMapping;
class DeadClassPropertyToNamespaceMapping : IPropertyToNamespaceMapping
{
    private string xmlNamespaceName;    
    public DeadClassPropertyToNamespaceMapping(string fullTypeName, Version version)
    {
        string typeName = fullTypeName.Split(".").Last();
        NodeNamespace =  OrigamNameSpace.CreateOrGet(fullTypeName, version).StringValue;
        xmlNamespaceName = XmlNamespaceTools.GetXmlNamespaceName(typeName);
    }
    public XNamespace NodeNamespace { get; }
    public XNamespace GetNamespaceByXmlAttributeName(string xmlAttributeName)
    {
        return NodeNamespace;
    }
    public void AddNamespacesToDocumentAndAdjustMappings(OrigamXDocument document)
    {
        xmlNamespaceName = document.AddNamespace(
            nameSpaceShortCut: xmlNamespaceName, 
            nameSpace: NodeNamespace.ToString());
    }
}
