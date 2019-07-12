using AeroWizard;
using Origam.UI;
using Origam.UI.Interfaces;
using Origam.UI.WizardForm;
using Origam.Workbench;
using System;
using System.Windows.Forms;

namespace Origam.Schema.EntityModel.Wizards
{
    public partial class Wizard : Form
    {
        SchemaBrowser _schemaBrowser;
        IWizardForm iwizard;

        public Wizard(IWizardForm objectForm)
        {
            InitializeComponent();
            InitData();
            pageStart.Text = "What will do.";
            aerowizard1.Title = objectForm.Description;
            iwizard = objectForm;
        }

        private void InitData()
        {
            lbTitle.Text = "The Wizard will create this elements necesary for the function of a menu:";
            listView1.View = View.List;
            _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
            listView1.SmallImageList = _schemaBrowser.EbrSchemaBrowser.imgList;
            listView1.StateImageList = _schemaBrowser.EbrSchemaBrowser.imgList;
        }


#region Inicialize&Commit
        private void PageStart_Initialize(object sender, WizardPageInitEventArgs e)
        {
            foreach (object[] item in iwizard.listItemType)
            {
                ListViewItem newItem = new ListViewItem((string)item[0]);
                newItem.ImageIndex = _schemaBrowser.ImageIndex((string)item[1]);
                listView1.Items.Add(newItem);
            }
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
        #endregion

        #region support
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
                    return ScreenForm;
            }
            return null;
        }
        #endregion
    }
    public enum PagesList
    {
        startPage,
        DatastructureNamePage,
        ScreenForm,
        finish
    }
}
