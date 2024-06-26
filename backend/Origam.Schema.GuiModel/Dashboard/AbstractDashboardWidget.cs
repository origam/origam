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
using System.ComponentModel;
using System.Collections;
using System.Xml.Serialization;

using Origam.DA.ObjectPersistence;


namespace Origam.Schema.GuiModel;
[XmlModelRoot(CategoryConst)]
public abstract class AbstractDashboardWidget : AbstractSchemaItem
{
	public const string CategoryConst = "DashboardWidget";
	public AbstractDashboardWidget() : base() {Init();}
	public AbstractDashboardWidget(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public AbstractDashboardWidget(Key primaryKey) : base(primaryKey) {Init();}
	private void Init()
	{
		this.ChildItemTypes.Add(typeof(DashboardWidgetParameter));
	}
	[Browsable(false)]
	public override bool UseFolders
	{
		get
		{
			return false;
		}
	}
	
	public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
	{
		return newNode is DashboardWidgetFolder || newNode is DashboardWidgetsSchemaItemProvider;
	}
	#region Properties
	private string _caption = "";
	[Category("User Interface")]
	[StringNotEmptyModelElementRule()]
	[Localizable(true)]
    [XmlAttribute("label")]
	public string Caption
	{
		get
		{
			return _caption;
		}
		set
		{
			_caption = value;
		}
	}
	private string _roles;
	[Category("Security")]
	[XmlAttribute("roles")]
    public string Roles
	{
		get
		{
			return _roles;
		}
		set
		{
			_roles = value;
		}
	}
	private string _features;
	[XmlAttribute("features")]
    public string Features
	{
		get
		{
			return _features;
		}
		set
		{
			_features = value;
		}
	}
	public abstract ArrayList Properties {get;}
	public override string Icon
	{
		get
		{
			return "29";
		}
	}
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	#endregion			
}
