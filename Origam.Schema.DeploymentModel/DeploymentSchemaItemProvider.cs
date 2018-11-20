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
using System.Linq;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.DeploymentModel
{
	/// <summary>
	/// Summary description for WorkflowSchemaItemProvide.
	/// </summary>
	public class DeploymentSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
	{
		public DeploymentSchemaItemProvider()
		{
		}

		public DeploymentVersion CurrentVersion()
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

			foreach(DeploymentVersion version in this.ChildItems)
			{
				// only version from the current extension
				if(version.SchemaExtension.PrimaryKey.Equals(schema.ActiveExtension.PrimaryKey))
				{
					if(version.IsCurrentVersion) return version;
				}
			}

			return null;
		}

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return DeploymentVersion.ItemTypeConst;
			}
		}

		public override bool AutoCreateFolder
		{
			get
			{
				return true;
			}
		}
		public override string Group
		{
			get
			{
				return "COMMON";
			}
		}
		#endregion

		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				return "46";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Deployment";
			}
			set
			{
				base.NodeText = value;
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				return null;
			}
		}

		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[1] {typeof(DeploymentVersion)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(type == typeof(DeploymentVersion))
			{
			    List<SchemaExtension> packages = ServiceManager
			        .Services
			        .GetService<IPersistenceService>()
			        .SchemaProvider
			        .RetrieveList<SchemaExtension>(null)
			        .Where(package => package.Id != schemaExtensionId)
			        .ToList();
                    

				DeploymentVersion item = new DeploymentVersion(schemaExtensionId, packages);
				item.RootProvider = this;
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewDeploymentVersion";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorWorkflowSchedulerUnknownType"));
		}

		#endregion
	}
}
