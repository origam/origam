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
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.UI.WizardForm;

public class CreateFieldWithRelationshipEntityWizardForm : AbstractWizardForm
{
    private IDataEntity _entity;
    public IDataEntity Entity
    {
        get { return _entity; }
        set { _entity = value; }
    }
    private ISchemaItem _relatedEntity = null;
    public ISchemaItem RelatedEntity
    {
        get { return _relatedEntity; }
        set { _relatedEntity = value; }
    }
    private ISchemaItem _baseEntityField = null;
    public ISchemaItem BaseEntityFieldSelect
    {
        get { return _baseEntityField; }
        set { _baseEntityField = value; }
    }
    private ISchemaItem _relatedEntityField = null;
    public ISchemaItem RelatedEntityFieldSelect
    {
        get { return _relatedEntityField; }
        set { _relatedEntityField = value; }
    }
    private Boolean _isparentChild = false;
    public Boolean ParentChildCheckbox
    {
        get { return _isparentChild; }
        set { _isparentChild = value; }
    }
    public string EnterAllInfo { get; set; }
    public string LookupWiz { get; set; }
    public string LookupName { get; internal set; }
    public string LookupKeyName { get; internal set; }

    internal void SetUpForm(ComboBox tableRelation, TextBox txtRelationName)
    {
        if (tableRelation.Items.Count == 0)
        {
            if (this.Entity == null)
                return;
            txtRelationName.Text = this.Entity.Name;
            foreach (ISchemaItem abstractSchemaIttem in this.Entity.RootProvider.ChildItems)
            {
                tableRelation.Items.Add(abstractSchemaIttem);
            }
        }
    }

    internal void SetUpFormKey(
        ComboBox BaseEntityField,
        ComboBox RelatedEntityField,
        TextBox txtKeyName
    )
    {
        BaseEntityField.Items.Clear();
        RelatedEntityField.Items.Clear();
        if (this.Entity == null)
            return;
        txtKeyName.Text = RelatedEntity.NodeText + "_RelationtionKey";
        foreach (var filter in RelatedEntity.ChildItemsByType<ISchemaItem>("DataEntityColumn"))
        {
            RelatedEntityField.Items.Add(filter);
        }
        foreach (IDataEntityColumn column in this.Entity.EntityColumns)
        {
            BaseEntityField.Items.Add(column);
        }
    }
}
