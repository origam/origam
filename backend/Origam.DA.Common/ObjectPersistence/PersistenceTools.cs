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
using System.Xml.Serialization;

namespace Origam.DA.ObjectPersistence;

//    public class PersistenceTools
//    {
//        public static XmlRootAttribute RootAttribute(Type type)
//        {
//            object[] attributes = type.GetCustomAttributes(typeof(XmlRootAttribute), true);
//
//            if (attributes != null && attributes.Length > 0)
//                return attributes[0] as XmlRootAttribute;
//            else
//                return null;
//        }
//
//        public static string FullElementName(Type type)
//        {
//            return FullElementName(RootAttribute(type));
//        }
//
//        public static string FullElementName(XmlRootAttribute rootAtt)
//        {
//            if (rootAtt == null)
//            {
//                return null;
//            }
//            return rootAtt.Namespace + rootAtt.string;
//        }
//
//        public static string FullElementName(string nameSpace, string category)
//        {
//            return nameSpace + category;
//        }
//    }
