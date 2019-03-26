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


namespace Origam.ProjectAutomation
{
    public class Project
    {
        private string _name;
        private string _dataDatabaseName;
        private string _modelDatabaseName;
        private string _databaseServerName;
        private string _databaseUserName;
        private string _databasePassword;
        private bool _databaseIntegratedAuthentication;
        private string _webRootName;
        private string _url;
        private string _binFolder;
        private string _dataConnectionString;
        private string _modelConnectionString;
        private string _builderDataConnectionString;
        private string _builderModelConnectionString;
        private string _architectUserName;
        private string _serverTemplateFolder;
        private string _newPackageId;
        private string _sourcesFolder;
        private string _baseUrl;
        private bool _gitrepo;
        private string _gitusername;
        private string _gitemail;

        // Root Menu package
        private string _basePackageId = "b9ab12fe-7f7d-43f7-bedc-93747647d6e4";

        public enum DatabaseType
        {
            MsSql,
            PostgreSql
        }

        #region Properties
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

        public string DataDatabaseName
        {
            get
            {
                return _dataDatabaseName;
            }
            set
            {
                _dataDatabaseName = value;
            }
        }

        public string ModelDatabaseName
        {
            get
            {
                return _modelDatabaseName;
            }
            set
            {
                _modelDatabaseName = value;
            }
        }

        public string DatabaseServerName
        {
            get
            {
                return _databaseServerName;
            }
            set
            {
                _databaseServerName = value;
            }
        }

        public string DatabaseUserName
        {
            get
            {
                return _databaseUserName;
            }
            set
            {
                _databaseUserName = value;
            }
        }

        public string DatabasePassword
        {
            get
            {
                return _databasePassword;
            }
            set
            {
                _databasePassword = value;
            }
        }

        public bool DatabaseIntegratedAuthentication
        {
            get
            {
                return _databaseIntegratedAuthentication;
            }
            set
            {
                _databaseIntegratedAuthentication = value;
            }
        }

        public bool GitRepository
        {
            get => _gitrepo;
            set => _gitrepo = value;
        }

        public string WebRootName
        {
            get
            {
                return _webRootName;
            }
            set
            {
                _webRootName = value;
            }
        }

        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }

        public string BinFolder
        {
            get
            {
                return _binFolder;
            }
            set
            {
                _binFolder = value;
            }
        }

        public string DataConnectionString
        {
            get
            {
                return _dataConnectionString;
            }
            set
            {
                _dataConnectionString = value;
            }
        }

        public string ModelConnectionString
        {
            get
            {
                return _modelConnectionString;
            }
            set
            {
                _modelConnectionString = value;
            }
        }

        public string BuilderDataConnectionString
        {
            get
            {
                return _builderDataConnectionString;
            }
            set
            {
                _builderDataConnectionString = value;
            }
        }

        public string BuilderModelConnectionString
        {
            get
            {
                return _builderModelConnectionString;
            }
            set
            {
                _builderModelConnectionString = value;
            }
        }

        public string BasePackageId
        {
            get
            {
                return _basePackageId;
            }
            set
            {
                _basePackageId = value;
            }
        }

        public string NewPackageId
        {
            get
            {
                return _newPackageId;
            }
            set
            {
                _newPackageId = value;
            }
        }

        public string ArchitectUserName
        {
            get
            {
                return _architectUserName;
            }
            set
            {
                _architectUserName = value;
            }
        }

        public string ServerTemplateFolder
        {
            get
            {
                return _serverTemplateFolder;
            }
            set
            {
                _serverTemplateFolder = value;
            }
        }

        public string SourcesFolder
        {
            get
            {
                return _sourcesFolder;
            }
            set
            {
                _sourcesFolder = value;
            }
        }

        public string BaseUrl
        {
            get
            {
                return _baseUrl;
            }
            set
            {
                _baseUrl = value;
            }
        }

        public string FullUrl
        {
            get
            {
                return BaseUrl + "/" + Url;
            }
        }

        public string Gitusername { get => _gitusername; set => _gitusername = value; }
        public string Gitemail { get => _gitemail; set => _gitemail = value; }
        public DatabaseType DatabaseTp { get; set; }
        #endregion
    }
}
