#region license
/*
Copyright 2005 - 2017 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

namespace Origam.Server.Architect
{
    public class ModelBrowserElement
    {
        private string _id = "";
        private string _name = "";
        private string _type = "";
        private string _icon = "";
        private bool _hasChildren = false;
        private bool _isEditable = false;

        public ModelBrowserElement(string id, string type, string name, string icon, bool hasChildren, bool isEditable)
        {
            this.Id = id;
            this.Type = type;
            this.Name = name;
            this.Icon = icon;
            this.HasChildren = hasChildren;
            this.IsEditable = isEditable;
        }

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

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

        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public string Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
            }
        }

        public bool HasChildren
        {
            get
            {
                return _hasChildren;
            }
            set
            {
                _hasChildren = value;
            }
        }

        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }
            set
            {
                _isEditable = value;
            }
        }
    }
}
