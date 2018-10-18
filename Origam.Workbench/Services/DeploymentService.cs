#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CSharpFunctionalExtensions;
using ICSharpCode.SharpZipLib.Zip;
using MoreLinq;
using Origam.DA;
using Origam.Schema;
using Origam.Schema.DeploymentModel;

namespace Origam.Workbench.Services
{
	/// <summary>
	/// Summary description for DeploymentService.
	/// </summary>
	public class DeploymentService : IDeploymentService
	{
		#region Local variables
		string _transactionId = null;
		SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
		OrigamModelVersionData _versionData = new OrigamModelVersionData();
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		private bool _versionsLoaded = false;

		private readonly Guid asapModelVersionQueryId =
			new Guid("f3e89044-68b2-49c1-a203-4fe3a7b1ca1d");
		private readonly Guid origamModelVersionQueryId =
			new Guid("c14d5f5b-df9b-46fa-9f94-2535b9c758e9");
		
		private readonly Dictionary<string, Guid> OrigamOwnedPackages 
			= new Dictionary<string, Guid>{
				{"Root", new Guid("147FA70D-6519-4393-B5D0-87931F9FD609")},
				{"Security", new Guid("951F2CDA-2867-4B99-8824-071FA8749EAD")}
			};
		#endregion

		#region Constructors
		public DeploymentService()
		{
		}
		#endregion

		#region IDeploymentService Members

		public void Deploy()
		{
			if(_transactionId == null)
			{
				_transactionId = Guid.NewGuid().ToString();
				try
				{
					Update();
					ResourceMonitor.Commit(_transactionId);
				}
				catch(Exception ex)
				{
					ResourceMonitor.Rollback(_transactionId);
					throw new Exception(ResourceUtils.GetString("ErrorUpdateFailed"), ex);
				}
				finally
				{
					_transactionId = null;
					ClearVersions();
				}
			}
			else
			{
				throw new Exception(ResourceUtils.GetString("ErrorUpdateRunning"));
			}
		}

		public bool CanUpdate(SchemaExtension extension)
		{
			TryLoadVersions();
			return CurrentDeployedVersion(extension) < extension.Version;
		}

		public PackageVersion CurrentDeployedVersion(SchemaExtension extension)
		{
			foreach(OrigamModelVersionData.OrigamModelVersionRow versionRow in _versionData.OrigamModelVersion)
			{
				if(versionRow.refSchemaExtensionId == extension.Id)
				{
					if(versionRow.IsVersionNull())
					{
						return PackageVersion.Zero;
					}
					else
					{
						return new PackageVersion(versionRow.Version);
					}
				}
			}

			return PackageVersion.Zero;
		}

		public void ExecuteActivity(Key key)
		{
			IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			AbstractUpdateScriptActivity activity = persistence.SchemaProvider.RetrieveInstance(typeof(AbstractUpdateScriptActivity), key) as AbstractUpdateScriptActivity;
            _transactionId = Guid.NewGuid().ToString();
            try
            {
                ExecuteActivity(activity);
                ResourceMonitor.Commit(_transactionId);
            }
            catch (Exception)
            {
                ResourceMonitor.Rollback(_transactionId);
                throw;
            }
            finally
            {
                _transactionId = null;
            }
        }

		public void CreateNewModelVersion(SchemaItemGroup group, string name, string version)
		{
			DeploymentHelper.CreateVersion(group, name, version);
		}
		#endregion

		#region IService Members

		public void UnloadService()
		{
			_schema = null;
			_versionData = null;
			_versionsLoaded = false;
		}

		public void InitializeService()
		{
		}

		#endregion

		#region Private Methods
		private void ExecuteActivity(AbstractUpdateScriptActivity activity)
		{
			Log(DateTime.Now + " Executing activity: " + activity.Name);

			try
			{
				if(activity is ServiceCommandUpdateScriptActivity)
				{
					ExecuteActivity(activity as ServiceCommandUpdateScriptActivity);
				}
				else if(activity is FileRestoreUpdateScriptActivity)
				{
					ExecuteActivity(activity as FileRestoreUpdateScriptActivity);
				}
				else
				{
					throw new ArgumentOutOfRangeException("activity", activity, ResourceUtils.GetString("ErrorUnsupportedActivityType"));
				}
			}
			catch(Exception ex)
			{
                if (log.IsFatalEnabled)
                {
                    log.Fatal("Error occurred while running deployment activity " + activity.Path, ex);
                }
				throw;
			}
		}
		
		private void ExecuteActivity(ServiceCommandUpdateScriptActivity activity)
		{
			IBusinessServicesService service = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
			IServiceAgent agent = service.GetAgent(activity.Service.Name, null, null);
			string result = agent.ExecuteUpdate(activity.CommandText, _transactionId);
			Log(DateTime.Now + " " + result);
		}

		private void ExecuteActivity(FileRestoreUpdateScriptActivity activity)
		{
			string fileName;

			switch(activity.TargetLocation)
			{
				case DeploymentFileLocation.ApplicationStartupFolder:
					fileName = Path.Combine(System.Windows.Forms.Application.StartupPath, activity.FileName);
					break;

				case DeploymentFileLocation.ReportsFolder:
					OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
					fileName = Path.Combine(settings.ReportsFolder(), activity.FileName);
					break;
			
				case DeploymentFileLocation.SystemFolder:
					fileName = Path.Combine(Environment.SystemDirectory, activity.FileName);
					break;
			
				case DeploymentFileLocation.Manual:
					fileName = activity.FileName;
					break;

				default:
					throw new ArgumentOutOfRangeException("TargetLocaltion", activity.TargetLocation, ResourceUtils.GetString("ErrorUnsupportedTarget"));
			}

			Log("Restoring file to: " + fileName);

			MemoryStream ms = new MemoryStream(activity.File);
			ZipInputStream s = new ZipInputStream(ms); 

			ZipEntry entry = s.GetNextEntry();
			FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);

			try
			{	
				int size;
				byte[] data = new byte[2048];

				do
				{
					size = s.Read(data, 0, data.Length);
					file.Write(data, 0, size);
				} while (size > 0);
			}
			finally
			{
				if(s != null) s.Close();
				if(file != null) file.Close();
				if(ms != null) ms.Close();
			}

			File.SetCreationTime(fileName, entry.DateTime);
		}

		private void Update()
		{
			TryLoadVersions();

			Log("=======================================================================" + Environment.NewLine);
			Log(DateTime.Now + " Starting update");

			IList<SchemaExtension> packages = _schema.ActiveExtension.IncludedPackages;
			packages.Add(_schema.ActiveExtension);
			
			AddMissingDeploymentDependencies(packages);

			IEnumerable<DeploymentVersion> unsortedDeployments = packages
				.Where(CanUpdate)
				.SelectMany(GetDeploymentVersions)
				.ToList();

			new DeploymentSorter()
				.SortToRespectDependencies(unsortedDeployments)
				.Cast<DeploymentVersion>()
				.Where(WasNotRunAlready)
				.SelectMany(deplVersion => deplVersion.UpdateScriptActivities)
				.ForEach(ExecuteActivity);

			foreach (SchemaExtension package in packages)
			{
				UpdateVersionData(package);
			}
			SaveVersions();
		}

		private bool WasNotRunAlready(DeploymentVersion deplversion)
		{
			PackageVersion currentDeployedVersion =
				CurrentDeployedVersion(deplversion.SchemaExtension);

			return deplversion.Version > currentDeployedVersion;
		}

		private void UpdateVersionData(SchemaExtension package)
		{
			TryLoadVersions();
			bool found = false;
			// update version number
			foreach (OrigamModelVersionData.OrigamModelVersionRow version in
				_versionData.OrigamModelVersion)
			{
				if (version.refSchemaExtensionId == package.Id)
				{
					version.Version = package.Version;
					version.DateUpdated = DateTime.Now;
					version.EndEdit();
					found = true;
					break;
				}
			}
			if (!found)
			{
				_versionData.OrigamModelVersion.AddOrigamModelVersionRow(
					DateTime.Now, package.Version, package.Id);
			}
		}

		private void AddMissingDeploymentDependencies(IList<SchemaExtension> packages)
		{
			foreach (SchemaExtension package in packages)
			{
				GetDeploymentVersions(package)
					.Where(deplVerstion =>
						deplVerstion.DeploymentDependencies.Count == 0)
					.ForEach(deplVerstion =>
						AddDeploymentDependencies(package, deplVerstion));
			}
		}

		private void AddDeploymentDependencies(SchemaExtension package,
			DeploymentVersion deplVersion)
		{
			if (IsOrigamOwned(package) && deplVersion.Version >= PackageVersion.Five)
			{
				throw new Exception($"Cannot automatically add dependencies to Origam owned package over version 5.0 these should have been added during {nameof(DeploymentVersion)} creation.");
			}
			deplVersion.DeploymentDependencies = package.IncludedPackages
				.Select(FindVersionToDependOn)
				.ToList();
			AddDependencyOnPreviousDeploymentVersion(deplVersion, package);
		}

		private void AddDependencyOnPreviousDeploymentVersion(
			DeploymentVersion deplVersion, SchemaExtension package)
		{
			Maybe<PackageVersion> mayBePackageVersion =
				GetPreviousVersion(deplVersion.Version, package);
			if (mayBePackageVersion.HasValue)
			{
				deplVersion.DeploymentDependencies.Add(
					new DeploymentDependency(package.Id, mayBePackageVersion.Value));
			}
		}

		private DeploymentDependency FindVersionToDependOn(
			SchemaExtension dependentPackage)
		{
			PackageVersion dependentVersion =
				IsOrigamOwned(dependentPackage)
					? GetPreviousVersion(PackageVersion.Five, dependentPackage).Value
					: dependentPackage.Version;
			return new DeploymentDependency(dependentPackage.Id, dependentVersion);
		}

		private bool IsOrigamOwned(SchemaExtension dependentPackage) => 
			OrigamOwnedPackages.ContainsValue(dependentPackage.Id);

		public bool IsEmptyDatabase()
        {
			TryLoadVersions();
			return _versionsLoaded == false;
        }

		private void ClearVersions()
		{
			_versionData.Clear();
			_versionsLoaded = false;
		}

		private DataSet LoadversionDataFrom(Guid queryId, string tableName)
		{
			IServiceAgent dataServiceAgent = 
				ServiceManager.Services.
					GetService<IBusinessServicesService>()
					.GetAgent("DataService", null, null);

			DataStructureQuery origamVersionQuery = new DataStructureQuery(queryId);
			origamVersionQuery.LoadByIdentity = false;
			
			dataServiceAgent.MethodName = "LoadDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", origamVersionQuery);
			dataServiceAgent.TransactionId = _transactionId;

			try
			{
				dataServiceAgent.Run();
				return  (DataSet)dataServiceAgent.Result;
			} 
			catch (DatabaseTableNotFoundException ex)
			{
				if (ex.TableName == tableName)
				{
					return null;
				}
				throw;
			}
		}

		private void TryLoadVersions()
		{
			if(_versionsLoaded) return;

			ClearVersions();

			DataSet versionDataFromAsapModelVersion =
				LoadversionDataFrom(asapModelVersionQueryId,"AsapModelVersion");
			DataSet data;
			if (versionDataFromAsapModelVersion != null && 
			    versionDataFromAsapModelVersion.Tables.Count != 0)
			{
				versionDataFromAsapModelVersion.Tables[0].TableName = "OrigamModelVersion";
				data = versionDataFromAsapModelVersion;
			}
			else
			{
				DataSet versionDataFromOrigamModelVersion =
					LoadversionDataFrom(origamModelVersionQueryId, "OrigamModelVersion");
				data = versionDataFromOrigamModelVersion;
			}
			if (data == null) return;
			_versionData.Merge(data);
			_versionsLoaded = true;
		}		

		private void SaveVersions()
		{
			if (TrySaveVersions(origamModelVersionQueryId)) return;
			if (TrySaveVersions(asapModelVersionQueryId))return;
			throw new Exception("Failed to save Model versions. Does a table named OrigamModelVersion or AsapModelVersion exist?");
		}

		private bool TrySaveVersions(Guid queryId)
		{
			IServiceAgent dataServiceAgent = ServiceManager.Services
				.GetService<IBusinessServicesService>()
				.GetAgent("DataService", null, null);
			dataServiceAgent.TransactionId = _transactionId;

			DataStructureQuery query = new DataStructureQuery(queryId);
			query.LoadByIdentity = false;
			query.LoadActualValuesAfterUpdate = false;
			query.FireStateMachineEvents = false;
			query.SynchronizeAttachmentsOnDelete = false;

			dataServiceAgent.MethodName = "StoreDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", query);
			dataServiceAgent.Parameters.Add("Data", _versionData);

			try
			{
				dataServiceAgent.Run();
			}
			catch (DatabaseTableNotFoundException ex)
			{
				return false;
			}
			return true;
		}
		
		private List<DeploymentVersion> GetDeploymentVersions(SchemaExtension extension)
		{
			return _schema.GetProvider<DeploymentSchemaItemProvider>()
				.ChildItemsByType(DeploymentVersion.ItemTypeConst)
				.Cast<DeploymentVersion>()
				.Where(deplVersion => deplVersion.SchemaExtensionId == extension.Id)
				.OrderBy(deplVersion => deplVersion)
				.ToList();
		}

		public Maybe<PackageVersion> GetPreviousVersion(PackageVersion version,
			SchemaExtension extension)
		{
			return _schema.GetProvider<DeploymentSchemaItemProvider>()
				.ChildItemsByType(DeploymentVersion.ItemTypeConst)
			    .Cast<DeploymentVersion>()
				.Where(depVersion => depVersion.SchemaExtensionId == extension.Id)
				.OrderBy(depVersion => depVersion.Version)
				.LastOrDefault(depVersion => depVersion.Version < version)
				?.Version;
		}

		private void Log(string text)
		{
			if(log.IsInfoEnabled)
			{
				log.Info(text);
			}
		}
		#endregion
	}
}
