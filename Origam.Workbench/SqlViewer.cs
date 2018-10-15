using Origam.Workbench.Pads;
using Origam.Gui.UI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using core = Origam.Workbench.Services.CoreServices;
namespace Origam.Workbench
{
    public partial class SqlViewer : AbstractViewContent, IToolStripContainer
    {
        public SqlViewer()
        {
            InitializeComponent();
        }

        public override object Content
        {
            get
            {
                return editor.Text; ;
            }

            set
            {
                editor.Text = value as string;
            }
        }

        public event EventHandler ToolStripsLoaded;
        public event EventHandler AllToolStripsRemoved;

        private void btnExecuteSql_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(editor.Text))
            {
                return;
            }
            string result = core.DataService.ExecuteSql(editor.Text);
            OutputPad outputPad = WorkbenchSingleton.Workbench.GetPad(
                typeof(OutputPad)) as OutputPad;
            outputPad.SetOutputText(result);
            outputPad.Show();
            editor.Focus();
        }

        protected override void ViewSpecificLoad(object objectToLoad)
        {
            this.Content = objectToLoad;
        }

        public override bool IsViewOnly
        {
            get
            {
                return true;
            }

            set
            {
                base.IsViewOnly = value;
            }
        }

        public IEnumerable<ToolStrip> ToolStrips
        {
            get
            {
                return new List<ToolStrip> { toolStrip1 };
            }
        }
    }
}
