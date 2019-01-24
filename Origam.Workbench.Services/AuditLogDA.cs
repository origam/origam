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
using System.Data;
using Origam.DA;
using Origam.Workbench.Services;
using Origam.Schema;

namespace Origam.Workbench
{
	/// <summary>
	/// Summary description for AuditLogDA.
	/// </summary>
	public class AuditLogDA
	{
		public static bool IsRecordIdValid(object recordId)
		{
			if(recordId is Guid) return true;

			string stringId = recordId as string;

			if(stringId != null)
			{
				try
				{
					Guid guidId = new Guid(stringId);
					return true;
				}
				catch
				{
					return false;
				}
			}

			return false;
		}

		public static DataSet RetrieveLog(Guid entityId, object recordId)
		{
			if(! IsRecordIdValid(recordId)) return null;

			IBusinessServicesService services = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
			IServiceAgent dataServiceAgent = services.GetAgent("DataService", null, null);

			DataStructureQuery query = new DataStructureQuery(new Guid("530eba45-40db-470d-8e53-8b98ace758ad"), new Guid("e0602afa-e86d-4f03-bcb8-aee19f976961"));
			query.Parameters.Add(new QueryParameter("AuditRecord_parRefParentRecordId", recordId));
        
			dataServiceAgent.MethodName = "LoadDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", query);

			dataServiceAgent.Run();

			DataSet result = dataServiceAgent.Result as DataSet;

			return result;
		}

		public static DataSet RetrieveLogTransformed(Guid entityId, object recordId)
		{
			if(! IsRecordIdValid(recordId)) return null;

			DataSet src = RetrieveLog(entityId, recordId);

			DataSet ds = new DataSet("ROOT");
			DataTable t = new OrigamDataTable("AuditLog");
			ds.Tables.Add(t);

			t.Columns.Add("Id", typeof(Guid));
			t.Columns.Add("Timestamp", typeof(DateTime));
			t.Columns.Add("User", typeof(string));
			t.Columns.Add("Property", typeof(string));
			t.Columns.Add("OldValue", typeof(string));
			t.Columns.Add("NewValue", typeof(string));
			t.Columns.Add("ActionType", typeof(int));

			foreach(DataRow r in src.Tables[0].Rows)
			{
				string user;

				try
				{
					IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();
					UserProfile profile = (profileProvider as IOrigamProfileProvider).GetProfile((Guid)r["RecordCreatedBy"]) as UserProfile;
					user = profile.FullName;
				}
				catch(Exception ex)
				{
					user = ex.Message;
				}

				string property;

				Guid columnId = (Guid)r["refColumnId"];
				try
				{

					IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
					AbstractSchemaItem item = persistence.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(columnId)) as AbstractSchemaItem;

					if(item == null) property = columnId.ToString();

					if(item is ICaptionSchemaItem)
					{
						if((item as ICaptionSchemaItem).Caption != null & (item as ICaptionSchemaItem).Caption != "")
						{
							property = (item as ICaptionSchemaItem).Caption;
						}
						else
						{
							property = item.Name;
						}
					}
					else
					{
						property = item.Name;
					}
				}
				catch
				{
					property = columnId.ToString();
				}

				t.LoadDataRow(new object[] {r["Id"], r["RecordCreated"], user, property, r["OldValue"], r["NewValue"], r["ActionType"]}, true);
			}

			return ds;
		}
	}
}
