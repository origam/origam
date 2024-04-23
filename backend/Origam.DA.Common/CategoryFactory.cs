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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.Extensions;
using Origam.OrigamEngine;
using ProtoBuf;

namespace Origam.DA;

public static class CategoryFactory
{
    public static string Create(Type type)
    {
            XmlRootAttribute rootAttribute = FindRootAttribute(type);
            return rootAttribute?.ElementName;
        }

    private static XmlRootAttribute FindRootAttribute(Type type)
    {
            object[] attributes = type.GetCustomAttributes(typeof(XmlRootAttribute), true);
        
            if (attributes != null && attributes.Length > 0)
                return (XmlRootAttribute) attributes[0];
            else
                return null;
        }
}