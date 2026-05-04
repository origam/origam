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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

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
    private IPersistenceService _persistence =
        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
        as IPersistenceService;
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
        this.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(
            this.DataTreeView_AfterSelect
        );
        this.BindingContextChanged += new System.EventHandler(
            this.DataTreeView_BindingContextChanged
        );
        this.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(
            this.DataTreeView_AfterLabelEdit
        );
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }

            if (listManager != null)
            {
                if (this.listManager.List is IBindingList)
                {
                    ((IBindingList)this.listManager.List).ListChanged -=
                        new ListChangedEventHandler(DataTreeView_ListChanged);
                }
                this.listManager.PositionChanged -= new EventHandler(listManager_PositionChanged);
                listManager = null;
            }
            this.BindingContextChanged -= new System.EventHandler(
                this.DataTreeView_BindingContextChanged
            );
            idProperty = null;
            nameProperty = null;
            parentIdProperty = null;
            valueProperty = null;
            valueConverter = null;
            if (items_Identifiers != null)
            {
                items_Identifiers.Clear();
                items_Identifiers = null;
            }
            if (items_Positions != null)
            {
                items_Positions.Clear();
                items_Positions = null;
            }
        }
        base.Dispose(disposing: disposing);
    }
    #endregion
    #region Win32
    [DllImport(dllName: "User32.dll")]
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
            get { return this.Tag; }
            set { this.Tag = value; }
        }

        /// <summary>
        /// Identifier of the parent node.
        /// </summary>
        public object ParentID
        {
            get { return this.parentID; }
            set { this.parentID = value; }
        }

        /// <summary>
        /// Position in the current currency manager.
        /// </summary>
        public int Position
        {
            get { return this.position; }
            set { this.position = value; }
        }
        #endregion
    }
    #endregion
    private bool _dontFireSelectEvent = false;

    protected override void OnAfterSelect(TreeViewEventArgs e)
    {
        if (!_dontFireSelectEvent)
        {
            base.OnAfterSelect(e: e);
        }
    }

    #region Properties

    /// <summary>
    /// Data source of the tree.
    /// </summary>
    [
        DefaultValue(value: (string)null),
        TypeConverter(
            typeName: "System.Windows.Forms.Design.DataSourceConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
        ),
        RefreshProperties(refresh: RefreshProperties.Repaint),
        Category(category: "Data"),
        Description(description: "Data source of the tree.")
    ]
    public object DataSource
    {
        get { return this.dataSource; }
        set
        {
            if (this.dataSource != value)
            {
                this.dataSource = value;
                try
                {
                    this.ResetData();
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Data member of the tree.
    /// </summary>
    [
        DefaultValue(value: ""),
        Editor(
            typeName: "System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            baseType: typeof(UITypeEditor)
        ),
        RefreshProperties(refresh: RefreshProperties.Repaint),
        Category(category: "Data"),
        Description(description: "Data member of the tree.")
    ]
    public string DataMember
    {
        get { return this.dataMember; }
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
        DefaultValue(value: ""),
        //		Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
        Category(category: "Data"),
        Description(
            description: "Identifier member, in most cases this is primary column of the table."
        )
    ]
    public string IDColumn
    {
        get { return this.idPropertyName; }
        set
        {
            if (this.idPropertyName != value)
            {
                this.idPropertyName = value;
                this.idProperty = null;
                if (this.valuePropertyName == null || this.valuePropertyName.Length == 0)
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
        DefaultValue(value: ""),
        //		Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
        Category(category: "Data"),
        Description(
            description: "Name member. Note: editing of this column available only with types that support converting from string."
        )
    ]
    public string NameColumn
    {
        get { return this.namePropertyName; }
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
        DefaultValue(value: ""),
        //		Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
        Category(category: "Data"),
        Description(
            description: "Identifier of the parent. Note: this member must have the same type as identifier column."
        )
    ]
    public string ParentIDColumn
    {
        get { return this.parentIdPropertyName; }
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
        DefaultValue(value: ""),
        //		Editor(typeof(FieldTypeEditor), typeof(UITypeEditor)),
        Category(category: "Data"),
        Description(description: "Value member. Form this column value will be taken.")
    ]
    public string ValueColumn
    {
        get { return this.valuePropertyName; }
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
    [Category(category: "Data"), Description(description: "Get value current selected item.")]
    public object Value
    {
        get
        {
            if (this.SelectedNode != null)
            {
                DataTreeViewNode node = this.SelectedNode as DataTreeViewNode;
                if (node != null && this.PrepareValueDescriptor())
                {
                    return this.valueProperty.GetValue(
                        component: this.listManager.List[index: node.Position]
                    );
                }
            }
            return null;
        }
        set
        {
            _dontFireSelectEvent = true;
            DataTreeViewNode node = this.items_Identifiers[key: value] as DataTreeViewNode;
            if (node != null)
            {
                this.SelectedNode = node;
            }
            _dontFireSelectEvent = false;
        }
    }
    private Guid _styleId;

    [Browsable(browsable: false)]
    public Guid StyleId
    {
        get { return _styleId; }
        set { _styleId = value; }
    }

    [TypeConverter(type: typeof(StylesConverter))]
    public UIStyle Style
    {
        get
        {
            return (UIStyle)
                _persistence.SchemaProvider.RetrieveInstance(
                    type: typeof(UIStyle),
                    primaryKey: new ModelElementKey(id: this.StyleId)
                );
        }
        set { this.StyleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
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
            _lastSelectedId = GetID(position: node.Position);
        }
        this.EndSelectionChanging();
    }

    private void DataTreeView_AfterLabelEdit(
        object sender,
        System.Windows.Forms.NodeLabelEditEventArgs e
    )
    {
        DataTreeViewNode node = e.Node as DataTreeViewNode;
        if (node != null)
        {
            if (this.PrepareValueConvertor() && this.valueConverter.IsValid(value: e.Label))
            {
                this.nameProperty.SetValue(
                    component: this.listManager.List[index: node.Position],
                    value: this.valueConverter.ConvertFromString(text: e.Label)
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
        if (this.items_Positions.Count == 0 & e.ListChangedType != ListChangedType.Reset)
        {
            return;
        }

        try
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                {
                    if (
                        !TryAddNode(
                            node: this.CreateNode(
                                currencyManager: this.listManager,
                                position: e.NewIndex
                            )
                        )
                    )
                    {
                        throw new ApplicationException(
                            message: ResourceUtils.GetString(
                                key: "ErrorAddItemToTree",
                                args: e.NewIndex.ToString()
                            )
                        );
                    }
                    break;
                }

                case ListChangedType.ItemChanged:
                {
                    DataTreeViewNode chnagedNode =
                        this.items_Positions[key: e.NewIndex] as DataTreeViewNode;
                    if (chnagedNode != null)
                    {
                        this.ChangeParent(node: chnagedNode);
                        this.RefreshData(node: chnagedNode);
                    }
                    else
                    {
                        throw new ApplicationException(
                            message: ResourceUtils.GetString(key: "ErrorItemNotFound")
                        );
                    }
                    break;
                }

                case ListChangedType.ItemMoved:
                {
                    if (e.NewIndex >= 0)
                    {
                        DataTreeViewNode movedNode =
                            this.items_Positions[key: e.OldIndex] as DataTreeViewNode;
                        if (movedNode != null)
                        {
                            this.items_Positions.Remove(key: e.OldIndex);
                            this.items_Positions.Add(key: e.NewIndex, value: movedNode);
                        }
                        else
                        {
                            throw new ApplicationException(
                                message: ResourceUtils.GetString(key: "ErrorItemNotFound")
                            );
                        }
                    }
                    break;
                }

                case ListChangedType.ItemDeleted:
                {
                    try
                    {
                        if (items_Positions.Contains(key: e.NewIndex))
                        {
                            DataTreeViewNode parent =
                                (items_Positions[key: e.NewIndex] as TreeNode).Parent
                                as DataTreeViewNode;

                            if (parent == null)
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
                    catch { }
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
                }

                case ListChangedType.Reset:
                {
                    this.ResetData();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Origam.UI.AsMessageBox.ShowError(
                owner: this.FindForm(),
                text: ex.Message,
                caption: ResourceUtils.GetString(key: "ErrorTitle"),
                exception: ex
            );
        }
    }
    #endregion
    #region Implementation
    private void Clear()
    {
        this.items_Positions.Clear();
        this.items_Identifiers.Clear();
        if (!(this.IsDisposed | this.Disposing))
        {
            try
            {
                _dontFireSelectEvent = true;
                this.Nodes.Clear();
            }
            catch { }
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
                this.listManager =
                    this.BindingContext[dataSource: this.dataSource, dataMember: this.dataMember]
                    as CurrencyManager;
                return true;
            }
            this.listManager = null;

            this.Clear();
        }
        return false;
    }

    private bool PrepareDescriptors()
    {
        if (
            this.idPropertyName.Length != 0
            && this.namePropertyName.Length != 0
            && this.parentIdPropertyName.Length != 0
        )
        {
            if (this.idProperty == null)
            {
                this.idProperty = this.listManager.GetItemProperties()[name: this.idPropertyName];
            }
            if (this.nameProperty == null)
            {
                this.nameProperty = this.listManager.GetItemProperties()[
                    name: this.namePropertyName
                ];
            }
            if (this.parentIdProperty == null)
            {
                this.parentIdProperty = this.listManager.GetItemProperties()[
                    name: this.parentIdPropertyName
                ];
            }
        }
        return (
            this.idProperty != null && this.nameProperty != null && this.parentIdProperty != null
        );
    }

    private bool PrepareValueDescriptor()
    {
        if (this.valueProperty == null)
        {
            if (this.valuePropertyName == string.Empty)
            {
                this.valuePropertyName = this.idPropertyName;
            }
            this.valueProperty = this.listManager.GetItemProperties()[name: this.valuePropertyName];
        }
        return (this.valueProperty != null);
    }

    private bool PrepareValueConvertor()
    {
        if (this.valueConverter == null)
        {
            this.valueConverter =
                TypeDescriptor.GetConverter(type: this.nameProperty.PropertyType) as TypeConverter;
        }

        return (
            this.valueConverter != null
            && this.valueConverter.CanConvertFrom(sourceType: typeof(string))
        );
    }

    private void WireDataSource()
    {
        this.listManager.PositionChanged += new EventHandler(listManager_PositionChanged);
        ((IBindingList)this.listManager.List).ListChanged += new ListChangedEventHandler(
            DataTreeView_ListChanged
        );
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
                    unsortedNodes.Add(
                        item: this.CreateNode(currencyManager: this.listManager, position: i)
                    );
                }

                int startCount;
                while (unsortedNodes.Count > 0)
                {
                    startCount = unsortedNodes.Count;
                    for (int i = unsortedNodes.Count - 1; i >= 0; i--)
                    {
                        if (this.TryAddNode(node: unsortedNodes[index: i]))
                        {
                            unsortedNodes.RemoveAt(index: i);
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
        if (!this.IsDisposed)
        {
            this.EndUpdate();
        }
        if (_lastSelectedId != null && this.items_Identifiers.ContainsKey(key: _lastSelectedId))
        {
            DataTreeViewNode node = items_Identifiers[key: _lastSelectedId] as DataTreeViewNode;
            this.SelectedNode = node;
        }
    }

    private bool TryAddNode(DataTreeViewNode node)
    {
        if (this.IsIDNull(id: node.ParentID))
        {
            this.AddNode(nodes: this.Nodes, node: node);
            return true;
        }

        if (this.items_Identifiers.ContainsKey(key: node.ParentID))
        {
            DataTreeViewNode parentNode =
                this.items_Identifiers[key: node.ParentID] as DataTreeViewNode;
            if (parentNode != null)
            {
                CheckRecursion(node: node, parentNode: parentNode);
                this.AddNode(nodes: parentNode.Nodes, node: node);
                return true;
            }
        }
        return false;
    }

    private void AddNode(TreeNodeCollection nodes, DataTreeViewNode node)
    {
        if (node.ID == null | node.ID == DBNull.Value)
        {
            return;
        }

        if (!this.items_Positions.ContainsKey(key: node.Position))
        {
            this.items_Positions.Add(key: node.Position, value: node);
            this.items_Identifiers.Add(key: node.ID, value: node);
            nodes.Add(node: node);
        }
    }

    private void ChangeParent(DataTreeViewNode node)
    {
        object dataParentID = this.parentIdProperty.GetValue(
            component: this.listManager.List[index: node.Position]
        );
        if (node.ParentID != dataParentID)
        {
            DataTreeViewNode newParentNode =
                this.items_Identifiers[key: dataParentID] as DataTreeViewNode;
            CheckRecursion(node: node, parentNode: newParentNode);
            node.Remove();
            if (newParentNode != null)
            {
                newParentNode.Nodes.Add(node: node);
            }
            else
            {
                this.Nodes.Add(node: node);
            }
        }
    }

    private void CheckRecursion(DataTreeViewNode node, DataTreeViewNode parentNode)
    {
        if (node == null | parentNode == null)
        {
            return;
        }

        if (node.ID.Equals(obj: parentNode.ID))
        {
            this.parentIdProperty.SetValue(
                component: this.listManager.List[index: node.Position],
                value: DBNull.Value
            );
            throw new NotSupportedException(
                message: "Stromové zobrazení: Není možné pøidat položku pod sebe sama."
            );
        }
        foreach (DataTreeViewNode child in node.Nodes)
        {
            if (child != null)
            {
                CheckRecursion(node: node, parentNode: child);
            }
        }
    }

    private void SynchronizeSelection()
    {
        if (this.listManager == null | this.selectionChanging)
        {
            return;
        }

        DataTreeViewNode node =
            this.items_Positions[key: this.listManager.Position] as DataTreeViewNode;
        if (node != null)
        {
            this.SelectedNode = node;
        }
    }

    private void RefreshData(DataTreeViewNode node)
    {
        if (this.listManager == null)
        {
            return;
        }

        int position = node.Position;
        node.ID = this.GetID(position: position);
        object name = this.GetName(position: position);
        node.Text = name == null ? "" : name.ToString();
        object parentId = GetParentID(position: position);
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
        DataTreeViewNode node = new DataTreeViewNode(position: position);
        this.RefreshData(node: node);
        return node;
    }

    private object GetName(int position)
    {
        return this.nameProperty.GetValue(component: this.listManager.List[index: position]);
    }

    private object GetID(int position)
    {
        return this.idProperty.GetValue(component: this.listManager.List[index: position]);
    }

    private object GetParentID(int position)
    {
        return this.parentIdProperty.GetValue(component: this.listManager.List[index: position]);
    }

    private bool IsIDNull(object id)
    {
        if (id == null || Convert.IsDBNull(value: id))
        {
            return true;
        }

        if (id.GetType() == typeof(string))
        {
            return (((string)id).Length == 0);
        }

        if (id.GetType() == typeof(Guid))
        {
            return ((Guid)id == Guid.Empty);
        }
        return false;
    }

    protected override void InitLayout()
    {
        base.InitLayout();
        ShowScrollBar(hWnd: Handle, wBar: SB_HORZ, bShow: false);
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
