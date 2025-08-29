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
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for BaseCaptionControl.
/// </summary>
public class BaseCaptionControl : System.Windows.Forms.UserControl, IAsCaptionControl, IAsControl
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
	private System.ComponentModel.Container components = null;
	Label _captionLabel = new Label();
	public BaseCaptionControl()
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
			if(this.Parent != null && this.Parent.Controls.Contains(_captionLabel) && _captionLabel.IsDisposed == false)
			{
				this.Parent.Controls.Remove(_captionLabel);
			}
			_captionLabel = null;
			if(components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose( disposing );
	}
	protected override void InitLayout()
	{
		base.InitLayout ();
		PaintCaption();
		this.Parent.Controls.Add(_captionLabel);
		
		ResetCaption();
		this.DataBindings.CollectionChanged += new CollectionChangeEventHandler(DataBindings_CollectionChanged);
	}
	protected override void OnMove(EventArgs e)
	{
		base.OnMove (e);
		PaintCaption();
	}
	public void OnControlMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel (e);
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
		}
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
	#region IAsCaptionControl Members
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
	string _gridColumnCaption = "";
	[Category("(ORIGAM)")]
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
	string _caption = "";
	[Category("(ORIGAM)")]
	public string Caption
	{
		get
		{
			return _caption;
		}
		set
		{
			_caption = value ?? "";
			this._captionLabel.Text = _caption;
			ResetCaption();
		}
	}
	CaptionPosition _captionPosition = CaptionPosition.Left;
	[Category("(ORIGAM)")]
	public CaptionPosition CaptionPosition
	{
		get
		{
			return _captionPosition;
		} 
		set
		{
			_captionPosition = value;
			PaintCaption();
		}
	}
	private int _captionLength = 100;
	[Category("(ORIGAM)")]
	public int CaptionLength
	{
		get
		{
			return _captionLength ;
		}
		set
		{
			_captionLength = value;
			PaintCaption();
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
	#region private methods
	private void PaintCaption()
	{
		if(_captionLabel == null | this.Parent == null | this.IsDisposed | this.Disposing)
        {
            return;
        }

        this._captionLabel.Width = this.CaptionLength;
		this._captionLabel.BackColor = Color.Transparent;
		this._captionLabel.AutoSize = true;
					
		switch(this.CaptionPosition)
		{
            case CaptionPosition.Left:
                {
                    this._captionLabel.Visible = true;
                    this._captionLabel.Top = this.Top + 2;
                    this._captionLabel.Left = this.Left - this.CaptionLength;
                    break;
                }

            case CaptionPosition.Right:
                {
                    this._captionLabel.Visible = true;
                    this._captionLabel.Top = this.Top + 2;
                    this._captionLabel.Left = this.Right;
                    break;
                }

            case CaptionPosition.Top:
                {
                    this._captionLabel.Visible = true;
                    this._captionLabel.Top = this.Top - this._captionLabel.Height;
                    this._captionLabel.Left = this.Left;
                    break;
                }

            case CaptionPosition.Bottom:
                {
                    this._captionLabel.Visible = true;
                    this._captionLabel.Top = this.Top + this.Height;
                    this._captionLabel.Left = this.Left;
                    break;
                }

            case CaptionPosition.None:
                {
                    this._captionLabel.Visible = false;
                    break;
                }
        }
	}
	private void DataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e)
	{
		if(this.DesignMode)
		{
			if(this.Caption == "")
			{
				if(e.Element != null)
				{
					if((e.Element as Binding).PropertyName == this.DefaultBindableProperty)
					{
						if(e.Action == CollectionChangeAction.Remove)
						{
							this._captionLabel.Text = "";
						}
						else
						{
							try
							{
								this._captionLabel.Text = ColumnCaption((e.Element as Binding));
							}
							catch
							{
								this._captionLabel.Text = "????";
							}
						}
					}
				}
			}
		}
	}
	private void ResetCaption()
	{
		if(this.Caption == "")
		{
			foreach(Binding binding in this.DataBindings)
			{
				if(binding.PropertyName == this.DefaultBindableProperty)
                {
                    try
					{
						this._captionLabel.Text = ColumnCaption(binding);
					}
					catch
					{
						this._captionLabel.Text = "????";
					}
                }
            }
		}
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
        {
            tableName = dataMember;
        }

        return tableName;
	}
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
                {
                    return table.Columns[binding.BindingMemberInfo.BindingField].Caption;
                }
            }
		}
		return binding.BindingMemberInfo.BindingField;
	}
	#endregion
	#region IAsControl Members
	public virtual string DefaultBindableProperty
	{
		get
		{
			throw new InvalidOperationException();
		}
	}
	
	#endregion
}
