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
using Origam.DA;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.UI;

namespace Origam.Schema;

/// <summary>
/// Summary description for Schema.
/// </summary>
[XmlPackageRoot("packageReference")]
[ClassMetaVersion("6.0.0")]
public class PackageReference : AbstractPersistent, IBrowserNode2, IComparable, IFilePersistent
{
    public PackageReference()
    {
        this.PrimaryKey = new ModelElementKey();
    }

    public PackageReference(Key primaryKey)
        : base(primaryKey, new ModelElementKey().KeyArray) { }

    public override string ToString()
    {
        return this.Package.Name + " -> " + this.ReferencedPackage.Name;
    }

    #region Properties

    public bool IsFileRootElement => FileParentId == Guid.Empty;

    public Guid PackageId;
    public Package Package
    {
        get
        {
            return (Package)
                this.PersistenceProvider.RetrieveInstance(
                    typeof(Package),
                    new ModelElementKey(this.PackageId)
                );
        }
        set { this.PackageId = (Guid)value.PrimaryKey["Id"]; }
    }
    public Guid ReferencedPackageId;

    [XmlPackageReference("referencedPackage", "ReferencedPackageId")]
    public Package ReferencedPackage
    {
        get
        {
            return (Package)
                this.PersistenceProvider.RetrieveInstance(
                    typeof(Package),
                    new ModelElementKey(this.ReferencedPackageId)
                );
        }
        set { this.ReferencedPackageId = (Guid)value.PrimaryKey["Id"]; }
    }
    public int ReferenceType
    {
        get { return 1; }
        set { }
    }
    public bool IncludeAllElements
    {
        get { return true; }
        set { }
    }
    #endregion
    #region IBrowserNode2 Members
    [Browsable(false)]
    public bool Hide
    {
        get { return !this.IsPersisted; }
        set { throw new InvalidOperationException(ResourceUtils.GetString("ErrorSetHide")); }
    }

    public bool CanDelete
    {
        get
        {
            // TODO:  Add SchemaExtension.CanDelete getter implementation
            return false;
        }
    }

    public void Delete()
    {
        this.IsDeleted = true;
        this.Persist();
    }

    public bool CanMove(IBrowserNode2 newNode)
    {
        return false;
    }

    [Browsable(false)]
    public IBrowserNode2 ParentNode
    {
        get { return null; }
        set { throw new InvalidOperationException(ResourceUtils.GetString("ErrorMoveExtension")); }
    }
    public byte[] NodeImage
    {
        get { return null; }
    }

    [Browsable(false)]
    public string NodeId
    {
        get { return this.PrimaryKey["Id"].ToString(); }
    }

    [Browsable(false)]
    public virtual string FontStyle
    {
        get { return "Regular"; }
    }
    #endregion
    #region IBrowserNode Members
    public bool HasChildNodes
    {
        get { return this.ChildNodes().Count > 0; }
    }
    public bool CanRename
    {
        get { return false; }
    }

    public BrowserNodeCollection ChildNodes()
    {
        // Get children
        return new BrowserNodeCollection(); //this.ChildExtensions.ToArray(typeof(IBrowserNode)) as IBrowserNode[]);
    }

    public string NodeText
    {
        get { return this.ReferencedPackage.Name; }
        set { }
    }
    public string NodeToolTipText
    {
        get
        {
            // TODO:  Add SchemaExtension.NodeToolTipText getter implementation
            return null;
        }
    }
    public string Icon => "09_packages-1.ico";
    public string RelativeFilePath
    {
        get { return Package.RelativeFilePath; }
    }
    public Guid FileParentId
    {
        get => PackageId;
        set => PackageId = value;
    }
    public bool IsFolder
    {
        get { return false; }
    }
    public IDictionary<string, Guid> ParentFolderIds =>
        new Dictionary<string, Guid> { { CategoryFactory.Create(typeof(Package)), PackageId } };
    public string Path
    {
        get { throw new NotImplementedException(); }
    }
    #endregion
    #region IComparable Members
    public int CompareTo(object obj)
    {
        IBrowserNode bn = obj as IBrowserNode;
        if (bn != null)
        {
            return this.NodeText.CompareTo(bn.NodeText);
        }
        else
        {
            throw new InvalidCastException();
        }
    }
    #endregion
}
