using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Origam.Extensions;

namespace Origam
{
    public class OrigamSettingsReader
    {
        private static string DefaultPathToOrigamSettings
        {
            get
            {
                string directoryPath = Path.GetDirectoryName(AssemblyTools.GetAssemblyLocation());
                return Path.Combine(directoryPath, "OrigamSettings.config");
            }
        }

        private readonly string pathToOrigamSettings;

        public OrigamSettingsReader(string pathToOrigamSettings = null)
        {

            this.pathToOrigamSettings = pathToOrigamSettings 
                                        ?? DefaultPathToOrigamSettings;
        }

        public OrigamSettingsCollection GetAll()
        {
            XmlReader reader=null;
            try
            {
                XmlNode arrayOfOrigamSettingsNode =
                    GetSettingsNode();
                if (arrayOfOrigamSettingsNode.ChildNodes.Count == 0)
                {
                    return new OrigamSettingsCollection();
                }

                reader = new XmlNodeReader(arrayOfOrigamSettingsNode);
                XmlSerializer deserializer =
                    new XmlSerializer(typeof(OrigamSettings[]));
                OrigamSettings[] movies =
                    (OrigamSettings[]) deserializer.Deserialize(reader);

                return new OrigamSettingsCollection(movies);
            }
            catch (Exception ex)
            {
                throw new OrigamSettingsException(ex.Message, ex);
            }
            finally
            {
                reader?.Dispose();
            }
        }

        private XmlNode GetSettingsNode()
        {
            return GetNodeByPath("OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings");
        }

        private XmlNode GetNodeByPath(string pathToNode)
        {
            XmlDocument document = new XmlDocument();
            document.Load(pathToOrigamSettings);

            var arrayOfOrigamSettingsNode = document.SelectSingleNode(
                pathToNode);
            if (arrayOfOrigamSettingsNode == null)
            {
                throw new OrigamSettingsException(string.Format(Strings.PathNotFound, pathToNode));
            }

            return arrayOfOrigamSettingsNode;
        }

        public static XmlDocument CreateEmptyDocument()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);
            
            XmlElement orSettings = doc.CreateElement("OrigamSettings");
            XmlElement serialization = doc.CreateElement("xmlSerializerSection");
            XmlAttribute typeAttr = doc.CreateAttribute("type");
            typeAttr.Value = "Origam.OrigamSettingsCollection, Origam, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            serialization.Attributes.Append(typeAttr);
            XmlElement settingsArray = doc.CreateElement("ArrayOfOrigamSettings");
            settingsArray.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            settingsArray.SetAttribute("xmlns:xsd", "http://www.w3.org/2001/XMLSchema");
            orSettings.AppendChild(serialization);
            serialization.AppendChild(settingsArray);
            
            XmlElement security = doc.CreateElement("Security");
            orSettings.AppendChild(security);
            doc.AppendChild(orSettings);
            return doc;
        }
	
        public void Write(OrigamSettingsCollection configuration)
        {
            XmlDocument document = CreateEmptyDocument();
            XmlNode arrayOfOrigamSettingsNode = 
                document.SelectSingleNode("OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings");
            var xmlDocument = new XmlDocument();
            var nav = xmlDocument.CreateNavigator();
            using (var writer = nav.AppendChild())
            {
                var ser = new XmlSerializer(typeof(OrigamSettings[]));
                ser.Serialize(writer, configuration.ToList<OrigamSettings>().ToArray());
            }
            arrayOfOrigamSettingsNode.InnerXml = xmlDocument.InnerXml;
            document.Save(pathToOrigamSettings);
        }
    }
}