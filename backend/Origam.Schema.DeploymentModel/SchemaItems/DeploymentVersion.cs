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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.DeploymentModel;

public interface IDeploymentVersion : IComparable
{
    PackageVersion Version { get; }
    List<DeploymentDependency> DeploymentDependencies { get; set; }
    bool HasDependencies { get; }
    Guid SchemaExtensionId { get; }
    string PackageName { get; }
}

[SchemaItemDescription(name: "Deployment Version", iconName: "icon_deployment-version.png")]
[HelpTopic(topic: "Deployment+Version")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
[DebuggerDisplay(value: "{PackageName} {ToString()}")]
public class DeploymentVersion : AbstractSchemaItem, IDeploymentVersion
{
    private IPersistenceService persistenceService =
        ServiceManager.Services.GetService<IPersistenceService>();
    private ISchemaService schemaService = ServiceManager.Services.GetService<ISchemaService>();
    public const string CategoryConst = "DeploymentVersion";

    public DeploymentVersion() { }

    public DeploymentVersion(Guid schemaExtensionId, List<Package> packagesToDependOn)
        : base(extensionId: schemaExtensionId)
    {
        deploymentDependencies = DeploymentDependency.FromPackages(
            packagesToDependOn: packagesToDependOn
        );
        DeploymentDependenciesCsv = DeploymentDependency.ToCsv(
            dependencies: deploymentDependencies
        );
    }

    public DeploymentVersion(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members
    public override bool IsDeleted
    {
        get => base.IsDeleted;
        set
        {
            if (value & IsCurrentVersion)
            {
                throw new InvalidOperationException(message: "Cannot delete current version.");
            }
            base.IsDeleted = value;
        }
    }

    public override string ToString()
    {
        if (IsCurrentVersion)
        {
            return Name + ResourceUtils.GetString(key: "Current");
        }
        return Name;
    }

    public override string ItemType => CategoryConst;
    public override string Icon =>
        IsCurrentVersion ? "icon_deployment-version-active.png" : "icon_deployment-version.png";
    #endregion
    #region Properties
    [Browsable(browsable: false)]
    public override bool UseFolders => false;

    [Browsable(browsable: false)]
    public bool IsCurrentVersion
    {
        get
        {
            var package =
                persistenceService.SchemaListProvider.RetrieveInstance(
                    type: typeof(Package),
                    primaryKey: Package.PrimaryKey
                ) as Package;
            return package.VersionString == VersionString;
        }
    }
    private string versionString;

    [Category(category: "Version Information")]
    [XmlAttribute(attributeName: "version")]
    public string VersionString
    {
        get => versionString;
        set
        {
            if ((versionString != null) && (versionString != value) && IsCurrentVersion)
            {
                throw new InvalidOperationException(
                    message: ResourceUtils.GetString(key: "ErrorChangePackageVersion")
                );
            }
            versionString = value;
            Version = new PackageVersion(completeVersionString: versionString);
        }
    }

    [Browsable(browsable: false)]
    public IEnumerable<AbstractUpdateScriptActivity> UpdateScriptActivities =>
        ChildItems
            .OrderBy(keySelector: activity =>
                ((AbstractUpdateScriptActivity)activity).ActivityOrder
            )
            .Cast<AbstractUpdateScriptActivity>();

    [Browsable(browsable: false)]
    public PackageVersion Version { get; private set; } = new(completeVersionString: "0.0");

    [XmlAttribute(attributeName: "deploymentDependenciesCsv")]
    public string DeploymentDependenciesCsv;
    private List<DeploymentDependency> deploymentDependencies;

    [Browsable(browsable: false)]
    public List<DeploymentDependency> DeploymentDependencies
    {
        get =>
            deploymentDependencies
            ?? (
                deploymentDependencies = DeploymentDependency.FromCsv(
                    dependenciesInCsvFormat: DeploymentDependenciesCsv
                )
            );
        set
        {
            if (!string.IsNullOrEmpty(value: DeploymentDependenciesCsv))
            {
                throw new Exception(
                    message: $"Cannot set {nameof(DeploymentDependencies)} because its value was set in constructor."
                );
            }
            deploymentDependencies = value;
        }
    }

    [Browsable(browsable: false)]
    public bool HasDependencies => DeploymentDependencies.Count != 0;
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes
    {
        get
        {
            if (schemaService.ActiveExtension.PrimaryKey.Equals(obj: Package.PrimaryKey))
            {
                return new[]
                {
                    typeof(FileRestoreUpdateScriptActivity),
                    typeof(ServiceCommandUpdateScriptActivity),
                };
            }
            return new Type[] { };
        }
    }

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        var order = MaxOrder() + 10;
        var itemName = order.ToString(format: "00000") + "_";
        ;
        var item = base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
        (item as AbstractUpdateScriptActivity).ActivityOrder = order;
        return item;
    }
    #endregion
    private int MaxOrder()
    {
        var max = 0;
        foreach (
            var activity in ChildItemsByType<AbstractUpdateScriptActivity>(
                itemType: AbstractUpdateScriptActivity.CategoryConst
            )
        )
        {
            if (activity.ActivityOrder > max)
            {
                max = activity.ActivityOrder;
            }
        }
        return max;
    }

    public override int CompareTo(object obj)
    {
        if (obj is DeploymentVersion otherDeploymentVersion)
        {
            return Version.CompareTo(other: otherDeploymentVersion.Version);
        }
        return base.CompareTo(obj: obj);
    }
}

public class DeploymentDependency
{
    public Guid PackageId { get; }
    public PackageVersion PackageVersion { get; }

    /// <summary></summary>
    /// <param name="dependenciesInCsvFormat"> example:
    /// 147FA70D-6519-4393-B5D0-87931F9FD609, 5.0
    /// 6CB854A9-8A7F-4283-AF88-E4CA72919144, 5.0
    /// </param>
    /// <returns></returns>
    public static List<DeploymentDependency> FromCsv(string dependenciesInCsvFormat)
    {
        if (string.IsNullOrEmpty(value: dependenciesInCsvFormat))
        {
            return new List<DeploymentDependency>();
        }
        return dependenciesInCsvFormat
            .Split(separator: '\n')
            .Select(selector: x => x.Trim())
            .Where(predicate: x => !string.IsNullOrEmpty(value: x))
            .Select(selector: ParseFromCsvLine)
            .ToList();
    }

    public static List<DeploymentDependency> FromPackages(List<Package> packagesToDependOn)
    {
        return packagesToDependOn
            .Select(selector: package => new DeploymentDependency(
                packageId: package.Id,
                packageVersion: package.Version
            ))
            .ToList();
    }

    public static string ToCsv(List<DeploymentDependency> dependencies)
    {
        return dependencies
            .Select(selector: dep => dep.PackageId + ", " + dep.PackageVersion)
            .Aggregate(seed: "", func: (csv, line) => csv + Environment.NewLine + line);
    }

    private static DeploymentDependency ParseFromCsvLine(string csvLine)
    {
        var packageIdAndVersion = csvLine.Split(separator: ',');
        if (!Guid.TryParse(input: packageIdAndVersion[0], result: out var packageId))
        {
            throw new ArgumentException(
                message: $"Could not parse csv line to DeploymentDependency: {csvLine}"
            );
        }
        if (
            !PackageVersion.TryParse(
                completeVersionString: packageIdAndVersion[1],
                version: out var version
            )
        )
        {
            throw new ArgumentException(
                message: $"Could not parse csv line to DeploymentDependency: {csvLine}"
            );
        }
        return new DeploymentDependency(packageId: packageId, packageVersion: version);
    }

    public DeploymentDependency(Guid packageId, PackageVersion packageVersion)
    {
        PackageId = packageId;
        PackageVersion = packageVersion;
    }
}
