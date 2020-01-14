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

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.DA.ObjectPersistence.Providers;
using Origam.Extensions;

namespace Origam.ModelImport
{
	/// <summary>
	/// Class provides function for batch import of packages 
	/// from XML files repository to DB.
	/// Runtime prerequisities:
	/// PersistenceService
	/// SchemaService
	/// Loaded schema list
	/// Active Configuration in ConfigurationManager
	/// Repository set in configuration
	/// </summary>
	public class ModelImport
	{
		private static readonly log4net.ILog log 
            = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event ModelImportStatusEventHandler StateChanged;
        public delegate void ModelImportStatusEventHandler(object sender, ModelImportStatusEventArgs e);
        private IPersistenceService persistenceService;

		public ModelImport()
		{
			persistenceService = ServiceManager.Services.GetService(
				typeof(IPersistenceService)) as IPersistenceService;
		}

		public bool Import()
		{
            if (log.IsInfoEnabled)
            {
                log.Info("Initializing...");
            }
			ArrayList modelFiles = GetModelFiles(GetRepositoryDirectory());
			if(modelFiles.Count == 0)
			{
                if (log.IsInfoEnabled)
                {
                    log.Info("No model files found. Import interrupted...");
                }
				return false;
			}
			ArrayList presentPackages = persistenceService.SchemaListProvider
                .RetrieveList<SchemaExtension>(null)
				.ToArrayList();
            string transactionId = Guid.NewGuid().ToString();
            if (log.IsInfoEnabled)
            {
                log.Info("Import will run under transaction " + transactionId);
            }
            try
            {
                foreach(ModelFileInfo modelFileInfo in modelFiles)
                {
                    ImportModelFile(modelFileInfo, presentPackages, transactionId);
                }
                ResourceMonitor.Commit(transactionId);
                if (log.IsInfoEnabled)
                {
                    log.Info("Import successful...");
                }
                return true;
            }
            catch(Exception)
            {
                if (log.IsFatalEnabled)
                {
                    log.Fatal("Failed to complete import of the packages. Rolling back...");
                }
                ResourceMonitor.Rollback(transactionId);
                if (log.IsInfoEnabled)
                {
                    log.Info("Import interrupted...");
                }
                return false;
            }
		}

		private void ImportModelFile(
			ModelFileInfo modelFileInfo, ArrayList presentPackages, 
			string transactionId)
		{
            string message = "Importing model " + modelFileInfo.ExtensionName + "...";
            if (log.IsInfoEnabled)
            {
                log.Info(message);
            }
            OnStateChanged(message);
			DataSet data = null;
			try
			{
				bool isNewPackage 
					= IsPackageNew(modelFileInfo.ExtensionId, presentPackages);
				if(! isNewPackage)
				{
					persistenceService.LoadSchema(
						new Guid(modelFileInfo.ExtensionId), true, true, transactionId);
				}
				else
				{
                    if (log.IsInfoEnabled)
                    {
                        log.Info("Model " + modelFileInfo.ExtensionName
                            + " is not in DB. It is going to be created.");
                    }
				}
                OnStateChanged("Loading XML file " + modelFileInfo.File);
                data = FillSchemaData(modelFileInfo, isNewPackage);
                OnStateChanged("Merging XML to model");
                persistenceService.MergePackage(
					new Guid(modelFileInfo.ExtensionId), data, transactionId);
                if (log.IsInfoEnabled)
                {
                    log.Info("Model " + modelFileInfo.ExtensionName + " imported.");
                }
			}
			catch(Exception ex)
			{
                message = "Failed to import model " + modelFileInfo.ExtensionName;
                if (log.IsErrorEnabled)
                {
                    log.Error(message, ex);
                }
				throw new Exception(message, ex);
			}
			finally
			{
				if(data != null)
				{
					data.Dispose();
				}
			}
		}

		private bool IsPackageNew(string extensionId, ArrayList presentPackages)
		{
			bool retVal = true;
			foreach(SchemaExtension extension in presentPackages)
			{
				if(extension.NodeId == extensionId)
				{
					retVal = false;
					break;
				}
			}
			return retVal;
		}

		private DataSet FillSchemaData(
			ModelFileInfo modelFileInfo, bool isNewPackage)
		{
			DataSet data = new DataSet();
			data.EnforceConstraints = false;
			data.ReadXml(modelFileInfo.File);
			return data;
		}

		private DataSet CreateEmptyDataSet()
		{
			return
				(persistenceService.SchemaProvider as DatabasePersistenceProvider).DataService
				.GetEmptyDataSet(new Guid(PersistenceService.SchemaDataStructureId));
		}


		private string GetRepositoryDirectory()
		{
			OrigamSettings settings 
				= ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
			if(settings == null)
			{
				throw new Exception(
					"Configuration manager doesn't have set OrigamSettings as active configuration.");
			}
			if((settings.ModelSourceControlLocation == null) 
			|| (settings.ModelSourceControlLocation.Length == 0))
			{
				throw new Exception("Model source repository location is not set.");
			}
            if (log.IsInfoEnabled)
            {
                log.Info("Using " + settings.ModelSourceControlLocation
                    + " as repository location...");
            }
			return settings.ModelSourceControlLocation;
		}

		private ArrayList GetModelFiles(string repositoryDirectory)
		{
            if (log.IsInfoEnabled)
            {
                log.Info("Searching for model files in "
                    + repositoryDirectory + "...");
            }
			ArrayList xmlFiles = new ArrayList();
			SearchForXmlFiles(repositoryDirectory, xmlFiles);
            if (log.IsDebugEnabled)
            {
                log.Debug("Found " + xmlFiles.Count + " xml files...");
                log.Debug("Parsing xml files...");
            }
			ArrayList modelFiles = new ArrayList();
			foreach(string xmlFile in xmlFiles)
			{
				ModelFileInfo modelFileInfo = FileToModelFileInfo(xmlFile);
				if(modelFileInfo != null)
				{
					modelFiles.Add(modelFileInfo);
				}
			}
            if (log.IsInfoEnabled)
            {
                log.Info("Found " + modelFiles.Count + " model files...");
            }
			return modelFiles;
		}

		private void SearchForXmlFiles(string directory, ArrayList xmlFiles)
		{
			foreach (string file in Directory.GetFiles(directory, "*.xml"))
			{
				xmlFiles.Add(file);
			}
			foreach(string subDirectory in Directory.GetDirectories(directory))
			{
				SearchForXmlFiles(subDirectory, xmlFiles);
			}
		}

		private ModelFileInfo FileToModelFileInfo(string xmlFile)
		{
			XmlTextReader xmlReader = null;
			try
			{
				xmlReader = new XmlTextReader(xmlFile);
				while(xmlReader.Read())
				{
					if(xmlReader.Name == "SchemaExtension")
					{
						ModelFileInfo modelFileInfo = new ModelFileInfo();
						modelFileInfo.File = xmlFile;
						modelFileInfo.ExtensionId = xmlReader.GetAttribute("Id");
						modelFileInfo.ExtensionName = xmlReader.GetAttribute("Name");
                        if (log.IsInfoEnabled)
                        {
                            log.Info("File " + xmlFile + " contains model "
                                + modelFileInfo.ExtensionName
                                + " [" + modelFileInfo.ExtensionId + "]");
                        }
						return modelFileInfo;
					}
				}
                if (log.IsWarnEnabled)
                {
                    log.Warn("File " + xmlFile + " doesn't seem to contain model. Skipping...");
                }
				return null;
			}
			finally
			{
				if(xmlReader != null)
				{
					xmlReader.Close();
				}
			}
		}

        private void OnStateChanged(string state)
        {
            if (StateChanged != null)
            {
                StateChanged(this, new ModelImportStatusEventArgs(state));
            }
        }
	}

	class ModelFileInfo
	{
		public string File;

		public string ExtensionId;

		public string ExtensionName;
	}
}
