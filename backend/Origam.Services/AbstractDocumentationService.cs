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
using Origam.Schema;

namespace Origam.Workbench.Services
{
	/// <summary>
	/// Summary description for AbstractDocumentationService.
	/// </summary>
	public abstract class AbstractDocumentationService : IDocumentationService
	{
		public AbstractDocumentationService()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		#region IDocumentationService Members

		public abstract string GetDocumentation(Guid schemaItemId, DocumentationType docType);

		public abstract DocumentationComplete LoadDocumentation(Guid schemaItemId);

		public abstract void SaveDocumentation(DocumentationComplete documentationData, Guid schemaItemId);

		public abstract void SaveDocumentation(DocumentationComplete documentationData);

		public void CloneDocumentation(List<ISchemaItem> clonedSchemaItems)
		{
			DocumentationComplete newDoc = new DocumentationComplete();

			foreach(ISchemaItem item in clonedSchemaItems)
			{
				if(item.OldPrimaryKey != null)
				{
					DocumentationComplete doc = 
						LoadDocumentation((Guid)item.OldPrimaryKey["Id"]);
					
					foreach(DocumentationComplete.DocumentationRow row in doc.Documentation.Rows)
					{
						DocumentationComplete.DocumentationRow newRow = newDoc.Documentation.NewDocumentationRow();

						// We set the new id.
						newRow.Id = Guid.NewGuid();
						
						// We copy all the other properties.
						newRow.Category = row.Category;
						newRow.Data = row.Data;
						newRow.refSchemaItemId = (Guid)item.PrimaryKey["Id"];

						newDoc.Documentation.AddDocumentationRow(newRow);
					}
				}
			}

			if(newDoc.Documentation.Rows.Count > 0)
			{
				SaveDocumentation(newDoc);
			}
		}

		public abstract DocumentationComplete GetAllDocumentation();
		#endregion

		#region IWorkbenchService Members

		public abstract void InitializeService();
		public abstract void UnloadService();

		public abstract event System.EventHandler Initialize;

		public abstract event System.EventHandler Unload;

		#endregion
	}

	public enum DocumentationType
	{
		USER_SHORT_HELP,
		USER_LONG_HELP,
		USER_WFSTEP_DESCRIPTION,
		DEV_INTERNAL,
		RULE_EXCEPTION_MESSAGE,
		EXAMPLE,
		EXAMPLE_JSON,
		EXAMPLE_XML
	}
}
