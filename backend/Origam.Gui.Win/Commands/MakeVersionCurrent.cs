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
			var schemaService = ServiceManager.Services.GetService<WorkbenchSchemaService>();
			DeploymentVersion version = (Owner as DeploymentVersion)!;

			var originalVersion = schemaService.ActiveExtension.VersionString;
			try
			{
				schemaService.ActiveExtension.VersionString = version.VersionString;
				Origam.Workbench.Commands.DeployVersion cmd3 =
					new Origam.Workbench.Commands.DeployVersion();
				cmd3.Run();
			}
			catch
			{
				schemaService.ActiveExtension.VersionString = originalVersion;
				throw;
			}
			schemaService.ActiveExtension.Persist();
			schemaService.SchemaBrowser.EbrSchemaBrowser.RefreshItem(version.RootProvider);
			schemaService.SchemaBrowser.EbrSchemaBrowser.SelectItem(version);
		}
	}
}