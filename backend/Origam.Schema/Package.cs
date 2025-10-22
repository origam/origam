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
using Origam.DA;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.UI;

namespace Origam.Schema;

/// <summary>
/// Summary description for Schema.
/// </summary>
[XmlPackageRoot("package")]
[ClassMetaVersion("6.1.0")]
public class Package : AbstractPersistent, IBrowserNode2, IComparable, IFilePersistent
{
    SchemaItemProviderGroup _commonModelGroup = new SchemaItemProviderGroup(
        "COMMON",
        "Common",
        "icon_01_common.png",
        0
    );
    SchemaItemProviderGroup _dataModelGroup = new SchemaItemProviderGroup(
        "DATA",
        "Data",
        "icon_05_data.png",
        1
    );
    SchemaItemProviderGroup _userInterfaceModelGroup = new SchemaItemProviderGroup(
        "UI",
        "User Interface",
        "icon_13_user-interface.png",
        2
    );
    SchemaItemProviderGroup _businessLogicModelGroup = new SchemaItemProviderGroup(
        "BL",
        "Business Logic",
        "icon_26_business-logic.png",
        3
    );
    SchemaItemProviderGroup _apiModelGroup = new SchemaItemProviderGroup(
        "API",
        "API",
        "icon_35_API.png",
        4
    );

    public Package()
    {
        this.PrimaryKey = new ModelElementKey();
    }

    public Package(Key primaryKey)
        : base(primaryKey, new ModelElementKey().KeyArray)
    {
        this._childNodes.Add(_commonModelGroup);
        this._childNodes.Add(_dataModelGroup);
        this._childNodes.Add(_userInterfaceModelGroup);
        this._childNodes.Add(_businessLogicModelGroup);
        this._childNodes.Add(_apiModelGroup);
    }

    public override void Persist()
    {
        Action persistsAction = () =>
        {
            if (!IsDeleted)
            {
                base.Persist();
            }
            foreach (var packageReference in References)
            {
                packageReference.Persist();
            }
            if (IsDeleted)
            {
                base.Persist();
            }
        };

        if (PersistenceProvider.IsInTransaction)
        {
            persistsAction();
        }
        else
        {
            PersistenceProvider.RunInTransaction(persistsAction);
        }
    }

    public override string ToString() => this.Name;

    #region Properties
    public bool IsFileRootElement => true;

    [XmlAttribute(AttributeName = "name")]
    public string name = "";
    public string Name
    {
        get => name;
        set
        {
            OldName = name;
            name = value;
        }
    }
    public string OldName { get; private set; } = "";
    public bool WasRenamed => !string.IsNullOrEmpty(OldName) && OldName != Name;

    /// <summary>
    /// Gets or sets the version of this schema extension.
    /// </summary>
    [XmlAttribute(AttributeName = "version")]
    public string VersionString { get; set; } = "";
    public PackageVersion Version => new PackageVersion(VersionString);

    [XmlAttribute(AttributeName = "copyright")]
    public string Copyright { get; set; } = "";

    [XmlAttribute(AttributeName = "description")]
    public string Description { get; set; } = "";
    public IList<Package> IncludedPackages
    {
        get
        {
            List<Package> result = new List<Package>();
            SortPackages(this, result);
            return result;
        }
    }

    private static void SortPackages(Package package, IList<Package> packages)
    {
        foreach (PackageReference reference in package.References)
        {
            if (!packages.Contains(reference.ReferencedPackage))
            {
                SortPackages(reference.ReferencedPackage, packages);
                packages.Add(reference.ReferencedPackage);
            }
        }
    }
    #endregion
    #region IBrowserNode2 Members
    [Browsable(false)]
    public bool Hide
    {
        get => !this.IsPersisted;
        set => throw new InvalidOperationException(ResourceUtils.GetString("ErrorSetHide"));
    }

    public bool CanDelete => false;

    public void Delete()
    {
        // TODO:  Add SchemaExtension.Delete implementation
    }

    public bool CanMove(IBrowserNode2 newNode) => false;

    [Browsable(false)]
    public IBrowserNode2 ParentNode
    {
        get => null;
        set => throw new InvalidOperationException(ResourceUtils.GetString("ErrorMoveExtension"));
    }
    public byte[] NodeImage => null;

    [Browsable(false)]
    public string NodeId => this.PrimaryKey["Id"].ToString();

    [Browsable(false)]
    public virtual string FontStyle => "Regular";
    #endregion
    #region IBrowserNode Members
    public bool HasChildNodes => this.ChildNodes().Count > 0;
    public bool CanRename => false;
    public List<PackageReference> References =>
        PersistenceProvider.RetrieveListByParent<PackageReference>(
            this.PrimaryKey,
            "SchemaExtension",
            "PackageReference",
            this.UseObjectCache
        );
    BrowserNodeCollection _childNodes = new BrowserNodeCollection();

    public BrowserNodeCollection ChildNodes()
    {
        return _childNodes;
        //return new BrowserNodeCollection(this.References.ToArray(typeof(IBrowserNode)) as IBrowserNode[]);
    }

    public void AddProvider(AbstractSchemaItemProvider provider)
    {
        SchemaItemProviderGroup group = null;
        foreach (SchemaItemProviderGroup childGroup in this.ChildNodes())
        {
            if (childGroup.NodeId == provider.Group)
            {
                group = childGroup;
                break;
            }
        }
        if (group == null)
        {
            throw new ArgumentOutOfRangeException("group", provider.Group, "unknown model group");
        }
        group.ChildNodes().Add(provider);
    }

    public string NodeText
    {
        get => this.Name;
        set => this.Name = value;
    }
    public string NodeToolTipText => null;
    public string Icon => "09_packages-1.ico";
    public string RelativeFilePath =>
        System.IO.Path.Combine(Name, PersistenceFiles.PackageFileName);
    public Guid FileParentId
    {
        get => Guid.Empty;
        set { }
    }
    public bool IsFolder => true;
    public IDictionary<string, Guid> ParentFolderIds => new Dictionary<string, Guid>();
    public string Path => null;
    #endregion
    #region IComparable Members
    public int CompareTo(object obj)
    {
        IBrowserNode bn = obj as IBrowserNode;
        if (bn != null)
        {
            return this.NodeText.CompareTo(bn.NodeText);
        }

        throw new InvalidCastException();
    }
    #endregion
}
