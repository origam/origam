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
using System.Xml.Linq;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers;

public class DataStructureColumnScriptContainer : UpgradeScriptContainer
{
    public override string FullTypeName { get; } =
        typeof(Origam.Schema.EntityModel.DataStructureColumn).FullName;

    public override List<string> OldFullTypeNames { get; }
    public override string[] OldPropertyXmlNames { get; }

    public DataStructureColumnScriptContainer()
    {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"),
                new Version("6.0.1"),
                (node, doc) =>
                {
                    XNamespace oldNameSpace= "http://schemas.origam.com/Origam.Schema.EntityModel.DataStructureColumn/6.0.0";
                    XNamespace newNameSpace= "http://schemas.origam.com/Origam.Schema.EntityModel.DataStructureColumn/6.0.1";
                    var mappingAttribute = node.Attribute(oldNameSpace+"xmlMappingType");
                    if (mappingAttribute != null && mappingAttribute.Value == "Hidden")
                    {
                        mappingAttribute.Value = "Default";
                        node.Add(new XAttribute(newNameSpace+"hideInOutput", "true"));
                    }
                }
            ));
        }
}