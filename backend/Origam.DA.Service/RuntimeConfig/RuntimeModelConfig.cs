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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Formatting = Newtonsoft.Json.Formatting;

namespace Origam.DA.Service;
public class RuntimeModelConfig : IRuntimeModelConfig
{
    private readonly string pathToConfigFile;
    private readonly FileSystemWatcher watcher;
    private readonly object lockObj = new object();
    
    public event EventHandler<List<Guid>> ConfigurationReloaded;
    private List<ConfigItem> configItems;
    public RuntimeModelConfig(string pathToConfigFile)
    {
        this.pathToConfigFile = pathToConfigFile;
        
        watcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(pathToConfigFile),
            Filter = Path.GetFileName(pathToConfigFile),
            NotifyFilter = NotifyFilters.LastWrite 
        };
        watcher.Changed += OnConfigFileChanged;
        watcher.EnableRaisingEvents = true;
    }
    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        var oldIds = configItems.Select(item => item.Id).ToList();
        Thread.Sleep(100); // trying to avoid IOException (The process cannot access the file XY because it is being used by another process.)
        configItems = ParseConfigFile();
        var newIds = configItems.Select(item => item.Id);
        var invalidatedItemIds = oldIds
            .Concat(newIds)
            .Distinct()
            .ToList();
        ConfigurationReloaded?.Invoke(this, invalidatedItemIds);
    }
    private List<ConfigItem> ParseConfigFile()
    {
        lock (lockObj)
        {
            if (!File.Exists(pathToConfigFile))
            {
                return new List<ConfigItem>();
            }
            return TryParseConfigFile();
        }
    }
    private List<ConfigItem> TryParseConfigFile()
    {
        for (int i = 0; i < 5; i++)
        {
            try
            {
                string json = File.ReadAllText(pathToConfigFile);
                return string.IsNullOrWhiteSpace(json)
                    ? new List<ConfigItem>()
                    : JsonConvert.DeserializeObject<List<ConfigItem>>(json);
            }
            catch (Exception ex)
            {
                if (ex is IOException && i < 3)
                {
                    Thread.Sleep(100);
                    continue;
                }
                throw new Exception(
                    String.Format(Strings.ErrorParsingConfig,
                        pathToConfigFile), ex);
            }
        }
        throw new NotImplementedException(); // this code should be never reached
    }
    public void SetConfigurationValues(IFilePersistent instance)
    {
        if (configItems == null)
        {
            configItems = ParseConfigFile();
        }
        var configItemsForTheInstance = configItems
            .Where(item => item.Id == instance.Id)
            .ToList();
        
        if (configItemsForTheInstance.Count == 0)
        {
            return;
        }
        
        foreach (var configItem in configItemsForTheInstance)
        {
            SetConfigurationValues(instance, configItem);
        }
    }
    private void SetConfigurationValues(IFilePersistent instance,
        ConfigItem configItem)
    {
        var xmlMemberAttributeInfo = Reflector
            .FindMembers(instance.GetType(), typeof(XmlAttributeAttribute))
            .Cast<MemberAttributeInfo>()
            .FirstOrDefault(memberInfo =>
                (memberInfo.Attribute as XmlAttributeAttribute)?.AttributeName ==
                configItem.PropertyName);
        var configMemberAttributeInfo = Reflector
            .FindMembers(instance.GetType(), typeof(RuntimeConfigurableAttribute))
            .Cast<MemberAttributeInfo>()
            .FirstOrDefault(memberInfo =>
                (memberInfo.Attribute as RuntimeConfigurableAttribute)?.Name ==
                configItem.PropertyName);
        var memberAttributeInfo =
            configMemberAttributeInfo ?? xmlMemberAttributeInfo;
        if (memberAttributeInfo == null)
        {
            throw new Exception(string.Format(Strings.ConfiguredPropertyNotFound,
                instance.Id, configItem.PropertyName, pathToConfigFile));
        }
#if !ORIGAM_CLIENT
            if (xmlMemberAttributeInfo != null)
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
            throw new Exception(String.Format(
                Strings.ErrorWhenSettingConfigProperty,
                configItem.PropertyValue, configItem.PropertyName,
                instance.Id, pathToConfigFile), ex);
        }
    }
    public void UpdateConfig(IPersistent instance)
    {
        lock (lockObj)
        {
            try
            {
                UpdateConfigInternal(instance);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(
                    Origam.Strings.CantSaveConfig, instance.Id,
                    pathToConfigFile), ex);
            }
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
                Equals(defaultValueAttribute.Value, value) || 
                instance.IsDeleted)
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
    public void Dispose()
    {
        watcher.Changed -= OnConfigFileChanged;
        watcher?.Dispose();
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
