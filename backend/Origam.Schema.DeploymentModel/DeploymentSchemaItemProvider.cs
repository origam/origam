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
using System.Linq;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.DeploymentModel
{
	public class DeploymentSchemaItemProvider : AbstractSchemaItemProvider
	{
		public DeploymentSchemaItemProvider() {}

		public DeploymentVersion CurrentVersion()
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			foreach(var abstractSchemaItem in ChildItems)
			{
				var version = (DeploymentVersion)abstractSchemaItem;
				// only version from the current extension
				if(version.Package.PrimaryKey.Equals(
					   schemaService.ActiveExtension.PrimaryKey))
				{
					if(version.IsCurrentVersion)
					{
						return version;
					}
				}
			}
			return null;
		}

		#region ISchemaItemProvider Members
		public override string RootItemType => DeploymentVersion.CategoryConst;

		public override bool AutoCreateFolder => true;

		public override string Group => "COMMON";

		#endregion

		#region IBrowserNode Members

		public override string Icon => "icon_02_deployment.png";

		public override string NodeText
		{
			get => "Deployment";
			set => base.NodeText = value;
		}

		public override string NodeToolTipText => null;

		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes => new[]
		{
			typeof(DeploymentVersion)
		};

		public override T NewItem<T>(
			Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(typeof(T) != typeof(DeploymentVersion))
			{
				return base.NewItem<T>(schemaExtensionId, group);
			}
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var packages = schemaService.ActiveExtension.IncludedPackages;
			var deploymentVersion = new DeploymentVersion(
				schemaExtensionId, packages.ToList())
			{
				RootProvider = this,
				PersistenceProvider = PersistenceProvider,
				Name = "NewDeploymentVersion",
				Group = group
			};
			ChildItems.Add(deploymentVersion);
			return deploymentVersion as T;
		}

		#endregion
	}
}
