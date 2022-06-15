#region license
/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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
using System.ComponentModel;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel
{
	[SchemaItemDescription("(Task) Accept Context Store Changes", "Tasks", 16)]
    [HelpTopic("Accept+Context+Store+Changes")]
    [ClassMetaVersion("6.0.0")]
    public class AcceptContextStoreChangesTask : AbstractWorkflowStep
    {
		public AcceptContextStoreChangesTask(Guid schemaExtensionId) 
			: base(schemaExtensionId) {}
		public AcceptContextStoreChangesTask(Key primaryKey) 
			: base(primaryKey)	{}
		#region Properties
		public Guid ContextStoreId;

		[TypeConverter(typeof(ContextStoreConverter))]
        [NotNullModelElementRule]
        [XmlReference("contextStore", "ContextStoreId")]
		public IContextStore ContextStore
		{
			get
			{
				var key = new ModelElementKey
				{
					Id = this.ContextStoreId
				};
				return (IContextStore)PersistenceProvider.RetrieveInstance(
					typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					ContextStoreId = Guid.Empty;
				}
				else
				{
					ContextStoreId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}
		#endregion
    }
}
