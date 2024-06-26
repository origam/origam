#region license
/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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

namespace Origam.Schema.EntityModel;
public interface IXsltFunctionSchemaItemProvider : ISchemaItemProvider
{
}
public class XsltFunctionSchemaItemProvider : AbstractSchemaItemProvider, IXsltFunctionSchemaItemProvider
{
    public const string CategoryConst = "XsltFunctionCollection";
	
    public XsltFunctionSchemaItemProvider()
    {
        ChildItemTypes.Add(typeof(XsltFunctionCollection));
    }
    #region ISchemaItemProvider Members
    public override string RootItemType => CategoryConst;
    public override string Group => "BL";
    #endregion
    #region IBrowserNode Members
    public override string Icon => "icon_31_services.png";
    public override string NodeText
    {
        get => "Xslt Function Collections";
        set => base.NodeText = value;
    }
    public override string NodeToolTipText => null;
    #endregion
    #region ISchemaItemFactory Members
    
    #endregion
}
