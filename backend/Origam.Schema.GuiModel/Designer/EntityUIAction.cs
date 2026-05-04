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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;

namespace Origam.Schema.GuiModel;

/// <summary>
/// Version history:
/// 6.1.0 - Moved ScreenCondition and SectionCondition to child elements
/// 6.2.0 - Added ActionButtonPlacement.PanelMenu
/// </summary>
[SchemaItemDescription(name: "UI Action", folderName: "UI Actions", icon: 5)]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.2.0")]
public abstract class EntityUIAction : AbstractSchemaItem
{
    public const string CategoryConst = "EntityUIAction";

    public EntityUIAction()
        : base()
    {
        Init();
    }

    public EntityUIAction(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public EntityUIAction(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        ChildItemTypes.Add(item: typeof(EntityUIActionParameterMapping));
        ChildItemTypes.Add(item: typeof(ScreenCondition));
        ChildItemTypes.Add(item: typeof(ScreenSectionCondition));
    }

    #region Overriden ISchemaItem members
    public override Type[] NameableTypes => new[] { typeof(EntityUIActionParameterMapping) };

    #endregion
    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType => CategoryConst;
    public Hashtable ParameterMappings
    {
        get
        {
            var mappingDictionary = ChildItemsByType<EntityUIActionParameterMapping>(
                    itemType: "EntityUIActionParameterMapping"
                )
                .ToDictionary(keySelector: e => e.Name, elementSelector: e => e.Field);

            return new Hashtable(d: mappingDictionary);
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.Rule != null)
        {
            dependencies.Add(item: this.Rule);
        }
        if (this.ButtonIcon != null)
        {
            dependencies.Add(item: this.ButtonIcon);
        }
        if (this.ConfirmationMessage != null)
        {
            dependencies.Add(item: this.ConfirmationMessage);
        }
        if (this.ConfirmationRule != null)
        {
            dependencies.Add(item: this.ConfirmationRule);
        }
        if (this.KeyboardShortcut != null)
        {
            dependencies.Add(item: this.KeyboardShortcut);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override bool CanMove(Origam.UI.IBrowserNode2 newNode) =>
        newNode is AbstractDataEntity || newNode is EntityDropdownAction;
    #endregion
    #region Properties
    [Category(category: "Condition"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "roles")]
    public string Roles { get; set; } = "";

    [Browsable(browsable: false)]
    public IEnumerable<Guid> ScreenIds =>
        ChildItems.OfType<ScreenCondition>().Select(selector: reference => reference.ScreenId);

    [Browsable(browsable: false)]
    public IEnumerable<Guid> ScreenSectionIds =>
        ChildItems
            .OfType<ScreenSectionCondition>()
            .Select(selector: reference => reference.ScreenSectionId);

    [StringNotEmptyModelElementRule()]
    [Localizable(isLocalizable: true)]
    [XmlAttribute(attributeName: "label")]
    public string Caption { get; set; } = "";

    [Category(category: "Condition")]
    [XmlAttribute(attributeName: "features")]
    public string Features { get; set; }

    [XmlAttribute(attributeName: "order")]
    public int Order { get; set; } = 10;

    [XmlAttribute(attributeName: "actionType")]
    public virtual PanelActionType ActionType { get; set; } = PanelActionType.OpenForm;

    [XmlAttribute(attributeName: "mode")]
    public PanelActionMode Mode { get; set; } = PanelActionMode.ActiveRecord;

    [Category(category: "Condition")]
    [
        DefaultValue(value: CredentialValueType.SavedValue),
        RefreshProperties(refresh: RefreshProperties.Repaint)
    ]
    [XmlAttribute(attributeName: "valueType")]
    public CredentialValueType ValueType { get; set; } = CredentialValueType.SavedValue;

    [DefaultValue(value: ActionButtonPlacement.Toolbar)]
    [XmlAttribute(attributeName: "placement")]
    public ActionButtonPlacement Placement { get; set; } = ActionButtonPlacement.Toolbar;

    [DefaultValue(value: ReturnRefreshType.None)]
    [XmlAttribute(attributeName: "refreshAfterReturn")]
    public ReturnRefreshType RefreshAfterReturn { get; set; } = ReturnRefreshType.None;

    [DefaultValue(value: 0)]
    [XmlAttribute(attributeName: "modalDialogWidth")]
    public int ModalDialogWidth { get; set; } = 0;

    [DefaultValue(value: 0)]
    [XmlAttribute(attributeName: "modalDialogHeight")]
    public int ModalDialogHeight { get; set; } = 0;
    public Guid GraphicsId;

    [TypeConverter(type: typeof(GuiModel.GraphicsConverter))]
    [XmlReference(attributeName: "buttonIcon", idField: "GraphicsId")]
    public GuiModel.Graphics ButtonIcon
    {
        get =>
            (GuiModel.Graphics)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.GraphicsId)
                );
        set => this.GraphicsId = value?.Id ?? Guid.Empty;
    }
    public Guid RuleId;

    [Category(category: "Condition")]
    [TypeConverter(type: typeof(EntityRuleConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "rule", idField: "RuleId")]
    public IEntityRule Rule
    {
        get =>
            (IEntityRule)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.RuleId)
                );
        set => this.RuleId = value?.Id ?? Guid.Empty;
    }
    public Guid KeyboardShortcutId;

    [Category(category: "Keyboard")]
    [TypeConverter(type: typeof(KeyboardShortcutsConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "keyboardShortcut", idField: "KeyboardShortcutId")]
    public KeyboardShortcut KeyboardShortcut
    {
        get =>
            (KeyboardShortcut)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.KeyboardShortcutId)
                );
        set => this.KeyboardShortcutId = value?.Id ?? Guid.Empty;
    }

    [Category(category: "Keyboard")]
    [XmlAttribute(attributeName: "scannerInputParameter")]
    public string ScannerInputParameter { get; set; }

    [Category(category: "Keyboard")]
    [XmlAttribute(attributeName: "scannerTerminator")]
    public string ScannerTerminator { get; set; }

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "default")]
    public bool IsDefault { get; set; } = false;

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "modal")]
    public bool IsModalDialog { get; set; } = false;
    public override byte[] NodeImage
    {
        get
        {
            if (ButtonIcon == null)
            {
                return null;
            }
            return ButtonIcon.GraphicsData.ToByteArray();
        }
    }
    public Guid ConfirmationMessageId;

    [Category(category: "References")]
    [TypeConverter(type: typeof(StringItemConverter))]
    [XmlReference(attributeName: "confirmationMessage", idField: "ConfirmationMessageId")]
    public StringItem ConfirmationMessage
    {
        get =>
            (StringItem)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.ConfirmationMessageId)
                );
        set => this.ConfirmationMessageId = value?.Id ?? Guid.Empty;
    }
    public Guid ConfirmationRuleId;

    [Category(category: "References")]
    [TypeConverter(type: typeof(EndRuleConverter))]
    [XmlReference(attributeName: "confirmationRule", idField: "ConfirmationRuleId")]
    [Description(
        description: "Validation rule, that is executed before the action is invoked. Input xml root element is rows and records are represented by row elements."
    )]
    public IEndRule ConfirmationRule
    {
        get =>
            (IEndRule)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.ConfirmationRuleId)
                );
        set => this.ConfirmationRuleId = value?.Id ?? Guid.Empty;
    }
    #endregion
    #region IComparable Members
    public override int CompareTo(object obj)
    {
        if (obj is EntityUIAction compared)
        {
            return this.Name.CompareTo(strB: compared.Name);
        }

        return base.CompareTo(obj: obj);
    }
    #endregion
}

public class EntityUIActionOrderComparer : IComparer<ISchemaItem>
{
    public int Compare(ISchemaItem x, ISchemaItem y) =>
        (x as EntityUIAction).Order.CompareTo(value: (y as EntityUIAction).Order);
}
