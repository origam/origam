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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Origam.Schema.DeploymentModel;
public interface IDeploymentVersion: IComparable
{
	PackageVersion Version { get; }
	List<DeploymentDependency> DeploymentDependencies { get; set; }
	bool HasDependencies { get; }
	Guid SchemaExtensionId { get; }
	string PackageName { get; }
}
[SchemaItemDescription("Deployment Version", "icon_deployment-version.png")]
[HelpTopic("Deployment+Version")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
[DebuggerDisplay("{PackageName} {ToString()}")]
public class DeploymentVersion : AbstractSchemaItem, IDeploymentVersion
{
	private IPersistenceService persistenceService 
		= ServiceManager.Services.GetService<IPersistenceService>();
	private ISchemaService schemaService 
		= ServiceManager.Services.GetService<ISchemaService>();
	public const string CategoryConst = "DeploymentVersion";
	public DeploymentVersion() {}
	public DeploymentVersion(Guid schemaExtensionId,
		List<Package> packagesToDependOn) : base(schemaExtensionId)
	{
		deploymentDependencies = 
			DeploymentDependency.FromPackages(packagesToDependOn);
		DeploymentDependenciesCsv =
			DeploymentDependency.ToCsv(deploymentDependencies);
	}
	public DeploymentVersion(Key primaryKey) : base(primaryKey)	{}
	#region Overriden ISchemaItem Members
	public override bool IsDeleted
	{
		get => base.IsDeleted;
		set
		{
			if(value & IsCurrentVersion)
			{
				throw new InvalidOperationException("Cannot delete current version.");
			}
			base.IsDeleted = value;
		}
	}
	public override string ToString()
	{
		if(IsCurrentVersion)
		{
			return Name + ResourceUtils.GetString("Current");
		}
		return Name;
	}
	
	public override string ItemType => CategoryConst;
	public override string Icon => IsCurrentVersion 
		? "icon_deployment-version-active.png" 
		: "icon_deployment-version.png";
	#endregion
	#region Properties
	[Browsable(false)]
	public override bool UseFolders => false;
	[Browsable(false)]
	public bool IsCurrentVersion
	{
		get
		{
			var package = persistenceService.SchemaListProvider
				.RetrieveInstance(typeof(Package), Package.PrimaryKey) 
				as Package;
			return package.VersionString == VersionString;
		}
	}
	private string versionString;
	[Category("Version Information")]
	[XmlAttribute("version")]
	public string VersionString
	{
		get => versionString;
		set
		{
			if((versionString != null) && (versionString != value) 
			&& IsCurrentVersion)
			{
				throw new InvalidOperationException(
					ResourceUtils.GetString("ErrorChangePackageVersion"));
			}
			versionString = value;
			Version = new PackageVersion(versionString);
		}
	}
    [Browsable(false)]
    public IEnumerable<AbstractUpdateScriptActivity> UpdateScriptActivities =>
		ChildItems
			.OrderBy(activity => 
				((AbstractUpdateScriptActivity)activity).ActivityOrder)
			.Cast<AbstractUpdateScriptActivity>();
    [Browsable(false)]
    public PackageVersion Version { get; private set; }
    
	[XmlAttribute("deploymentDependenciesCsv")]
	public string DeploymentDependenciesCsv;
	private List<DeploymentDependency> deploymentDependencies;
    [Browsable(false)]
    public List<DeploymentDependency> DeploymentDependencies
	{
		get =>
			deploymentDependencies ?? (deploymentDependencies =
				DeploymentDependency.FromCsv(DeploymentDependenciesCsv));
		set
		{
			if(!string.IsNullOrEmpty(DeploymentDependenciesCsv))
			{
				throw new Exception(
					$"Cannot set {nameof(DeploymentDependencies)} because its value was set in constructor.");
			}
			deploymentDependencies = value;
		}
	}
    [Browsable(false)]
    public bool HasDependencies => DeploymentDependencies.Count != 0;
	#endregion
	#region ISchemaItemFactory Members
	public override Type[] NewItemTypes
	{
		get
		{
			if(schemaService.ActiveExtension.PrimaryKey.Equals(
				   Package.PrimaryKey))
			{
				return new[] 
				{
					typeof(FileRestoreUpdateScriptActivity),
					typeof(ServiceCommandUpdateScriptActivity)
				};
			}
			return new Type[] {};
		}
	}
    public override T NewItem<T>(
        Guid schemaExtensionId, SchemaItemGroup group)
	{
		var order = MaxOrder() + 10;
		var itemName = order.ToString("00000") + "_";;
		var item = base.NewItem<T>(schemaExtensionId, group, itemName);
		(item as AbstractUpdateScriptActivity).ActivityOrder = order;
		return item;
	}
	#endregion
	private int MaxOrder()
	{
		var max = 0;
		foreach(var activity 
		        in ChildItemsByType<AbstractUpdateScriptActivity>(
			        AbstractUpdateScriptActivity.CategoryConst))
		{
			if(activity.ActivityOrder > max)
			{
				max = activity.ActivityOrder;
			}
		}
		return max;
	}
    public override int CompareTo(object obj)
    {
        if(obj is DeploymentVersion otherDeploymentVersion)
        {
            return Version.CompareTo(otherDeploymentVersion.Version);
        }
        return base.CompareTo(obj);
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
	public static List<DeploymentDependency> FromCsv(
		string dependenciesInCsvFormat)
	{
		if (string.IsNullOrEmpty(dependenciesInCsvFormat))
		{
			return new List<DeploymentDependency>();
		}
		return dependenciesInCsvFormat
			.Split('\n')
			.Select(x => x.Trim())
			.Where(x => !string.IsNullOrEmpty(x))
			.Select(ParseFromCsvLine)
			.ToList();
	}
	
	public static List<DeploymentDependency> FromPackages(
		List<Package> packagesToDependOn)
	{
		return packagesToDependOn
			.Select(package =>new DeploymentDependency(
				package.Id, package.Version))
			.ToList();
	}
	
	public static string ToCsv(List<DeploymentDependency> dependencies)
	{
		return dependencies
			.Select(dep => dep.PackageId + ", " + dep.PackageVersion)
			.Aggregate("", (csv, line) => csv + Environment.NewLine + line);
	}
	private static DeploymentDependency ParseFromCsvLine(string csvLine)
	{
		var packageIdAndVersion = csvLine.Split(',');
		if(!Guid.TryParse(packageIdAndVersion[0], out var packageId))
		{
			throw new ArgumentException(
				$"Could not parse csv line to DeploymentDependency: {csvLine}");
		}
		if(!PackageVersion.TryParse(packageIdAndVersion[1], 
			   out var version))
		{
			throw new ArgumentException(
				$"Could not parse csv line to DeploymentDependency: {csvLine}");
		}
		return new DeploymentDependency(packageId, version);
	}
	public DeploymentDependency(
		Guid packageId, PackageVersion packageVersion)
	{
		PackageId = packageId;
		PackageVersion = packageVersion;
	}
}
