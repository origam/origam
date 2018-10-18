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
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;

namespace Origam.Workbench.Services
{
	/// <summary>
	/// Summary description for DocumentationService.
	/// </summary>
	public class DocumentationService : AbstractDocumentationService
	{
		#region Private Members
		private IDataService _dataService;
		private SchemaService _schemaService;
private static int counter = 0;

		private int counterInstance = 0;
		private readonly IPersistenceProvider persistenceProvider;

		private const string DOCUMENTATION_SINGLE = "133472de-c49d-4a13-924d-a668f248f4fa";
		private const string DOCUMENTATION_SINGLE_FILTER = "1c67c7b9-743c-49ea-9f78-02d66f67a297";
		private const string DOCUMENTATION_COMPLETE = "f4b65a69-d581-46a7-b911-98933ccff8df";
		private const string DOCUMENTATION_COMPLETE_FILTER = "beb983a5-b47c-415a-a572-aab56c348233";

		#endregion

		#region Constructors
		public DocumentationService()
		{
			counter++;
			counterInstance = counter;

			// we take the model's data service
			IPersistenceService persistence = ServiceManager.Services.GetService<IPersistenceService>();
			persistenceProvider = persistence.SchemaProvider;
			_dataService = (persistence.SchemaProvider as DatabasePersistenceProvider).DataService;
			_schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

			if(_dataService == null)
			{
				throw new Exception(ResourceUtils.GetString("ErrorNoDataServiceNoDocsService"));
			}
		}
		#endregion

		#region IService Members
		public override event EventHandler Initialize;
		public override event EventHandler Unload;

		public override void InitializeService()
		{
			OnInitialize(EventArgs.Empty);
		}

		public override void UnloadService()
		{
			_dataService = null;
			_schemaService = null;

			OnUnload(EventArgs.Empty);
		}
		
		protected void OnInitialize(EventArgs e)
		{
			if (Initialize != null) 
			{
				Initialize(this, e);
			}
		}
		
		protected void OnUnload(EventArgs e)
		{
			if (Unload != null) 
			{
				Unload(this, e);
			}
		}
		#endregion

		#region IDocumentationService Members
		public override string GetDocumentation(Guid schemaItemId, DocumentationType docType)
		{
			DataStructureQuery query = new DataStructureQuery(new Guid(DOCUMENTATION_SINGLE), new Guid(DOCUMENTATION_SINGLE_FILTER));

			query.Parameters.Add(new QueryParameter("Documentation_parSchemaItemId", schemaItemId));
			query.Parameters.Add(new QueryParameter("Documentation_parCategory", docType.ToString()));

			string result = (string)_dataService.GetScalarValue(query, "Data", SecurityManager.CurrentPrincipal, null) ?? "";

			return persistenceProvider.LocalizationCache
				.GetLocalizedString(schemaItemId, "Documentation " + docType, result);
		}

		public override DocumentationComplete LoadDocumentation(Guid schemaItemId)
		{
			DataStructureQuery query = new DataStructureQuery(new Guid(DOCUMENTATION_COMPLETE), new Guid(DOCUMENTATION_COMPLETE_FILTER));
			
			query.Parameters.Add(new QueryParameter("Documentation_parSchemaItemId", schemaItemId));
			query.LoadByIdentity = false;

			DocumentationComplete documentationData = new DocumentationComplete();
			_dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal, documentationData, null);
			return documentationData;
		}

		public override void SaveDocumentation(DocumentationComplete documentationData,
			Guid schemaItemId)
		{
			SaveDocumentation(documentationData);
		}

		public override DocumentationComplete GetAllDocumentation()
		{
			DataStructureQuery query = new DataStructureQuery(new Guid(DOCUMENTATION_COMPLETE), Guid.Empty);
			query.LoadByIdentity = false;

			DocumentationComplete documentationData = new DocumentationComplete();
			_dataService.LoadDataSet(query, SecurityManager.CurrentPrincipal, documentationData, null);
			return documentationData;
		}

		public override void SaveDocumentation(DocumentationComplete documentationData)
		{
			DataStructureQuery query = new DataStructureQuery(new Guid(DOCUMENTATION_COMPLETE), new Guid(DOCUMENTATION_COMPLETE_FILTER));
			query.LoadByIdentity = false;
			
			_dataService.UpdateData(query, SecurityManager.CurrentPrincipal, documentationData, null);
		}
		#endregion
	}
}
