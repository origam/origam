using Origam.Workbench;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Origam.Schema.EntityModel.Wizards
{
    public partial class Wizard : Form
    {
        SchemaBrowser _schemaBrowser;
        Stack<PagesList> pages;
        public Wizard()
        {
            InitializeComponent();
            lbTitle.Text = "The Wizard will create this elements necesary for the function of a menu:";
            listView1.View = View.List;
            _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
            listView1.SmallImageList = _schemaBrowser.EbrSchemaBrowser.imgList;
            listView1.StateImageList = _schemaBrowser.EbrSchemaBrowser.imgList;
        }

        private void BtClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void Label(string title)
        {
            lbTitle.Text = title;
        }

        public void ShowObjcts(ArrayList list)
        {
            int ii = 0;
            foreach (object[] item in list)
            {
                ListViewItem newItem = new ListViewItem((string)item[0] );
                newItem.ImageIndex = _schemaBrowser.ImageIndex((string)item[1]);
                listView1.Items.Add(newItem);
                ii++;
            }
        }

        private void BtOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        public void ListOfImages(ImageList imgList)
        {
            listView1.View = View.SmallIcon;
            listView1.LargeImageList = imgList;
            listView1.SmallImageList = imgList;
            listView1.StateImageList = imgList;
        }

        public void SetDescription(string description)
        {
            this.pageStart.Text = description;
        }

        internal void ShowPages(Stack<PagesList> stackPages)
        {
            pages = stackPages;
        }
    }
    public enum PagesList
    {
        startPage,
        finish
    }
}
