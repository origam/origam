using System;
using System.Xml.Serialization;

namespace Origam.DA.ObjectPersistence
{
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
//            return rootAtt.Namespace + rootAtt.ElementName;
//        }
//
//        public static string FullElementName(string nameSpace, string elementName)
//        {
//            return nameSpace + elementName;
//        }
//    }
}
