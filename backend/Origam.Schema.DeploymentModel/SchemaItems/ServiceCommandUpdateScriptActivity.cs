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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.WorkflowModel;
using static Origam.DA.Common.Enums;

namespace Origam.Schema.DeploymentModel;

/// <summary>
/// Summary description for ServiceCommandUpdateScriptActivity.
/// </summary>
[SchemaItemDescription("Service Command Update Activity", 
	"icon_service-command-update-activity.png")]
[HelpTopic("Service+Command+Update+Activity")]
[ClassMetaVersion("6.0.0")]
public class ServiceCommandUpdateScriptActivity : AbstractUpdateScriptActivity
{
	public ServiceCommandUpdateScriptActivity() : base()
	{
            InitializeProperyContainers();
        }

	public ServiceCommandUpdateScriptActivity(Guid schemaExtensionId) : base(schemaExtensionId)
	{
            InitializeProperyContainers();
        }

	public ServiceCommandUpdateScriptActivity(Key primaryKey) : base(primaryKey)
	{
            InitializeProperyContainers();
        }

	private void InitializeProperyContainers()
	{
            commandText = new PropertyContainer<string>(
                containerName: nameof(commandText),
                containingObject: this);
        }

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			dependencies.Add(this.Service);

			base.GetExtraDependencies (dependencies);
		}

	#region Properties

	[Category("Update Script Activity")]
	[XmlAttribute("platform")]
	public DatabaseType DatabaseType { get; set; } 

	public Guid ServiceId;

	[Category("Service Command Information")]
	[TypeConverter(typeof(ServiceConverter))]
	[XmlReference("service", "ServiceId")]
	public IService Service
	{
		get
		{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ServiceId;

				return (IService)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
		set
		{
				if(value == null)
				{
					this.ServiceId = Guid.Empty;
				}
				else
				{
					this.ServiceId = (Guid)value.PrimaryKey["Id"];
				}
			}
	}

	private PropertyContainer<string> commandText;

	[Category("Service Command Information")]
	[XmlExternalFileReference(containerName: nameof(commandText),
		extension: ExternalFileExtension.Txt)]
	public string CommandText
	{
		get => commandText.Get();
		set => commandText.Set(value);
	}
	#endregion
}