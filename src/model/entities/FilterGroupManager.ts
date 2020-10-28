import { IFilterGroup } from "model/entities/types/IFilterGroup";
import { IFilterConfiguration } from "model/entities/types/IFilterConfiguration";
import { action } from "mobx";
import { IFilter } from "model/entities/types/IFilter";
import {
  IUIGridFilterCoreConfiguration,
  IUIGridFilterFieldConfiguration,
} from "model/entities/types/IApi";
import { filterTypeToNumber } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operatots";
import { getApi } from "model/selectors/getApi";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getDataView } from "model/selectors/DataView/getDataView";

export class FilterGroupManager {
  ctx: any;
  filterGroups: IFilterGroup[] = [];
  private _defaultFilter: IFilterGroup | undefined;

  constructor(private filterConfiguration: IFilterConfiguration) {
    this.ctx = filterConfiguration;
  }

  get defaultFilter(): IFilterGroup | undefined {
    return this._defaultFilter;
  }

  set defaultFilter(value: IFilterGroup | undefined) {
    this._defaultFilter = value;
    this.setFilterGroup(this._defaultFilter);
  }

  get activeFilters() {
    return this.filterConfiguration.activeFilters;
  }

  @action.bound
  setFilterGroup(filterGroup: IFilterGroup | undefined) {
    this.filterConfiguration.clearFilters();
    if (filterGroup?.filters) {
      this.filterConfiguration.setFilters(filterGroup.filters);
    }
  }

  filtreToServerVersion(filter: IFilter): IUIGridFilterFieldConfiguration {
    return {
      operator: filterTypeToNumber(filter.setting.type),
      property: filter.propertyId,
      value1: filter.setting.val1,
      value2: filter.setting.val2,
    };
  }

  async saveFilter(name: string, isGlobal: boolean) {
    const filterGroupServerVerion: IUIGridFilterCoreConfiguration = {
      details: this.activeFilters.map((filter) => this.filtreToServerVersion(filter)),
      id: undefined,
      isGlobal: isGlobal,
      name: name,
    };

    const api = getApi(this.ctx);
    const filterGrouId = await api.saveFilter({
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      PanelId: getDataView(this.ctx).modelInstanceId,
      Filter: filterGroupServerVerion,
      IsDefault: false,
    });

    console.log("filterGrouId: " + filterGrouId);
  }

  parent?: any;
}