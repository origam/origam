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
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class PackageReferenceService(
    SchemaService schemaService,
    IPersistenceService persistenceService,
    TabService tabService,
    ModelCheckService modelCheckService,
    ModelTransactionRunner transactionRunner
)
{
    public PackageReferencesInfo GetReferences()
    {
        Package package = RequireActivePackage();
        HashSet<Guid> referencedIds = package
            .References.Select(reference => reference.ReferencedPackageId)
            .ToHashSet();
        return new PackageReferencesInfo
        {
            PackageId = package.Id,
            Candidates = schemaService
                .AllPackages.Where(candidate => candidate.Id != package.Id)
                .OrderBy(candidate => candidate.Name)
                .Select(candidate => new PackageReferenceCandidate
                {
                    Id = candidate.Id,
                    Name = candidate.Name,
                    IsReferenced = referencedIds.Contains(candidate.Id),
                    CreatesCycle = Includes(candidate, package.Id),
                })
                .ToList(),
        };
    }

    public void UpdateReferences(UpdatePackageReferencesModel input)
    {
        Package package = RequireActivePackage();
        if (input.PackageId != package.Id)
        {
            throw new UserOrigamException(Strings.Package_ReferencesOnlyActive);
        }

        Dictionary<Guid, Package> allPackages = schemaService.AllPackages.ToDictionary(candidate =>
            candidate.Id
        );
        List<Package> referencedPackages = input
            .ReferencedPackageIds.Distinct()
            .Select(id =>
                allPackages.TryGetValue(id, out Package referenced)
                    ? referenced
                    : throw new UserOrigamException(Strings.Package_NotFound)
            )
            .ToList();
        foreach (Package referenced in referencedPackages)
        {
            if (referenced.Id == package.Id)
            {
                throw new UserOrigamException(Strings.Package_ReferenceSelf);
            }
            // A cycle makes Package.IncludedPackages recurse until the server crashes.
            if (Includes(referenced, package.Id))
            {
                throw new UserOrigamException(
                    string.Format(Strings.Package_ReferenceCycle, referenced.Name, package.Name)
                );
            }
        }

        List<PackageReference> currentReferences = package.References;
        HashSet<Guid> newReferencedIds = referencedPackages
            .Select(referenced => referenced.Id)
            .ToHashSet();
        List<PackageReference> removedReferences = currentReferences
            .Where(reference => !newReferencedIds.Contains(reference.ReferencedPackageId))
            .ToList();
        List<Package> addedPackages = referencedPackages
            .Where(referenced =>
                currentReferences.All(reference => reference.ReferencedPackageId != referenced.Id)
            )
            .ToList();
        if (removedReferences.Count == 0 && addedPackages.Count == 0)
        {
            return;
        }
        CheckNoLostDependenciesOrThrow(package, referencedPackages);

        tabService.CloseAllTabs();
        transactionRunner.Run(() =>
        {
            foreach (PackageReference reference in removedReferences)
            {
                reference.Delete();
            }
            foreach (Package referenced in addedPackages)
            {
                PackageReference reference = new PackageReference
                {
                    PersistenceProvider = persistenceService.SchemaListProvider,
                };
                reference.Package = package;
                reference.ReferencedPackage = referenced;
                reference.Persist();
            }
        });
        // The loaded providers only know the packages included before the change.
        schemaService.LoadSchema(package.Id);
        modelCheckService.ClearCache();
    }

    private Package RequireActivePackage()
    {
        return schemaService.ActiveExtension
            ?? throw new UserOrigamException(Strings.Package_ReferencesOnlyActive);
    }

    // A package stays reachable when another remaining reference includes it, so only the
    // packages that drop out of the whole included set are checked.
    private void CheckNoLostDependenciesOrThrow(Package package, List<Package> referencedPackages)
    {
        HashSet<Guid> stillIncluded = referencedPackages
            .SelectMany(referenced =>
                referenced.IncludedPackages.Select(included => included.Id).Append(referenced.Id)
            )
            .ToHashSet();
        HashSet<Guid> lostPackageIds = package
            .IncludedPackages.Select(included => included.Id)
            .Where(id => !stillIncluded.Contains(id))
            .ToHashSet();
        if (lostPackageIds.Count == 0)
        {
            return;
        }

        List<ISchemaItem> items =
            persistenceService.SchemaListProvider.RetrieveListByPackage<ISchemaItem>(package.Id);
        foreach (ISchemaItem item in items)
        {
            ISchemaItem lostDependency = item.GetDependencies(ignoreErrors: true)
                .Append(item.ParentItem)
                .FirstOrDefault(dependency =>
                    dependency != null && lostPackageIds.Contains(dependency.SchemaExtensionId)
                );
            if (lostDependency != null)
            {
                throw new UserOrigamException(
                    string.Format(
                        Strings.Package_ReferenceStillUsed,
                        lostDependency.Package.Name,
                        item.Path,
                        lostDependency.Path
                    )
                );
            }
        }
    }

    private static bool Includes(Package package, Guid packageId)
    {
        return package.IncludedPackages.Any(included => included.Id == packageId);
    }
}
