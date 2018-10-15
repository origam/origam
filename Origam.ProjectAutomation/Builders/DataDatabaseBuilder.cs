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

using Origam;

namespace Origam.ProjectAutomation
{
    public class DataDatabaseBuilder : AbstractDatabaseBuilder
    {
        string _databaseName;

        public override string Name
        {
            get
            {
                return "Create Data Database";
            }
        }

        public override void Execute(Project project)
        {
            _databaseName = project.DataDatabaseName;
            this.DataService.ConnectionString = DataService.BuildConnectionString(
                project.DatabaseServerName, "", project.DatabaseUserName,
                project.DatabasePassword, project.DatabaseIntegratedAuthentication, false);
            this.DataService.CreateDatabase(_databaseName);
        }

        public string BuildConnectionString(Project project, bool pooling)
        {
            return DataService.BuildConnectionString(project.DatabaseServerName,
                project.DataDatabaseName, project.DatabaseUserName,
                project.DatabasePassword, project.DatabaseIntegratedAuthentication, pooling);
        }

        public override void Rollback()
        {
            OrigamUserContext.Reset();
            this.DataService.DropDatabase(_databaseName);
        }
    }
}
