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

/********************************************************************
	created:	2005/03/27
	created:	27:3:2005   7:05
	filename: 	DataTreeView.cs	
	author:		Mike Chaliy
	
	purpose:	Data binding enabled hierarchical tree view control.
*********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.Gui.Win;
/// <summary>
/// Data binding enabled hierarchical tree view control.
/// </summary>
public class AsTreeView : TreeView, IAsDataConsumer
{
	const int SB_HORZ = 0;
	#region Fields
	
	private System.ComponentModel.Container components = null;
	private object dataSource;
	private string dataMember;
	private CurrencyManager listManager;
	private string idPropertyName;
	private string namePropertyName;
	private string parentIdPropertyName;
	private string valuePropertyName;
	private PropertyDescriptor idProperty;
	private PropertyDescriptor nameProperty;
	private PropertyDescriptor parentIdProperty;
	private PropertyDescriptor valueProperty;
	private TypeConverter valueConverter;
	private Hashtable items_Positions;
	private Hashtable items_Identifiers;
	private bool selectionChanging;
	private object _lastSelectedId = null;
	private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
	#endregion
	#region Constructors
	/// <summary>
	/// Default constructor.
	/// </summary>
	public AsTreeView()
	{
		this.idPropertyName = string.Empty;
		this.namePropertyName = string.Empty;
		this.parentIdPropertyName = string.Empty;
		this.items_Positions = new Hashtable();
		this.items_Identifiers = new Hashtable();
		this.selectionChanging = false;
		this.FullRowSelect = true;
		this.HideSelection = false;
		this.HotTracking = true;
		this.Sorted = true;
		this.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.DataTreeView_AfterSelect);
		this.BindingContextChanged += new System.EventHandler(this.DataTreeView_BindingContextChanged);
		this.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.DataTreeView_AfterLabelEdit);
	}
	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			if( components != null )
				components.Dispose();
			if(listManager != null)
			{
				if(this.listManager.List is IBindingList)
				{
					((IBindingList)this.listManager.List).ListChanged -= new ListChangedEventHandler(DataTreeView_ListChanged);
				}
				this.listManager.PositionChanged -= new EventHandler(listManager_PositionChanged);
				listManager = null;
			}
			this.BindingContextChanged -= new System.EventHandler(this.DataTreeView_BindingContextChanged);
			idProperty = null;
			nameProperty = null;
			parentIdProperty = null;
			valueProperty = null;
			valueConverter = null;
			if(items_Identifiers != null)
			{
				items_Identifiers.Clear();
				items_Identifiers = null;
			}
			if(items_Positions != null)
			{
				items_Positions.Clear();
				items_Positions = null;
			}
		}
		base.Dispose( disposing );
	}
	#endregion
	#region Win32
	[DllImport("User32.dll")] 
	static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
	#endregion
	#region Internal classes
	/// <summary>
	/// Tree node with additional data related information.
	/// </summary>
	public class DataTreeViewNode : TreeNode
	{
		#region Fields
	
		private int position;		
		private object parentID;
		#endregion
		#region Constructors
		/// <summary>
		/// Default constructor of the node.
		/// </summary>
		public DataTreeViewNode(int position)
		{
			this.position = position;
		}
		#endregion
		#region Implementation
		#endregion
		#region Properties
		/// <summary>
		/// Identifier of the node.
		/// </summary>
		public object ID
		{
			get
			{				
				return this.Tag;				
			}
			set
			{
				this.Tag = value;
			}
		}
		/// <summary>
		/// Identifier of the parent node.
		/// </summary>
		public object ParentID
		{
			get
			{
				return this.parentID;
			}
			set
			{
				this.parentID = value;
			}
		}

		/// <summary>
		/// Position in the current currency manager.
		/// </summary>
		public int Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.position = value;
			}
		}
		#endregion
	}
	#endregion
	private bool _dontFireSelectEvent = false;
	protected override void OnAfterSelect(TreeViewEventArgs e)
	{
		if(! _dontFireSelectEvent)
		{
			base.OnAfterSelect (e);
		}
	}
	#region Properties
	
	/// <summary>
	/// Data source of the tree.
	/// </summary>
	[
	DefaultValue((string) null),
	TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
	RefreshProperties(RefreshProperties.Repaint),
	Category("Data"),
	Description("Data source of the tree.")
	]
	public object DataSource
	{
		get
		{
			return this.dataSource;
		}
		set
		{
			if (this.dataSource != value)
			{
				this.dataSource = value;
				try
				{
					this.ResetData();
				}
				catch {}
			}
		}
	}
	/// <summary>
	/// Data member of the tree.
	/// </summary>
	[
	DefaultValue(""),
	Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)),
	RefreshProperties(RefreshProperties.Repaint),
	Category("Data"),
	Description("Data member of the tree.")
	]
	public string DataMember
	{
		get
		{
			return this.dataMember;
		}
		set
		{
			if (this.dataMember != value)
			{
				this.dataMember = value;
				this.ResetData();
			}
		}
	}
	
	/// <summary>
	/// Identifier member, in most cases this is primary column of the table.
	/// </summary>
	[
	DefaultValue(""),
//		Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
	Category("Data"),
	Description("Identifier member, in most cases this is primary column of the table.")		
	]
	public string IDColumn
	{
		get
		{
			return this.idPropertyName;
		}
		set
		{
			if (this.idPropertyName != value)
			{
				this.idPropertyName = value;
				this.idProperty = null;
				if (this.valuePropertyName == null
					|| this.valuePropertyName.Length == 0)
				{
					this.ValueColumn = this.idPropertyName;
				}
				this.ResetData();
			}
		}
	}
	/// <summary>
	/// Name member. Note: editing of this column available only with types that support converting from string.
	/// </summary>
	[
	DefaultValue(""),
//		Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
	Category("Data"),
	Description("Name member. Note: editing of this column available only with types that support converting from string.")		
	]
	public string NameColumn
	{
		get
		{
			return this.namePropertyName;
		}
		set
		{
			if (this.namePropertyName != value)
			{
				this.namePropertyName = value;
				this.nameProperty = null;
				this.ResetData();
			}
		}
	}
	/// <summary>
	/// Identifier of the parent. Note: this member must have the same type as identifier column.
	/// </summary>
	[
	DefaultValue(""),
//		Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
	Category("Data"),
	Description("Identifier of the parent. Note: this member must have the same type as identifier column.")
	]
	public string ParentIDColumn
	{
		get
		{
			return this.parentIdPropertyName;
		}
		set
		{
			if (this.parentIdPropertyName != value)
			{
				this.parentIdPropertyName = value;
				this.parentIdProperty = null;
				this.ResetData();
			}
		}
	}
	/// <summary>
	/// Value member. Form this column value will be taken.
	/// </summary>
	[
	DefaultValue(""),
//		Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
	Category("Data"),
	Description("Value member. Form this column value will be taken.")
	]
	public string ValueColumn
	{
		get
		{
			return this.valuePropertyName;
		}
		set
		{
			if (this.valuePropertyName != value)
			{
				this.valuePropertyName = value;
				this.valueProperty = null;
				this.valueConverter = null;
			}
		}
	}
	/// <summary>
	/// Get value current selected item.
	/// </summary>
	[		
	Category("Data"),
	Description("Get value current selected item.")
	]
	public object Value
	{
		get
		{
			if (this.SelectedNode != null)
			{
				DataTreeViewNode node = this.SelectedNode as DataTreeViewNode;
				if (node != null && this.PrepareValueDescriptor())
				{
					return this.valueProperty.GetValue(this.listManager.List[node.Position]);
				}
			}	
			return null;
		}
		
		set
		{
			_dontFireSelectEvent = true;
			DataTreeViewNode node = this.items_Identifiers[value] as DataTreeViewNode;
			if(node != null)
			{
				this.SelectedNode = node;
			}
			_dontFireSelectEvent = false;
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
	private void DataTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
	{
		this.BeginSelectionChanging();
		
		DataTreeViewNode node = e.Node as DataTreeViewNode;
		if (node == null)
		{
			_lastSelectedId = null;
		}
		else
		{
			((AsForm)this.FindForm()).FormGenerator.IgnoreDataChanges = true;
			try
			{
				this.listManager.Position = node.Position;
			}
			finally
			{
				((AsForm)this.FindForm()).FormGenerator.IgnoreDataChanges = false;
			}
			_lastSelectedId = GetID(node.Position);
		}
		this.EndSelectionChanging();
	}
	private void DataTreeView_AfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
	{
		DataTreeViewNode node = e.Node as DataTreeViewNode;
		if (node != null)
		{
			if (this.PrepareValueConvertor()
				&& this.valueConverter.IsValid(e.Label)
				)
			{
				this.nameProperty.SetValue(
					this.listManager.List[node.Position],
					this.valueConverter.ConvertFromString(e.Label)
					);
				this.listManager.EndCurrentEdit();
				return;
			}
		}
		e.CancelEdit = true;
	}
	private void DataTreeView_BindingContextChanged(object sender, System.EventArgs e)
	{
		this.ResetData();
	}
	private void listManager_PositionChanged(object sender, EventArgs e)
	{
		_dontFireSelectEvent = true;
		try
		{
			this.SynchronizeSelection();
		}
		finally
		{
			_dontFireSelectEvent = false;
		}
	}
	private void DataTreeView_ListChanged(object sender, ListChangedEventArgs e)
	{	
		if(this.items_Positions.Count == 0 & e.ListChangedType != ListChangedType.Reset) return;
		try
		{
			switch(e.ListChangedType)
			{
				case ListChangedType.ItemAdded:
					if (!TryAddNode(this.CreateNode(this.listManager, e.NewIndex)))
					{
						throw new ApplicationException(ResourceUtils.GetString("ErrorAddItemToTree", e.NewIndex.ToString()));
					}
					break;
				case ListChangedType.ItemChanged:
					DataTreeViewNode chnagedNode = this.items_Positions[e.NewIndex] as DataTreeViewNode;
					if (chnagedNode != null)
					{
						
						this.ChangeParent(chnagedNode);
						this.RefreshData(chnagedNode);
					}
					else
					{
						throw new ApplicationException(ResourceUtils.GetString("ErrorItemNotFound"));
					}
					break;
				case ListChangedType.ItemMoved:
					if(e.NewIndex >= 0)
					{
						DataTreeViewNode movedNode = this.items_Positions[e.OldIndex] as DataTreeViewNode;
						if (movedNode != null)
						{						
							this.items_Positions.Remove(e.OldIndex);
							this.items_Positions.Add(e.NewIndex, movedNode);
						}
						else
						{
							throw new ApplicationException(ResourceUtils.GetString("ErrorItemNotFound"));
						}
					}
					break;
				case ListChangedType.ItemDeleted:
					try
					{
						if(items_Positions.Contains(e.NewIndex))
						{
							DataTreeViewNode parent = (items_Positions[e.NewIndex] as TreeNode).Parent as DataTreeViewNode;
						
							if(parent == null)
							{
								_lastSelectedId = null;
							}
							else
							{
								_lastSelectedId = parent.Position;
							}
							this.ResetData();
						}
//							foreach(DataTreeViewNode node in items_Positions.Values)
//							{
//								if(parent.ID == node.ID)
//								{
//									node.EnsureVisible();
//									node.Expand();
//									this.SelectedNode = node;
//									break;
//								}
//							}
					}
					catch{}
					//						DataTreeViewNode deletedNode = this.items_Positions[e.OldIndex] as DataTreeViewNode;
					//						if (deletedNode != null)
					//						{								
					//							this.items_Positions.Remove(e.OldIndex);
					//							this.items_Identifiers.Remove(deletedNode.ID);
					//							deletedNode.Remove();
					//						}
					//						else
					//						{
					//							throw new ApplicationException("Item not found or wrong type.");
					//						}
					break;					
				case ListChangedType.Reset:
					this.ResetData();
					break;
			
			}
		}
		catch (Exception ex)
		{
			Origam.UI.AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorTitle"), ex);
		}
	}		
	#endregion
	#region Implementation
	private void Clear()
	{
		this.items_Positions.Clear();
		this.items_Identifiers.Clear();
		if(!(this.IsDisposed | this.Disposing))
		{
			try
			{
				_dontFireSelectEvent = true;
				this.Nodes.Clear();
			}
			catch{}
			finally
			{
				_dontFireSelectEvent = false;
			}
		}
	}
	
	private bool PrepareDataSource()
	{
		if (this.BindingContext != null)
		{
			if (this.dataSource != null)
			{
				this.listManager = this.BindingContext[this.dataSource, this.dataMember] as CurrencyManager;
				return true;
			}
			else
			{
				this.listManager = null;
				this.Clear();
			}
		}
		return false;
	}		
	private bool PrepareDescriptors()
	{
		if (this.idPropertyName.Length != 0
			&& this.namePropertyName.Length != 0
			&& this.parentIdPropertyName.Length != 0)
		{
			if (this.idProperty == null)
			{
				this.idProperty = this.listManager.GetItemProperties()[this.idPropertyName];
			}
			if (this.nameProperty == null)
			{
				this.nameProperty = this.listManager.GetItemProperties()[this.namePropertyName];
			}
			if (this.parentIdProperty == null)
			{
				this.parentIdProperty = this.listManager.GetItemProperties()[this.parentIdPropertyName];
			}
		}			
		return (this.idProperty != null
			&& this.nameProperty != null
			&& this.parentIdProperty != null);
	}
	private bool PrepareValueDescriptor()
	{
		if (this.valueProperty == null)
		{
			if (this.valuePropertyName == string.Empty)
			{
				this.valuePropertyName = this.idPropertyName;
			}
			this.valueProperty = this.listManager.GetItemProperties()[this.valuePropertyName];
		}			
		return (this.valueProperty != null);
	}
	private bool PrepareValueConvertor()
	{
		if (this.valueConverter == null)
		{
			this.valueConverter = TypeDescriptor.GetConverter(this.nameProperty.PropertyType) as TypeConverter;				
		}

		return (this.valueConverter != null
			&& this.valueConverter.CanConvertFrom(typeof(string))
			);
	}
	private void WireDataSource()
	{
		this.listManager.PositionChanged += new EventHandler(listManager_PositionChanged);
		((IBindingList)this.listManager.List).ListChanged += new ListChangedEventHandler(DataTreeView_ListChanged);
	}
	private void ResetData()
	{
		this.BeginUpdate();
		this.Clear();
		if (this.PrepareDataSource())
		{				
			this.WireDataSource();
			if (this.PrepareDescriptors())
			{
				var unsortedNodes = new List<DataTreeViewNode>();			
				for (int i = 0; i < this.listManager.Count; i++)
				{
					unsortedNodes.Add(this.CreateNode(this.listManager, i));
				}
		
				int startCount;
				while (unsortedNodes.Count > 0)
				{	
					startCount = unsortedNodes.Count;
					for (int i = unsortedNodes.Count-1; i >= 0 ; i--)
					{					
						if (this.TryAddNode(unsortedNodes[i]))
						{
							unsortedNodes.RemoveAt(i);
						}
					}
					if (startCount == unsortedNodes.Count)
					{
						break;
						//throw new ApplicationException("Tree view confused when try to make your data hierarchical.");
					}
				}
			}
		}
		if(! this.IsDisposed)
		{
			this.EndUpdate();
		}
		if(_lastSelectedId != null && this.items_Identifiers.ContainsKey(_lastSelectedId))
		{
			DataTreeViewNode node = items_Identifiers[_lastSelectedId] as DataTreeViewNode;
			this.SelectedNode = node;
		}
	}
	private bool TryAddNode(DataTreeViewNode node)
	{
		if (this.IsIDNull(node.ParentID))
		{
			this.AddNode(this.Nodes, node);				
			return true;
		}
		else
		{
			if (this.items_Identifiers.ContainsKey(node.ParentID))
			{
				DataTreeViewNode parentNode = this.items_Identifiers[node.ParentID] as DataTreeViewNode;
				if (parentNode != null)
				{
					CheckRecursion(node, parentNode);
					this.AddNode(parentNode.Nodes, node);				
					return true;
				}
			}
		}
		return false;
	}
	private void AddNode(TreeNodeCollection nodes, DataTreeViewNode node)
	{
		if(node.ID == null | node.ID == DBNull.Value) return;
		if(!this.items_Positions.ContainsKey(node.Position))
		{
			this.items_Positions.Add(node.Position, node);
			this.items_Identifiers.Add(node.ID, node);
			nodes.Add(node);
		}
	}
	
	private void ChangeParent(DataTreeViewNode node)
	{			
		object dataParentID = this.parentIdProperty.GetValue(this.listManager.List[node.Position]);
		if (node.ParentID != dataParentID)
		{
			DataTreeViewNode newParentNode = this.items_Identifiers[dataParentID] as DataTreeViewNode;
			CheckRecursion(node, newParentNode);
			node.Remove();
			if (newParentNode != null)
			{
				newParentNode.Nodes.Add(node);
			}
			else
			{
				this.Nodes.Add(node);
			}
		}
	}
	private void CheckRecursion(DataTreeViewNode node, DataTreeViewNode parentNode)
	{
		if(node == null | parentNode == null) return;
		if(node.ID.Equals(parentNode.ID))
		{
			this.parentIdProperty.SetValue(this.listManager.List[node.Position], DBNull.Value);
			throw new NotSupportedException("Stromové zobrazení: Není možné pøidat položku pod sebe sama.");
		}
		foreach(DataTreeViewNode child in node.Nodes)
		{
			if(child != null)
			{
				CheckRecursion(node, child);
			}
		}
	}
	
	private void SynchronizeSelection()
	{
		if(this.listManager == null | this.selectionChanging) return;
		DataTreeViewNode node = this.items_Positions[this.listManager.Position] as DataTreeViewNode;
		if (node != null)
		{
			this.SelectedNode = node;
		}
	}
	private void RefreshData(DataTreeViewNode node)
	{
		if(this.listManager == null) return;
		int position = node.Position;
		node.ID = this.GetID(position);
		object name = this.GetName(position);
		node.Text = name == null ? "" : name.ToString();
		object parentId = GetParentID(position);
		node.ParentID = parentId == null ? "" : parentId;
	}
	/// <summary>
	/// Create a DataTreeViewNode with currency manager and position.
	/// </summary>
	/// <param name="currencyManager"></param>
	/// <param name="position"></param>
	/// <returns></returns>
	private DataTreeViewNode CreateNode(CurrencyManager currencyManager, int position)
	{
		DataTreeViewNode node = new DataTreeViewNode(position);
		this.RefreshData(node);
		return node;
	}
	private object GetName(int position)
	{
		return this.nameProperty.GetValue(this.listManager.List[position]);
	}
	private object GetID(int position)
	{
		return this.idProperty.GetValue(this.listManager.List[position]);
	}
	private object GetParentID(int position)
	{
		return this.parentIdProperty.GetValue(this.listManager.List[position]);
	}
	private bool IsIDNull(object id)
	{
		if (id == null
			|| Convert.IsDBNull(id))
		{
			return true;
		}
		else
		{
			if (id.GetType() == typeof(string))
			{
				return (((string)id).Length == 0);
			}
			else if (id.GetType() == typeof(Guid))
			{
				return ((Guid)id == Guid.Empty);
			}				
		}
		return false;
	}
	protected override void InitLayout()
	{ 
		base.InitLayout(); 
		ShowScrollBar(Handle, SB_HORZ, false); 
	} 
	private void BeginSelectionChanging()
	{
		this.selectionChanging = true;
	}
	private void EndSelectionChanging()
	{
		this.selectionChanging = false;
	}
	#endregion
	#region IAsDataServiceConsumer Members
	#endregion
}
