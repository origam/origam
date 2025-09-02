using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

namespace Origam.DA.Service.MetaModelUpgrade;

public class ScriptContainerLocator
{
    private readonly Assembly scriptAssembly;
    private List<UpgradeScriptContainer> scriptContainers;

    public ScriptContainerLocator(Assembly scriptAssembly)
    {
        this.scriptAssembly = scriptAssembly;
    }

    internal UpgradeScriptContainer TryFindByTypeName(string className)
    {
        if (scriptContainers == null)
        {
            InitializeContainerList();
        }
        var containers = scriptContainers
            .Where(container =>
                container.FullTypeName == className
                || (
                    container.OldFullTypeNames != null
                    && container.OldFullTypeNames.Contains(className)
                )
            )
            .ToArray();
        if (containers.Length == 1)
        {
            return containers[0];
        }
        if (containers.Length == 0)
        {
            return null;
        }
        throw new ClassUpgradeException(
            $"More than one ancestor of {typeof(UpgradeScriptContainer).Name} which upgrades type of \"{className}\" was found"
        );
    }

    internal UpgradeScriptContainer FindByTypeName(string className)
    {
        var scriptContainer = TryFindByTypeName(className);
        return scriptContainer
            ?? throw new ClassUpgradeException(
                $"Could not find ancestor of {typeof(UpgradeScriptContainer).Name} which upgrades type of \"{className}\""
            );
    }

    private void InitializeContainerList()
    {
        scriptContainers = scriptAssembly
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(UpgradeScriptContainer)))
            .Select(Activator.CreateInstance)
            .Cast<UpgradeScriptContainer>()
            .ToList();
    }
}
