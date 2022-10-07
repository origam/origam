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
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Pads;

namespace Origam.Workbench.Services
{
	/// <summary>
	/// Summary description for WorkbenchSchemaService.
	/// </summary>
	public class WorkbenchSchemaService : SchemaService
	{
		SchemaBrowser _schemaBrowser;
		public SchemaBrowser SchemaBrowser
		{
			get
			{
				return _schemaBrowser;
			}
			set
			{
				if(_schemaBrowser != null)
				{
					_schemaBrowser.EbrSchemaBrowser.NodeClick -= new EventHandler(ebrSchemaBrowser_NodeClick);
					_schemaBrowser.EbrSchemaBrowser.NodeDoubleClick -= new EventHandler(ebrSchemaBrowser_NodeDoubleClick);
				}

				_schemaBrowser = value;
				
				if(_schemaBrowser != null)
				{
					_schemaBrowser.EbrSchemaBrowser.NodeClick += new EventHandler(ebrSchemaBrowser_NodeClick);
					_schemaBrowser.EbrSchemaBrowser.NodeDoubleClick += new EventHandler(ebrSchemaBrowser_NodeDoubleClick);
				}
			}
		}

		ExtensionPad _schemaListBrowser;
		public ExtensionPad SchemaListBrowser
		{
			get
			{
				return _schemaListBrowser;
			}
			set
			{
				_schemaListBrowser = value;
			}
		}
		public ContextMenuStrip SchemaContextMenu
		{
			get
			{
				return _schemaBrowser.EbrSchemaBrowser.ContextMenuStrip;
			}
			set
			{
                _schemaBrowser.EbrSchemaBrowser.ContextMenuStrip = value;
			}
		}

		public AbstractSchemaItem ActiveSchemaItem
		{
			get
			{
				if(_schemaBrowser?.EbrSchemaBrowser?.ActiveNode is AbstractSchemaItem)
					return _schemaBrowser.EbrSchemaBrowser.ActiveNode as AbstractSchemaItem;
				else
					return null;
			}
		}

		public IBrowserNode2 ActiveNode
		{
			get
			{
                AbstractViewContent viewContent = WorkbenchSingleton.Workbench.ActiveDocument
                    as AbstractViewContent;
                if (viewContent != null && viewContent.Focused
                    && viewContent is IBrowserNode2)
                {
                    return viewContent.Content as IBrowserNode2;
                }
                else
                {
                    return _schemaBrowser?.EbrSchemaBrowser?.ActiveNode;
                }
            }
		}


		public bool Disconnect()
		{
			if(UnloadSchema())
			{
				if(_schemaBrowser != null)
				{
					_schemaBrowser.EbrSchemaBrowser.NodeClick -= new EventHandler(ebrSchemaBrowser_NodeClick);
					_schemaBrowser.EbrSchemaBrowser.NodeDoubleClick -= new EventHandler(ebrSchemaBrowser_NodeDoubleClick);
                    _schemaBrowser.EbrSchemaBrowser.ContextMenuStrip?.Dispose();
                    _schemaBrowser.EbrSchemaBrowser.ContextMenuStrip = null;
                }
                
				_schemaBrowser = null;
				_schemaListBrowser = null;

				OrigamUserContext.Reset();

				return true;
			}
			else
			{
				return false;
			}
		}

		private void ebrSchemaBrowser_NodeClick(object sender, EventArgs e)
		{
			OnActiveNodeChanged(new EventArgs());
		}

		private void ebrSchemaBrowser_NodeDoubleClick(object sender, EventArgs e)
		{
			if(this.ActiveSchemaItem != null)
			{
				Commands.EditSchemaItem cmd = new Origam.Workbench.Commands.EditSchemaItem();
				cmd.Owner = this.ActiveSchemaItem;
				cmd.Run();
			}
		}

		override protected void SchemaProvider_InstancePersisted(object sender, IPersistent persistedObject)
		{
            if (this.SchemaBrowser != null && persistedObject is IBrowserNode browserNode)
            {
                this.SchemaBrowser.EbrSchemaBrowser.RefreshItem(browserNode);
            }
			base.SchemaProvider_InstancePersisted(sender, persistedObject);
		}

		public void SelectItem(AbstractSchemaItem schemaItem)
		{
			SchemaBrowser.EbrSchemaBrowser.SelectItem(schemaItem);
		}
	}
}
