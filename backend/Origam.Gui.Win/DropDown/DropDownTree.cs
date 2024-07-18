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
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for DropDownList.
/// </summary>
public class DropDownTree : System.Windows.Forms.Form, ILookupDropDownPart
{
	private System.Windows.Forms.TreeView tree;
	private System.ComponentModel.IContainer components;
	private System.Windows.Forms.Timer timer1;
	private SortedList items_Identifiers;
	public DropDownTree()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
		this.items_Identifiers = new SortedList();
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
			//this.DataSource = null;
		}
		base.Dispose( disposing );
	}
//		/// <summary>
//		/// Paint the form and draw a neat border.
//		/// </summary>
//		/// <param name="e">Information about the paint event</param>
//		protected override void OnPaint(PaintEventArgs e)
//		{
//			base.OnPaint(e);
//			Rectangle borderRect = new Rectangle(this.ClientRectangle.Location, this.ClientRectangle.Size);
//			borderRect.Width -= 1;
//			borderRect.Height -= 1;
//			e.Graphics.DrawRectangle(SystemPens.ControlDark, borderRect);			
//		}
//
	#region Windows Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.tree = new System.Windows.Forms.TreeView();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.SuspendLayout();
		// 
		// tree
		// 
		this.tree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tree.FullRowSelect = true;
		this.tree.HideSelection = false;
		this.tree.ImageIndex = -1;
		this.tree.Location = new System.Drawing.Point(0, 0);
		this.tree.Name = "tree";
		this.tree.SelectedImageIndex = -1;
		this.tree.Size = new System.Drawing.Size(292, 336);
		this.tree.Sorted = true;
		this.tree.TabIndex = 0;
		this.tree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.List_KeyDown);
		this.tree.DoubleClick += new System.EventHandler(this.tree_DoubleClick);
		this.tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_AfterSelect);
		// 
		// timer1
		// 
		this.timer1.Enabled = true;
		this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
		// 
		// DropDownTree
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(292, 336);
		this.ControlBox = false;
		this.Controls.Add(this.tree);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		this.KeyPreview = true;
		this.Name = "DropDownTree";
		this.ShowInTaskbar = false;
		this.Text = "DropDownList";
		this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DropDownTree_KeyDown);
		this.ResumeLayout(false);
	}
	#endregion
	#region Properties
	public override bool Focused
	{
		get
		{
			if(base.Focused | tree.Focused) 
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
	private bool _canceled = false;
	public bool Canceled
	{
		get
		{
			return _canceled;
		}
		set
		{
			_canceled = value;
		}
	}
//		private ILookupControl _lookupControl;
//		public ILookupControl LookupControl
//		{
//			get
//			{
//				return _lookupControl;
//			}
//			set
//			{
//				_lookupControl = value;
//			}
//		}
	private DataView _dataSource;
	public DataView DataSource
	{
		get
		{
			return _dataSource;
		}
		set
		{
			_dataSource = value;
			if(_dataSource != null)
			{
				items_Identifiers.Clear();
				FillTree();
			}
		}
	}
	private string _valueMember;
	public string ValueMember
	{
		get
		{
			return _valueMember;
		}
		set
		{
			_valueMember = value;
		}
	}
	private string _displayMember;
	public string DisplayMember
	{
		get
		{
			return _displayMember;
		}
		set
		{
			_displayMember = value;
		}
	}
	private string _parentMember;
	public string ParentMember
	{
		get
		{
			return _parentMember;
		}
		set
		{
			_parentMember = value;
		}
	}
	private string _selectedText = "";
	public string SelectedText
	{
		get
		{
			return _selectedText;
		}
		set
		{
			throw new NotImplementedException();
		}
	}
	private bool _selectingValue = false;
	private object _selectedValue;
	public object SelectedValue
	{
		get
		{
			return _selectedValue;
		}
		set
		{
			_selectedValue = value;
			if(value != DBNull.Value)
			{
				if(items_Identifiers.Contains(value))
				{
					_selectingValue = true;
					tree.SelectedNode = items_Identifiers[value] as TreeNode;
					_selectingValue = false;
				}
			}
		}
	}
	private BaseDropDownControl _dropDownControl;
	public BaseDropDownControl DropDownControl
	{
		get
		{
			return _dropDownControl;
		}
		set
		{
			_dropDownControl = value;
		}
	}
	#endregion
	#region Methods
	public void SelectItem()
	{
		if(tree.SelectedNode is DataTreeViewNode)
		{
			_selectedValue = (tree.SelectedNode as DataTreeViewNode).ID;
			_selectedText = tree.SelectedNode.Text;
			this.tree.KeyDown -= new System.Windows.Forms.KeyEventHandler(this.List_KeyDown);
			this.tree.AfterSelect -= new TreeViewEventHandler(tree_AfterSelect);
			this.Close();
		}
	}
	public void MoveUp()
	{
		this.tree.Focus();
	}
	public void MoveDown()
	{
		this.tree.Focus();
	}
	#endregion
	#region Event Handlers
	private void List_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{
		switch(e.KeyCode)
		{
			case Keys.Up:
			case Keys.Down:
				if(e.Alt)
				{
					_readyToClose = true;
				}
				break;
			case Keys.Tab:
			case Keys.Return:
				_readyToClose = true;
				break;
		}
	}
	#endregion
	private void tree_AfterSelect(object sender, TreeViewEventArgs e)
	{
//			if(_selectingValue) return;
//
//			_readyToClose = true;
	}
	private void FillTree()
	{
		tree.Nodes.Clear();
		tree.BeginUpdate();
		var unsortedNodes = new List<DataTreeViewNode>();			
		for (int i = 0; i < this.DataSource.Count; i++)
		{
			unsortedNodes.Add(new DataTreeViewNode(
				this.DataSource[i].Row[this.ValueMember],
				this.DataSource[i].Row[this.ParentMember],
				this.DataSource[i].Row[this.DisplayMember].ToString()
				));
		}
		
		int startCount;
		while (unsortedNodes.Count > 0)
		{	
			startCount = unsortedNodes.Count;
			for (int i = unsortedNodes.Count-1; i >= 0 ; i--)
			{					
				if (TryAddNode(unsortedNodes[i]))
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
		tree.EndUpdate();
	}
	private bool TryAddNode(DataTreeViewNode node)
	{
		if (node.ParentID == DBNull.Value)
		{
			this.AddNode(this.tree.Nodes, node);				
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
	private void CheckRecursion(DataTreeViewNode node, DataTreeViewNode parentNode)
	{
		if(node.ID.Equals(parentNode.ID))
		{
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
	
	private void AddNode(TreeNodeCollection nodes, DataTreeViewNode node)
	{
		if(node.ID == null | node.ID == DBNull.Value) return;
		if(!this.items_Identifiers.ContainsKey(node.ID))
		{
			this.items_Identifiers.Add(node.ID, node);
			nodes.Add(node);
		}
	}
	private bool _readyToClose = false;
	private void timer1_Tick(object sender, System.EventArgs e)
	{
		if(_readyToClose) this.SelectItem();
	}
	private void DropDownTree_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{
		if(e.KeyCode == Keys.Escape)
		{
			this.Canceled = true;
			this.Close();
		}
	}
	private void tree_DoubleClick(object sender, System.EventArgs e)
	{
		if(_selectingValue) return;
		_readyToClose = true;
	}
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
		public DataTreeViewNode(object id, object parentId, string text)
		{
			this.ID = id;
			this.ParentID = parentId;
			this.Text = text;
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
}
