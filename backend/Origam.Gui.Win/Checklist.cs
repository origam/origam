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
using System.Collections;
using System.ComponentModel;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for Checklist.
/// </summary>
public class Checklist : BaseCaptionControl, IOrigamMetadataConsumer
{
	private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
	private System.Windows.Forms.ColorDialog colorDialog1;
	private System.Windows.Forms.TextBox textBox1;
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public Checklist()
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
				if(components != null)
				{
					components.Dispose();
				}
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
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 	 // textBox1
            // 	 this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(328, 22);
            this.textBox1.TabIndex = 0;
            // 	 // Checklist
            // 	 this.Controls.Add(this.textBox1);
            this.Name = "Checklist";
            this.Size = new System.Drawing.Size(328, 24);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	#endregion

	public override string DefaultBindableProperty
	{
		get
		{
				return "Value";
			}
	}

	#region Properties
	private Guid _lookupId;
	[Browsable(false)]
	public Guid LookupId
	{
		get
		{
				return _lookupId;
			}
		set
		{
				_lookupId = value;
			}
	}

	public string Value
	{
		get
		{
				return null;
			}
		set
		{
			}
	}

	private bool _readOnly = false;
	public bool ReadOnly
	{
		get
		{
				return _readOnly;
			}
		set
		{
				_readOnly = value;
			}
	}

	private int _columnWidth = 100;
	public int ColumnWidth
	{
		get
		{
				return _columnWidth;
			}
		set
		{
				_columnWidth = value;
			}
	}

	ColumnParameterMappingCollection _parameterMappings = new ColumnParameterMappingCollection();
	[TypeConverter(typeof(ColumnParameterMappingCollectionConverter))]
	public ColumnParameterMappingCollection ParameterMappings
	{
		get
		{
				return _parameterMappings;
			}
	}


	[TypeConverter(typeof(DataLookupConverter))]
	public AbstractDataLookup DataLookup
	{
		get
		{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.LookupId;

				return (AbstractDataLookup)_persistence.SchemaProvider.RetrieveInstance(typeof(AbstractDataLookup), key);
			}
		set
		{
				if(value == null)
				{
					this.LookupId = Guid.Empty;

					ClearMappingItems();
				}
				else
				{
					// if same as before, no action is needed
					if(this.LookupId == (Guid)value.PrimaryKey["Id"])
					{
						return;
					}
					
					this.LookupId = (Guid)value.PrimaryKey["Id"];

					ClearMappingItems();
					CreateMappingItemsCollection();
				}
			}
	}
	#endregion

	#region methods
	private bool _itemsLoaded = false;
	private void ClearMappingItems()
	{
			try
			{
				if(!_itemsLoaded)
					return;

				ArrayList col = new ArrayList(_origamMetadata.ChildItemsByType(ColumnParameterMapping.CategoryConst));

				foreach(ColumnParameterMapping mapping in col)
				{
					mapping.IsDeleted = true;
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("AsDropDown:ERROR=>" + ex.ToString());
			}
		}

	public void CreateMappingItemsCollection()
	{
			if(!_itemsLoaded)
				return;

			if(this.DataLookup != null)
			{
				foreach(DictionaryEntry entry in this.DataLookup.ParameterReferences)
				{
					string parameterName = entry.Key.ToString();
					ColumnParameterMapping mapping = _origamMetadata
						.NewItem<ColumnParameterMapping>(
							_origamMetadata.SchemaExtensionId, null);
					mapping.Name = parameterName;
				}
			}

			//Refill Parameter collection (and dictionary)
			FillParameterCache(this._origamMetadata as ControlSetItem);
		}

	private void FillParameterCache(ControlSetItem controlItem)
	{
			if( controlItem ==null)
				return;
			
			ParameterMappings.Clear();
			
			foreach(ColumnParameterMapping mapInfo in controlItem.ChildItemsByType(ColumnParameterMapping.CategoryConst))
			{
				if(! mapInfo.IsDeleted)
				{
					ParameterMappings.Add(mapInfo);
				}
			}
		}
	#endregion

	#region IOrigamMetadataConsumer Members

	private AbstractSchemaItem _origamMetadata;
	public AbstractSchemaItem OrigamMetadata
	{
		get
		{
				return _origamMetadata;
			}
		set
		{
				_origamMetadata = value;
				_itemsLoaded = true;

				FillParameterCache(_origamMetadata as ControlSetItem);
			}
	}

	#endregion
}