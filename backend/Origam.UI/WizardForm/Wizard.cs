#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using System.Drawing;
using System.Windows.Forms;
using AeroWizard;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.UI.Interfaces;

namespace Origam.UI.WizardForm;

public partial class Wizard : Form
{
    IWizardForm iwizard;

    public Wizard(IWizardForm objectForm)
    {
        InitializeComponent();
        iwizard = objectForm;
        StartPage.Text = "What will happen...";
        if (iwizard.Title != null)
        {
            aerowizard1.Title = iwizard.Title;
        }

        InitData();
    }

    private void InitData()
    {
        lbTitle.Text =
            "The Wizard will create following elements necessary for the function of the menu:";
        listView1.View = View.List;
        listView1.SmallImageList = iwizard.ImageList;
        listView1.StateImageList = iwizard.ImageList;
        progresslistview.SmallImageList = iwizard.ImageList;
        progresslistview.StateImageList = iwizard.ImageList;
    }

    #region Inicialize&Commit
    private void PageStart_Initialize(object sender, WizardPageInitEventArgs e)
    {
        txtLabel.Text = iwizard.Description;
        txtLabel.Size = TextRenderer.MeasureText(text: iwizard.Description, font: txtLabel.Font);
        iwizard.ListView(listView: listView1);
        GetNextPage(actualPage: PagesList.StartPage, sender: sender);
    }

    private void PageStart_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        IsFinish(sender: sender, e: e);
    }

    private void StructureNamePage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        tbDataStructureName.Text = iwizard.NameOfEntity;
        GetNextPage(actualPage: PagesList.StructureNamePage, sender: sender);
    }

    private void StructureNamePage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        if (iwizard.IsExistsNameInDataStructure(name: tbDataStructureName.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: "The Name already Exists!",
                caption: "Name Exists",
                exception: null
            );
            e.Cancel = true;
            return;
        }
        iwizard.NameOfEntity = tbDataStructureName.Text;
        IsFinish(sender: sender, e: e);
    }

    private void ScreenFormPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        GetNextPage(actualPage: PagesList.ScreenForm, sender: sender);
        ScreenWizardForm screenWizard = (ScreenWizardForm)iwizard;
        screenWizard.SetUpForm(lstField: lstFields);
        lstFields.CheckOnClick = screenWizard.CheckOnClick;
        lblRole.Visible = screenWizard.IsRoleVisible;
        txtRole.Visible = screenWizard.IsRoleVisible;
        txtRole.Text = screenWizard.Role;
    }

    private void ScreenFormPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        ScreenWizardForm screenWizard = (ScreenWizardForm)iwizard;
        screenWizard.SelectedFields = lstFields.CheckedItems;
        screenWizard.Role = txtRole.Text;
        screenWizard.Caption = txtScreenCaption.Text;
        IsFinish(sender: sender, e: e);
    }

    private void LookupFormPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        LookupForm form = (LookupForm)iwizard;
        form.SetUpForm(
            cboIdFilter: cboIdFilter,
            cboListFilter: cboListFilter,
            cboDisplayField: cboDisplayField,
            txtName: txtName
        );
        txtName.Text = form.LookupName;
        cboDisplayField.SelectedItem = form.NameColumn;
        cboIdFilter.SelectedItem = form.IdFilter;
        cboListFilter.SelectedItem = form.ListFilter;
        GetNextPage(actualPage: PagesList.LookupForm, sender: sender);
    }

    private void LookupFormPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        LookupForm form = (LookupForm)iwizard;
        form.LookupName = txtName.Text;
        form.NameColumn = cboDisplayField.SelectedItem as IDataEntityColumn;
        form.IdFilter = cboIdFilter.SelectedItem as EntityFilter;
        form.ListFilter = cboListFilter.SelectedItem as EntityFilter;
        if (
            form.LookupName == ""
            | form.Entity == null
            | form.NameColumn == null
            | form.IdColumn == null
            | form.IdFilter == null
        )
        {
            MessageBox.Show(
                text: ResourceUtils.GetString(key: "EnterAllInfo"),
                caption: ResourceUtils.GetString(key: "LookupWiz"),
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Asterisk
            );
            e.Cancel = true;
            return;
        }
        IsFinish(sender: sender, e: e);
    }

    private void FieldLookupEntityPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        CreateFieldWithLookupEntityWizardForm form = (CreateFieldWithLookupEntityWizardForm)iwizard;
        grdInitialValues.AutoGenerateColumns = false;
        grdInitialValues.DataSource = form.InitialValues;
        txtNameFieldName.Text = form.NameFieldName;
        txtNameFieldCaption.Text = form.NameFieldCaption;
        txtKeyFieldName.Text = form.KeyFieldName;
        txtKeyFieldCaption.Text = form.KeyFieldCaption;
        UpdateScreen();
        if (form.ForceTwoColumns)
        {
            chkTwoColumn.Checked = true;
            chkTwoColumn.Visible = false;
            chkAllowNulls.Visible = false;
            lblCaption.Visible = false;
            txtCaption.Visible = false;
        }
        GetNextPage(actualPage: PagesList.FieldLookup, sender: sender);
    }

    private void FieldLookupEntityPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        CreateFieldWithLookupEntityWizardForm form = (CreateFieldWithLookupEntityWizardForm)iwizard;
        form.LookupName = lookupname.Text;
        form.LookupCaption = txtCaption.Text;
        form.AllowNulls = chkAllowNulls.Checked;
        form.NameFieldName = txtNameFieldName.Text;
        form.NameFieldCaption = txtNameFieldCaption.Text;
        form.KeyFieldName = txtKeyFieldName.Text;
        form.KeyFieldCaption = txtKeyFieldCaption.Text;
        form.TwoColumns = chkTwoColumn.Checked;
        form.ForceTwoColumns = !chkTwoColumn.Enabled;
        if (form.LookupName == "" || (txtCaption.Visible && form.LookupCaption == ""))
        {
            MessageBox.Show(
                text: form.EnterAllInfo,
                caption: form.LookupWiz,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Asterisk
            );
            e.Cancel = true;
            return;
        }
        if (!form.AllowNulls && form.InitialValues.Count > 0 && form.DefaultInitialValue == null)
        {
            if (
                MessageBox.Show(
                    text: form.DefaultValueNotSet,
                    caption: form.LookupWiz,
                    buttons: MessageBoxButtons.OKCancel,
                    icon: MessageBoxIcon.Warning
                ) == DialogResult.Cancel
            )
            {
                e.Cancel = true;
                return;
            }
        }
        IsFinish(sender: sender, e: e);
    }

    private void FinishPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        GetNextPage(actualPage: PagesList.Finish, sender: sender);
        ISchemaItem[] results = new ISchemaItem[0];
        try
        {
            iwizard.Command.Execute();
            results = ((AbstractMenuCommand)iwizard.Command).GeneratedModelElements.ToArray();
        }
        catch (Exception ex)
        {
            ListViewItem newItem = new ListViewItem(items: new string[] { "error", ex.Message });
            progresslistview.Items.Add(value: newItem);
            e.Cancel = true;
        }
        for (int i = 0; i < results.LongLength; i++)
        {
            ISchemaItem item = results[i];
            ListViewItem newItem = new ListViewItem(
                items: new string[] { item.Path, item.ModelDescription() }
            )
            {
                ImageIndex = iwizard.Command.GetImageIndex(icon: item.RootItem.Icon),
            };
            progresslistview.Items.Add(value: newItem);
        }
        this.aerowizard1.FinishButtonText = "Show Result";
        this.aerowizard1.CancelButtonText = "Close";
    }

    private void FinishPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        IsFinish(sender: sender, e: e);
    }

    private void RelationShipEntityPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        GetNextPage(actualPage: PagesList.FieldEntity, sender: sender);
        CreateFieldWithRelationshipEntityWizardForm wizardForm =
            (CreateFieldWithRelationshipEntityWizardForm)iwizard;
        wizardForm.SetUpForm(tableRelation: tableRelation, txtRelationName: txtRelationName);
    }

    private void RelationShipEntityPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        CreateFieldWithRelationshipEntityWizardForm wizardForm =
            (CreateFieldWithRelationshipEntityWizardForm)iwizard;
        wizardForm.LookupName = txtRelationName.Text;
        wizardForm.LookupKeyName = txtKeyName.Text;
        if (
            wizardForm.LookupName == ""
            || wizardForm.RelatedEntity == null
            || wizardForm.BaseEntityFieldSelect == null
            || wizardForm.RelatedEntityFieldSelect == null
            || txtKeyName.Text == ""
        )
        {
            MessageBox.Show(
                text: wizardForm.EnterAllInfo,
                caption: wizardForm.LookupWiz,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Asterisk
            );
            e.Cancel = true;
            return;
        }
        IsFinish(sender: sender, e: e);
    }

    private void ChildEntityPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        GetNextPage(actualPage: PagesList.ChildEntity, sender: sender);
        ChildEntityForm EntityForm = (ChildEntityForm)iwizard;
        EntityForm.EntityName = txtchildEntityName.Text;
        EntityForm.SetUpForm(
            txtchildEntityName: txtchildEntityName,
            cboEntity1: cboEntity1,
            cboEntity2: cboEntity2
        );
    }

    private void ChildEntityPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        ChildEntityForm EntityForm = (ChildEntityForm)iwizard;
        EntityForm.Entity2 = cboEntity2.SelectedItem as IDataEntity;
        EntityForm.EntityName = txtchildEntityName.Text;
        if (txtchildEntityName.Text == "" | EntityForm.Entity1 == null)
        {
            MessageBox.Show(
                text: EntityForm.EnterAllInfo,
                caption: EntityForm.ChildEntityWiz,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Asterisk
            );
            e.Cancel = true;
            return;
        }
        IsFinish(sender: sender, e: e);
    }

    private void ForeignKeyPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        GetNextPage(actualPage: PagesList.ForeignForm, sender: sender);
        ForeignKeyForm foreignKey = (ForeignKeyForm)iwizard;
        foreignKey.SetUpForm(
            txtFieldName: txtFkFieldName,
            cboEntity: cboEntity,
            cboLookup: cboLookup,
            cboField: cboField,
            chkAllowNulls: chkAllowNulls
        );
    }

    private void ForeignKeyPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        ForeignKeyForm foreignKey = (ForeignKeyForm)iwizard;
        foreignKey.ForeignKeyName = txtFkFieldName.Text;
        foreignKey.Caption = txtfkCaptionName.Text;
        foreignKey.ForeignEntity = cboEntity.SelectedItem as IDataEntity;
        foreignKey.ForeignField = cboField.SelectedItem as IDataEntityColumn;
        foreignKey.Lookup = cboLookup.SelectedItem as IDataLookup;
        foreignKey.AllowNulls = checkBoxAllowNulls.Checked;
        if (foreignKey.ForeignEntity == null)
        {
            MessageBox.Show(
                text: foreignKey.SelectForeignEntity,
                caption: foreignKey.ForeignKeyWiz,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Asterisk
            );
            e.Cancel = true;
            return;
        }
        if (foreignKey.ForeignField == null)
        {
            MessageBox.Show(
                text: foreignKey.SelectForeignField,
                caption: foreignKey.ForeignKeyWiz,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Asterisk
            );
            e.Cancel = true;
            return;
        }
        if (txtFkFieldName.Text == "")
        {
            MessageBox.Show(
                text: foreignKey.EnterKeyName,
                caption: foreignKey.ForeignKeyWiz,
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Asterisk
            );
            e.Cancel = true;
            return;
        }
        IsFinish(sender: sender, e: e);
    }

    private void MenuFromPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        MenuFromForm menufrom = (MenuFromForm)iwizard;
        txtMenuRole.Text = menufrom.Role;
        GetNextPage(actualPage: PagesList.MenuPage, sender: sender);
    }

    private void MenuFromPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        MenuFromForm menufrom = (MenuFromForm)iwizard;
        menufrom.Role = string.IsNullOrEmpty(value: txtMenuRole.Text) ? "*" : txtMenuRole.Text;
        menufrom.Caption = txtMenuCaption.Text;
        IsFinish(sender: sender, e: e);
    }

    private void SummaryPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender: sender);
        iwizard.Command.SetSummaryText(summary: richTextBoxSummary);
        richTextBoxSummary.BackColor = Color.White;
        GetNextPage(actualPage: PagesList.SummaryPage, sender: sender);
        this.aerowizard1.NextButtonText = "Start";
    }

    private void SummaryPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        IsFinish(sender: sender, e: e);
    }
    #endregion
    #region support
    private void RefreshName()
    {
        ChildEntityForm EntityForm = (ChildEntityForm)iwizard;
        if (cboEntity1.SelectedItem != null & cboEntity2.SelectedItem != null)
        {
            EntityForm.Entity1 = cboEntity1.SelectedItem as IDataEntity;
            EntityForm.Entity2 = cboEntity2.SelectedItem as IDataEntity;
            txtchildEntityName.Text = EntityForm.Entity1.Name + EntityForm.Entity2.Name;
        }
    }

    private void UpdateScreen()
    {
        lblKeyFieldName.Visible =
            lblKeyFieldCaption.Visible =
            txtKeyFieldName.Visible =
            txtKeyFieldCaption.Visible =
                chkTwoColumn.Checked;
        grdInitialValues.Columns.Clear();
        if (chkTwoColumn.Checked)
        {
            grdInitialValues.Columns.AddRange(
                dataGridViewColumns: new DataGridViewColumn[] { colDefault, colCode, colName }
            );
            colCode.DisplayIndex = 0;
            colName.DisplayIndex = 1;
            colDefault.DisplayIndex = 2;
        }
        else
        {
            grdInitialValues.Columns.AddRange(
                dataGridViewColumns: new DataGridViewColumn[] { colDefault, colName }
            );
            colName.DisplayIndex = 0;
            colDefault.DisplayIndex = 1;
        }
    }

    private void CboDisplayField_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cboDisplayField.SelectedItem != null)
        {
            UpdateLookupName();
        }
    }

    private void cboIdFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cboIdFilter.SelectedItem != null)
        {
            UpdateLookupName();
        }
    }

    private void UpdateLookupName()
    {
        var form = (LookupForm)iwizard;
        var fieldSegment = (cboDisplayField.SelectedItem as IDataEntityColumn)?.Name ?? "";
        var filterSegment = (cboIdFilter.SelectedItem as ISchemaItem)?.Name ?? "";
        var lookupName = form.Entity.Name;
        if (fieldSegment != "")
        {
            lookupName += "_" + fieldSegment;
        }
        if (filterSegment != "")
        {
            lookupName += "_" + filterSegment;
        }
        txtName.Text = lookupName;
    }

    private void IsFinish(object sender, WizardPageConfirmEventArgs e)
    {
        if (((WizardPage)sender).IsFinishPage && !e.Cancel)
        {
            DialogResult = DialogResult.OK;
        }
    }

    private void GetNextPage(PagesList actualPage, object sender)
    {
        this.aerowizard1.NextButtonText = "Next";
        WizardPage wizardPage = (WizardPage)sender;
        bool findPage = false;
        foreach (PagesList pglist in iwizard.Pages)
        {
            if (findPage)
            {
                wizardPage.NextPage = getWizardPage(nextPage: pglist);
                return;
            }
            if (pglist == actualPage)
            {
                findPage = true;
            }
        }
        wizardPage.IsFinishPage = true;
    }

    private WizardPage getWizardPage(PagesList nextPage)
    {
        switch (nextPage)
        {
            case PagesList.StructureNamePage:
            {
                return StructureNamePage;
            }
            case PagesList.ScreenForm:
            {
                return ScreenFormPage;
            }
            case PagesList.LookupForm:
            {
                return LookupFormPage;
            }
            case PagesList.FieldLookup:
            {
                return FieldLookupEntity;
            }
            case PagesList.Finish:
            {
                return finishPage;
            }
            case PagesList.FieldEntity:
            {
                return RelationShipEntityPage;
            }
            case PagesList.ChildEntity:
            {
                return childEntityPage;
            }
            case PagesList.ForeignForm:
            {
                return foreignKeyPage;
            }
            case PagesList.SummaryPage:
            {
                return SummaryPage;
            }
            case PagesList.MenuPage:
            {
                return menuFromPage;
            }
            default:
            {
                MessageBox.Show(
                    text: "Not Set WizardPage",
                    caption: "Error",
                    buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Error
                );
                break;
            }
        }
        return null;
    }

    private void TxtNameFieldCaption_TextChanged(object sender, EventArgs e)
    {
        colName.HeaderText = txtNameFieldCaption.Text;
    }

    private void TxtKeyFieldCaption_TextChanged(object sender, EventArgs e)
    {
        colCode.HeaderText = txtKeyFieldCaption.Text;
    }

    private void ChkTwoColumn_CheckedChanged(object sender, EventArgs e)
    {
        UpdateScreen();
    }

    private void CheckParentChild_CheckedChanged(object sender, EventArgs e)
    {
        ((CreateFieldWithRelationshipEntityWizardForm)iwizard).ParentChildCheckbox =
            this.checkParentChild.Checked;
    }

    private void TableRelation_SelectedIndexChanged(object sender, EventArgs e)
    {
        CreateFieldWithRelationshipEntityWizardForm relations =
            (CreateFieldWithRelationshipEntityWizardForm)iwizard;
        relations.RelatedEntity = (ISchemaItem)tableRelation.SelectedItem;
        if (this.tableRelation.Name != "")
        {
            this.groupBoxKey.Enabled = true;
            relations.SetUpFormKey(
                BaseEntityField: BaseEntityField,
                RelatedEntityField: RelatedEntityField,
                txtKeyName: txtKeyName
            );
        }
    }

    private void BaseEntityField_SelectedIndexChanged(object sender, EventArgs e)
    {
        CreateFieldWithRelationshipEntityWizardForm relations =
            (CreateFieldWithRelationshipEntityWizardForm)iwizard;
        relations.BaseEntityFieldSelect = (ISchemaItem)BaseEntityField.SelectedItem;
    }

    private void RelatedEntityField_SelectedIndexChanged(object sender, EventArgs e)
    {
        CreateFieldWithRelationshipEntityWizardForm relations =
            (CreateFieldWithRelationshipEntityWizardForm)iwizard;
        relations.RelatedEntityFieldSelect = (ISchemaItem)RelatedEntityField.SelectedItem;
    }

    private void CboEntity1_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshName();
    }

    private void CboEntity2_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshName();
    }

    private void CboField_SelectedIndexChanged(object sender, EventArgs e)
    {
        ForeignKeyForm foreignKey = (ForeignKeyForm)iwizard;
        foreignKey.ForeignEntity = cboEntity.SelectedItem as IDataEntity;
        foreignKey.ForeignField = cboField.SelectedItem as IDataEntityColumn;
        if (foreignKey.ForeignEntity != null && foreignKey.ForeignField != null)
        {
            txtFkFieldName.Text =
                "ref" + foreignKey.ForeignEntity.Name + foreignKey.ForeignField.Name;
        }
    }

    private void CboEntity_SelectedIndexChanged(object sender, EventArgs e)
    {
        ForeignKeyForm foreignKey = (ForeignKeyForm)iwizard;
        foreignKey.ForeignEntity = cboEntity.SelectedItem as IDataEntity;
        cboField.Items.Clear();
        try
        {
            cboField.BeginUpdate();
            if (foreignKey.ForeignEntity != null)
            {
                foreach (IDataEntityColumn column in foreignKey.ForeignEntity.EntityColumns)
                {
                    cboField.Items.Add(item: column);
                }
            }
        }
        finally
        {
            cboField.EndUpdate();
        }
    }

    private void SetPageTitle(object sender)
    {
        if (iwizard.PageTitle != null)
        {
            ((WizardPage)sender).Text = iwizard.PageTitle;
        }
    }

    private void tbDataStructureName_TextChanged(object sender, EventArgs e)
    {
        if (iwizard.IsExistsNameInDataStructure(name: tbDataStructureName.Text))
        {
            this.label1.Text = "Name of Structure already exists.";
        }
        else
        {
            this.label1.Text = "";
        }
    }
}
    #endregion
