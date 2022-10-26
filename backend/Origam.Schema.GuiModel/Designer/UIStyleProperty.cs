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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System.Collections;
using System.Xml.Serialization;

namespace Origam.Schema.GuiModel
{
    /// <summary>
    /// Summary description for Graphics.
    /// </summary>
    [SchemaItemDescription("Style Property", "icon_style-property.png")]
    [HelpTopic("Styles")]
    [XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public class UIStyleProperty : AbstractSchemaItem
    {
        public const string CategoryConst = "StyleProperty";

        public UIStyleProperty() : base() { Init(); }

        public UIStyleProperty(Guid schemaExtensionId) : base(schemaExtensionId) { Init(); }

        public UIStyleProperty(Key primaryKey) : base(primaryKey) { Init(); }

        private void Init()
        {

        }

        #region Properties
        public Guid ControlStylePropertyId;

        [TypeConverter(typeof(ControlStylePropertyConverter))]
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotNullModelElementRule()]
        [XmlReference("property", "ControlStylePropertyId")]
        public ControlStyleProperty Property
        {
            get
            {
                return (ControlStyleProperty)this.PersistenceProvider.RetrieveInstance(
                    typeof(ControlStyleProperty), new ModelElementKey(this.ControlStylePropertyId));
            }
            set
            {
                this.ControlStylePropertyId = (Guid)value.PrimaryKey["Id"];
                UpdateName();
            }
        }

        private string _value;
        [RefreshProperties(RefreshProperties.Repaint)]
        [NotNullModelElementRule()]
        [XmlAttribute("value")]
        public string Value
        {
            get
            {
                if (_value == null) return null;
                return _value.Trim();
            }
            set
            {
                _value = value;
                UpdateName();
            }
        }
        #endregion

        #region Overriden AbstractSchemaItem Members
        public override void GetExtraDependencies(ArrayList dependencies)
        {
            if (this.Property != null)
            {
                dependencies.Add(this.Property);
            }
            base.GetExtraDependencies(dependencies);
        }

        public override string ItemType
        {
            get
            {
                return CategoryConst;
            }
        }
        #endregion

        private void UpdateName()
        {
            if (this.Property != null && this.Value != null)
            {
                this.Name = this.Property.Name + ": " + this.Value;
            }
        }
    }
}
