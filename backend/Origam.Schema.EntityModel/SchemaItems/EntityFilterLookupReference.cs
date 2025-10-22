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
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for TransformationReference.
/// </summary>
[SchemaItemDescription("Lookup Reference", "icon_lookup-reference.png")]
[HelpTopic("Lookup+Reference")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class EntityFilterLookupReference : AbstractSchemaItem
{
    public const string CategoryConst = "EntityFilterLookupReference";

    public EntityFilterLookupReference()
        : base()
    {
        Init();
    }

    public EntityFilterLookupReference(Guid schemaExtensionId)
        : base(schemaExtensionId)
    {
        Init();
    }

    public EntityFilterLookupReference(Key primaryKey)
        : base(primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.AddRange(
            new Type[]
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
            base.GetParameterReferences(Lookup, list);
        }

        base.GetParameterReferences(this, list);
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(this.Lookup);
        base.GetExtraDependencies(dependencies);
    }

    public override IList<string> NewTypeNames
    {
        get
        {
            try
            {
                IBusinessServicesService agents =
                    ServiceManager.Services.GetService(typeof(IBusinessServicesService))
                    as IBusinessServicesService;
                IServiceAgent agent = agents.GetAgent("DataService", null, null);
                return agent.ExpectedParameterNames(this, "LoadData", "Parameters");
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

    [Category("Reference")]
    [TypeConverter(typeof(DataLookupConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [XmlReference("lookup", "LookupId")]
    public IDataLookup Lookup
    {
        get
        {
            return (ISchemaItem)
                    this.PersistenceProvider.RetrieveInstance(
                        typeof(ISchemaItem),
                        new ModelElementKey(this.LookupId)
                    ) as IDataLookup;
        }
        set
        {
            this.LookupId = (Guid)value.PrimaryKey["Id"];
            this.Name = this.Lookup.Name;
        }
    }
    #endregion
}
