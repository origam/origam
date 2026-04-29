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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for TransformationReference.
/// </summary>
[SchemaItemDescription(name: "Lookup Reference", iconName: "icon_lookup-reference.png")]
[HelpTopic(topic: "Lookup+Reference")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class EntityFilterLookupReference : AbstractSchemaItem
{
    public const string CategoryConst = "EntityFilterLookupReference";

    public EntityFilterLookupReference()
        : base()
    {
        Init();
    }

    public EntityFilterLookupReference(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public EntityFilterLookupReference(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.AddRange(
            collection: new Type[]
            {
                typeof(ParameterReference),
                typeof(EntityColumnReference),
                typeof(FunctionCall),
                typeof(DataConstantReference),
            }
        );
    }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.Lookup != null)
        {
            base.GetParameterReferences(parentItem: Lookup, list: list);
        }

        base.GetParameterReferences(parentItem: this, list: list);
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Lookup);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override IList<string> NewTypeNames
    {
        get
        {
            try
            {
                IBusinessServicesService agents =
                    ServiceManager.Services.GetService(
                        serviceType: typeof(IBusinessServicesService)
                    ) as IBusinessServicesService;
                IServiceAgent agent = agents.GetAgent(
                    serviceType: "DataService",
                    ruleEngine: null,
                    workflowEngine: null
                );
                return agent.ExpectedParameterNames(
                    item: this,
                    method: "LoadData",
                    parameter: "Parameters"
                );
            }
            catch
            {
                return new string[] { };
            }
        }
    }
    #endregion
    #region Properties
    public Guid LookupId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(DataLookupConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "lookup", idField: "LookupId")]
    public IDataLookup Lookup
    {
        get
        {
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: new ModelElementKey(id: this.LookupId)
                    ) as IDataLookup;
        }
        set
        {
            this.LookupId = (Guid)value.PrimaryKey[key: "Id"];
            this.Name = this.Lookup.Name;
        }
    }
    #endregion
}
