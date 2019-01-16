using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Origam.DA.ObjectPersistence;
using Origam.Workbench.Pads;

namespace OrigamArchitect
{
    public partial class RuleWindow : Form
    {
        internal static FindRulesPad resultsPad;
        internal static List<Dictionary<IFilePersistent, string>> listKeys;
        public RuleWindow()
        {
            InitializeComponent();
        }

        internal static DialogResult ShowData(frmMain parent, string message, string title, FindRulesPad results, List<Dictionary<IFilePersistent, string>> lKeys)
        {
            resultsPad = results;
            listKeys = lKeys;
            var messageBox = new RuleWindow()
            {
                Text = title,
                Visible = false
            };
            messageBox.label1.Text = message;
            return messageBox.ShowDialog(parent);
        }

        private void No_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
            resultsPad.DisplayResults(listKeys);
            //resultsPad.Show();
           // resultsPad.TopMost = true;
        }
    }
}
