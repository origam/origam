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
using System.Linq;

namespace Origam.Schema.DeploymentModel
{
	public interface IDeploymentVersion: IComparable
	{
		PackageVersion Version { get; }
		List<DeploymentDependency> DeploymentDependencies { get; set; }
		bool HasDependencies { get; }
		Guid SchemaExtensionId { get; }
		string PackageName { get; }
	}

	/// <summary>
	/// Summary description for DeploymentVersion.
	/// </summary>
	[SchemaItemDescription("Deployment Version", "icon_deployment-version.png")]
    [HelpTopic("Deployment+Version")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class DeploymentVersion : AbstractSchemaItem, ISchemaItemFactory, IDeploymentVersion
	{
		IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		ISchemaService _schemaService = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

		public const string CategoryConst = "DeploymentVersion";

		public DeploymentVersion() : base() {}

		public DeploymentVersion(Guid schemaExtensionId,
			List<Package> packagesToDependOn) : base(schemaExtensionId)
		{
			deploymentDependencies = 
				DeploymentDependency.FromPackages(packagesToDependOn);
			DeploymentDependenciesCsv =
				DeploymentDependency.ToCsv(deploymentDependencies);
		}

		public DeploymentVersion(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		public override bool IsDeleted
		{
			get
			{
				return base.IsDeleted;
			}
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
				return this.Name + ResourceUtils.GetString("Current");
			}
			else
			{
				return this.Name;
			}
		}
		
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override string Icon
		{
			get
			{
				if(IsCurrentVersion)
				{
					return "icon_deployment-version-active.png";
				}
				else
				{
					return "icon_deployment-version.png";
				}
			}
		}

		#endregion

		#region Properties
		[Browsable(false)]
		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}
		
		[Browsable(false)]
		public bool IsCurrentVersion
		{
			get
			{
				Package ext = _persistence.SchemaListProvider.RetrieveInstance(typeof(Package), Package.PrimaryKey) as Package;
				return ext.VersionString == this.VersionString;
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
				if(versionString != null && versionString != value && IsCurrentVersion)
				{
					throw new InvalidOperationException(ResourceUtils.GetString("ErrorChangePackageVersion"));
				}

				versionString = value;
				Version = new PackageVersion(versionString);
			}
		}

        [Browsable(false)]
        public IEnumerable<AbstractUpdateScriptActivity> UpdateScriptActivities =>
			ChildItems
				.ToGeneric()
				.OrderBy(activity => ((AbstractUpdateScriptActivity)activity).ActivityOrder)
				.Cast<AbstractUpdateScriptActivity>();

        [Browsable(false)]
        public PackageVersion Version { get; private set; }
        
		[XmlAttribute("deploymentDependenciesCsv")]
		public string DeploymentDependenciesCsv;

		private List<DeploymentDependency> deploymentDependencies;

        [Browsable(false)]
        public List<DeploymentDependency> DeploymentDependencies
		{
			get
			{
				if (deploymentDependencies == null)
				{
					deploymentDependencies =
						DeploymentDependency.FromCsv(DeploymentDependenciesCsv);
				}
				return deploymentDependencies;
			}
			set
			{
				if (!string.IsNullOrEmpty(DeploymentDependenciesCsv))
				{
					throw new Exception("Cannot set "+nameof(DeploymentDependencies) +" because its value was set in constructor.");
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
				ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

				if(schema.ActiveExtension.PrimaryKey.Equals(this.Package.PrimaryKey))
				{
					return new Type[] {
										  typeof(FileRestoreUpdateScriptActivity),
										  typeof(ServiceCommandUpdateScriptActivity)
									  };
				}
				else
				{
					return new Type[] {};
				}
			}
		}

        public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			int order = MaxOrder() + 10;

			if(type == typeof(FileRestoreUpdateScriptActivity))
			{
				item = new FileRestoreUpdateScriptActivity(schemaExtensionId);
				item.Name = order.ToString("00000") + "_";
				(item as AbstractUpdateScriptActivity).ActivityOrder = order;
			}
			else if(type == typeof(ServiceCommandUpdateScriptActivity))
			{
				item = new ServiceCommandUpdateScriptActivity(schemaExtensionId);
				item.Name = order.ToString("00000") + "_";
				(item as AbstractUpdateScriptActivity).ActivityOrder = order;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorDeploymentVersionUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion

		private int MaxOrder()
		{
			int max = 0;
			foreach(AbstractUpdateScriptActivity activity in this.ChildItemsByType(AbstractUpdateScriptActivity.CategoryConst))
			{
				if(activity.ActivityOrder > max) max = activity.ActivityOrder;
			}

			return max;
		}

        public override int CompareTo(object obj)
        {
            DeploymentVersion otherDepl = obj as DeploymentVersion;
            if(otherDepl != null)
            {
                return this.Version.CompareTo(otherDepl.Version);
            }
            else
            {
                return base.CompareTo(obj);
            }
        }
	}

	public class DeploymentDependency
	{
		public Guid PackageId { get; }
		public PackageVersion PackageVersion { get; }

		/// <summary>
		///
		/// </summary>
		/// <param name="dependenciesInCsvFormat"> example:
		///
		/// 147FA70D-6519-4393-B5D0-87931F9FD609, 5.0
		/// 6CB854A9-8A7F-4283-AF88-E4CA72919144, 5.0
		///
		/// </param>
		/// <returns></returns>
		public static List<DeploymentDependency> FromCsv(string dependenciesInCsvFormat)
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
				.Select(package =>new DeploymentDependency(package.Id, package.Version))
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
			string[] packageIdAndVersion = csvLine.Split(',');
			if(!Guid.TryParse(packageIdAndVersion[0], out Guid packageId)){
				throw new ArgumentException("Could not parse csv line to DeploymentDependency: "+csvLine);
			}
			if (!PackageVersion.TryParse(packageIdAndVersion[1], out var version))
			{
				throw new ArgumentException("Could not parse csv line to DeploymentDependency: "+csvLine);
			}
			return new DeploymentDependency(packageId, version);
		}

		public DeploymentDependency(Guid packageId, PackageVersion packageVersion)
		{
			this.PackageId = packageId;
			this.PackageVersion = packageVersion;
		}
	}
}
