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
using System.Linq;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.ItemCollection;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Schema.RuleModel;
using Origam.Schema.WorkflowModel;
using Origam.Schema.WorkflowModel.WorkQueue;
using Origam.Workbench.Services;

namespace Origam.DA.Service_net2Tests;
internal static class TypeTools
{
    public static IEnumerable<Type> AllProviderTypes =>
        ((SchemaService) ServiceManager.Services
            .GetService(typeof(SchemaService)))
        .Providers
        .Select(provider => provider.GetType());
    public static ISchemaItemCollection GetAllItems(Type providerType)
    {
        SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService))
            as SchemaService;
        ISchemaItemProvider provider = schema.GetProvider(providerType);
        return provider.ChildItems;
    }
}
