import {IFilter} from "model/entities/types/IFilter";
import {FilterSetting} from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSetting";
import {filterTypeFromNumber} from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operatots";
import {IPanelConfiguration} from "model/entities/types/IPanelConfiguration";
import {IProperty} from "model/entities/types/IProperty";


export function addFiltersToPanelConfiguration(panelConfigurations: Map<string, IPanelConfiguration>, properties: IProperty[], panelConfigurationJson: any){
    const initialFilter = panelConfigurationJson.initialFilter;
    if(initialFilter){
      const filters: IFilter[] = initialFilter.details
        .map((detail: any) => {
          const property = properties.find(prop => prop.id === detail.property)!;
          return {
            propertyId: detail.property,
            dataType: property.column,
            setting: new FilterSetting(
              filterTypeFromNumber(detail.operator),
              true,
              detail.value1,
              detail.value2)
          }});
      const filterGroup = {
        filters: filters,
        id: initialFilter.id,
        isGlobal: initialFilter.isGlobal,
        name: initialFilter.name
      };
      const panelConfiguration = panelConfigurations
        .get(panelConfigurationJson.panel.instanceId);
      if(panelConfiguration){
        panelConfiguration.defaultFilter = filterGroup;
      }
    }
}