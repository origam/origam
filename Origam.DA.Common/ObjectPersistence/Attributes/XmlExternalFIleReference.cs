using System;
using System.IO;

namespace Origam.DA.ObjectPersistence
{
    
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class XmlExternalFileReference: Attribute
    {
        public ExternalFileExtension Extension { get; }
        public string ContainerName { get; }
        public string Namespace { get; }

        public XmlExternalFileReference( string containerName,
            ExternalFileExtension extension = ExternalFileExtension.Xml)
        {
            Extension = extension;
            ContainerName = containerName;
        }
    }
    
    /// <summary>
    /// Edit IsSearchable if you want your newly defined ExternalFileExtension to be
    /// marked as searchable.
    /// </summary>
    public enum ExternalFileExtension
    {
        Xml,
        Png,
        Xslt,
        Bin,
        Txt
    }

    public static class ExternalFileExtensionTools
    {
        public static bool IsSearchable(this ExternalFileExtension extension)
        {
            switch (extension)
            {
                case ExternalFileExtension.Xml:
                    return true;
                case ExternalFileExtension.Png:
                    return false;
                case ExternalFileExtension.Xslt:
                    return true;
                case ExternalFileExtension.Bin:
                    return false;
                case ExternalFileExtension.Txt:
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryParse(FileInfo fileinfo, out ExternalFileExtension value) => 
            TryParsePrivate(fileinfo.Extension, out value);

        public static bool TryParse(string filePath, out ExternalFileExtension value)
        {
            string extension = Path.GetExtension(filePath);
            return TryParsePrivate(extension, out value);
        }

        private static bool TryParsePrivate(string extension, out ExternalFileExtension ext)
        {
            if (extension == "")
            {
                ext = ExternalFileExtension.Bin;
                return false;
            }
            string extensionWithoutDot = extension.Substring(1);
            bool parseSucess = Enum.TryParse(
                value: extensionWithoutDot,
                ignoreCase: true, 
                result: out ExternalFileExtension parsedExt);
            ext = parsedExt;
            return parseSucess;
        }
    }  
}