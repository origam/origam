using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema;

namespace Origam.DA.Service
{
    public class RuntimeModelConfig : IRuntimeModelConfig
    {
        private readonly string pathToConfigFile;

        private List<ConfigItem> configItems;

        public RuntimeModelConfig(string pathToConfigFile)
        {
            this.pathToConfigFile = pathToConfigFile;
        }

        private List<ConfigItem> ParseConfigFile()
        {
            if (!File.Exists(pathToConfigFile))
            {
                return new List<ConfigItem>();
            }
            try
            {
                string json = File.ReadAllText(pathToConfigFile);
                if (string.IsNullOrWhiteSpace(json))
                {
                    json = "[]";
                }
                return JsonSerializer.Deserialize<List<ConfigItem>>(json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when parsing runtime config file: \"{pathToConfigFile}\"", ex);
            }
        }

        public void SetConfigurationValues(IFilePersistent instance)
        {
            if (configItems == null)
            {
                configItems = ParseConfigFile();
            }

            ConfigItem configItem = configItems
                .FirstOrDefault(item => item.Id == instance.Id);
            
            if (configItem == null)
            {
                return;
            }
            
            var memberAttributeInfo = Reflector
                .FindMembers(instance.GetType(), typeof(RuntimeConfigurableAttribute))
                .Cast<MemberAttributeInfo>()
                .FirstOrDefault(memberInfo => 
                    (memberInfo.Attribute as RuntimeConfigurableAttribute)?.Name == configItem.PropertyName);
            
            if (memberAttributeInfo == null)
            {
                throw new Exception(
                    $"$Error processing runtime configuration. Object with" +
                    $" id \"{instance.Id}\" does not have the attribute named" +
                    $" \"{configItem.PropertyName}\" so it's value cannot be set " +
                    $"as requested in the runtime configuration file: \"{pathToConfigFile}\"");
            }
            
#if !ORIGAM_CLIENT
            if (PropertyHasXmlAttribute(memberAttributeInfo.MemberInfo))
            {
                return;
            }
#endif

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

        private bool PropertyHasXmlAttribute(MemberInfo memberInfo)
        {
            Attribute xmlAttribute = memberInfo
                .GetCustomAttributes()
                .FirstOrDefault(attribute =>
                    attribute is XmlAttributeAttribute ||
                    attribute is XmlReferenceAttribute);
            return xmlAttribute != null;
        }

        public void UpdateConfig(IPersistent instance)
        {
            try
            {
                UpdateConfigInternal(instance);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"An error occured when trying to save values from " +
                    $"object id: \"{instance.Id}\" to configuration file: \"{pathToConfigFile}\"\n", ex);
            }
        }

        private void UpdateConfigInternal(IPersistent instance)
        {
            var memberAttrInfos = Reflector
                .FindMembers(instance.GetType(),
                    typeof(RuntimeConfigurableAttribute));
            if (memberAttrInfos.Count == 0)
            {
                return;
            }

            foreach (MemberAttributeInfo memberAttrInfo in memberAttrInfos)
            {
                string name =
                    (memberAttrInfo.Attribute as RuntimeConfigurableAttribute).Name;
                string value = Reflector
                    .GetValue(memberAttrInfo.MemberInfo, instance)
                    .ToString();
                var defaultValueAttribute =
                    memberAttrInfo.MemberInfo.GetCustomAttributes()
                            .FirstOrDefault(attribute => attribute is DefaultValueAttribute) 
                        as DefaultValueAttribute;
                
                if (defaultValueAttribute != null &&
                    defaultValueAttribute.Value.ToString() == value)
                {
                    RemoveConfigItem(instance.Id);
                }
                else
                {
                    SetConfigItemValues(instance, name, value);
                }
            }

            string json = JsonSerializer.Serialize(
                configItems,
                new JsonSerializerOptions{WriteIndented = true});
            File.WriteAllText(pathToConfigFile, json);
        }

        private void RemoveConfigItem(Guid id)
        { 
            configItems.RemoveAll(configItem => configItem.Id == id);
        }

        private void SetConfigItemValues(IPersistent instance, string name, string value)
        {
            ConfigItem configItem = configItems
                .FirstOrDefault(item => item.Id == instance.Id);
            bool itemFound = true;
            if (configItem == null)
            {
                itemFound = false;
                configItem = new ConfigItem(instance.Id, "");
            }
            configItem.PropertyName = name;
            configItem.PropertyValue = value;
            if (string.IsNullOrEmpty(configItem.Description))
            {
                configItem.Description = (instance as ISchemaItem)?.Name ?? "";
            }
            if (!itemFound)
            {
                configItems.Add(configItem);
            }
        }
    }
    
    class ConfigItem
    {
        public Guid Id { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public string Description { get; set; } = "";

        public ConfigItem()
        {
        }

        public ConfigItem(Guid id, string propertyName)
        {
            Id = id;
            PropertyName = propertyName;
        }

        public ConfigItem(Guid id, string propertyName, string propertyValue)
        {
            Id = id;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }
    }
}