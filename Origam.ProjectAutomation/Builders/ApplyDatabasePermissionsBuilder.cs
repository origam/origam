#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using Origam.DA.Service;
using System;
using static Origam.ProjectAutomation.Project;

namespace Origam.ProjectAutomation
{
    public class ApplyDatabasePermissionsBuilder : AbstractDatabaseBuilder
    {
        string _loginName;
        bool _integratedAuthentication = false;
        DatabaseType _databaseType;

        public override string Name
        {
            get
            {
                return "Apply Database Permissions";
            }
        }

        public override void Execute(Project project)
        {
            _databaseType = project.DatabaseTp;
            DataService(_databaseType).ConnectionString =
                DataService(_databaseType).BuildConnectionString(project.DatabaseServerName,project.Port,
                project.DataDatabaseName, project.DatabaseUserName,
                project.DatabasePassword, project.DatabaseIntegratedAuthentication, false);
            if (project.DatabaseIntegratedAuthentication)
            {
                _loginName = string.Format("[IIS APPPOOL\\{0}]", project.Name);
                _integratedAuthentication = project.DatabaseIntegratedAuthentication;
                string command1 = string.Format("CREATE LOGIN {0} FROM WINDOWS WITH DEFAULT_DATABASE=[master]", _loginName);
                string command2 = string.Format("CREATE USER {0} FOR LOGIN {0}", _loginName);
                string command3 = string.Format("ALTER ROLE [db_datareader] ADD MEMBER {0}", _loginName);
                string command4 = string.Format("ALTER ROLE [db_datawriter] ADD MEMBER {0}", _loginName);
                string transaction1 = Guid.NewGuid().ToString();
                try
                {
                    DataService(_databaseType).ExecuteUpdate(command1, transaction1);
                    DataService(_databaseType).ExecuteUpdate(command2, transaction1);
                    DataService(_databaseType).ExecuteUpdate(command3, transaction1);
                    DataService(_databaseType).ExecuteUpdate(command4, transaction1);
                    ResourceMonitor.Commit(transaction1);
                }
                catch (Exception)
                {
                    ResourceMonitor.Rollback(transaction1);
                    throw;
                }
            }
        }

        public override void Rollback()
        {
            if (_integratedAuthentication)
            {
                string command1 = string.Format("DROP LOGIN {0}", _loginName);
                DataService(_databaseType).ExecuteUpdate(command1, null);
            }
        }
    }
}
