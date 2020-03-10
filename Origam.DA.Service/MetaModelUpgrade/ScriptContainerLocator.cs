using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public class ScriptContainerLocator
    {
        private readonly Assembly scriptAssembly;
        private List<UpgradeScriptContainer> scriptContainers;

        public ScriptContainerLocator(Assembly scriptAssembly)
        {
            this.scriptAssembly = scriptAssembly;
        }
        
        internal UpgradeScriptContainer FindByTypeName(string className)
        {
            if (scriptContainers == null)
            {
                InitializeContainerList();
            }

            var containers = scriptContainers
                .Where(container =>
                    container.FullTypeName == className ||
                    container.OldFullTypeNames != null &&
                    container.OldFullTypeNames.Contains(className))
                .ToArray();
            if (containers.Length != 1)
            {
                throw new ClassUpgradeException(
                    $"Could not find exactly one ancestor of {typeof(UpgradeScriptContainer).Name} which upgrades type of \"{className}\"");
            }
            return containers[0];
        }
        
        private void InitializeContainerList()
        {
            scriptContainers = scriptAssembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(UpgradeScriptContainer)))
                .Select(Activator.CreateInstance)
                .Cast<UpgradeScriptContainer>()
                .ToList();
        }
    }
}