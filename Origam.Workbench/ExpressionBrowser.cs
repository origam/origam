#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Commands;
using Origam.Workbench.Services;

namespace Origam.Workbench
{
	/// <summary>
	/// Summary description for ExpressionBrowser.
	/// </summary>
	public class ExpressionBrowser : System.Windows.Forms.UserControl
	{
		private Origam.UI.NativeTreeView tvwExpressionBrowser;
		public event FilterEventHandler QueryFilterNode;
		public event EventHandler ExpressionSelected;
		public event EventHandler NodeClick;
		public event EventHandler NodeDoubleClick;
		public event EventHandler NodeUnderMouseChanged;
		public System.Windows.Forms.ImageList imgList;
		private System.Windows.Forms.ComboBox cboFilter;
		private System.ComponentModel.IContainer components;
		private delegate void TreeViewDelegate(TreeNode nod);
		private TreeNode mNodSpecial = new TreeNode("_Special");
		private Rectangle dragBoxFromMouseDown;

		private IDocumentationService _documentationService;
		private SchemaService _schemaService;
		private System.Windows.Forms.ToolTip toolTip1;
		private bool _refreshPaused = false;
		private bool _disposed = false;
		private Font _boldFont;

		private Hashtable _customImages = new Hashtable();

		public ExpressionBrowser()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			tvwExpressionBrowser.ItemHeight = 18;
			_boldFont = new Font(tvwExpressionBrowser.Font, FontStyle.Bold);
			// handle the exception because of the WinForms Designer
			try
			{
				_documentationService = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;
				_schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
			}
			catch {}
		}

		public void RefreshRootNodeText()
		{
			TreeNode rootNode = tvwExpressionBrowser.RootNode;
			if (rootNode == null) return;
			var rootNodeTag = (SchemaExtension) rootNode.Tag;
			rootNode.Text = rootNodeTag.Name;
		}

        public TreeNode GetFirstNode()
        {
            return tvwExpressionBrowser.Nodes.Count>0? tvwExpressionBrowser.Nodes[0]:null;
        }
		public void ReloadTreeAndRestoreExpansionState()
		{
			if (_schemaService.ActiveExtension == null) return;
            tvwExpressionBrowser.StoreExpansionState();
            RemoveAllNodes();
            _schemaService.ClearProviderCaches();
            AddRootNode(_schemaService.ActiveExtension);
            tvwExpressionBrowser.RestoreExpansionState();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				components?.Dispose();
			}
			base.Dispose( disposing );

			_disposed = true;
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ExpressionBrowser));
			this.tvwExpressionBrowser = new Origam.UI.NativeTreeView();
			this.imgList = new System.Windows.Forms.ImageList(this.components);
			this.cboFilter = new System.Windows.Forms.ComboBox();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// tvwExpressionBrowser
			// 
			this.tvwExpressionBrowser.AllowDrop = true;
			this.tvwExpressionBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tvwExpressionBrowser.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tvwExpressionBrowser.FullRowSelect = true;
            this.tvwExpressionBrowser.HideSelection = false;
			this.tvwExpressionBrowser.ImageList = this.imgList;
			this.tvwExpressionBrowser.Location = new System.Drawing.Point(0, 0);
			this.tvwExpressionBrowser.Name = "tvwExpressionBrowser";
			this.tvwExpressionBrowser.ShowLines = false;
			this.tvwExpressionBrowser.Size = new System.Drawing.Size(174, 144);
			this.tvwExpressionBrowser.TabIndex = 0;
			this.tvwExpressionBrowser.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvwExpressionBrowser_BeforeLabelEdit);
			this.tvwExpressionBrowser.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvwExpressionBrowser_BeforeExpand);
			this.tvwExpressionBrowser.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvwExpressionBrowser_MouseDown);
			this.tvwExpressionBrowser.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.tvwExpressionBrowser_AfterCollapse);
			this.tvwExpressionBrowser.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tvwExpressionBrowser_KeyPress);
			this.tvwExpressionBrowser.DragOver += new System.Windows.Forms.DragEventHandler(this.tvwExpressionBrowser_DragOver);
			this.tvwExpressionBrowser.MouseHover += new System.EventHandler(this.tvwExpressionBrowser_MouseHover);
			this.tvwExpressionBrowser.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvwExpressionBrowser_DragDrop);
			this.tvwExpressionBrowser.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwExpressionBrowser_AfterSelect);
			this.tvwExpressionBrowser.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvwExpressionBrowser_AfterLabelEdit);
			this.tvwExpressionBrowser.Click += new System.EventHandler(this.tvwExpressionBrowser_Click);
			this.tvwExpressionBrowser.DoubleClick += new System.EventHandler(this.tvwExpressionBrowser_DoubleClick);
			this.tvwExpressionBrowser.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tvwExpressionBrowser_MouseMove);
			// 
			// imgList
			// 
			this.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imgList.ImageSize = new System.Drawing.Size(16, 16);
			this.imgList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList.ImageStream")));
			this.imgList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// cboFilter
			// 
			this.cboFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cboFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboFilter.Location = new System.Drawing.Point(0, 0);
			this.cboFilter.Name = "cboFilter";
			this.cboFilter.Size = new System.Drawing.Size(176, 21);
			this.cboFilter.TabIndex = 1;
			this.cboFilter.Visible = false;
			this.cboFilter.SelectedIndexChanged += new System.EventHandler(this.cboFilter_SelectedIndexChanged);
			// 
			// toolTip1
			// 
			this.toolTip1.AutoPopDelay = 20000;
			this.toolTip1.InitialDelay = 500;
			this.toolTip1.ReshowDelay = 100;
			// 
			// ExpressionBrowser
			// 
			this.Controls.Add(this.cboFilter);
			this.Controls.Add(this.tvwExpressionBrowser);
			this.Name = "ExpressionBrowser";
			this.Size = new System.Drawing.Size(176, 144);
			this.BackColorChanged += new System.EventHandler(this.ExpressionBrowser_BackColorChanged);
			this.ResumeLayout(false);

		}
		#endregion

		protected virtual void OnExpressionSelected(System.EventArgs e) 
		{
			//Invokes the delegates.
			ExpressionSelected?.Invoke(this, e);
		}

		protected virtual void OnNodeClick(System.EventArgs e) 
		{
			//Invokes the delegates.
			NodeClick?.Invoke(this, e);
		}

		private bool _inNodeDoubleClick = false;

		protected virtual void OnNodeDoubleClick(System.EventArgs e) 
		{
			if(_inNodeDoubleClick) return;

			if (NodeDoubleClick != null) 
			{
				_inNodeDoubleClick = true;

				try
				{
					//Invokes the delegates.
					NodeDoubleClick(this, e); 
				}
				catch(Exception ex)
				{
					AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorArchitectCommand"), ex);
				}
				finally
				{
					_inNodeDoubleClick = false;
				}
			}
		}

		protected virtual void OnNodeUnderMouseChanged(System.EventArgs e) 
		{
			if (NodeUnderMouseChanged != null) 
			{
				//Invokes the delegates.
				NodeUnderMouseChanged(this, e); 
			}
		}

		private void tvwExpressionBrowser_DoubleClick(object sender, System.EventArgs e)
		{
			if (tvwExpressionBrowser.SelectedNode !=null)
				if (tvwExpressionBrowser.SelectedNode.Tag !=null)
					if(tvwExpressionBrowser.SelectedNode.Tag is IBrowserNode)
					{
						OnNodeDoubleClick(new EventArgs());
					}
		}
	

		public IBrowserNode2 ActiveNode
		{
			get
			{
				IBrowserNode2 node = null;
				if ((tvwExpressionBrowser.SelectedNode != null) && (tvwExpressionBrowser.SelectedNode.Tag !=null))
					if(tvwExpressionBrowser.SelectedNode.Tag is IBrowserNode2)
					{
						node = tvwExpressionBrowser.SelectedNode.Tag as IBrowserNode2;
					}

				return node;
			}
		}

		bool mbShowFilter = false;
		public bool ShowFilter
		{
			get => mbShowFilter;
			set
			{
				mbShowFilter = value;
				
				cboFilter.Visible = mbShowFilter;
				
				if(cboFilter.Visible)
				{
					tvwExpressionBrowser.Top = cboFilter.Height;
					tvwExpressionBrowser.Height = this.Height - cboFilter.Height;
				}
				else
				{
					tvwExpressionBrowser.Top = 0;
					tvwExpressionBrowser.Height = this.Height;
				}
			}
		}

		public bool AllowEdit
		{
			get => tvwExpressionBrowser.LabelEdit;
			set => tvwExpressionBrowser.LabelEdit = value;
		}

		public bool DisableOtherExtensionNodes { get; set; } = true;

		public bool CheckSecurity { get; set; } = false;

		public TreeNode NodeUnderMouse { get; set; } = null;

		private bool _loadingTree = false;

		private void LoadTree(TreeNode parentNode)
		{
			try
			{
				_loadingTree = true;
				LoadTreeRecursive(parentNode);
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ResourceUtils.GetString("ErrorWhenReadChildNodes", parentNode.FullPath, Environment.NewLine + Environment.NewLine + ex.Message),
					ResourceUtils.GetString("ErrorTitle"), ex);
			}
			finally
			{
				_loadingTree = false;
			}
		}

		private bool Filter(IBrowserNode node)
		{
			if(QueryFilterNode != null)
			{
				ExpressionBrowserEventArgs e = new ExpressionBrowserEventArgs(node);
				QueryFilterNode(this, e);
				
				return e.Filter;
			}
			return false;
		}

		private void LoadTreeRecursive(TreeNode parentNode)
		{
			try
			{
				tvwExpressionBrowser.BeginUpdate();

				if(this.IsDisposed) return;

				// remove any child nodes, we will refresh them anyway
				parentNode.Nodes.Clear();

				IBrowserNode bnode = parentNode.Tag as IBrowserNode;

				if(bnode != null && HasChildNodes(bnode))
				{
					ArrayList childNodes = new ArrayList(bnode.ChildNodes());
					childNodes.Sort();

					foreach(IBrowserNode2 child in childNodes)
					{
						bool filtered = Filter(child);
						
						if(! filtered)
						{
							bool isAuthorized = true;

							if(this.CheckSecurity)
							{
								// check if user has access to this item, if not, we don't display it
								if(child is IAuthorizationContextContainer)
								{
									IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
									if(! authorizationProvider.Authorize(SecurityManager.CurrentPrincipal, (child as IAuthorizationContextContainer).AuthorizationContext))
									{
										isAuthorized = false;
									}
								}
							}

							if(isAuthorized & !child.Hide)
							{
								TreeNode tnode = RenderBrowserNode(child);
								//add dummy child node, because our node has some children
								if(HasChildNodes(child))
								{
									tnode.Nodes.Add(new DummyNode());
								}
								parentNode.Nodes.Add(tnode);
							}
						}
					}
				}
			}
			finally
			{
				tvwExpressionBrowser.EndUpdate();
			}
		}

		private void cboFilter_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

		private void tvwExpressionBrowser_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			TreeView tree = sender as TreeView;

			if(tree.GetNodeAt(e.X, e.Y) != null)
				tree.SelectedNode = tree.GetNodeAt(e.X, e.Y);

			// Starts a drag-and-drop operation with that item.
			if(e.Button == MouseButtons.Left)
			{
				Size dragSize = SystemInformation.DragSize;

				// Create a rectangle using the DragSize, with the mouse position being
				// at the center of the rectangle.
				dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width /2),
					e.Y - (dragSize.Height /2)), dragSize);
			}
			else
				// Reset the rectangle if the mouse is not over an item in the ListBox.
				dragBoxFromMouseDown = Rectangle.Empty;
			
		}

		private void tvwExpressionBrowser_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			LoadTree(e.Node);
		}

		private void tvwExpressionBrowser_AfterCollapse(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if(e.Node != mNodSpecial)
			{
				e.Node.Nodes.Clear();
				e.Node.Nodes.Add(new DummyNode());
			}
		}

		private void tvwExpressionBrowser_AfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
		{
			if (e.Label == null) return;
			if (!(e.Node.Tag is IBrowserNode bn))
			{
				e.CancelEdit = true;
				return;
			}
			if (bn is SchemaItemGroup group &&
			    !group.CanRenameTo(e.Label))
			{
                e.CancelEdit = true;
                return;
			}
			_refreshPaused = true;
		    IPersistenceProvider persistenceProvider =
		        ServiceManager.Services.GetService<IPersistenceService>().SchemaProvider;

		    persistenceProvider.BeginTransaction();
            bn.NodeText = e.Label;
		    persistenceProvider.EndTransaction();
            _refreshPaused = false;
		}

		private void tvwExpressionBrowser_BeforeLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
		{
			IBrowserNode bnode = (IBrowserNode)e.Node.Tag;

			if(! (bnode != null && bnode.CanRename && _schemaService.IsItemFromExtension(bnode) & this.DisableOtherExtensionNodes))
			{
				e.CancelEdit = true;
			}
		}

		private TreeNode RenderBrowserNode(IBrowserNode2 bnode)
		{
			int imageIndex = ImageIndex(bnode);
			TreeNode tnode = new TreeNode(bnode.NodeText, imageIndex, imageIndex);
			tnode.Tag = bnode;
            tnode.NodeFont = GetFont(bnode);
			RecolorNode(tnode);
			
			return tnode;
		}

        private Font GetFont(IBrowserNode2 bnode)
        {
            if (bnode.FontStyle.ToFont() == FontStyle.Bold)
            {
                return _boldFont;
            }
            else
            {
                return new Font(tvwExpressionBrowser.Font, bnode.FontStyle.ToFont());
            }
        }

		private void RecolorNode(TreeNode node)
		{
#if ORIGAM_CLIENT
#else
			Origam.DA.ObjectPersistence.IPersistent item = node.Tag as Origam.DA.ObjectPersistence.IPersistent;
			node.BackColor = tvwExpressionBrowser.BackColor;
			node.ForeColor = Color.Black;

			if(item != null)
			{
				if(this.DisableOtherExtensionNodes & ! _schemaService.IsItemFromExtension(item))
				{
					node.ForeColor = Color.Gray;
				}

				Pads.FindSchemaItemResultsPad resultsPad = 
					WorkbenchSingleton.Workbench.GetPad(typeof(Pads.FindSchemaItemResultsPad)) as Pads.FindSchemaItemResultsPad;
                if (resultsPad != null)
                {
                    foreach (ISchemaItem result in resultsPad.Results)
                    {
                        IBrowserNode bnode = item as IBrowserNode;

                        if (result.PrimaryKey.Equals(item.PrimaryKey) ||
                            (bnode.GetType() == result.GetType()
                            && bnode != null
                            && !bnode.HasChildNodes
                            && result.RootItem.PrimaryKey.Equals(item.PrimaryKey)))
                        {
                            node.BackColor = OrigamColorScheme.TabActiveStartColor;
                            node.ForeColor = OrigamColorScheme.TabActiveForeColor;
                            break;
                        }
                    }
                }
			}
			if(node.Tag is SchemaItemProviderGroup)
			{
				node.BackColor = Color.FromArgb(150, 150, 150);
				node.ForeColor = Color.White;
				node.NodeFont = _boldFont;
			}
			if(node.Tag is AbstractSchemaItemProvider)
			{
				node.BackColor = OrigamColorScheme.FormBackgroundColor;
				node.NodeFont = _boldFont;
			}
#endif
		}

		private int ImageIndex(IBrowserNode bnode)
		{
			int imageIndex = -1;

			if(bnode is IBrowserNode2 && (bnode as IBrowserNode2).NodeImage != null)
			{
				Image nodeImage = (bnode as IBrowserNode2).NodeImage.ToBitmap();

				if(_customImages.Contains(bnode))
				{
					imgList.Images[(int)_customImages[bnode]] = nodeImage;
					imageIndex = (int)_customImages[bnode];
				}

				if(imageIndex == -1)
				{
					imageIndex = imgList.Images.Add(nodeImage, Color.Magenta);
					_customImages.Add(bnode, imageIndex);
				}
			}
			else
			{
				imageIndex = Convert.ToInt32(bnode.Icon);
			}

			return imageIndex;
		}

		public void AddRootNode(IBrowserNode2 node)
		{	
			TreeNode tnode = RenderBrowserNode(node);

			try
			{
				if(HasChildNodes(node))
				{
					tnode.Nodes.Add(new DummyNode());
				}

				this.tvwExpressionBrowser.Nodes.Add(tnode);
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ResourceUtils.GetString("ErrorWhenAddRoot", node.NodeText, Environment.NewLine + Environment.NewLine + ex.Message), 
					ResourceUtils.GetString("ErrorTitle"), ex);
			}
			tnode.Expand();
		}

		public void RemoveAllNodes()
		{
			if(! _disposed)
			{
				tvwExpressionBrowser.Nodes.Clear();
			}
		}

		public void RemoveBrowserNode(IBrowserNode2 browserNode)
		{
			TreeNode foundNode = null;

			foreach(TreeNode node in tvwExpressionBrowser.Nodes)
			{
				foundNode = LookUpNode(node, browserNode);

				if(foundNode != null) foundNode.Remove();
			}
		}

		public void RefreshAllNodes()
		{
			tvwExpressionBrowser.BeginUpdate();
			foreach(TreeNode node in tvwExpressionBrowser.Nodes)
			{
				LoadTree(node);
			}
			tvwExpressionBrowser.EndUpdate();
		}

		public void Redraw()
		{
			tvwExpressionBrowser.BeginUpdate();
			RecolorNodes(tvwExpressionBrowser.Nodes);
			tvwExpressionBrowser.EndUpdate();
		}
		
		private void RecolorNodes(TreeNodeCollection nodes)
		{
			foreach(TreeNode node in nodes)
			{
				RecolorNode(node);
				RecolorNodes(node.Nodes);
			}
		}

		public void RefreshActiveNode()
		{
			if(this.tvwExpressionBrowser.SelectedNode != null)
			{
				RefreshNode(tvwExpressionBrowser.SelectedNode);
			}
		}

		private bool HasChildNodes(IBrowserNode node)
		{
			if(QueryFilterNode == null)
			{
				return node.HasChildNodes;
			}
			else
			{
				foreach(IBrowserNode child in node.ChildNodes())
				{
					if(! Filter(child))
					{
						return true;
					}
				}
				return false;
			}
		}

		private void RefreshNode(TreeNode treeNode)
		{
			if(_refreshPaused) return;

			IBrowserNode2 node = treeNode.Tag as IBrowserNode2;

			if(node != null)
			{
				treeNode.Text = node.NodeText;
				treeNode.ImageIndex = ImageIndex(node);
				treeNode.SelectedImageIndex = treeNode.ImageIndex;
                treeNode.NodeFont = GetFont(node);
            }

			// after the node refresh is requested (because it was updated)
			// we reset cache on all the parent nodes because otherwise
			// they could return back the old--cached--version when asked
			// for child items (e.g. after collapsing and re-expanding them)
			TreeNode parent = treeNode;
			while(parent != null)
			{
				AbstractSchemaItem item = parent.Tag as AbstractSchemaItem;
				if(item != null)
				{
                    if (item.ClearCacheOnPersist)
                    {
                        item.ClearCache();
                    }
				}
				parent = parent.Parent;
			}

			LoadTree(treeNode);
		}

		private TreeNode LookUpNode(TreeNode parentNode, IBrowserNode browserNode)
		{
			TreeNodeCollection collection;

			if(parentNode == null)
			{
				collection = this.tvwExpressionBrowser.Nodes;
			}
			else
			{
				// this is the node, we return
				if(parentNode.Tag == browserNode)
					return parentNode;

				// we try to compare the key
				if(parentNode.Tag is DA.ObjectPersistence.IPersistent && browserNode is DA.ObjectPersistence.IPersistent && (parentNode.Tag as DA.ObjectPersistence.IPersistent).PrimaryKey.Equals((browserNode as DA.ObjectPersistence.IPersistent).PrimaryKey))
					return parentNode;

				collection = parentNode.Nodes;
			}

			// we go to each child
			foreach(TreeNode node in collection)
			{
				// this is the child, we return
				if(node.Tag == browserNode)
					return node;

				if(node.Tag is DA.ObjectPersistence.IPersistent && browserNode is DA.ObjectPersistence.IPersistent && (node.Tag as DA.ObjectPersistence.IPersistent).PrimaryKey.Equals((browserNode as DA.ObjectPersistence.IPersistent).PrimaryKey))
					return node;

				// we try to find in child nodes of this node
				TreeNode foundNode = null;

				if(node.Nodes.Count > 0)
					foundNode = LookUpNode(node, browserNode);

				// we found it in child nodes
				if(foundNode != null)
					return foundNode;

				// we did not find, so we go to next node
			}

			return null;
		}

		private void tvwExpressionBrowser_Click(object sender, System.EventArgs e)
		{
			OnNodeClick(new EventArgs());
		}

		private void SchemaItem_Deleted(object sender, EventArgs e)
		{
			RemoveBrowserNode(sender as IBrowserNode2);
		}

		private void tvwExpressionBrowser_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(! AllowEdit) return;

			bool success = false;
			bool renameCopy = true;

			TreeNode node = e.Data.GetData(typeof(TreeNode)) as TreeNode;

			// if it was anything ELSE than TreeNode, we exit
			if(node == null) return;

			TreeNode dropNode = (sender as TreeView).GetNodeAt((sender as TreeView).PointToClient(new Point(e.X, e.Y))) as TreeNode;

			ISchemaItem item;
			ISchemaItem originalItem = (node.Tag as ISchemaItem); //.GetFreshItem() as ISchemaItem;
			
			if(e.Effect == DragDropEffects.Copy)
			{
				item = originalItem.Clone() as ISchemaItem;
			}
			else
			{
				item = originalItem;
			}

			if(item == null) return;

			if(dropNode.Tag == item.RootProvider)
			{
				// Moving schema item to the root (no group)
				item.Group = null;
				success = true;
			}
			else if(dropNode.Tag is SchemaItemGroup)
			{
				// Moving schema item between groups
				if((dropNode.Tag as SchemaItemGroup).ParentItem == item.ParentItem 
					&& (dropNode.Tag as SchemaItemGroup).RootProvider == item.RootProvider)
				{
					item.Group = dropNode.Tag as SchemaItemGroup;
					success = true;
				}
			}
			else
			{
				AbstractSchemaItem dropElement = dropNode.Tag as AbstractSchemaItem;

				if(item.CanMove(dropElement))
				{
						if(item != dropElement)		// cannot move to itself
						{
							item.ParentNode = dropElement;

							if(dropElement.IsAbstract && ! item.IsAbstract)
							{
								item.IsAbstract = true;
							}

							success = true;
							
							if(item.ParentNode != dropElement.ParentNode)
							{
								renameCopy = false;
							}
						}
				}
			}

			if(success)
			{
				if(e.Effect == DragDropEffects.Copy)
				{
					item.SetExtensionRecursive(_schemaService.ActiveExtension);

					if(renameCopy)
					{
						item.Name = ResourceUtils.GetString("CopyOf", item.Name);
					}
				}
				else
				{
					if(node.Parent != null)	node.Remove();
					MoveItemFile(item);
				}

				LoadTree(dropNode);

				if(e.Effect == DragDropEffects.Copy)
				{
					Origam.Workbench.Commands.EditSchemaItem edit = new Origam.Workbench.Commands.EditSchemaItem();
					edit.Owner = item;
					edit.Run();
				}
			}
		}

		private static void MoveItemFile(ISchemaItem item)
		{
			IPersistenceService persistence =
				ServiceManager.Services.GetService(typeof(IPersistenceService)) as
					IPersistenceService;
			persistence.SchemaProvider.BeginTransaction();
			item.Persist();
			persistence.SchemaProvider.EndTransaction();
		}

		private void tvwExpressionBrowser_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			TreeNode nodeUnderMouse = tvwExpressionBrowser.GetNodeAt(tvwExpressionBrowser.PointToClient(new Point( e.X, e.Y)));

			if(nodeUnderMouse != null)
			{
				if(tvwExpressionBrowser.TopNode == nodeUnderMouse)
				{
					if(tvwExpressionBrowser.TopNode.PrevVisibleNode != null)
					{
						tvwExpressionBrowser.TopNode.PrevVisibleNode.EnsureVisible();
					}
				}

				if(nodeUnderMouse.NextVisibleNode != null && nodeUnderMouse.NextVisibleNode.IsVisible == false)
				{
					nodeUnderMouse.NextVisibleNode.EnsureVisible();
				}
			}

			TreeNode node = e.Data.GetData(typeof(TreeNode)) as TreeNode;

			if(! AllowEdit) return;

			bool isCopy = (e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy;

			// if it was anything ELSE than TreeNode, we exit
			if(node == null) return;

			// if the node is not from the current extension, we cannot move it, so we exit
			if(! isCopy)
			{
				if(! _schemaService.CanEditItem(node.Tag))
				{
					return;
				}
			}

			// Moving schema item between groups
			if(node.Tag is ISchemaItem)
			{
				ISchemaItem item = node.Tag as ISchemaItem;
				TreeNode dropNode = (sender as TreeView).GetNodeAt((sender as TreeView).PointToClient(new Point(e.X, e.Y))) as TreeNode;
	
				if(dropNode.Tag == item.RootProvider)
				{
					// we can move an item to the root -> group = null
					e.Effect = isCopy ? DragDropEffects.Copy : DragDropEffects.Move;
					return;
				}
				else if(dropNode.Tag is SchemaItemGroup)
				{
					if((dropNode.Tag as SchemaItemGroup).ParentItem == item.ParentItem 
						&& (dropNode.Tag as SchemaItemGroup).RootProvider == item.RootProvider)
					{
						e.Effect = isCopy ? DragDropEffects.Copy : DragDropEffects.Move;
						return;
					}
				}
				else
				{
					if(item.CanMove(dropNode.Tag as IBrowserNode2))
					{
						e.Effect = isCopy ? DragDropEffects.Copy : DragDropEffects.Move;
						return;
					}
				}
			}
			
			e.Effect = DragDropEffects.None;
		}

		private void tvwExpressionBrowser_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			TreeNode nodeUnderMouse = tvwExpressionBrowser.GetNodeAt(e.X, e.Y);

			if(! AllowEdit) return;

			if(NodeUnderMouse != nodeUnderMouse)
			{
				NodeUnderMouse = nodeUnderMouse;
				OnNodeUnderMouseChanged(EventArgs.Empty);
			}

			if((sender as TreeView).SelectedNode == null) return;

			if ((e.Button & MouseButtons.Left) == MouseButtons.Left) 
			{

				// If the mouse moves outside the rectangle, start the drag.
				if (dragBoxFromMouseDown != Rectangle.Empty && 
					!dragBoxFromMouseDown.Contains(e.X, e.Y)) 
				{
					
					(sender as TreeView).DoDragDrop((sender as TreeView).SelectedNode, DragDropEffects.Move | DragDropEffects.Copy);
				}
			}

		}

		private void child_Changed(object sender, EventArgs e)
		{
			if(_loadingTree) return; // do not listen to events, while loading the tree

			IBrowserNode node = sender as IBrowserNode;

			TreeNode tnode = LookUpNode(null, node);

			if(tnode == null) return;

			RefreshNode(tnode);
		}

		private void tvwExpressionBrowser_KeyPress(object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar == (char)Keys.Enter)
			{
				tvwExpressionBrowser_DoubleClick(sender, EventArgs.Empty);
				e.Handled = true;
			}
		}

		private void tvwExpressionBrowser_MouseHover(object sender, EventArgs e)
		{
		}

		private void tvwExpressionBrowser_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if(! _loadingTree)
			{
				OnNodeClick(EventArgs.Empty);
			}
		}

		public void SetToolTip(string text)
		{
			toolTip1.SetToolTip(tvwExpressionBrowser, text);
		}

		public void SelectItem(AbstractSchemaItem item)
		{
			ArrayList items = new ArrayList();

			AbstractSchemaItem parentItem = item;

			while(parentItem != null)
			{
				items.Add(parentItem);
				parentItem = parentItem.ParentItem;
			}

			SchemaItemGroup parentGroup = (items[items.Count-1] as AbstractSchemaItem).Group;

			while(parentGroup != null)
			{
				items.Add(parentGroup);
				parentGroup = parentGroup.ParentGroup;
			}

			TreeNode foundNode = null;
          
            if (tvwExpressionBrowser.Nodes.Count == 1)
			{
				if(! tvwExpressionBrowser.Nodes[0].IsExpanded)
				{
					tvwExpressionBrowser.Nodes[0].Expand();
				}

				foreach(TreeNode modelGroupNode in tvwExpressionBrowser.Nodes[0].Nodes)
				{
					foreach(IBrowserNode provider in (modelGroupNode.Tag as IBrowserNode).ChildNodes())
					{
						foreach(IBrowserNode firstChild in provider.ChildNodes())
						{
							if(firstChild is DA.ObjectPersistence.IPersistent)
							{
								Key key = (firstChild as DA.ObjectPersistence.IPersistent).PrimaryKey;

								if(key.Equals((items[items.Count-1] as DA.ObjectPersistence.IPersistent).PrimaryKey))
								{
									modelGroupNode.Expand();
									foreach(TreeNode providerNode in modelGroupNode.Nodes)
									{
										if(providerNode.Tag == provider)
										{
											providerNode.Expand();
											break;
										}
									}
									foundNode = LookUpNode(null, firstChild);
									break;
								}
							}
						}
					}
					if(foundNode != null) break;
				}
			}

			if(foundNode == null) return; //don't throw this exception, because sometimes we don't find the deepest item in the tree, e.g. with forms - throw new ArgumentOutOfRangeException("item", item, "Schema item not found in the model!");

			for(int i = items.Count - 2; i >= 0; i--)
			{
				foundNode.Expand();
				TreeNode node = LookUpNode(foundNode, items[i] as IBrowserNode);
				
				// node not found, we try to find it in its subitems
				if(node == null)
				{
					foreach(TreeNode ch in foundNode.Nodes)
					{
						ch.Expand();

						node = LookUpNode(ch, items[i] as IBrowserNode);
						if(node != null) break;

						ch.Collapse();
					}

					if(node == null) break;
				}
				
				foundNode = node;
			}

			foundNode.EnsureVisible();
			tvwExpressionBrowser.SelectedNode = foundNode;
		}

		public void RefreshItem(IBrowserNode node)
		{
            if(node is null)
            {
                return;
            }
            if (! IsHandleCreated)
            {
                return;
            }
            TreeNode tnode = null;
			try
			{
				tvwExpressionBrowser.BeginUpdate();

				bool expandNode = false;

				tnode = LookUpNode(null, node);

				if(node is DA.ObjectPersistence.IPersistent && (node as DA.ObjectPersistence.IPersistent).IsDeleted)
				{
					if(tnode != null)
					{
						tnode.Remove();
					}
					return;
				}

				if(tnode == null) 
				{
					// node was not found, we try to find its parent
					if(node is ISchemaItem)
					{
						ISchemaItem item = node as ISchemaItem;
						IBrowserNode parent = item.ParentItem;
						if(parent == null)
						{
							parent = item.Group;
						}
						if(parent == null)
						{
							parent = item.RootProvider;
						}

						tnode = LookUpNode(null, parent);
					}

					if(tnode == null) return;

					expandNode = true;
				}
				else
				{
					// node was found, so we refresh the inner pointer to the model element
					tnode.Tag = node;
				}

				RefreshNode(tnode);

				if(expandNode)
				{
					tnode.Expand();

//					// try to find a subfolder, if one exists and expand it
					string subfolderName = SchemaItemFolderDescription(node.GetType());

					foreach(TreeNode subnode in tnode.Nodes)
					{
						if(subnode.Tag is NonpersistentSchemaItemNode & subnode.Text == subfolderName)
						{
							subnode.Expand();
							break;
						}
					}
					tnode = LookUpNode(null, node);
				}
			}
			finally
			{
				tvwExpressionBrowser.EndUpdate();
			}
            if (tnode != null)
            {
                tnode.EnsureVisible();
                tvwExpressionBrowser.SelectedNode = tnode;
            }
        }

		private string SchemaItemFolderDescription(Type type)
		{
			object[] attributes = type.GetCustomAttributes(typeof(SchemaItemDescriptionAttribute), true);

			if(attributes != null && attributes.Length > 0)
				return (attributes[0] as SchemaItemDescriptionAttribute).FolderName;
			else
				return "";

		}

		private void ExpressionBrowser_BackColorChanged(object sender, System.EventArgs e)
		{
			tvwExpressionBrowser.BackColor = this.BackColor;
		}

	}

	public delegate void FilterEventHandler(object sender, ExpressionBrowserEventArgs e);
	
	public class ExpressionBrowserEventArgs : System.EventArgs
	{
		public ExpressionBrowserEventArgs(object queriedObject)
		{
			QueriedObject = queriedObject;
		}

		public object QueriedObject { get; }
		public bool Filter { get; set; } = false;
	}
}
