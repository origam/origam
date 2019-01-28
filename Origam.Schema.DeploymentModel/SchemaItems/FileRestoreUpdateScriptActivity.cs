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
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.DeploymentModel
{
	public enum DeploymentFileLocation
	{
		Manual = 0,
		SystemFolder = 1,
		ApplicationStartupFolder = 2,
		ReportsFolder = 3
	}

	/// <summary>
	/// Summary description for FileRestoreUpdateScriptActivity.
	/// </summary>
	[SchemaItemDescription("File Restore Update Activity", 48)]
    [HelpTopic("File+Restore+Update+Activity")]
	public class FileRestoreUpdateScriptActivity : AbstractUpdateScriptActivity
	{
		public FileRestoreUpdateScriptActivity() : base()
        {
            InitializeProperyContainers();
        }

		public FileRestoreUpdateScriptActivity(Guid schemaExtensionId) : base(schemaExtensionId)
        {
            InitializeProperyContainers();
        }

        public FileRestoreUpdateScriptActivity(Key primaryKey) : base(primaryKey)
        {
            InitializeProperyContainers();
        }

        private void InitializeProperyContainers()
        {
            content = new PropertyContainer<byte[]>(
                containerName: nameof(content),
                containingObject: this);
        }

        public override string Icon
		{
			get
			{
				return "48";
			}
		}

		#region Properties
		private PropertyContainer<byte[]> content;

		[Category("File Information")]
		[EntityColumn("BLB01")]
        [XmlExternalFileReference(containerName: nameof(content), 
            extension: ExternalFileExtension.Bin)]
        public Byte[] File 
		{
            get => content.Get();
            set => content.Set(value);
        }

        private string _manualLocation;
		[Category("File Information")]
		[EntityColumn("LS01")]
        [XmlAttribute("fileName")]
		public string FileName 
		{
			get
			{
				return _manualLocation;
			}
			set
			{
				_manualLocation = value;
			}
		}

		private DeploymentFileLocation _targetLocation = DeploymentFileLocation.ReportsFolder;
		[Category("File Information")]
		[EntityColumn("I05")]
        [XmlAttribute("targetLocation")]
		public DeploymentFileLocation TargetLocation 
		{
			get
			{
				return _targetLocation;
			}
			set
			{
				_targetLocation = value;
			}
		}
		#endregion
	}
}
