#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Origam.DA;
using Origam.Rule;
using Origam.Schema.GuiModel;
using Origam.Schema.GuiModel.Designer;
using Origam.Gui.UI;
using Origam.Service.Core;

namespace Origam.Gui.Win
{
    public class ActionButtonManager : IDisposable
    {

        private readonly Func<CurrencyManager> bindingManagerGetter;
        private readonly Func<Guid> parentIdGetter;
        private readonly Func<DataSet> dataSourceGetter;
        private readonly Func<Guid> FormPanelIdGetter;
        private readonly Func<string> dataMemberGetter;
        private readonly Func<FormGenerator> formGeneratorGetter;
        private readonly Func<Guid> formIdGetter;
        private readonly Func<ToolStrip> toolStripGetter;
        private IList<ToolStripItem> actionButtons;
        private ToolStripItem dafaultButton;

        public IList<ToolStripItem> ActionButtons
        {
            set
            {
                actionButtons = value;
                dafaultButton = (ToolStripItem) actionButtons
                    .Where(item => item is IActionContainer)
                    .Cast<IActionContainer>()
                    .FirstOrDefault(container => container.GetAction().IsDefault);
            }
        }


        public ActionButtonManager(Func<CurrencyManager> bindingManagerGetter,
            Func<Guid> parentIdGetter, Func<DataSet> dataSourceGetter, 
            Func<Guid> formPanelIdGetter, Func<string> dataMemberGetter, 
            Func<ToolStrip> toolStripGetter,Func<FormGenerator> formGeneratorGetter,
            Func<Guid> formIdGetter)
        {
            this.bindingManagerGetter = bindingManagerGetter;
            this.parentIdGetter = parentIdGetter;
            this.dataSourceGetter = dataSourceGetter;
            FormPanelIdGetter = formPanelIdGetter;
            this.dataMemberGetter = dataMemberGetter;
            this.toolStripGetter = toolStripGetter;
            this.formGeneratorGetter = formGeneratorGetter;
            this.formIdGetter = formIdGetter;
        }
        
        public void RunDefaultAction()
        {
            dafaultButton?.PerformClick();
        }  
        
        public void UpdateActionButtons()
        {
            if (actionButtons == null) return;

            var disabledActionIds = GetDisabledActionIds();
            UpdateToolStripItemVisibility(disabledActionIds);

            var toolStrip = toolStripGetter.Invoke();
            bool toolStripShouldBeShown = toolStrip.Items
                .Cast<ToolStripItem>()
                .Any(item => item.Enabled);

            toolStrip.Enabled = toolStripShouldBeShown;
            toolStrip.Visible = toolStripShouldBeShown;
        }

        public void BindActionButtons()
        {
            if (actionButtons == null) return;

            foreach (var actionButton in actionButtons)
            {
                if (actionButton is ToolStripActionDropDownButton dropDownbutton)
                {
                    dropDownbutton.ToolStripMenuItems.ForEach(item =>
                        item.Click += actionButton_Click);
                } 
                else
                {
                    actionButton.Click += actionButton_Click;
                }
            }
        }

        public void Dispose()
        {
            if (actionButtons == null) return;

            foreach (var actionButton in actionButtons)
            {
                if (actionButton is ToolStripActionDropDownButton dropDownbutton
                )
                {
                    dropDownbutton.ToolStripMenuItems.ForEach(item =>
                        item.Click -= actionButton_Click);

                } else
                {
                    actionButton.Click -= actionButton_Click;
                }
            }
        }

        private IList<string> GetDisabledActionIds()
        {
            var currencyManager = bindingManagerGetter.Invoke();
            Guid entityId = parentIdGetter.Invoke();
            RuleEngine ruleEngine = formGeneratorGetter.Invoke().FormRuleEngine;
            if (ruleEngine == null) return new List<string>();

            if (entityId == Guid.Empty) return new List<string>();

            bool noDataToDisplay = currencyManager.Position == -1;
            if (noDataToDisplay)
            {
                return ruleEngine
                    .GetDisabledActions(null, null, entityId, formIdGetter())
                    .Cast<string>()
                    .ToList();
            }

            DataRow row = (currencyManager.Current as DataRowView).Row;

            if (!DatasetTools.HasRowValidParent(row)) return new List<string>();

            XmlContainer originalData =
                DatasetTools.GetRowXml(row, DataRowVersion.Original);
            XmlContainer actualData = DatasetTools.GetRowXml(row,
                row.HasVersion(DataRowVersion.Proposed)
                    ? DataRowVersion.Proposed
                    : DataRowVersion.Default);

            return ruleEngine
                .GetDisabledActions(originalData, actualData, entityId, formIdGetter())
                .Cast<string>()
                .ToList();
        }

        private void UpdateToolStripItemVisibility(
            IList<string> disabledActionIds)
        {
            foreach (var actionButton in actionButtons)
            {
                if (actionButton is ToolStripActionDropDownButton dropDownbutton
                )
                {
                    dropDownbutton.ToolStripMenuItems.ForEach(item =>
                        UpdateEnabledState(disabledActionIds, item));

                    var showDropDown
                        = dropDownbutton.ToolStripMenuItems.Any(item =>
                            item.Enabled);
                    dropDownbutton.Visible = showDropDown;
                    dropDownbutton.Enabled = showDropDown;
                } else
                {
                    UpdateEnabledState(disabledActionIds, actionButton);
                }
            }
        }

        private static void UpdateEnabledState(IList<string> disabledActionIds,
            ToolStripItem actionItem)
        {
            var showButton = !disabledActionIds.Contains(
                ((IActionContainer) actionItem).GetAction().Id.ToString());
            actionItem.Enabled = showButton;
            actionItem.Visible = showButton;
        }

        private void actionButton_Click(object sender, EventArgs e)
        {
            var actionButton = sender as IActionContainer;
            var desktopEntityUiActionRunnerClient
                = new DesktopEntityUIActionRunnerClient(
                    formGeneratorGetter.Invoke(),
                    dataSourceGetter.Invoke());

            var actionRunner
                = new DesktopEntityUIActionRunner(
                    desktopEntityUiActionRunnerClient);

            // parameter mappings and input parameters are used only in Origam Online
            actionRunner.ExecuteAction(
                sessionFormIdentifier: null,
                requestingGrid: FormPanelIdGetter.Invoke().ToString(),
                entity: dataMemberGetter.Invoke(),
                actionType: actionButton.GetAction().ActionType.ToString(),
                actionId: actionButton.GetAction().Id.ToString(),
                parameterMappings: actionButton.GetAction().ParameterMappings,
                selectedIds: GetSelectedItemsForAction(),
                inputParameters: new Hashtable());
        }

        private List<string> GetSelectedItemsForAction()
        {
            var currencyManager = bindingManagerGetter.Invoke();
            var selectedItems = new List<string>();
            if ((currencyManager.Current is DataRowView dataRowView)
                && (dataRowView.Row.Table
                    .PrimaryKey
                    .Length > 0))
            {
                selectedItems.Add(dataRowView
                    .Row[dataRowView.Row.Table.PrimaryKey[0]].ToString());
            }

            return selectedItems;
        }

    }
}