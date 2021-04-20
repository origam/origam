using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using Origam.DA.ObjectPersistence;

namespace Origam.DA.Service
{
    public class RuntimeModelConfig : IRuntimeModelConfig
    {
        private readonly string pathToConfigFile;

        private Dictionary<Guid, ConfigItem> configItems;

        public RuntimeModelConfig(string pathToConfigFile)
        {
            this.pathToConfigFile = pathToConfigFile;
            ParseConfigFile();
        }

        private void ParseConfigFile()
        {
            if (!File.Exists(pathToConfigFile))
            {
                throw new Exception($"Config file does not exist: {pathToConfigFile}");
            }
            try
            {
                string json = File.ReadAllText(pathToConfigFile);
                configItems = new ConfigItemConverter()
                    .ReadToEnumerable(json)
                    .ToDictionary(
                        configItem => configItem.OrigamObjectId,
                        configItem => configItem);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when parsing runtime config file: {pathToConfigFile}", ex);
            }
        }

        public void SetConfigurationValues(IFilePersistent instance)
        {
            if (!configItems.ContainsKey(instance.Id))
            {
                return;
            }

            ConfigItem configItem = configItems[instance.Id];
            
            var xmlMemberAttributeInfo = Reflector
                .FindMembers(instance.GetType(), typeof(XmlAttributeAttribute))
                .Cast<MemberAttributeInfo>()
                .FirstOrDefault(memberInfo => 
                    (memberInfo.Attribute as XmlAttributeAttribute)?.AttributeName == configItem.PropertyName);
#if !ORIGAM_CLIENT
            if (xmlMemberAttributeInfo != null)
            {
                return;
            }
#endif
            var runtimeConfigMemberAttributeInfo = Reflector
                .FindMembers(instance.GetType(), typeof(RuntimeConfigurableAttribute))
                .Cast<MemberAttributeInfo>()
                .FirstOrDefault(memberInfo => 
                    (memberInfo.Attribute as RuntimeConfigurableAttribute)?.Name == configItem.PropertyName);

            var memberAttributeInfo = runtimeConfigMemberAttributeInfo ?? xmlMemberAttributeInfo;
            
            if (memberAttributeInfo == null)
            {
                throw new Exception(
                    $"$Error processing runtime configuration. Object with" +
                    $" id \"{instance.Id}\" does not have the attribute named" +
                    $" \"{configItem.PropertyName}\" so it's value cannot be set " +
                    $"as requested in the runtime configuration file: \"{pathToConfigFile}\"");
            }

            try
            {
                object value = InstanceTools.GetCorrectlyTypedValue(
                    memberAttributeInfo.MemberInfo, configItem.PropertyValue);
                Reflector.SetValue(memberAttributeInfo.MemberInfo, instance,
                    value);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"An error occured when trying to set runtime config " +
                    $"value \"{configItem.PropertyValue}\" of property " +
                    $"\"{configItem.PropertyName}\" on object id: \"{instance.Id}\"." +
                    $" Configuration file: \"{pathToConfigFile}\"\n", ex);
            }
        }
    }

    class ConfigItemConverter
    {
        public IEnumerable<ConfigItem> ReadToEnumerable(string json)
        {
            JObject jsonConfig = JObject.Parse(json);

            JToken firstConfigNode = jsonConfig["RuntimeConfig"];
            if (!(firstConfigNode is JArray configArray))
            {
                throw new Exception(
                    "The first node in the runtimeModelConfig has to be an array of config nodes named \"runtimeConfig\"");
            }

            return configArray
                .Children<JToken>()
                .Select(ParseConfigNode);
        }
        private ConfigItem ParseConfigNode(JToken configNode)
        {
            string strId = configNode["Id"]?.ToString();
            if (!Guid.TryParse(strId, out Guid id))
            {
                throw new ArgumentException($"Cannot parse id value \"{strId}\" to Guid");
            }

            var configurationProperties = configNode
                .OfType<JProperty>()
                .Where(prop => prop.Name != "Id" && prop.Name != "Description")
                .ToList();
            if (configurationProperties.Count != 1)
            {
                throw new ArgumentException(
                    "Every configuration node must contain a property" +
                    " called \"id\", a property named the same as the origam " +
                    "object's property to be changed and can contain a " +
                    "\"description\" property. The following node fails the " +
                    "described criteria: \n " + configNode);
            }

            return new ConfigItem( 
                origamObjectId: id,
                propertyName: configurationProperties[0].Name,
                propertyValue: configurationProperties[0].Value.ToString());
        }
    }
    

    class ConfigItem
    {
        public Guid OrigamObjectId { get; }
        public string PropertyName { get; }
        public string PropertyValue { get; }

        public ConfigItem(Guid origamObjectId, string propertyName, string propertyValue)
        {
            OrigamObjectId = origamObjectId;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }
    }
}