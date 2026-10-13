#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Utils;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class PackageService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    TabService tabService,
    ModelCheckService modelCheckService,
    GitNodeStatusService gitNodeStatusService
)
{
    private static readonly Guid RootPackageId = new("147FA70D-6519-4393-B5D0-87931F9FD609");

    public PackageModel Create(CreatePackageModel input)
    {
        string name = ValidateName(input.Name);
        Guid packageId = Guid.NewGuid();
        // CreatePackage loads the new package as the active one, so tabs of the previous one
        // must not survive it.
        tabService.CloseAllTabs();
        PackageHelper.CreatePackage(
            packageName: name,
            packageId: packageId,
            referencePackageId: RootPackageId
        );
        ClearCaches();
        return new PackageModel(id: packageId, name: name);
    }

    public void Delete(DeletePackageModel input)
    {
        Package package =
            schemaService.AllPackages.FirstOrDefault(candidate => candidate.Id == input.Id)
            ?? throw new UserOrigamException(Strings.Package_NotFound);

        // Checked before unloading, so a refused delete leaves the active package loaded.
        List<string> dependentPackageNames = persistenceService
            .SchemaListProvider.RetrieveList<PackageReference>()
            .Where(reference => reference.ReferencedPackageId == package.Id)
            .Select(reference => reference.Package.Name)
            .OrderBy(name => name)
            .ToList();
        if (dependentPackageNames.Count > 0)
        {
            throw new UserOrigamException(
                string.Format(
                    Strings.Package_ReferencedBy,
                    package.Name,
                    string.Join(separator: ", ", dependentPackageNames)
                )
            );
        }

        if (schemaService.ActiveSchemaExtensionId == package.Id)
        {
            tabService.CloseAllTabs();
            schemaService.UnloadSchema();
        }
        try
        {
            persistenceService.SchemaProvider.DeletePackage(package.Id);
        }
        finally
        {
            ClearCaches();
        }
    }

    private string ValidateName(string rawName)
    {
        string name = rawName?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new UserOrigamException(Strings.Package_NameEmpty);
        }
        if (FileNameRules.HasInvalidChars(name))
        {
            throw new UserOrigamException(Strings.Package_NameInvalidChars);
        }
        if (FileNameRules.IsReservedOrUnsafe(name))
        {
            throw new UserOrigamException(Strings.Package_NameReserved);
        }
        bool nameTaken = schemaService.AllPackages.Any(existing =>
            string.Equals(existing.Name?.Trim(), name, StringComparison.OrdinalIgnoreCase)
        );
        if (nameTaken)
        {
            throw new UserOrigamException(Strings.Package_NameDuplicate);
        }
        return name;
    }

    private void ClearCaches()
    {
        modelCheckService.ClearCache();
        gitNodeStatusService.ClearCache();
    }
}
