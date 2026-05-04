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
using System.Data;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using ICSharpCode.SharpZipLib.Zip;
using MoreLinq;
using Origam.DA;
using Origam.DA.Common.DatabasePlatform;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Workbench.Services;

/// <summary>
/// Summary description for DeploymentService.
/// </summary>
public class DeploymentService : IDeploymentService
{
    #region Local variables
    string _transactionId = null;
    SchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
    private static OrigamModelVersionData _versionData = null;
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    public OrigamModelVersionData VersionData
    {
        get
        {
            if (_versionData == null)
            {
                _versionData = new OrigamModelVersionData();
            }

            return _versionData;
        }
        set { _versionData = value; }
    }
    private bool _versionsLoaded = false;
    private readonly Guid asapModelVersionQueryId = new Guid(
        g: "f3e89044-68b2-49c1-a203-4fe3a7b1ca1d"
    );
    private readonly Guid origamModelVersionQueryId = new Guid(
        g: "c14d5f5b-df9b-46fa-9f94-2535b9c758e9"
    );

    private readonly Dictionary<string, Guid> OrigamOwnedPackages = new Dictionary<string, Guid>
    {
        { "Root", new Guid(g: "147FA70D-6519-4393-B5D0-87931F9FD609") },
        { "Security", new Guid(g: "951F2CDA-2867-4B99-8824-071FA8749EAD") },
    };
    #endregion
    #region Constructors
    public DeploymentService() { }
    #endregion
    #region IDeploymentService Members
    public void Deploy()
    {
        RunWithErrorHandling(action: Update);
    }

    public void ForceDeployCurrentPackage()
    {
        RunWithErrorHandling(action: ForceUpdateCurrentPackageOnly);
    }

    private void RunWithErrorHandling(Action action)
    {
        if (_transactionId == null)
        {
            TryLoadVersions();
            _transactionId = Guid.NewGuid().ToString();
            try
            {
                action();
                ResourceMonitor.Commit(transactionId: _transactionId);
            }
            catch (Exception ex)
            {
                ResourceMonitor.Rollback(transactionId: _transactionId);
                throw new Exception(
                    message: ResourceUtils.GetString(key: "ErrorUpdateFailed"),
                    innerException: ex
                );
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
            throw new Exception(message: ResourceUtils.GetString(key: "ErrorUpdateRunning"));
        }
    }

    private void SaveVersionAfterUpdate()
    {
        IList<Package> packages = _schema.ActiveExtension.IncludedPackages;
        packages.Add(item: _schema.ActiveExtension);
        AddMissingDeploymentDependencies(packages: packages);
        packages?.ForEach(action: package => UpdateVersionData(package: package));
        SaveVersions();
        ClearVersions();
    }

    public bool CanUpdate(Package extension)
    {
        TryLoadVersions();
        return CurrentDeployedVersion(extension: extension) < extension.Version;
    }

    public PackageVersion CurrentDeployedVersion(Package extension)
    {
        TryLoadVersions();
        foreach (
            OrigamModelVersionData.OrigamModelVersionRow versionRow in VersionData.OrigamModelVersion
        )
        {
            if (versionRow.refSchemaExtensionId == extension.Id)
            {
                if (versionRow.IsVersionNull())
                {
                    return PackageVersion.Zero;
                }

                return new PackageVersion(completeVersionString: versionRow.Version);
            }
        }
        return PackageVersion.Zero;
    }

    public void ExecuteActivity(Key key)
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        AbstractUpdateScriptActivity activity =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractUpdateScriptActivity),
                primaryKey: key
            ) as AbstractUpdateScriptActivity;
        _transactionId = Guid.NewGuid().ToString();
        try
        {
            ExecuteActivity(activity: activity);
            ResourceMonitor.Commit(transactionId: _transactionId);
        }
        catch (Exception)
        {
            ResourceMonitor.Rollback(transactionId: _transactionId);
            throw;
        }
        finally
        {
            _transactionId = null;
        }
    }

    public void CreateNewModelVersion(SchemaItemGroup group, string name, string version)
    {
        DeploymentHelper.CreateVersion(group: group, name: name, version: version);
    }
    #endregion
    #region IService Members
    public void UnloadService()
    {
        _schema = null;
        VersionData = null;
        _versionsLoaded = false;
    }

    public void InitializeService() { }
    #endregion
    #region Private Methods
    private void ExecuteActivity(AbstractUpdateScriptActivity activity)
    {
        Log(text: "Executing activity: " + activity.Name);
        try
        {
            if (activity is ServiceCommandUpdateScriptActivity)
            {
                ServiceCommandUpdateScriptActivity activityPlatform =
                    activity as ServiceCommandUpdateScriptActivity;
                ExecuteActivity(activity: activityPlatform);
            }
            else if (activity is FileRestoreUpdateScriptActivity)
            {
                ExecuteActivity(activity: activity as FileRestoreUpdateScriptActivity);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "activity",
                    actualValue: activity,
                    message: ResourceUtils.GetString(key: "ErrorUnsupportedActivityType")
                );
            }
        }
        catch (Exception ex)
        {
            if (log.IsFatalEnabled)
            {
                log.Fatal(
                    message: "Error occurred while running deployment activity " + activity.Path,
                    exception: ex
                );
            }
            throw;
        }
    }

    private void ExecuteActivity(ServiceCommandUpdateScriptActivity activity)
    {
        IBusinessServicesService service =
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService;
        IServiceAgent agent = service.GetAgent(
            serviceType: activity.Service.Name,
            ruleEngine: null,
            workflowEngine: null
        );
        string result = "";
        if (
            activity.DatabaseType
            != ((AbstractSqlDataService)DataServiceFactory.GetDataService()).PlatformName
        )
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            settings.DeployPlatforms?.ForEach(action: platform =>
            {
                DatabaseType databaseType = (DatabaseType)
                    Enum.Parse(
                        enumType: typeof(DatabaseType),
                        value: platform.GetParseEnum(dataDataService: platform.DataService)
                    );
                if (databaseType == activity.DatabaseType)
                {
                    agent.SetDataService(
                        dataService: DataServiceFactory.GetDataService(deployPlatform: platform)
                    );
                    result = agent.ExecuteUpdate(
                        command: activity.CommandText,
                        transactionId: _transactionId
                    );
                }
            });
        }
        else
        {
            result = agent.ExecuteUpdate(
                command: activity.CommandText,
                transactionId: _transactionId
            );
        }
        Log(text: result);
    }

    private void ExecuteActivity(FileRestoreUpdateScriptActivity activity)
    {
        string fileName;
        switch (activity.TargetLocation)
        {
            case DeploymentFileLocation.ReportsFolder:
            {
                OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
                fileName = Path.Combine(path1: settings.ReportsFolder(), path2: activity.FileName);
                break;
            }

            case DeploymentFileLocation.Manual:
            {
                fileName = activity.FileName;
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "TargetLocaltion",
                    actualValue: activity.TargetLocation,
                    message: ResourceUtils.GetString(key: "ErrorUnsupportedTarget")
                );
            }
        }
        Log(text: "Restoring file to: " + fileName);
        MemoryStream ms = new MemoryStream(buffer: activity.File);
        ZipInputStream s = new ZipInputStream(baseInputStream: ms);
        ZipEntry entry = s.GetNextEntry();
        FileStream file = new FileStream(
            path: fileName,
            mode: FileMode.Create,
            access: FileAccess.Write
        );
        try
        {
            int size;
            byte[] data = new byte[2048];
            do
            {
                size = s.Read(buffer: data, offset: 0, count: data.Length);
                file.Write(array: data, offset: 0, count: size);
            } while (size > 0);
        }
        finally
        {
            if (s != null)
            {
                s.Close();
            }

            if (file != null)
            {
                file.Close();
            }

            if (ms != null)
            {
                ms.Close();
            }
        }
        File.SetCreationTime(path: fileName, creationTime: entry.DateTime);
    }

    private void Update()
    {
        Log(
            text: "======================================================================="
                + Environment.NewLine
        );
        Log(text: DateTime.Now + " Starting update");
        IList<Package> packages = _schema.ActiveExtension.IncludedPackages;
        packages.Add(item: _schema.ActiveExtension);

        AddMissingDeploymentDependencies(packages: packages);
        IEnumerable<DeploymentVersion> unsortedDeployments = packages
            .Where(predicate: CanUpdate)
            .SelectMany(selector: GetDeploymentVersions)
            .ToList();
        var deploymentSorter = new DeploymentSorter();
        deploymentSorter.SortingFailed += (sender, message) =>
        {
            Log(text: message);
            throw new Exception(message: message);
        };
        deploymentSorter
            .SortToRespectDependencies(deplVersionsToSort: unsortedDeployments)
            .Cast<DeploymentVersion>()
            .Where(predicate: WasNotRunAlready)
            .ForEach(action: deplVersion =>
            {
                Log(text: $"{deplVersion.Package.Name}: {deplVersion.Version}");
                foreach (var activity in deplVersion.UpdateScriptActivities)
                {
                    ExecuteActivity(activity: activity);
                }
            });
    }

    private void ForceUpdateCurrentPackageOnly()
    {
        Log(
            text: "======================================================================="
                + Environment.NewLine
        );
        Log(text: DateTime.Now + " Starting update");
        IEnumerable<DeploymentVersion> deployments = GetDeploymentVersions(
            extension: _schema.ActiveExtension
        );

        deployments
            .Where(predicate: WasNotRunAlready)
            .ForEach(action: deplVersion =>
            {
                Log(text: $"{deplVersion.Package.Name}: {deplVersion.Version}");
                foreach (var activity in deplVersion.UpdateScriptActivities)
                {
                    ExecuteActivity(activity: activity);
                }
            });
    }

    private bool WasNotRunAlready(DeploymentVersion deplversion)
    {
        PackageVersion currentDeployedVersion = CurrentDeployedVersion(
            extension: deplversion.Package
        );
        return deplversion.Version > currentDeployedVersion;
    }

    private void UpdateVersionData(Package package)
    {
        TryLoadVersions();
        bool found = false;
        // update version number
        foreach (
            OrigamModelVersionData.OrigamModelVersionRow version in VersionData.OrigamModelVersion
        )
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
                DateUpdated: DateTime.Now,
                Version: package.Version,
                refSchemaExtensionId: package.Id
            );
        }
    }

    private void AddMissingDeploymentDependencies(IList<Package> packages)
    {
        foreach (Package package in packages)
        {
            GetDeploymentVersions(extension: package)
                .Where(predicate: deplVersion => deplVersion.DeploymentDependencies.Count == 0)
                .ForEach(action: deplVersion =>
                    AddDeploymentDependencies(package: package, deplVersion: deplVersion)
                );
        }
    }

    private void AddDeploymentDependencies(Package package, DeploymentVersion deplVersion)
    {
        if (package.IncludedPackages.Count == 0)
        {
            return;
        }
        if (IsOrigamOwned(dependentPackage: package) && deplVersion.Version >= PackageVersion.Five)
        {
            throw new Exception(
                message: $"Cannot automatically add dependencies to Origam owned package over version 5.0 these should have been added during {nameof(DeploymentVersion)} creation."
            );
        }
        deplVersion.DeploymentDependencies = package
            .IncludedPackages.Select(selector: FindVersionToDependOn)
            .ToList();
        AddDependencyOnPreviousDeploymentVersion(deplVersion: deplVersion, package: package);
    }

    private void AddDependencyOnPreviousDeploymentVersion(
        DeploymentVersion deplVersion,
        Package package
    )
    {
        Maybe<PackageVersion> mayBePackageVersion = GetPreviousVersion(
            version: deplVersion.Version,
            extension: package
        );
        if (mayBePackageVersion.HasValue)
        {
            deplVersion.DeploymentDependencies.Add(
                item: new DeploymentDependency(
                    packageId: package.Id,
                    packageVersion: mayBePackageVersion.Value
                )
            );
        }
    }

    private DeploymentDependency FindVersionToDependOn(Package dependentPackage)
    {
        PackageVersion dependentVersion = IsOrigamOwned(dependentPackage: dependentPackage)
            ? GetPreviousVersion(version: PackageVersion.Five, extension: dependentPackage).Value
            : dependentPackage.Version;
        return new DeploymentDependency(
            packageId: dependentPackage.Id,
            packageVersion: dependentVersion
        );
    }

    private bool IsOrigamOwned(Package dependentPackage) =>
        OrigamOwnedPackages.ContainsValue(value: dependentPackage.Id);

    public bool IsEmptyDatabase()
    {
        string localTransaction = Guid.NewGuid().ToString();
        DataSet versionDataFromOrigamModelVersion = LoadVersionDataFrom(
            queryId: origamModelVersionQueryId,
            tableName: "OrigamModelVersion",
            localTransaction: localTransaction
        );
        if (versionDataFromOrigamModelVersion == null)
        {
            DataSet versionDataFromAsapModelVersion = LoadVersionDataFrom(
                queryId: asapModelVersionQueryId,
                tableName: "AsapModelVersion",
                localTransaction: localTransaction
            );
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

    private DataSet LoadVersionDataFrom(Guid queryId, string tableName, string localTransaction)
    {
        IServiceAgent dataServiceAgent = ServiceManager
            .Services.GetService<IBusinessServicesService>()
            .GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
        DataStructureQuery origamVersionQuery = new DataStructureQuery(dataStructureId: queryId);
        origamVersionQuery.LoadByIdentity = false;
        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: origamVersionQuery);
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
            ResourceMonitor.Commit(transactionId: localTransaction);
        }
    }

    private void TryLoadVersions()
    {
        if (_versionsLoaded)
        {
            return;
        }

        ClearVersions();
        string localTransaction = Guid.NewGuid().ToString();
        DataSet data = null;
        DataSet versionDataFromOrigamModelVersion = LoadVersionDataFrom(
            queryId: origamModelVersionQueryId,
            tableName: "OrigamModelVersion",
            localTransaction: localTransaction
        );
        if (versionDataFromOrigamModelVersion == null)
        {
            DataSet versionDataFromAsapModelVersion = LoadVersionDataFrom(
                queryId: asapModelVersionQueryId,
                tableName: "AsapModelVersion",
                localTransaction: localTransaction
            );
            if (versionDataFromAsapModelVersion == null)
            {
                _versionsLoaded = true;
                return;
            }
            if (
                versionDataFromAsapModelVersion != null
                && versionDataFromAsapModelVersion.Tables.Count != 0
            )
            {
                versionDataFromAsapModelVersion.Tables[index: 0].TableName = "OrigamModelVersion";
                data = versionDataFromAsapModelVersion;
            }
        }
        else
        {
            data = versionDataFromOrigamModelVersion;
        }
        if (data == null)
        {
            return;
        }

        VersionData.Merge(dataSet: data);
        _versionsLoaded = true;
    }

    private void SaveVersions()
    {
        if (TrySaveVersions(queryId: origamModelVersionQueryId))
        {
            return;
        }

        if (TrySaveVersions(queryId: asapModelVersionQueryId))
        {
            return;
        }

        throw new Exception(
            message: "Failed to save Model versions. Does a table named OrigamModelVersion or AsapModelVersion exist?"
        );
    }

    private bool TrySaveVersions(Guid queryId)
    {
        IServiceAgent dataServiceAgent = ServiceManager
            .Services.GetService<IBusinessServicesService>()
            .GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
        dataServiceAgent.TransactionId = _transactionId;
        DataStructureQuery query = new DataStructureQuery(dataStructureId: queryId);
        query.LoadByIdentity = false;
        query.LoadActualValuesAfterUpdate = false;
        query.FireStateMachineEvents = false;
        query.SynchronizeAttachmentsOnDelete = false;
        dataServiceAgent.MethodName = "StoreDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Parameters.Add(key: "Data", value: VersionData);
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
        return _schema
            .GetProvider<DeploymentSchemaItemProvider>()
            .ChildItemsByType<DeploymentVersion>(itemType: DeploymentVersion.CategoryConst)
            .Where(predicate: deplVersion => deplVersion.SchemaExtensionId == extension.Id)
            .OrderBy(keySelector: deplVersion => deplVersion)
            .ToList();
    }

    public Maybe<PackageVersion> GetPreviousVersion(PackageVersion version, Package extension)
    {
        return _schema
            .GetProvider<DeploymentSchemaItemProvider>()
            .ChildItemsByType<DeploymentVersion>(itemType: DeploymentVersion.CategoryConst)
            .Where(predicate: depVersion => depVersion.SchemaExtensionId == extension.Id)
            .OrderBy(keySelector: depVersion => depVersion.Version)
            .LastOrDefault(predicate: depVersion => depVersion.Version < version)
            ?.Version;
    }

    private void Log(string text)
    {
        if (log.IsInfoEnabled)
        {
            log.Info(message: text);
        }
    }
    #endregion
}
