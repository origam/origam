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
using System.Reflection;
using System.Xml.Linq;
using Origam.DA.Common;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public interface IMetaModelUpgrader
    {
        bool RunUpgradeScripts(XElement classNode,
            DocumentContainer documentContainer, string className,
            Version persistedClassVersion,
            Version currentClassVersion);

        void UpgradeToVersion6(OrigamXDocument document);
    }
    
    public class MetaModelUpgrader: IMetaModelUpgrader
    {
        private readonly ScriptContainerLocator scriptLocator;

        public MetaModelUpgrader()
        {
            scriptLocator = new ScriptContainerLocator(GetType().Assembly);
        }        
        public MetaModelUpgrader(Assembly scriptAssembly)
        {
            scriptLocator = new ScriptContainerLocator(scriptAssembly);
        }

        public bool RunUpgradeScripts(XElement classNode,
            DocumentContainer documentContainer, string className,
            Version persistedClassVersion,
            Version currentClassVersion)
        {
            var upgradeScriptContainer = scriptLocator.TryFindByTypeName(className);
            if (upgradeScriptContainer == null)
            {
                if (currentClassVersion == Versions.Last)
                {
                    throw new ClassUpgradeException($" No {nameof(ClassMetaVersionAttribute)} was found on \"{className}\" and no upgrade scripts for that class were found either. May be you meant to add the attribute?");
                }
                throw new ClassUpgradeException($"Could not find ancestor of {typeof(UpgradeScriptContainer).Name} which upgrades type of \"{className}\"");
            }
            
            return upgradeScriptContainer.Upgrade(documentContainer, classNode, persistedClassVersion, currentClassVersion);
        }

        public void UpgradeToVersion6(OrigamXDocument document)
        {
            new Version6Upgrader(scriptLocator, document).Run();
        }
    }
    
    public class DisabledMetaModelUpgrader: IMetaModelUpgrader
    {
        public bool RunUpgradeScripts(XElement classNode,
            DocumentContainer documentContainer, string className,
            Version persistedClassVersion,
            Version currentClassVersion)
        {
            throw new Exception($"An instance of class {className} with version: {persistedClassVersion} was found. This class version is outdated, the current version is {currentClassVersion}. Please open the model in Architect to solve the issue.");
        }

        public void UpgradeToVersion6(OrigamXDocument document)
        {
            throw new Exception($"File contains pre version 6 classes. Please open the model in Architect to solve the issue.");
        }
    }
}