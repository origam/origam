using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;

using Origam;
using Origam.UI;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema
{
	/// <summary>
	/// Schema root. Identifies an application.
	/// </summary>
	[EntityName("Schema")]
	public class Schema : AbstractPersistent, IBrowserNode2
	{
		public Schema()
		{
			this.PrimaryKey.Add("Id", Guid.NewGuid());
		}

		public Schema(Key primaryKey) : base(primaryKey, new string[1] {"Id"})	{}

		#region Properties
		private string _name = "";

		/// <summary>
		/// Gets or sets name of the schema.
		/// </summary>
		[EntityColumn("Name")] public string Name
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

		public ArrayList Versions
		{
			get
			{
				// Get children
				ArrayList list = this.PersistenceProvider.RetrieveList(typeof(SchemaVersion), "refSchemaId = '" + this.PrimaryKey["Id"] + "'");

				// Set parent for each child
				foreach(SchemaVersion ver in list)
				{
					ver.Schema = this;
				}
				
				return list;
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

		[Browsable(false)] public bool HasChildNodes
		{
			get
			{
				return this.Versions.Count > 0;
			}
		}

		[Browsable(false)] public bool CanRename
		{
			get
			{
				return true;
			}
		}

		[Browsable(false)] public string Icon
		{
			get
			{
				// TODO:  Add Schema.ImageIndex getter implementation
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

			foreach(SchemaVersion ver in this.Versions)
				col.Add(ver);

			return col;
		}

		[Browsable(false)] public string NodeText
		{
			get
			{
				return this.Name;;
			}
			set
			{
				this.Name = value;
				this.Persist();
			}
		}

		[Browsable(false)] public string NodeToolTipText
		{
			get
			{
				// TODO:  Add Schema.NodeToolTipText getter implementation
				return null;
			}
		}

		[Browsable(false)] 
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
				throw new InvalidOperationException("Cannot move schema.");
			}
		}
		#endregion
	}
}
