#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using Origam.DA.ObjectPersistence;
using Origam.Schema;
using System;

namespace Origam.Workbench.Services
{
    public class PackageHelper
    {
        public static void CreatePackage(string packageName, Guid packageId, Guid referencePackageId)
        {
            IPersistenceService persistenceService = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            string versionNumber = "1.0.0";
            SchemaExtension newExtension = new SchemaExtension(new ModelElementKey(packageId));
            newExtension.PersistenceProvider = persistenceService.SchemaListProvider;
            newExtension.Name = packageName;
            newExtension.VersionString = versionNumber;
            PackageReference origamRootReference = new PackageReference();
            origamRootReference.PersistenceProvider = persistenceService.SchemaListProvider;
            origamRootReference.Package = newExtension;
            origamRootReference.ReferencedPackageId = referencePackageId;
            newExtension.Persist();
            origamRootReference.Persist();

            IDatabasePersistenceProvider dbListProvider = persistenceService.SchemaListProvider
                as IDatabasePersistenceProvider;
            if (dbListProvider != null)
            {
                dbListProvider.Update(null);
            }

            SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            schema.LoadSchema(packageId, false, false);
            foreach (ISchemaItemProvider provider in schema.Providers)
            {
                if (provider.AutoCreateFolder)
                {
                    SchemaItemGroup group = provider.NewGroup(schema.ActiveSchemaExtensionId);
                    group.Name = packageName;
                    group.Persist();

                    if (provider.GetType().Name == "DeploymentSchemaItemProvider")
                    {
                        IDeploymentService depl = ServiceManager.Services.GetService(typeof(IDeploymentService)) as IDeploymentService;
                        depl.CreateNewModelVersion(group, versionNumber, versionNumber);
                    }
                }
            }
            if (schema.SupportsSave)
            {
                schema.SaveSchema();
            }
            IDeploymentService deployment = ServiceManager.Services.GetService(typeof(IDeploymentService)) as IDeploymentService;
            deployment.Deploy();
        }
    }
}
