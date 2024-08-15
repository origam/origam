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

import { IDataView } from "model/entities/types/IDataView";
import { getProperties } from "model/selectors/DataView/getProperties";
import { getConfigurationManager } from "model/selectors/TablePanelView/getConfigurationManager";
import { getApi } from "model/selectors/getApi";
import { getSessionId } from "model/selectors/getSessionId";
import { getActivePanelView } from "model/selectors/DataView/getActivePanelView";
import { runGeneratorInFlowWithHandler, runInFlowWithHandler, wrapInFlowWithHandler } from "utils/runInFlowWithHandler";
import { askYesNoQuestion } from "gui/Components/Dialog/DialogUtils";
import { getWorkbenchLifecycle } from "model/selectors/getWorkbenchLifecycle";
import React from "react";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { MobileSimpleDropdown } from "gui/connections/MobileComponents/Form/MobileSimpleDropdown";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { ISectionPluginData } from "plugins/interfaces/ISectionPluginData";
import { IScreenPluginData } from "plugins/interfaces/IScreenPluginData";
import { IOption, SimpleDropdown } from "gui/Components/Dialogs/SimpleDropdown";
import { IPluginProperty } from "plugins/interfaces/IPluginProperty";
import { IPluginTableRow } from "plugins/interfaces/IPluginTableRow";
import { IPluginDataView } from "plugins/interfaces/IPluginDataView";
import { IGuiHelper } from "plugins/interfaces/IGuiHelper";
import {
  getFilterGroupManager
} from "model/selectors/DataView/getFilterGroupManager";


export function createSectionPluginData(dataView: IDataView): ISectionPluginData | undefined {
  if (!dataView) {
    return undefined;
  }
  return {
    dataView: new PluginDataView(dataView),
    guiHelper: new GuiHelper(dataView)
  }
}

export function createScreenPluginData(formScreen: IFormScreen): IScreenPluginData | undefined  {
  if (!formScreen) {
    return undefined;
  }
  return {
    dataViews: formScreen.dataViews.map(dataView => new PluginDataView(dataView)),
    guiHelper: new GuiHelper(formScreen)
  }
}

class GuiHelper implements IGuiHelper {

  constructor(private ctx: any) {
  }

  askYesNoQuestion(title: string, question: string){
    return askYesNoQuestion(this.ctx, title, question);
  }

  runGeneratorInFlowWithHandler(generator: Generator): Promise<void> {
    return runGeneratorInFlowWithHandler({ctx: this.ctx, generator: generator});
  }

  runInFlowWithHandler(action: (() => Promise<any>) | (() => void)) {
    return runInFlowWithHandler({ctx: this.ctx, action: action});
  }

  wrapInFlowWithHandler(action: (() => Promise<any>) | (() => void)) {
    return wrapInFlowWithHandler({ctx:this.ctx, action: action});
  }

  isMobileLayoutActive(){
    return isMobileLayoutActive(this.ctx);
  }

  openMenuItem(args: { itemId: any; idParameter?: string }): Promise<void> {
   let workbenchLifecycle = getWorkbenchLifecycle(this.ctx);
   return runGeneratorInFlowWithHandler({
     ctx: this.ctx,
     generator: function*() {
       yield*workbenchLifecycle.onMainMenuItemIdClick({
         event: undefined,
         itemId: args.itemId,
         idParameter: args.idParameter,
         isSingleRecordEdit: false
       });
     }()
   });
  }

  renderDropDown<T>(args: {
    options: IOption<T>[],
    selectedOption: IOption<T>,
    onOptionClick: (option: IOption<T>) => void,
    width?: string,
    className?: string
  })
  {
    if(isMobileLayoutActive(this.ctx)){
      return (
        <MobileSimpleDropdown
          options={args.options}
          selectedOption={args.selectedOption}
          onOptionClick={args.onOptionClick}
          width={args.width}
          className={args.className}
          ctx={this.ctx}/>
      );
    }else{
      return (
        <SimpleDropdown
          options={args.options}
          selectedOption={args.selectedOption}
          onOptionClick={args.onOptionClick}
          width={args.width}
          className={args.className}
      />
    );
    }
  }
}

class PluginDataView implements IPluginDataView {
  properties: IPluginProperty[];
  entity: string;

  get tableRows(): IPluginTableRow[] {
    return this.dataView.tableRows;
  }

  constructor(
    private dataView: IDataView,
  ) {
    this.properties = getProperties(this.dataView)
      .map(property => {
        return{
          id: property.id,
          name: property.name,
          type: property.column,
          momentFormatterPattern: property.formatterPattern
        }
      });
    this.entity = this.dataView.entity;
  }

  async saveConfiguration(pluginName: string, configuration: string): Promise<void> {
    const configurationManager = getConfigurationManager(this.dataView);
    const filterGroupManager = getFilterGroupManager(this.dataView);
    configurationManager.setCustomConfiguration(pluginName, configuration);
    const customConfigurations: {[key:string]: string} = {};
    customConfigurations[pluginName] = configuration;

    await getApi(this.dataView).saveObjectConfiguration({
      sessionFormIdentifier: getSessionId(this.dataView),
      instanceId: this.dataView.modelInstanceId,
      tableConfigurations: configurationManager.allTableConfigurations,
      customConfigurations: customConfigurations,
      alwaysShowFilters: filterGroupManager.alwaysShowFilters,
      defaultView: getActivePanelView(this.dataView),
    });
  }

  getConfiguration(pluginName: string){
    const configurationManager = getConfigurationManager(this.dataView);
    return configurationManager.getCustomConfiguration(pluginName)
  }

  getCellText(row: any[], propertyId: string): any {
    const property = getProperties(this.dataView).find(prop => prop.id === propertyId);
    if (!property) {
      throw new Error("Property named \"" + propertyId + "\" was not found");
    }
    return this.dataView.dataTable.getCellText(row, property);
  }

  getCellValue(row: any[], propertyId: string): any {
    const property = getProperties(this.dataView).find(prop => prop.id === propertyId);
    if (!property) {
      throw new Error("Property named \"" + propertyId + "\" was not found");
    }
    return this.dataView.dataTable.getCellValue(row, property);
  }

  getRowId(row: IPluginTableRow) {
    return Array.isArray(row)
      ? this.dataView.dataTable.getRowId(row)
      : row.columnLabel + row.columnValue + row.groupLevel + row.isExpanded;
  }
}

