using AeroWizard;
using Origam.UI;
using Origam.UI.Interfaces;
using Origam.UI.WizardForm;
using Origam.Workbench;
using System.Windows.Forms;

namespace Origam.Schema.EntityModel.Wizards
{
    public partial class Wizard : Form
    {
        SchemaBrowser _schemaBrowser;
        IWizardForm iwizard;

        public Wizard()
        {
            InitializeComponent();
            InitData();
        }

        private void InitData()
        {
            lbTitle.Text = "The Wizard will create this elements necesary for the function of a menu:";
            listView1.View = View.List;
            _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
            listView1.SmallImageList = _schemaBrowser.EbrSchemaBrowser.imgList;
            listView1.StateImageList = _schemaBrowser.EbrSchemaBrowser.imgList;
        }

        public Wizard(IWizardForm objectForm)
        {
            InitializeComponent();
            InitData();
            pageStart.Text = objectForm.Description;
            iwizard = objectForm;
        }

        private void PageStart_Initialize(object sender, WizardPageInitEventArgs e)
        {
            foreach (object[] item in iwizard.listItemType)
            {
                ListViewItem newItem = new ListViewItem((string)item[0]);
                newItem.ImageIndex = _schemaBrowser.ImageIndex((string)item[1]);
                listView1.Items.Add(newItem);
            }
            GetNextPage(PagesList.startPage, pageStart);
        }

        private void PageStart_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            if(pageStart.IsFinishPage) DialogResult = DialogResult.OK;
        }

        private void DataStructureNamePage_Initialize(object sender, WizardPageInitEventArgs e)
        {
            tbDataStructureName.Text = ((DataStructureForm)iwizard).NameOfEntity;
        }

        private void DataStructureNamePage_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            DataStructureForm structureform = (DataStructureForm)iwizard;
            if (structureform.IsExistsName(tbDataStructureName.Text))
            {
                AsMessageBox.ShowError(this, "The Name already Exists!", "Name Exists", null);
                e.Cancel = true;
                return;
            }
            structureform.NameOfEntity = tbDataStructureName.Text;
        }

        private WizardPage getWizardPage(PagesList nextPage)
        {
            switch (nextPage)
            {
                case PagesList.DatastructureNamePage:
                    return DataStructureNamePage;
            }
            return null;
        }

        private void GetNextPage(PagesList actualPage, WizardPage wizardPage)
        {
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
    }
    public enum PagesList
    {
        startPage,
        DatastructureNamePage,
        finish
    }
}
