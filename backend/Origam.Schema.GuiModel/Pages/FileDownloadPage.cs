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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Schema.GuiModel;

[SchemaItemDescription("File Download Page", "data-page.png")]
[HelpTopic("File+Download+Page")]
[ClassMetaVersion("6.0.0")]
public class FileDownloadPage : AbstractPage, IDataStructureReference
{
	public FileDownloadPage() : base() {Init();}
	public FileDownloadPage(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public FileDownloadPage(Key primaryKey) : base(primaryKey) {Init();}

	private void Init()
	{
			this.ChildItemTypes.Add(typeof(PageParameterMapping));
		}

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			dependencies.Add(this.DataStructure);
			if(this.Method != null) dependencies.Add(this.Method);
			if(this.SortSet != null) dependencies.Add(this.SortSet);

			base.GetExtraDependencies (dependencies);
		}

	public override IList<string> NewTypeNames
	{
		get
		{
				try
				{
					IBusinessServicesService agents = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
					IServiceAgent agent = agents.GetAgent("DataService", null, null);
					return agent.ExpectedParameterNames(this, "LoadData", "Parameters");
				}
				catch
				{
					return new string[] {};
				}
			}
	}
	#region Properties
	public Guid DataStructureId;

	[TypeConverter(typeof(DataStructureConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[XmlReference("dataStructure", "DataStructureId")]
	public DataStructure DataStructure
	{
		get
		{
				return (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureId));
			}
		set
		{
				this.Method = null;
				this.SortSet = null;
				this.DataStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
	}

	public Guid DataStructureMethodId;

	[TypeConverter(typeof(DataStructureReferenceMethodConverter))]
	[XmlReference("method", "DataStructureMethodId")]
	public DataStructureMethod Method
	{
		get
		{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureMethodId));
			}
		set
		{
				this.DataStructureMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
	}
  
	public Guid DataStructureSortSetId;

	[TypeConverter(typeof(DataStructureReferenceSortSetConverter))]
	[XmlReference("sortSet", "DataStructureSortSetId")]
	public DataStructureSortSet SortSet
	{
		get
		{
				return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureSortSetId));
			}
		set
		{
				this.DataStructureSortSetId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
	}

	private string _contentField = "";
	[Category("File Data")]
	[StringNotEmptyModelElementRule()]
	[XmlAttribute("contentField")]
	public string ContentField
	{
		get
		{
				return _contentField;
			}
		set
		{
				_contentField = value;
			}
	}

	private string _fileNameField = "";
	[Category("File Data")]
	[StringNotEmptyModelElementRule()]
	[XmlAttribute("fileNameField")]
	public string FileNameField
	{
		get
		{
				return _fileNameField;
			}
		set
		{
				_fileNameField = value;
			}
	}
	#endregion			
}