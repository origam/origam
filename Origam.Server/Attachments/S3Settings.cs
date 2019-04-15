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

using System.Configuration;
using System.Web.Configuration;

namespace Origam.Server
{
    public class S3Settings : ConfigurationSection
    {
        private static S3Settings settings 
            = WebConfigurationManager.GetSection("S3Settings") as S3Settings;

        public static S3Settings Settings
        {
            get
            {
                return settings;
            }
        }

        [ConfigurationProperty("AWSAccessKey", IsRequired = true)]
        public string AWSAccessKey
        {
            get
            {
                return (string)this["AWSAccessKey"];
            }
            set
            {
                this["AWSAccessKey"] = value;
            }
        }

        [ConfigurationProperty("AWSSecretKey", IsRequired = true)]
        public string AWSSecretKey
        {
            get
            {
                return (string)this["AWSSecretKey"];
            }
            set
            {
                this["AWSSecretKey"] = value;
            }
        }

        [ConfigurationProperty("BucketName", IsRequired = true)]
        public string BucketName
        {
            get
            {
                return (string)this["BucketName"];
            }
            set
            {
                this["BucketName"] = value;
            }
        }
        /**
         * in ms
         */
        [ConfigurationProperty("Timeout", IsRequired = true)]
        public int Timeout
        {
            get
            {
                return (int)this["Timeout"];
            }
            set
            {
                this["Timeout"] = value;
            }
        }
        /**
         * in ms
         */
        [ConfigurationProperty("UrlExpiration", IsRequired = true)]
        public int UrlExpiration
        {
            get
            {
                return (int)this["UrlExpiration"];
            }
            set
            {
                this["UrlExpiration"] = value;
            }
        }
    }
}
