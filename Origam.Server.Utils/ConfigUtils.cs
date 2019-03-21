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
﻿using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;

public class ConfigUtils
{
    public static string GetValue(string section, string key)
    {
        NameValueCollection sectionData = (NameValueCollection)
            System.Configuration.ConfigurationManager.GetSection(section);
        if (sectionData == null)
        {
            throw (new ArgumentOutOfRangeException("Section Name not found: " + section));
        }

        string value = sectionData.Get(key);

        return value;
    }

    public static NameValueCollection GetCollection(string section)
    {
        NameValueCollection sectionData = (NameValueCollection)
            System.Configuration.ConfigurationManager.GetSection(section);
        if (sectionData == null)
        {
            throw (new ArgumentOutOfRangeException("Section Name not found: " + section));
        }
        return sectionData;
    }

    public static string AboutUrl()
    {
        return ConfigurationManager.AppSettings["aboutUrl"] ?? "https://www.origam.com";
    }

    public static string BottomLogoUrl()
    {
        return ConfigurationManager.AppSettings["bottomLogoUrl"] ?? "https://www.origam.com";
    }

    public static string SignOutUrl()
    {
        return ConfigurationManager.AppSettings["signOutUrl"] ?? "~/";
    }

    public static bool UserRegistrationAllowed()
    {
        string allowed = ConfigurationManager.AppSettings["userRegistration_Allowed"];
        if (string.IsNullOrEmpty(allowed))
        {
            return false;
        }
        else
        {
            return XmlConvert.ToBoolean(allowed);
        }
    }
}
