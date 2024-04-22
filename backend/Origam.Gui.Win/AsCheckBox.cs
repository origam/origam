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
using System.Data;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for AsCheckBox.
/// </summary>
[ToolboxBitmap(typeof(AsCheckBox))]
public class AsCheckBox : CheckBox, IAsControl, IAsCaptionControl
{
	private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;

	public event EventHandler valueChanged;

	public AsCheckBox()
	{
			this.FlatStyle = FlatStyle.Flat;
			this.BackColor = System.Drawing.Color.Transparent;

			this.CheckedChanged +=new EventHandler(AsCheckBox_CheckedChanged);
			this.Click += new EventHandler(AsCheckBox_Click);
			this.KeyPress += new KeyPressEventHandler(AsCheckBox_KeyPress);
			this.EnabledChanged += new EventHandler(AsCheckBox_EnabledChanged);

			this.DataBindings.CollectionChanged += new CollectionChangeEventHandler(DataBindings_CollectionChanged);
		}

	#region IAsControl Members

	public object Value
	{
		get
		{
				if(this.CheckState == CheckState.Indeterminate)
				{
					return DBNull.Value;
				}
				else
				{
					return this.Checked;
				}
			}
		set
		{
				if(value==DBNull.Value | value == null)
				{
					if(this.ThreeState)
					{
						this.CheckState = CheckState.Indeterminate;
					}
					else
					{
						this.Checked = false;
					}
				}
				else if (value is bool)
				{
					if(this.ThreeState)
					{
						if((bool)value)
						{
							this.CheckState = CheckState.Checked;
						}
						else
						{
							this.CheckState = CheckState.Unchecked;
						}	
					}
					else
					{
						this.Checked = (bool)value;
					}
				}
		}

            
	}

	public string DefaultBindableProperty
	{
		get
		{
				return "Value";
			}
	}

	#endregion

	#region Properties
	public override string Text
	{
		get => base.Text;
		set
		{
				if(value != null && !value.StartsWith("AsCheckBox"))
				{
					base.Text = value;
				}
			}
	}

	int _gridColumnWidth;
	[Category("(ORIGAM)")]
	[DefaultValue(100)] 
	[Description(CaptionDoc.GridColumnWidthDescription)]
	public int GridColumnWidth
	{
		get
		{
				return _gridColumnWidth;
			}
		set
		{
				_gridColumnWidth = value;
			}
	}

	private bool _readOnly = false;

	[Browsable(true)]
	[Category("Behavior")]
	[DefaultValue(false)]
	public bool ReadOnly
	{
		get
		{
				return _readOnly;
			}
		set
		{
				_readOnly = value;
				if(! this.DesignMode) this.Enabled = !value;
			}
	}

	private bool _hideOnForm = false;
	public bool HideOnForm
	{
		get
		{
				return _hideOnForm;
			}
		set
		{
				_hideOnForm = value;

				if(value && ! this.DesignMode)
				{
					this.Hide();
				}
			}
	}

	private bool _enabled = false;

	public new bool Enabled
	{
		get
		{
				if(this.ReadOnly) 
				{
					return false;
				}
				else
				{
					return _enabled;
				}
			}
		set
		{
				_enabled = value;

				if(this.ReadOnly)
				{
					if(this.DesignMode)
					{
						base.Enabled = value;
					}
					else
					{
						base.Enabled = false;
					}
				}
				else
				{
					base.Enabled = value;
				}
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

	#region Events
	void OnValueChanged(EventArgs e)
	{
			if (valueChanged != null) 
			{
				valueChanged(this, e);
			}

			this.OnValidating(new CancelEventArgs(false));
		}
	#endregion

	#region Event Handlers
	private void AsCheckBox_CheckedChanged(object sender, EventArgs e)
	{
			//OnValueChanged(EventArgs.Empty);
		}

	private void AsCheckBox_Click(object sender, EventArgs e)
	{
			OnValueChanged(EventArgs.Empty);
			System.Diagnostics.Debug.WriteLine("Checked changed");
		}

	private void AsCheckBox_KeyPress(object sender, KeyPressEventArgs e)
	{
			//OnValueChanged(EventArgs.Empty);
		}

	private void AsCheckBox_EnabledChanged(object sender, EventArgs e)
	{
			if(this.ReadOnly & base.Enabled) this.Enabled = false;
		}
		
	private void DataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e)
	{
			if(this.DesignMode)
			{
				if(this.Text == "")
				{
					if(e.Element != null)
					{
						if((e.Element as Binding).PropertyName == this.DefaultBindableProperty)
						{
							if(e.Action == CollectionChangeAction.Remove)
							{
								this.Text = "";
							}
							else
							{
								try
								{
									this.Text = ColumnCaption((e.Element as Binding));
								}
								catch
								{
									this.Text = "????";
								}
							}
						}
					}
				}
			}
		}
	#endregion
		
	#region IAsCaptionControl Members

	public string Caption
	{
		get
		{
				return base.Text;
			}
		set
		{
				base.Text = value ?? "";
			}
	}

	private string _gridColumnCaption;
	public string GridColumnCaption
	{
		get
		{
				return _gridColumnCaption;
			}
		set
		{
				_gridColumnCaption = value;
			}
	}

	public int CaptionLength
	{
		get
		{
				return this.Width;
			}
		set
		{
				//this.Width = value;
			}
	}

	public CaptionPosition CaptionPosition
	{
		get
		{
				return CaptionPosition.Right;
			}
		set
		{
				// TODO:  Add AsCheckBox.CaptionPosition setter implementation
			}
	}

	#endregion

	#region Private Methods
	private string ColumnCaption(Binding binding)
	{
			if(binding.DataSource is DataSet)
			{
				DataSet dataset = binding.DataSource as DataSet;
				// Get Table
				DataTable table = dataset.Tables[TableName(dataset, binding.BindingMemberInfo.BindingMember)];
				
				if(table != null)
				{
					if(table.Columns.Contains(binding.BindingMemberInfo.BindingField))
						return table.Columns[binding.BindingMemberInfo.BindingField].Caption;
				}
			}

			return binding.BindingMemberInfo.BindingField;
		}

	private string TableName(DataSet ds, string dataMember)
	{
			// In case that dataMember is a path through relations, we find the last table
			// so we can take a caption out of it
			string tableName = "";
			if(dataMember.IndexOf(".") > 0)
			{
				string[] path = dataMember.Split(".".ToCharArray());
				DataTable table = ds.Tables[path[0]];
				for(int i = 1; i < path.Length - 1; i++)
				{
					table = table.ChildRelations[path[i]].ChildTable;
				}

				if(table != null)
				{
					tableName = table.TableName;
				}
			}
			else
				tableName = dataMember;

			return tableName;
		}

	private void ResetCaption()
	{
			if(this.Caption == "")
			{
				foreach(Binding binding in this.DataBindings)
				{
					if(binding.PropertyName == "Text")
						try
						{
							base.Text = ColumnCaption(binding);
						}
						catch
						{
							base.Text = "????";
						}
				}
			}
		}
	#endregion
}