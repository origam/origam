using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;

using Origam.UI;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema
{
	/// <summary>
	/// 
	/// </summary>
	[EntityName("SchemaVersion")]
	public class SchemaVersion : AbstractPersistent, IBrowserNode2
	{
		public SchemaVersion(System.Guid id)
		{
			this.PrimaryKey.Add("Id", id);
		}

		public SchemaVersion(Key primaryKey) : base(primaryKey, new string[1] {"Id"})	{}

		#region Properties
		/// <summary>
		/// Gets or sets the name of this schema version.
		/// </summary>
		[EntityColumn("Name")] private string _name = "";
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		private Schema _schema = null;
		/// <summary>
		/// Gets or sets the name of this schema version.
		/// </summary>
		[EntityColumn("refSchema", true)] 
		public Schema Schema
		{
			get
			{
				return _schema;
			}
			set
			{
				_schema = value;
			}
		}

		public ArrayList Extensions
		{
			get
			{
				// Get children
				ArrayList list = this.PersistenceProvider.RetrieveList(typeof(SchemaExtension), "SchemaVersionId = '" + this.PrimaryKey["Id"] + "' and refParentSchemaExtensionId is null");

				return list;
			}
		}

		public ArrayList AllExtensions
		{
			get
			{
				ArrayList result = this.Extensions;

				foreach(SchemaExtension extension in this.Extensions)
				{
					result.AddRange(extension.ChildExtensionsRecursive);
				}

				return result;
			}
		}
		#endregion

		#region IBrowserNode Members
		[Browsable(false)] 
		public bool Hide
		{
			get
			{
				return !this.IsPersisted;
			}
			set
			{
				throw new InvalidOperationException("Cannot set Hide property");
			}
		}

		public bool HasChildNodes
		{
			get
			{
				return this.ChildNodes().Count > 0;
			}
		}

		public bool CanRename
		{
			get
			{
				// TODO:  Add SchemaVersion.CanRename getter implementation
				return false;
			}
		}

		public string Icon
		{
			get
			{
				// TODO:  Add SchemaVersion.ImageIndex getter implementation
				return "0";
			}
		}

		public Bitmap NodeImage
		{
			get
			{
				return null;
			}
		}

		public BrowserNodeCollection ChildNodes()
		{
			BrowserNodeCollection col = new BrowserNodeCollection();

			foreach(SchemaExtension ext in this.Extensions)
				col.Add(ext);

			return col;
		}

		public string NodeText
		{
			get
			{
				// TODO:  Add SchemaVersion.NodeText getter implementation
				return this.Name;
			}
			set
			{
				this.Name = value;
				this.Persist();
			}
		}

		public string NodeToolTipText
		{
			get
			{
				// TODO:  Add SchemaVersion.NodeToolTipText getter implementation
				return null;
			}
		}

		public bool CanDelete
		{
			get
			{
				return true;
			}
		}

		public void Delete()
		{
			this.IsDeleted = true;
		}

		public bool CanMove(IBrowserNode2 newNode)
		{
			return false;
		}

		[Browsable(false)]
		public IBrowserNode2 ParentNode
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException("Cannot move schema version.");
			}
		}
		#endregion

	}
}
