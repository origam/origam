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
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.Extensions;

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Version history:
	/// 6.1.0 - Moved ScreenCondition and SectionCondition to child elements
	/// 6.2.0 - Added ActionButtonPlacement.PanelMenu
	/// </summary>
	[SchemaItemDescription("UI Action", "UI Actions", 5)]
	[XmlModelRoot(CategoryConst)]
	[ClassMetaVersion("6.2.0")]
	public abstract class EntityUIAction : AbstractSchemaItem
	{
		public const string CategoryConst = "EntityUIAction";

		public EntityUIAction() : base() {Init();}

		public EntityUIAction(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}

		public EntityUIAction(Key primaryKey) : base(primaryKey) {Init();}
	
		private void Init()
		{
			ChildItemTypes.Add(typeof(EntityUIActionParameterMapping));
			ChildItemTypes.Add(typeof(ScreenCondition));
			ChildItemTypes.Add(typeof(ScreenSectionCondition));
		}
		
		#region Overriden AbstractSchemaItem members

		public override Type[] NameableTypes
			=> new[] { typeof(EntityUIActionParameterMapping) };
		
		#endregion

		#region Overriden AbstractDataEntityColumn Members
		
		[EntityColumn("ItemType")]
		public override string ItemType => CategoryConst;

		public Hashtable ParameterMappings {
			get
			{
				var mappingDictionary = ChildItemsByType("EntityUIActionParameterMapping")
					.Cast<EntityUIActionParameterMapping>()
					.ToDictionary(e => e.Name,e => e.Field);
				
				return new Hashtable(mappingDictionary);
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
            if (this.Rule != null)
            {
                dependencies.Add(this.Rule);
            }
            if (this.ButtonIcon != null)
            {
                dependencies.Add(this.ButtonIcon);
            }
			if (this.ConfirmationMessage != null)
			{
				dependencies.Add(this.ConfirmationMessage);
			}
			if(this.ConfirmationRule != null)
			{
				dependencies.Add(this.ConfirmationRule);
			}
            if (this.KeyboardShortcut != null)
            {
                dependencies.Add(this.KeyboardShortcut);
            }
			base.GetExtraDependencies (dependencies);
		}

		public override bool CanMove(Origam.UI.IBrowserNode2 newNode) =>
			   newNode is AbstractDataEntity
			|| newNode is EntityDropdownAction;
		#endregion

		#region Properties

		[Category("Condition"), RefreshProperties(RefreshProperties.Repaint)]
		[EntityColumn("LS01")]
		[StringNotEmptyModelElementRule()]
        [XmlAttribute("roles")]
		public string Roles { get; set; } = "";

		[Browsable(false)]
		public IEnumerable<Guid> ScreenIds => ChildItems
			.ToGeneric()
			.OfType<ScreenCondition>()
			.Select(reference => reference.ScreenId);

		[Browsable(false)]
		public IEnumerable<Guid> ScreenSectionIds  => ChildItems
			.ToGeneric()
			.OfType<ScreenSectionCondition>()
			.Select(reference => reference.ScreenSectionId);

		[EntityColumn("SS01")]
		[StringNotEmptyModelElementRule()]
		[Localizable(true)]
        [XmlAttribute("label")]
		public string Caption { get; set; } = "";

		[Category("Condition")]
		[EntityColumn("SS02")]
        [XmlAttribute("features")]
		public string Features { get; set; }

		[EntityColumn("I01")]
        [XmlAttribute("order")]
		public int Order { get; set; } = 10;

		[EntityColumn("I02")]
        [XmlAttribute("actionType")]
		public virtual PanelActionType ActionType { get; set; } = PanelActionType.OpenForm;

		[EntityColumn("I03")]
        [XmlAttribute("mode")]
		public PanelActionMode Mode { get; set; } = PanelActionMode.ActiveRecord;

		[Category("Condition")]
		[DefaultValue(CredentialValueType.SavedValue), RefreshProperties(RefreshProperties.Repaint)]
		[EntityColumn("I04")]
        [XmlAttribute("valueType")]
		public CredentialValueType ValueType { get; set; } = CredentialValueType.SavedValue;

		[DefaultValue(ActionButtonPlacement.Toolbar)]
		[EntityColumn("I05")]
        [XmlAttribute("placement")]
		public ActionButtonPlacement Placement { get; set; } = ActionButtonPlacement.Toolbar;

		[DefaultValue(ReturnRefreshType.None)]
		[EntityColumn("I06")]
        [XmlAttribute("refreshAfterReturn")]
		public ReturnRefreshType RefreshAfterReturn { get; set; } = ReturnRefreshType.None;

		[DefaultValue(0)]
		[EntityColumn("F01")]
        [XmlAttribute("modalDialogWidth")]
		public int ModalDialogWidth { get; set; } = 0;

		[DefaultValue(0)]
		[EntityColumn("F02")]
        [XmlAttribute("modalDialogHeight")]
		public int ModalDialogHeight { get; set; } = 0;

		[EntityColumn("G02")]  
		public Guid GraphicsId;

		[TypeConverter(typeof(GuiModel.GraphicsConverter))]
        [XmlReference("buttonIcon", "GraphicsId")]
		public GuiModel.Graphics ButtonIcon
		{
			get => (GuiModel.Graphics)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.GraphicsId));
			set => this.GraphicsId = value?.Id ?? Guid.Empty;
		}

		[EntityColumn("G03")]  
		public Guid RuleId;

		[Category("Condition")]
		[TypeConverter(typeof(EntityRuleConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("rule", "RuleId")]
        public IEntityRule Rule
		{
			get => (IEntityRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.RuleId));
			set => this.RuleId = value?.Id ?? Guid.Empty;
		}

		[EntityColumn("G04")]  
		public Guid KeyboardShortcutId;

		[Category("Keyboard")]
		[TypeConverter(typeof(KeyboardShortcutsConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("keyboardShortcut", "KeyboardShortcutId")]
        public KeyboardShortcut KeyboardShortcut
		{
			get => (KeyboardShortcut)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.KeyboardShortcutId));
			set => this.KeyboardShortcutId = value?.Id ?? Guid.Empty;
		}

		[Category("Keyboard")]
		[EntityColumn("SS03")]
        [XmlAttribute("scannerInputParameter")]
		public string ScannerInputParameter { get; set; }

		[Category("Keyboard")]
		[EntityColumn("SS04")]
        [XmlAttribute("scannerTerminator")]
		public string ScannerTerminator { get; set; }

		[DefaultValue(false)]
		[EntityColumn("B01")]
        [XmlAttribute("default")]
		public bool IsDefault { get; set; } = false;

		[DefaultValue(false)]
		[EntityColumn("B02")]
        [XmlAttribute("modal")]
		public bool IsModalDialog { get; set; } = false;

		public override byte[] NodeImage
		{
			get
			{
				if(ButtonIcon == null)
				{
					return null;
				}
				return ButtonIcon.GraphicsData.ToByteArray();
			}
		}

        [EntityColumn("G06")]
        public Guid ConfirmationMessageId;

        [Category("References")]
        [TypeConverter(typeof(StringItemConverter))]
        [XmlReference("confirmationMessage", "ConfirmationMessageId")]
        public StringItem ConfirmationMessage
        {
            get => (StringItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ConfirmationMessageId));
	        set => this.ConfirmationMessageId =  value?.Id ?? Guid.Empty;
        }

        [EntityColumn("G07")]
        public Guid ConfirmationRuleId;

        [Category("References")]
        [TypeConverter(typeof(EndRuleConverter))]
        [XmlReference("confirmationRule", "ConfirmationRuleId")]
		[Description("Validation rule, that is executed before the action is invoked. Input xml root element is rows and records are represented by row elements.")]
        public IEndRule ConfirmationRule
        {
            get => (IEndRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ConfirmationRuleId));
	        set => this.ConfirmationRuleId = value?.Id ?? Guid.Empty;
        }
		#endregion

		#region IComparable Members
		public override int CompareTo(object obj)
		{
			if(obj is EntityUIAction compared)
			{
				return this.Name.CompareTo(compared.Name);
			}
			else
			{
                return base.CompareTo(obj);
			}
		}
		#endregion
	}

    public class EntityUIActionOrderComparer : IComparer
    {
        public int Compare(object x, object y) 
            => (x as EntityUIAction).Order.CompareTo(
                (y as EntityUIAction).Order);
    }
}
