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
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for BasePanel.
/// </summary>
public class BasePanel : System.Windows.Forms.UserControl, IDataStructureReference
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;
	private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;

	public BasePanel()
	{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
			if( disposing )
			{
				OrigamMetadata = null;
				components?.Dispose();
			}
			base.Dispose( disposing );
		}

	#region Component Designer generated code
	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
			components = new System.ComponentModel.Container();
		}
	#endregion

	#region Properties

	public AbstractSchemaItem OrigamMetadata { get; set; }

	[Browsable(false)]
	public Guid IndependentDataSourceId { get; set; }

	[Category("Independent Data Source")]
	[TypeConverter(typeof(DataStructureConverter))]
	public DataStructure IndependentDataSource
	{
		get
		{
				return (DataStructure)this.OrigamMetadata.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.IndependentDataSourceId));
			}
		set
		{
				this.IndependentDataSourceId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
	}

	private Guid _independentDataSourceMethodId;

	[Browsable(false)]
	public Guid IndependentDataSourceMethodId
	{
		get
		{
				return _independentDataSourceMethodId;
			}
		set
		{
				_independentDataSourceMethodId = value;
			}
	}

	[Category("Independent Data Source")]
	[TypeConverter(typeof(DataStructureReferenceMethodConverter))]
	public DataStructureMethod IndependentDataSourceMethod
	{
		get
		{
				return (DataStructureMethod)this.OrigamMetadata.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.IndependentDataSourceMethodId));
			}
		set
		{
				this.IndependentDataSourceMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
	}

	private Guid _independentDataSourceSortId;

	[Browsable(false)]
	public Guid IndependentDataSourceSortId
	{
		get
		{
				return _independentDataSourceSortId;
			}
		set
		{
				_independentDataSourceSortId = value;
			}
	}

	[Category("Independent Data Source")]
	[TypeConverter(typeof(DataStructureReferenceSortSetConverter))]
	public DataStructureSortSet IndependentDataSourceSort
	{
		get
		{
				return (DataStructureSortSet)this.OrigamMetadata.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.IndependentDataSourceSortId));
			}
		set
		{
				this.IndependentDataSourceSortId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
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
	#endregion

	#region IDataStructureReference Members

	[Browsable(false)]
	public DataStructureMethod Method
	{
		get
		{
				return this.IndependentDataSourceMethod;
			}
		set
		{
				throw new InvalidOperationException();
			}
	}

	[Browsable(false)]
	public DataStructure DataStructure
	{
		get
		{
				return this.IndependentDataSource;
			}
		set
		{
				throw new InvalidOperationException();
			}
	}

	#endregion
}