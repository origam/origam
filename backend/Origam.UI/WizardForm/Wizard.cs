using AeroWizard;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Origam.UI.WizardForm;
public partial class Wizard : Form
{
    IWizardForm iwizard;
    public Wizard(IWizardForm objectForm)
    {
        InitializeComponent();
        iwizard = objectForm;
        StartPage.Text = "What will happen...";
        if(iwizard.Title!=null) aerowizard1.Title =  iwizard.Title;
        InitData();
    }
    private void InitData()
    {
        lbTitle.Text = "The Wizard will create following elements necessary for the function of the menu:";
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
        txtLabel.Size = TextRenderer.MeasureText(iwizard.Description, txtLabel.Font);
        iwizard.ListView(listView1);
        GetNextPage(PagesList.StartPage, sender);
    }
    private void PageStart_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        IsFinish(sender, e);
    }
    private void StructureNamePage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        tbDataStructureName.Text = iwizard.NameOfEntity;
        GetNextPage(PagesList.StructureNamePage, sender);
    }
       private void StructureNamePage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        if (iwizard.IsExistsNameInDataStructure(tbDataStructureName.Text))
        {
            AsMessageBox.ShowError(this, "The Name already Exists!", "Name Exists", null);
            e.Cancel = true;
            return;
        }
        iwizard.NameOfEntity = tbDataStructureName.Text;
        IsFinish(sender, e);
    }
    private void ScreenFormPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        GetNextPage(PagesList.ScreenForm, sender);
        ScreenWizardForm screenWizard = (ScreenWizardForm)iwizard;
        screenWizard.SetUpForm(lstFields);
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
        IsFinish(sender, e);
    }
    private void LookupFormPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        LookupForm form = (LookupForm)iwizard;
        form.SetUpForm(cboIdFilter, cboListFilter, cboDisplayField, txtName);
        txtName.Text = form.LookupName;
        cboDisplayField.SelectedItem = form.NameColumn;
        cboIdFilter.SelectedItem = form.IdFilter;
        cboListFilter.SelectedItem = form.ListFilter;
        GetNextPage(PagesList.LookupForm, sender);
    }
    private void LookupFormPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        LookupForm form = (LookupForm)iwizard;
        form.LookupName = txtName.Text;
        form.NameColumn = cboDisplayField.SelectedItem as IDataEntityColumn;
        form.IdFilter = cboIdFilter.SelectedItem as EntityFilter;
        form.ListFilter = cboListFilter.SelectedItem as EntityFilter;
        if (form.LookupName == ""
            | form.Entity == null
            | form.NameColumn == null
            | form.IdColumn == null
            | form.IdFilter == null)
        {
            MessageBox.Show(ResourceUtils.GetString("EnterAllInfo"), ResourceUtils.GetString("LookupWiz"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            e.Cancel = true;
            return;
        }
        IsFinish(sender, e);
    }
    private void FieldLookupEntityPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        CreateFieldWithLookupEntityWizardForm form = (CreateFieldWithLookupEntityWizardForm)iwizard;
        grdInitialValues.AutoGenerateColumns = false;
        grdInitialValues.DataSource = form.InitialValues;
        txtNameFieldName.Text = form.NameFieldName;
        txtNameFieldCaption.Text = form.NameFieldCaption;
        txtKeyFieldName.Text = form.KeyFieldName;
        txtKeyFieldCaption.Text= form.KeyFieldCaption;
        UpdateScreen();
        if (form.ForceTwoColumns)
        {
            chkTwoColumn.Checked = true;
            chkTwoColumn.Visible = false;
            chkAllowNulls.Visible = false;
            lblCaption.Visible = false;
            txtCaption.Visible = false;
        }
        GetNextPage(PagesList.FieldLookup, sender);
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
        if (form.LookupName == ""
            || (txtCaption.Visible && form.LookupCaption == ""))
        {
            MessageBox.Show(form.EnterAllInfo,
                form.LookupWiz, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            e.Cancel = true;
            return;
        }
        if (!form.AllowNulls && form.InitialValues.Count > 0 && form.DefaultInitialValue == null)
        {
            if (MessageBox.Show(form.DefaultValueNotSet,
                form.LookupWiz, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
                == DialogResult.Cancel)
            {
                e.Cancel=true;
                return;
            }
        }
        IsFinish(sender, e);
    }
    private void FinishPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        GetNextPage(PagesList.Finish, sender);
        ISchemaItem[] results = new ISchemaItem[0] ;
        try
        {
            iwizard.Command.Execute();
            results = ((AbstractMenuCommand)iwizard.Command).GeneratedModelElements.ToArray();
        }
        catch (Exception ex)
        {
            ListViewItem newItem = new ListViewItem(new string[] { "error", ex.Message });
            progresslistview.Items.Add(newItem);
            e.Cancel = true;
        }
        for (int i = 0; i < results.LongLength; i++)
        {
            ISchemaItem item = results[i];
            ListViewItem newItem = new ListViewItem(new string[] { item.Path, item.ModelDescription() })
            {
                ImageIndex = iwizard.Command.GetImageIndex(item.RootItem.Icon)
            };
            progresslistview.Items.Add(newItem);
        }
        this.aerowizard1.FinishButtonText = "Show Result";
        this.aerowizard1.CancelButtonText = "Close";
    }
 
    private void FinishPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        IsFinish(sender, e);
    }
    private void RelationShipEntityPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        GetNextPage(PagesList.FieldEntity, sender);
        CreateFieldWithRelationshipEntityWizardForm wizardForm = (CreateFieldWithRelationshipEntityWizardForm)iwizard;
        wizardForm.SetUpForm(tableRelation,txtRelationName);
    }
    private void RelationShipEntityPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        CreateFieldWithRelationshipEntityWizardForm wizardForm = (CreateFieldWithRelationshipEntityWizardForm)iwizard;
        wizardForm.LookupName = txtRelationName.Text;
        wizardForm.LookupKeyName = txtKeyName.Text;
        if (wizardForm.LookupName == ""
            || wizardForm.RelatedEntity == null
            || wizardForm.BaseEntityFieldSelect == null
            || wizardForm.RelatedEntityFieldSelect == null
            || txtKeyName.Text == "")
        {
            MessageBox.Show(wizardForm.EnterAllInfo,
                wizardForm.LookupWiz, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            e.Cancel = true;
            return;
        }
        IsFinish(sender, e);
    }
    private void ChildEntityPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        GetNextPage(PagesList.ChildEntity, sender);
        ChildEntityForm EntityForm = (ChildEntityForm)iwizard;
        EntityForm.EntityName = txtchildEntityName.Text;
        EntityForm.SetUpForm(txtchildEntityName, cboEntity1, cboEntity2);
    }
    private void ChildEntityPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        ChildEntityForm EntityForm = (ChildEntityForm)iwizard;
        EntityForm.Entity2 = cboEntity2.SelectedItem as IDataEntity;
        EntityForm.EntityName = txtchildEntityName.Text;
        if (txtchildEntityName.Text == ""
            | EntityForm.Entity1 == null
            )
        {
            MessageBox.Show(EntityForm.EnterAllInfo, EntityForm.ChildEntityWiz, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            e.Cancel = true;
            return;
        }
        IsFinish(sender, e);
    }
    private void ForeignKeyPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        GetNextPage(PagesList.ForeignForm, sender);
        ForeignKeyForm foreignKey = (ForeignKeyForm)iwizard;
        foreignKey.SetUpForm(txtFkFieldName, cboEntity, cboLookup, cboField, chkAllowNulls);
    }
    private void ForeignKeyPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        ForeignKeyForm foreignKey= (ForeignKeyForm)iwizard;
        foreignKey.ForeignKeyName = txtFkFieldName.Text;
        foreignKey.Caption = txtfkCaptionName.Text;
        foreignKey.ForeignEntity = cboEntity.SelectedItem as IDataEntity;
        foreignKey.ForeignField = cboField.SelectedItem as IDataEntityColumn;
        foreignKey.Lookup = cboLookup.SelectedItem as IDataLookup;
        foreignKey.AllowNulls = checkBoxAllowNulls.Checked;
        if (foreignKey.ForeignEntity == null)
        {
            MessageBox.Show(foreignKey.SelectForeignEntity, foreignKey.ForeignKeyWiz, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            e.Cancel = true;
            return;
        }
        if (foreignKey.ForeignField == null)
        {
            MessageBox.Show(foreignKey.SelectForeignField, foreignKey.ForeignKeyWiz, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            e.Cancel = true;
            return;
        }
        if (txtFkFieldName.Text == "")
        {
            MessageBox.Show(foreignKey.EnterKeyName, foreignKey.ForeignKeyWiz, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            e.Cancel = true;
            return;
        }
        IsFinish(sender, e);
    }
    private void MenuFromPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        MenuFromForm menufrom = (MenuFromForm)iwizard;
        txtMenuRole.Text = menufrom.Role;
        GetNextPage(PagesList.MenuPage, sender);
    }
    private void MenuFromPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        MenuFromForm menufrom = (MenuFromForm)iwizard;
        menufrom.Role = string.IsNullOrEmpty(txtMenuRole.Text)?"*": txtMenuRole.Text;
        menufrom.Caption = txtMenuCaption.Text;
        IsFinish(sender, e);
    }
    private void SummaryPage_Initialize(object sender, WizardPageInitEventArgs e)
    {
        SetPageTitle(sender);
        iwizard.Command.SetSummaryText(richTextBoxSummary);
        richTextBoxSummary.BackColor = Color.White;
        GetNextPage(PagesList.SummaryPage, sender);
        this.aerowizard1.NextButtonText = "Start";
    }
    private void SummaryPage_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        IsFinish(sender, e);
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
        lblKeyFieldName.Visible = lblKeyFieldCaption.Visible
            = txtKeyFieldName.Visible = txtKeyFieldCaption.Visible
            = chkTwoColumn.Checked;
        grdInitialValues.Columns.Clear();
        if (chkTwoColumn.Checked)
        {
            grdInitialValues.Columns.AddRange(new DataGridViewColumn[] {
                colDefault,
                colCode,
                colName
                });
            colCode.DisplayIndex = 0;
            colName.DisplayIndex = 1;
            colDefault.DisplayIndex = 2;
        }
        else
        {
            grdInitialValues.Columns.AddRange(new DataGridViewColumn[] {
                colDefault,
                colName
                });
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
        var fieldSegment 
            = (cboDisplayField.SelectedItem as IDataEntityColumn)?.Name ?? "";
        var filterSegment 
            = (cboIdFilter.SelectedItem as ISchemaItem)?.Name ?? "";
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
                wizardPage.NextPage = getWizardPage(pglist);
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
                return StructureNamePage;
            case PagesList.ScreenForm:
                return ScreenFormPage;
            case PagesList.LookupForm:
                return LookupFormPage;
            case PagesList.FieldLookup:
                return FieldLookupEntity;
            case PagesList.Finish:
                return finishPage;
            case PagesList.FieldEntity:
                return RelationShipEntityPage;
            case PagesList.ChildEntity:
                return childEntityPage;
            case PagesList.ForeignForm:
                return foreignKeyPage;
            case PagesList.SummaryPage:
                return SummaryPage;
            case PagesList.MenuPage:
                return menuFromPage;
            default:
                MessageBox.Show("Not Set WizardPage","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                break;
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
        ((CreateFieldWithRelationshipEntityWizardForm)iwizard)
            .ParentChildCheckbox = this.checkParentChild.Checked;
    }
    private void TableRelation_SelectedIndexChanged(object sender, EventArgs e)
    {
        CreateFieldWithRelationshipEntityWizardForm relations = (CreateFieldWithRelationshipEntityWizardForm)iwizard;
        relations.RelatedEntity = (ISchemaItem)tableRelation.SelectedItem;
        if (this.tableRelation.Name != "")
        {
            this.groupBoxKey.Enabled = true;
            relations.SetUpFormKey(BaseEntityField, RelatedEntityField, txtKeyName);
        }
    }
    private void BaseEntityField_SelectedIndexChanged(object sender, EventArgs e)
    {
        CreateFieldWithRelationshipEntityWizardForm relations = (CreateFieldWithRelationshipEntityWizardForm)iwizard;
        relations.BaseEntityFieldSelect = (ISchemaItem)BaseEntityField.SelectedItem;
    }
    private void RelatedEntityField_SelectedIndexChanged(object sender, EventArgs e)
    {
        CreateFieldWithRelationshipEntityWizardForm relations = (CreateFieldWithRelationshipEntityWizardForm)iwizard;
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
            txtFkFieldName.Text = "ref" + foreignKey.ForeignEntity.Name + foreignKey.ForeignField.Name;
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
                    cboField.Items.Add(column);
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
        if(iwizard.PageTitle!=null)
            ((WizardPage)sender).Text = iwizard.PageTitle;
    }
    private void tbDataStructureName_TextChanged(object sender, EventArgs e)
    {
        if (iwizard.IsExistsNameInDataStructure(tbDataStructureName.Text))
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
