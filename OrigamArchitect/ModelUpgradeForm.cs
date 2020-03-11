using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Extensions;

namespace OrigamArchitect
{
    public partial class ModelUpgradeForm : Form
    {
        private readonly IMetaModelUpgradeService metaModelUpgradeService;

        public ModelUpgradeForm(
            IMetaModelUpgradeService metaModelUpgradeService)
        {
            this.metaModelUpgradeService = metaModelUpgradeService;
            metaModelUpgradeService.UpgradeProgress += (sender, info) =>
            {
                void ProgressAction()
                {
                    progressBar.Minimum = 0;
                    progressBar.Maximum = info.TotalFiles;
                    progressBar.Step = 1;
                    progressBar.Value = info.FilesDone;
                    currentFileLabel.Text = $"Files processed: {info.FilesDone} / {info.TotalFiles}";
                }

                this.RunWithInvoke(ProgressAction);
            };
            metaModelUpgradeService.UpgradeFinished += (sender, args) =>
            {
                this.RunWithInvoke(Close);
            };
            InitializeComponent();
        }
        
        private void OnCancelButtonClick(object sender, EventArgs args)
        {
            metaModelUpgradeService.Cancel();
            this.RunWithInvoke(()=> currentFileLabel.Text = "Canceling...");
        }
    }
}
