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
                    infoLabel.Text = $"Files processed: {info.FilesDone} / {info.TotalFiles}";
                }

                if (progressBar.InvokeRequired)
                {
                    Invoke((Action) ProgressAction);
                }
                else
                {
                    ProgressAction();
                }
            };
            metaModelUpgradeService.UpgradeFinished += (sender, args) =>
            {
                if (InvokeRequired)
                {
                    Invoke((Action) Close);
                }
                else
                {
                    Close();
                }
            };
            InitializeComponent();
        }
        
        private void OnCancelButtonClick(object sender, EventArgs args)
        {
            metaModelUpgradeService.Cancel();
        }
    }
}
