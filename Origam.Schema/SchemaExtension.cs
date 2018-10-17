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

using Origam.UI;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using System.Collections.Generic;
using Origam.DA;

namespace Origam.Schema
{
	/// <summary>
	/// Summary description for Schema.
	/// </summary>
	[EntityName("SchemaExtension")]
    [XmlPackageRoot("package")]
	public class SchemaExtension : AbstractPersistent, IBrowserNode2, IComparable, IFilePersistent
	{
       // public const string NAMESPACE = "http://schemas.origam.com/*.*.*/package";

        SchemaItemProviderGroup _commonModelGroup = new SchemaItemProviderGroup("COMMON", "Common", "7", 0);
		SchemaItemProviderGroup _dataModelGroup = new SchemaItemProviderGroup("DATA", "Data", "7", 1);
		SchemaItemProviderGroup _userInterfaceModelGroup = new SchemaItemProviderGroup("UI", "User Interface", "7", 2);
		SchemaItemProviderGroup _businessLogicModelGroup = new SchemaItemProviderGroup("BL", "Business Logic", "7", 3);
		SchemaItemProviderGroup _apiModelGroup = new SchemaItemProviderGroup("API", "API", "7", 4);
		
		public SchemaExtension()
		{
			this.PrimaryKey = new ModelElementKey();
		}

		public SchemaExtension(Key primaryKey) : base(primaryKey, new ModelElementKey().KeyArray) 
		{
			this._childNodes.Add(_dataModelGroup);
			this._childNodes.Add(_businessLogicModelGroup);
			this._childNodes.Add(_userInterfaceModelGroup);
			this._childNodes.Add(_apiModelGroup);
			this._childNodes.Add(_commonModelGroup);
		}

		public override string ToString() => this.Name;

		#region Properties
		public bool IsFileRootElement => true;

		[EntityColumn("Name")]
		[XmlAttribute(AttributeName = "name")]
		public string name  = "";

		public string Name
		{
			get => name;
			set
			{
				OldName = name;
				name = value;
			}
		}

		public string OldName { get; private set; } = "";
		public bool WasRenamed =>
			!string.IsNullOrEmpty(OldName) && OldName != Name;

		/// <summary>
		/// Gets or sets the version of this schema extension.
		/// </summary>
		[EntityColumn("Version")]
        [XmlAttribute(AttributeName = "version")]
        public string VersionString { get; set; } = "";

		public PackageVersion Version => new PackageVersion(VersionString);

		[EntityColumn("Copyright")]
        [XmlAttribute(AttributeName = "copyright")]
        public string Copyright { get; set; } = "";

		[EntityColumn("Description")]
        [XmlAttribute(AttributeName = "description")]
        public string Description { get; set; } = "";

		public IList<SchemaExtension> IncludedPackages
        {
            get
            {
	            List<SchemaExtension> result = new List<SchemaExtension>();
                SortPackages(this, result);
                return result;
            }
        }

        private static void SortPackages(SchemaExtension package, IList<SchemaExtension> packages)
        {
            foreach (PackageReference reference in package.References)
            {
                if (!packages.Contains(reference.ReferencedPackage))
                {
                    SortPackages(reference.ReferencedPackage, packages);
                    packages.Add(reference.ReferencedPackage);
                }
            }
        }
        #endregion

		#region IBrowserNode2 Members
		[Browsable(false)] 
		public bool Hide
		{
			get => !this.IsPersisted;
			set => throw new InvalidOperationException(ResourceUtils.GetString("ErrorSetHide"));
		}
		
		public bool CanDelete => false;

		public void Delete()
		{
			// TODO:  Add SchemaExtension.Delete implementation
		}

		public bool CanMove(IBrowserNode2 newNode) => false;

		[Browsable(false)]
		public IBrowserNode2 ParentNode
		{
			get => null;
			set => throw new InvalidOperationException(ResourceUtils.GetString("ErrorMoveExtension"));
		}

		public byte[] NodeImage => null;

		[Browsable(false)] 
		public string NodeId => this.PrimaryKey["Id"].ToString();

		[Browsable(false)]
        public virtual string FontStyle => "Regular";
		#endregion

		#region IBrowserNode Members

		public bool HasChildNodes => this.ChildNodes().Count > 0;

		public bool CanRename => false;

		public List<PackageReference> References =>
			PersistenceProvider
				.RetrieveListByParent<PackageReference>(this.PrimaryKey,
					"SchemaExtension", "PackageReference", this.UseObjectCache);

		BrowserNodeCollection _childNodes = new BrowserNodeCollection();

		public BrowserNodeCollection ChildNodes()
		{
			return _childNodes;
			//return new BrowserNodeCollection(this.References.ToArray(typeof(IBrowserNode)) as IBrowserNode[]);
		}

		public void AddProvider(AbstractSchemaItemProvider provider)
		{
			SchemaItemProviderGroup group = null;
			foreach(SchemaItemProviderGroup childGroup in this.ChildNodes())
			{
				if(childGroup.NodeId == provider.Group)
				{
					group = childGroup;
					break;
				}
			}
			if(group == null)
			{
				throw new ArgumentOutOfRangeException("group", provider.Group, "unknown model group");
			}
			group.ChildNodes().Add(provider);
		}

		public string NodeText
		{
			get => this.Name;
			set => this.Name = value;
		}

		public string NodeToolTipText => null;

		public string Icon => "59";

		public string RelativeFilePath => this.Name + "\\"+ PersistenceFiles.PackageFileName;

		public Guid FileParentId
		{
			get => Guid.Empty;
			set { }
		}

		public bool IsFolder => true;

		public IDictionary<ElementName, Guid> ParentFolderIds => new Dictionary<ElementName, Guid>();

		public string Path => null;
		#endregion

        #region IComparable Members
        public int CompareTo(object obj)
		{
			IBrowserNode bn = obj as IBrowserNode;

			if(bn != null)
			{
				return this.NodeText.CompareTo(bn.NodeText);
			}
			else
			{
				throw new InvalidCastException();
			}
		}
		#endregion
	}
}
