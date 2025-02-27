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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Origam.Schema.EntityModel;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers;

public class XslTransformationScriptContainer : UpgradeScriptContainer
{
    public override string FullTypeName { get; } 
        = typeof(XslTransformation).FullName;

    public override string[] OldPropertyXmlNames { get; } = {};
    public override List<string> OldFullTypeNames { get; }

    public XslTransformationScriptContainer() 
    {
        upgradeScripts.Add(
            new UpgradeScript(
                fromVersion: new Version("6.0.0"),
                toVersion: new Version("6.0.1"),
                transformation: (node, doc) =>
                {
                    XNamespace newNameSpace= "http://schemas.origam.com/Origam.Schema.EntityModel.XslTransformation/6.0.1";
                    node.Attributes()
                        .FirstOrDefault(x => x.Name.LocalName == "engineType")
                        ?.Remove();
                }));
    }
}