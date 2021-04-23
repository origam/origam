#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Newtonsoft.Json;
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
                return JsonConvert.DeserializeObject<List<ConfigItem>>(json);
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
                object value = Reflector.GetValue(memberAttrInfo.MemberInfo, instance);
                var defaultValueAttribute =
                    memberAttrInfo.MemberInfo.GetCustomAttributes()
                            .FirstOrDefault(attribute => attribute is DefaultValueAttribute) 
                        as DefaultValueAttribute;
                
                if (defaultValueAttribute != null &&
                    Equals(defaultValueAttribute.Value, value))
                {
                    RemoveConfigItem(instance.Id);
                }
                else
                {
                    SetConfigItemValues(instance, name, value);
                }
            }

            string json = JsonConvert.SerializeObject(configItems, Formatting.Indented);
            File.WriteAllText(pathToConfigFile, json);
        }

        private void RemoveConfigItem(Guid id)
        { 
            configItems.RemoveAll(configItem => configItem.Id == id);
        }

        private void SetConfigItemValues(IPersistent instance, string name, object value)
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
            configItem.PropertyValue = XmlTools.ConvertToString(value);
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
        [JsonProperty(Required = Required.Always)]
        public Guid Id { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string PropertyName { get; set; }
        [JsonProperty(Required = Required.Always)]
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