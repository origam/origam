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

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for DataConstantItemProvider.
	/// </summary>
	public class DatabaseDataTypeSchemaItemProvider : AbstractSchemaItemProvider, 
        ISchemaItemFactory
	{
		public DatabaseDataTypeSchemaItemProvider()
		{
            this.ChildItemTypes.Add(typeof(DatabaseDataType));
		}

        public DatabaseDataType FindDataType(string name)
        {
            foreach (DatabaseDataType item in ChildItems)
            {
                if (string.Compare(name, 
                    item.MappedDatabaseTypeName, true) == 0)
                {
                    return item;
                }
            }
            return null;
        }

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return DatabaseDataType.CategoryConst;
			}
		}
		public override bool AutoCreateFolder
		{
			get
			{
				return true;
			}
		}
		public override string Group
		{
			get
			{
				return "DATA";
			}
		}
		#endregion

		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
				return "icon_08_database-data-types.png";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Database Data Types";
			}
			set
			{
				base.NodeText = value;
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
				return null;
			}
		}

		#endregion
	}
}
