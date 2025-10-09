#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

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

namespace OrigamArchitect;

public partial class ModelUpgradeForm : Form
{
    private readonly IMetaModelUpgradeService metaModelUpgradeService;

    public ModelUpgradeForm(IMetaModelUpgradeService metaModelUpgradeService)
    {
        this.metaModelUpgradeService = metaModelUpgradeService;
        metaModelUpgradeService.UpgradeProgress += (sender, info) =>
        {
            void ProgressAction()
            {
                if (this.Visible)
                {
                    progressBar.Minimum = 0;
                    progressBar.Maximum = info.TotalFiles;
                    progressBar.Step = 1;
                    progressBar.Value = info.FilesDone;
                    currentFileLabel.Text =
                        $"Files processed: {info.FilesDone} / {info.TotalFiles}";
                }
            }
            this.RunWithInvoke(ProgressAction);
        };
        metaModelUpgradeService.UpgradeFinished += (sender, args) =>
        {
            this.RunWithInvoke(Close);
        };
        InitializeComponent();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        metaModelUpgradeService.Cancel();
        this.RunWithInvoke(() => currentFileLabel.Text = "Canceling...");
    }
}
