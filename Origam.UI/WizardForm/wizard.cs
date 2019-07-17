using AeroWizard;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.UI.Interfaces;
using Origam.UI.WizardForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Origam.UI.WizardForm

{
    public partial class Wizard : Form
    {
        IWizardForm iwizard;
        public Wizard(IWizardForm objectForm)
        {
            InitializeComponent();
            iwizard = objectForm;
            StartPage.Text = "What will happen...";
            aerowizard1.Title = objectForm.Description;
            InitData();
        }
        private void InitData()
        {
            lbTitle.Text = "The Wizard will create this elements necesary for the function of a menu:";
            listView1.View = View.List;
            listView1.SmallImageList = iwizard.ImageList;
            listView1.StateImageList = iwizard.ImageList;
        }


        #region Inicialize&Commit
        private void PageStart_Initialize(object sender, WizardPageInitEventArgs e)
        {
            iwizard.ListView(listView1);
            GetNextPage(PagesList.startPage, sender);
        }

        private void PageStart_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            IsFinish(sender, e);
        }

        private void StructureNamePage_Initialize(object sender, WizardPageInitEventArgs e)
        {
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
            GetNextPage(PagesList.ScreenForm, sender);
            ScreenWizardForm screenWizard = (ScreenWizardForm)iwizard;
            screenWizard.SetUpForm(lstFields);
            lblRole.Visible = screenWizard.IsRoleVisible;
            txtRole.Visible = screenWizard.IsRoleVisible;
        }

        private void ScreenFormPage_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            IsFinish(sender, e);
        }

        private void LookupFormPage_Initialize(object sender, WizardPageInitEventArgs e)
        {
            LookupForm form = (LookupForm)iwizard;
            form.SetUpForm(cboIdFilter, cboListFilter, cboDisplayField, txtName);
            GetNextPage(PagesList.StructureNamePage, sender);
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
            GetNextPage(PagesList.finish, sender);
            try
            {
                iwizard.Command.Execute();
                foreach(ListViewItem listView in iwizard.ItemTypeList)
                {
                    tbProgres.Text += listView.Text;
                    tbProgres.Text += Environment.NewLine;
                }
                tbProgres.Text += "Done ...";
            }
            catch (Exception ex)
            {
                tbProgres.Text = ex.Message;
                e.Cancel = true;
            }
            this.aerowizard1.FinishButtonText = "Show Result";
        }

        private void FinishPage_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            IsFinish(sender, e);
        }

        private void RelationShipEntityPage_Initialize(object sender, WizardPageInitEventArgs e)
        {
            GetNextPage(PagesList.FieldEntity, sender);
            CreateFieldWithRelationshipEntityWizardForm wizardForm = (CreateFieldWithRelationshipEntityWizardForm)iwizard;
            wizardForm.SetUpForm(tableRelation,txtRelationName);
        }

        private void RelationShipEntityPage_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            CreateFieldWithRelationshipEntityWizardForm wizardForm = (CreateFieldWithRelationshipEntityWizardForm)iwizard;
            wizardForm.LookupName = txtRelationName.Text;

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
            LookupForm form = (LookupForm)iwizard;
            var selectName = (cboDisplayField.SelectedItem as IDataEntityColumn).Name;
            if (selectName != "Name")
            {
                this.txtName.Text = form.Entity.Name + "_" + selectName;
            }
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
                case PagesList.finish:
                    return finishPage;
                case PagesList.FieldEntity:
                    return RelationShipEntityPage;
                case PagesList.ChildEntity:
                    return childEntityPage;
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
            relations.RelatedEntity = (AbstractSchemaItem)tableRelation.SelectedItem;
            if (this.tableRelation.Name != "")
            {
                this.groupBoxKey.Enabled = true;
                relations.SetUpFormKey(BaseEntityField, RelatedEntityField, txtKeyName);
            }
        }

        private void BaseEntityField_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateFieldWithRelationshipEntityWizardForm relations = (CreateFieldWithRelationshipEntityWizardForm)iwizard;
            relations.BaseEntityFieldSelect = (AbstractSchemaItem)BaseEntityField.SelectedItem;
        }

        private void RelatedEntityField_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateFieldWithRelationshipEntityWizardForm relations = (CreateFieldWithRelationshipEntityWizardForm)iwizard;
            relations.RelatedEntityFieldSelect = (AbstractSchemaItem)RelatedEntityField.SelectedItem;
        }

        private void CboEntity1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshName();
        }

        private void CboEntity2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshName();
        }
    }
    #endregion
}
