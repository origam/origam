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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema;

/// <summary>
/// Summary description for SchemaItem.
/// </summary>
public interface ISchemaItem : IPersistent, ISchemaItemProvider, ICloneable
{
    /// <summary>
    /// Gets or sets the schema extensions, under which this item was defined.
    /// </summary>
    Package Package { get; set; }
    ModelElementKey OldPrimaryKey { get; set; }
    string Name { get; set; }
    SchemaItemAncestorCollection AllAncestors { get; }
    List<SchemaItemParameter> Parameters { get; }
    Dictionary<string, ParameterReference> ParameterReferences { get; }
    bool HasParameterReferences { get; }
    string ItemType { get; }
    ISchemaItem GetChildByName(string name);
    ISchemaItem GetChildById(Guid id);
    List<ISchemaItem> ChildItemsByTypeRecursive(string itemType);

    /// <summary>
    /// Gets or sets the parent schema item.
    /// </summary>
    ISchemaItem ParentItem { get; set; }

    /// <summary>
    /// Gets the root schema item.
    /// </summary>
    ISchemaItem RootItem { get; }

    /// <summary>
    /// Gets or sets the group where this item is located.
    /// </summary>
    SchemaItemGroup Group { get; set; }
    IEnumerable<ISchemaItem> Parents { get; }

    /// <summary>
    /// Gets or sets the item from which this item has been derived
    /// </summary>
    ISchemaItem DerivedFrom { get; set; }
    bool IsAbstract { get; set; }
    bool IsPersistable { get; set; }
    List<ISchemaItem> GetDependencies(bool ignoreErrors);
    void UpdateReferences();
    object Clone(bool keepKeys);
    void SetExtensionRecursive(Package extension);
    string ModelDescription();

    public void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    );
    bool DeleteChildItems { get; set; }
    bool ClearCacheOnPersist { get; set; }
    bool ThrowEventOnPersist { get; set; }
    bool Inheritable { get; set; }
    bool NeverRetrieveChildren { get; set; }
    string Path { get; }
    bool UseFolders { get; }
    IEnumerable<ISchemaItem> ChildrenRecursive { get; }
    T FirstParentOfType<T>()
        where T : class;
    string PackageName { get; }
    string RelativeFilePath { get; }
    Guid SchemaExtensionId { get; set; }
    Guid GroupId { get; set; }
    Guid ParentItemId { get; set; }
    SchemaItemAncestorCollection Ancestors { get; }
    void InvalidateChildrenPersistenceCache();
    ISchemaItem GetChildByIdRecursive(Guid id);
    void Dump();
    List<ISchemaItem> GetUsage();
}
