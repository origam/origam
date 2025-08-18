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

using Origam.DA.Common;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Origam.Schema.GuiModel;
public enum ControlToolBoxVisibility {Nowhere, PanelDesigner, FormDesigner, PanelAndFormDesigner};
/// <summary>
/// Summary description for ControlItem.
/// </summary>
[SchemaItemDescription("Widget", "icon_widget.png")]
[HelpTopic("Widgets")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ControlItem : AbstractSchemaItem, ISchemaItemFactory
{
	public const string CategoryConst = "Control";
    public ControlItem() : base() { Init(); }
    public ControlItem(Guid schemaExtensionId) : base(schemaExtensionId) { Init(); }
    public ControlItem(Key primaryKey) : base(primaryKey) { Init(); }
    private void Init()
    {
        this.ChildItemTypes.Add(typeof(ControlPropertyItem));
        this.ChildItemTypes.Add(typeof(ControlStyleProperty));
    }
	#region Properties
	private ControlToolBoxVisibility _controlToolBoxVisibility;
    [XmlAttribute("toolboxVisibility")]
	public ControlToolBoxVisibility ControlToolBoxVisibility
	{
		get
		{
			return _controlToolBoxVisibility;
		}
		set
		{
			_controlToolBoxVisibility = value;
		}

	}
	private string _controlType;
    [XmlAttribute("typeName")]
	public string ControlType
	{
		get
		{
			return _controlType;
		}
		set
		{
			_controlType = value;
		}
	}
	private string _controlNamespace;
    [XmlAttribute("namespace")]
	public string ControlNamespace
	{
		get
		{
			return _controlNamespace;
		}
		set
		{
			_controlNamespace = value;
		}
	}
	private bool _isComplexType;
    [XmlAttribute("isComplex")]
	public bool IsComplexType
	{
		get
		{
			return _isComplexType;
		}
		set
		{
			_isComplexType = value;
		}
	}
	public Guid PanelControlSetId;
    [XmlReference("screenSection", "PanelControlSetId")]
    public PanelControlSet PanelControlSet
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.PanelControlSetId;
			
			try
			{
				return (PanelControlSet)this.PersistenceProvider.RetrieveInstance(typeof(PanelControlSet), key);
			}
			catch 
			{
				return null;
			}
		}
		set
		{
			if(value!=null)
			{
				this.PanelControlSetId = (Guid)value.PrimaryKey["Id"];
			}
			else
			{
				this.PanelControlSetId = System.Guid.Empty;
			}
		}
	}
    private bool _requestSaveAfterChangeAllowed;
    [XmlAttribute("requestSaveAfterChangeAllowed")]
    public bool RequestSaveAfterChangeAllowed
    {
        get
        {
            return _requestSaveAfterChangeAllowed;
        }
        set
        {
            _requestSaveAfterChangeAllowed = value;
        }
    }
	#endregion
#region Overriden ISchemaItem Members
	[Browsable(false)] 
	public override bool CanDelete
	{
		get
		{
			if(this.PanelControlSet == null)
                return true;
			else
				return false;
		}
	}
	public override string ItemType
	{
		get
		{
			return ControlItem.CategoryConst;
		}
	}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		// we will not add panel control set, because panel handles deletion of this control,
		// instead, ControlSetItem references directly the Panel definition
		//if(this.PanelControlSet != null) dependencies.Add(this.PanelControlSet);
		base.GetExtraDependencies (dependencies);
	}
	#endregion
}

