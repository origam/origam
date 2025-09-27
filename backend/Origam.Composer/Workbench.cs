#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Origam.DA.Service.MetaModelUpgrade;
using Origam.Workbench.Services;

namespace Origam.Composer;

public class Workbench(SchemaService schema)
{
    public void InitializeDefaultServices()
    {
        ServiceManager.Services.AddService(new MetaModelUpgradeService());
        ServiceManager.Services.AddService(schema);
        schema.SchemaLoaded += _schema_SchemaLoaded;
    }

    private void _schema_SchemaLoaded(object sender, bool isInteractive)
    {
        OrigamEngine.OrigamEngine.InitializeSchemaItemProviders(schema);
        var deployment = ServiceManager.Services.GetService<IDeploymentService>();
        var parameterService = ServiceManager.Services.GetService<IParameterService>();

        var isEmpty = deployment.IsEmptyDatabase();
        if (isEmpty)
        {
            deployment.Deploy();
        }

        parameterService.RefreshParameters();
        // we have to initialize the new user after parameter service gets loaded
        // otherwise it would fail generating SQL statements
        if (isEmpty)
        {
            string userName = SecurityManager.CurrentPrincipal.Identity.Name;
            IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();
            profileProvider.AddUser("Architect (" + userName + ")", userName);
        }
    }
}
