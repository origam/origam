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

import { getApi } from "model/selectors/getApi";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getActivePanelView } from "model/selectors/DataView/getActivePanelView";
import { getSessionId } from "model/selectors/getSessionId";
import { getProperties } from "model/selectors/DataView/getProperties";
import { getConfigurationManager } from "model/selectors/TablePanelView/getConfigurationManager";
import { runGeneratorInFlowWithHandler } from "utils/runInFlowWithHandler";

export function saveColumnConfigurations(ctx: any) {
  return function*saveColumnConfigurations() {
    const dataView = getDataView(ctx);
    const configurationManager = getConfigurationManager(ctx);
    const tablePanelView = getTablePanelView(ctx);

    if (configurationManager.allTableConfigurations.length === 0) {
      return;
    }

    const activeTableConfiguration = configurationManager.activeTableConfiguration;
    for (const property of getProperties(ctx).slice(1)) {
      activeTableConfiguration.updateColumnWidth(property.id, property.columnWidth);
    }
    activeTableConfiguration.sortColumnConfigurations(tablePanelView.tablePropertyIds);

    yield getApi(ctx).saveObjectConfiguration({
      sessionFormIdentifier: getSessionId(ctx),
      instanceId: dataView.modelInstanceId,
      tableConfigurations: configurationManager.allTableConfigurations,
      alwaysShowFilters: configurationManager.alwaysShowFilters,
      defaultView: getActivePanelView(ctx),
    });
  };
}

export async function saveColumnConfigurationsAsync(ctx: any) {
  await runGeneratorInFlowWithHandler({
    ctx: ctx,
    generator: function*() {
      yield*saveColumnConfigurations(ctx)();
    }()
  });
}
