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
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Gui.Win.Commands
{
	public class MakeVersionCurrent : AbstractCommand
	{
		public override void Run()
		{
			WorkbenchSchemaService _schemaService = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
			IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			
			DeploymentVersion version = this.Owner as DeploymentVersion;

			Package ext = _persistence.SchemaProvider.RetrieveInstance(typeof(Package), _schemaService.ActiveExtension.PrimaryKey) as Package;

			Origam.Workbench.Commands.DeployVersion cmd3 = new Origam.Workbench.Commands.DeployVersion();
			cmd3.Run();

			ext.VersionString = version.VersionString;
			ext.Persist();
			_schemaService.ActiveExtension.Refresh();
			_schemaService.SchemaBrowser.EbrSchemaBrowser.RefreshItem(version.RootProvider);
			_schemaService.SchemaBrowser.EbrSchemaBrowser.SelectItem(version);
		}
	}
}