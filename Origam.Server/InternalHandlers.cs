#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

using System;
using System.Configuration;
using System.Web.Configuration;
using System.Reflection;

namespace Origam.Server
{
    public class InternalHandlers : ConfigurationSection
    {
        private static InternalHandlers settings
           = WebConfigurationManager.GetSection("InternalHandlers") as InternalHandlers;

        [ConfigurationProperty("AttachmentHandler", DefaultValue="Origam.Server.AttachmentSQLDbHandler")]
        public string AttachmentHandler
        {
            get
            {
                return (string)this["AttachmentHandler"];
            }
            set
            {
                this["AttachmentHandler"] = value;
            }
        }

        public static IAttachmentHandler GetAttachmentHandler()
        {
            string className;
            if (settings != null)
            {
                 className = settings.AttachmentHandler;
            }
            else
            {
                className = "Origam.Server.AttachmentSQLDbHandler";
            }
            Type handlerType = Type.GetType(className);
            if (handlerType == null)
            {
                throw new ConfigurationException(String.Format("Can't initialize attachment handler. Class `{0}' not found.", className));
            }
            PropertyInfo instance = handlerType.GetProperty("Instance");
            return instance.GetValue(null, null) as IAttachmentHandler;
        }

    }
}
