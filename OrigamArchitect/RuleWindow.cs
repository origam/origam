using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Origam.Workbench.Pads;

namespace OrigamArchitect
{
    public partial class RuleWindow : Form
    {
        internal static FindRulesPad resultsPad;
        internal static List<Origam.Schema.AbstractSchemaItem> listKeys;
        public RuleWindow()
        {
            InitializeComponent();
        }

        internal static DialogResult ShowData(frmMain parent, string errorMessage, string title, FindRulesPad results, List<Origam.Schema.AbstractSchemaItem> lKeys)
        {
            resultsPad = results;
            listKeys = lKeys;
            var messageBox = new RuleWindow()
            {
                Text = title,
                Visible = false
            };
            messageBox.text.Text = errorMessage;
            return messageBox.ShowDialog(parent);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void showResultButton_Click(object sender, EventArgs e)
        {
            this.Close();
            resultsPad.DisplayResults(listKeys.ToArray());
            resultsPad.Show();
            resultsPad.TopMost = true;
        }
    }
}
