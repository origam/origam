using AeroWizard;
using Origam.Schema.EntityModel;
using Origam.UI;
using Origam.UI.Interfaces;
using Origam.UI.WizardForm;
using System;
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
            listView1.SmallImageList = iwizard.imgList;
            listView1.StateImageList = iwizard.imgList;
        }


#region Inicialize&Commit
        private void PageStart_Initialize(object sender, WizardPageInitEventArgs e)
        {
            iwizard.ListView(listView1);
            GetNextPage(PagesList.startPage, sender);
        }

        private void PageStart_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            IsFinish(sender,e);
        }

        private void DataStructureNamePage_Initialize(object sender, WizardPageInitEventArgs e)
        {
            tbDataStructureName.Text = iwizard.NameOfEntity;
            GetNextPage(PagesList.DatastructureNamePage, sender);
        }

        private void DataStructureNamePage_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            if (iwizard.IsExistsNameInDataStructure(tbDataStructureName.Text))
            {
                AsMessageBox.ShowError(this, "The Name already Exists!", "Name Exists", null);
                e.Cancel = true;
                return;
            }
            iwizard.NameOfEntity = tbDataStructureName.Text;
            IsFinish(sender,e);
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
            form.SetUpForm(cboIdFilter, cboListFilter,cboDisplayField, txtName);
            GetNextPage(PagesList.DatastructureNamePage, sender);
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

        #endregion

        #region support

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
                case PagesList.DatastructureNamePage:
                    return DataStructureNamePage;
                case PagesList.ScreenForm:
                    return ScreenFormPage;
                case PagesList.LookupForm:
                    return LookupFormPage;
            }
            return null;
        }

        
    }
    #endregion
}
