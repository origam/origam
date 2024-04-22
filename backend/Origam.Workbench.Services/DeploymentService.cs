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
using Origam.Workbench.Services.CoreServices;
using Origam.DA.Service;

namespace Origam.Workbench.Services;

/// <summary>
/// Summary description for DeploymentService.
/// </summary>
public class DeploymentService : IDeploymentService
{

	#region Local variables
	string _transactionId = null;
	SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
	private static OrigamModelVersionData _versionData = null;
	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
	public OrigamModelVersionData VersionData
	{
		get
		{
                if(_versionData==null)
                    _versionData = new OrigamModelVersionData();
                return _versionData;
            }
		set
		{
                _versionData = value;
            }
	}

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
			RunWithErrorHandling(Update);
		}
		
	public void ForceDeployCurrentPackage()
	{
			RunWithErrorHandling(ForceUpdateCurrentPackageOnly);
		}

	private void RunWithErrorHandling(Action action)
	{
			if(_transactionId == null)
			{
				TryLoadVersions();
				_transactionId = Guid.NewGuid().ToString();
				try
				{
					action();
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
				SaveVersionAfterUpdate();
			}
			else
			{
				throw new Exception(ResourceUtils.GetString("ErrorUpdateRunning"));
			}
		}

	private void SaveVersionAfterUpdate()
	{
            IList <Package> packages = _schema.ActiveExtension.IncludedPackages;
            packages.Add(_schema.ActiveExtension);

            AddMissingDeploymentDependencies(packages);

            packages?.ForEach(package => UpdateVersionData(package));
            SaveVersions();
            ClearVersions();
        }

	public bool CanUpdate(Package extension)
	{
			TryLoadVersions();
			return CurrentDeployedVersion(extension) < extension.Version;
		}

	public PackageVersion CurrentDeployedVersion(Package extension)
	{
			TryLoadVersions();
			foreach(OrigamModelVersionData.OrigamModelVersionRow versionRow in VersionData.OrigamModelVersion)
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
            VersionData = null;
			_versionsLoaded = false;
		}

	public void InitializeService()
	{
		}

	#endregion

	#region Private Methods
	private void ExecuteActivity(AbstractUpdateScriptActivity activity)
	{
			Log("Executing activity: " + activity.Name);

			try
			{
				if(activity is ServiceCommandUpdateScriptActivity)
				{
                    ServiceCommandUpdateScriptActivity activityPlatform = activity as ServiceCommandUpdateScriptActivity;
			    	ExecuteActivity(activityPlatform);
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
            string result = "";
            if (activity.DatabaseType != ((AbstractSqlDataService)DataServiceFactory.GetDataService()).PlatformName)
            {
                OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
                settings.DeployPlatforms?.ForEach(platform =>
                {
                    DA.Common.Enums.DatabaseType databaseType =
                        (DA.Common.Enums.DatabaseType)Enum.Parse(typeof(DA.Common.Enums.DatabaseType), platform.GetParseEnum(platform.DataService));
                    if (databaseType == activity.DatabaseType)
                    {
                        agent.SetDataService(DataServiceFactory.GetDataService(platform));
                        result = agent.ExecuteUpdate(activity.CommandText, _transactionId);
                    }
                });
            }
            else
            {
                result = agent.ExecuteUpdate(activity.CommandText, _transactionId);
            }
			Log(result);
		}

	private void ExecuteActivity(FileRestoreUpdateScriptActivity activity)
	{
			string fileName;

			switch(activity.TargetLocation)
			{
				case DeploymentFileLocation.ReportsFolder:
					OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
					fileName = Path.Combine(settings.ReportsFolder(), activity.FileName);
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
            Log("=======================================================================" + Environment.NewLine);
			Log(DateTime.Now + " Starting update");

			IList<Package> packages = _schema.ActiveExtension.IncludedPackages;
			packages.Add(_schema.ActiveExtension);
			
			AddMissingDeploymentDependencies(packages);

			IEnumerable<DeploymentVersion> unsortedDeployments = packages
				.Where(CanUpdate)
				.SelectMany(GetDeploymentVersions)
				.ToList();

			var deploymentSorter = new DeploymentSorter();
			deploymentSorter.SortingFailed += (sender, message) =>
			{
				Log(message);
				throw new Exception(message);
			};
			deploymentSorter
				.SortToRespectDependencies(unsortedDeployments)
				.Cast<DeploymentVersion>()
				.Where(WasNotRunAlready)
				.ForEach(deplVersion =>
				{
					Log($"{deplVersion.Package.Name}: {deplVersion.Version}");
					foreach (var activity in deplVersion.UpdateScriptActivities)
					{
						ExecuteActivity(activity);
					}
				});
        }
		
	private void ForceUpdateCurrentPackageOnly()
	{
			Log("=======================================================================" + Environment.NewLine);
			Log(DateTime.Now + " Starting update");

			IEnumerable<DeploymentVersion> deployments =
				GetDeploymentVersions(_schema.ActiveExtension);
			
			deployments
				.Where(WasNotRunAlready)
				.ForEach(deplVersion =>
				{
					Log($"{deplVersion.Package.Name}: {deplVersion.Version}");
					foreach (var activity in deplVersion.UpdateScriptActivities)
					{
						ExecuteActivity(activity);
					}
				});
		}

	private bool WasNotRunAlready(DeploymentVersion deplversion)
	{
			PackageVersion currentDeployedVersion =
				CurrentDeployedVersion(deplversion.Package);

			return deplversion.Version > currentDeployedVersion;
		}

	private void UpdateVersionData(Package package)
	{
			TryLoadVersions();
			bool found = false;
			// update version number
			foreach (OrigamModelVersionData.OrigamModelVersionRow version in
                VersionData.OrigamModelVersion)
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
                VersionData.OrigamModelVersion.AddOrigamModelVersionRow(
					DateTime.Now, package.Version, package.Id);
			}
		}

	private void AddMissingDeploymentDependencies(IList<Package> packages)
	{
			foreach (Package package in packages)
			{
				GetDeploymentVersions(package)
					.Where(deplVersion =>
						deplVersion.DeploymentDependencies.Count == 0)
					.ForEach(deplVersion =>
						AddDeploymentDependencies(package, deplVersion));
			}
		}

	private void AddDeploymentDependencies(Package package,
		DeploymentVersion deplVersion)
	{
			if (package.IncludedPackages.Count == 0)
			{
				return;
			}
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
		DeploymentVersion deplVersion, Package package)
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
		Package dependentPackage)
	{
			PackageVersion dependentVersion =
				IsOrigamOwned(dependentPackage)
					? GetPreviousVersion(PackageVersion.Five, dependentPackage).Value
					: dependentPackage.Version;
			return new DeploymentDependency(dependentPackage.Id, dependentVersion);
		}

	private bool IsOrigamOwned(Package dependentPackage) => 
		OrigamOwnedPackages.ContainsValue(dependentPackage.Id);

	public bool IsEmptyDatabase()
	{
	        string localTransaction = Guid.NewGuid().ToString();
	        DataSet versionDataFromOrigamModelVersion =
		        LoadVersionDataFrom(origamModelVersionQueryId, "OrigamModelVersion", localTransaction);
	        if (versionDataFromOrigamModelVersion == null)
	        {
		        DataSet versionDataFromAsapModelVersion =
			        LoadVersionDataFrom(asapModelVersionQueryId, "AsapModelVersion", localTransaction);
		        if (versionDataFromAsapModelVersion == null)
		        {
			        return true;
		        }
	        }

	        return false;
        }

	private void ClearVersions()
	{
            VersionData.Clear();
			_versionsLoaded = false;
		}
	private DataSet LoadVersionDataFrom(Guid queryId, string tableName,string localTransaction)
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
                dataServiceAgent.TransactionId = localTransaction;
                try
                {
                    dataServiceAgent.Run();
                    return (DataSet)dataServiceAgent.Result;
                }
                catch (DatabaseTableNotFoundException ex)
                {
                    if (ex.TableName == tableName)
                    {
                        return null;
                    }
                    throw;
                }
                finally
                {
                    ResourceMonitor.Commit(localTransaction);
                }
		}

	private void TryLoadVersions()
	{
            if (_versionsLoaded) return;

            ClearVersions();
            string localTransaction = Guid.NewGuid().ToString();
            DataSet data = null;

            DataSet versionDataFromOrigamModelVersion =
                LoadVersionDataFrom(origamModelVersionQueryId, "OrigamModelVersion", localTransaction);
            if (versionDataFromOrigamModelVersion == null)
            {
                DataSet versionDataFromAsapModelVersion =
                    LoadVersionDataFrom(asapModelVersionQueryId, "AsapModelVersion", localTransaction);
                if (versionDataFromAsapModelVersion == null)
                {
	                _versionsLoaded = true;
	                return;
                }
                if (versionDataFromAsapModelVersion != null &&
                    versionDataFromAsapModelVersion.Tables.Count != 0)
                {
                    versionDataFromAsapModelVersion.Tables[0].TableName = "OrigamModelVersion";
                    data = versionDataFromAsapModelVersion;
                }
            }
            else
            {
                data = versionDataFromOrigamModelVersion;
            }

            if (data == null) return;
            VersionData.Merge(data);
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
			dataServiceAgent.Parameters.Add("Data", VersionData);

			try
			{
				dataServiceAgent.Run();
			}
			catch
			{
				return false;
			}
			return true;
		}
		
	private List<DeploymentVersion> GetDeploymentVersions(Package extension)
	{
			return _schema.GetProvider<DeploymentSchemaItemProvider>()
				.ChildItemsByType(DeploymentVersion.CategoryConst)
				.Cast<DeploymentVersion>()
				.Where(deplVersion => deplVersion.SchemaExtensionId == extension.Id)
				.OrderBy(deplVersion => deplVersion)
				.ToList();
		}

	public Maybe<PackageVersion> GetPreviousVersion(PackageVersion version,
		Package extension)
	{
			return _schema.GetProvider<DeploymentSchemaItemProvider>()
				.ChildItemsByType(DeploymentVersion.CategoryConst)
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