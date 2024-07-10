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
using System.Windows.Forms;
using System.ComponentModel;

using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for CollapsiblePanel.
/// </summary>
public class CollapsiblePanel : TabPage
{
	private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
	private int _indentLevel = 0;
	private bool _isHeightFixed = false;
	private bool _isOpen = false;
	public CollapsiblePanel() : base()
	{
	}
	public int IndentLevel
	{
		get
		{
			return _indentLevel;
		}
		set
		{
			_indentLevel = value;
		}
	}
	public bool IsHeightFixed
	{
		get
		{
			return _isHeightFixed;
		}
		set
		{
			_isHeightFixed = value;
		}
	}
	public bool IsOpen
	{
		get
		{
			return _isOpen;
		}
		set
		{
			_isOpen = value;
		}
	}
	private Guid _styleId;
	[Browsable(false)]
	public Guid StyleId
	{
		get
		{
			return _styleId;
		}
		set
		{
			_styleId = value;
		}
	}
	[TypeConverter(typeof(StylesConverter))]
	public UIStyle Style
	{
		get
		{
			return (UIStyle)_persistence.SchemaProvider.RetrieveInstance(typeof(UIStyle), new ModelElementKey(this.StyleId));
		}
		set
		{
			this.StyleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		}
	}
}
