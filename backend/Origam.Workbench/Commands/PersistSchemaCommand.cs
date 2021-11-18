#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Workbench.Commands
{
	/// <summary>
	/// Persists all changes to the schema to the database.
	/// </summary>
	public class PersistSchema : AbstractMenuCommand
	{
		SchemaService schema = ServiceManager.Services
			.GetService<SchemaService>() ;

		public override bool IsEnabled
        {
            get
            {
                return schema.IsSchemaLoaded 
                        && !(ServiceManager.Services
                        .GetService<IPersistenceService>() 
                        is FilePersistenceService);
            }
            set
            {
                base.IsEnabled = value;
            }
        }

		public override void Run()
		{
			schema.SaveSchema();
		}

		public override void Dispose()
		{
			schema = null;
		}
	}
}
