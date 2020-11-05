import {IFilterGroup} from "model/entities/types/IFilterGroup";
import {IFilterConfiguration} from "model/entities/types/IFilterConfiguration";
import {action, observable} from "mobx";
import {IFilter} from "model/entities/types/IFilter";
import {
  IUIGridFilterCoreConfiguration,
  IUIGridFilterFieldConfiguration,
} from "model/entities/types/IApi";
import {filterTypeToNumber} from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import {getApi} from "model/selectors/getApi";
import {getDataStructureEntityId} from "model/selectors/DataView/getDataStructureEntityId";
import {getDataView} from "model/selectors/DataView/getDataView";
import {getSessionId} from "model/selectors/getSessionId";

export class FilterGroupManager {
  ctx: any;
  filterGroups: IFilterGroup[] = [];
  private _defaultFilter: IFilterGroup | undefined;
  @observable
  selectedFilterGroupId: string | undefined;

  get isSelectedFilterGroupDefault() {
    if (!this.selectedFilterGroupId) {
      return false;
    }
    return this.defaultFilter?.id === this.selectedFilterGroupId;
  }

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

  get nofilterActive(){
    return this.activeFilters.length === 0 ||
      this.activeFilters.every(filter => !filter.setting.isComplete)
  }

  @action.bound
  setFilterGroup(filterGroup: IFilterGroup | undefined) {
    this.filterConfiguration.clearFilters();
    if (filterGroup?.filters) {
      this.filterConfiguration.setFilters(filterGroup.filters);
    }
    this.selectedFilterGroupId = filterGroup?.id;
  }

  filterToServerVersion(filter: IFilter): IUIGridFilterFieldConfiguration {
    return {
      operator: filterTypeToNumber(filter.setting.type),
      property: filter.propertyId,
      value1: filter.setting.val1ServerForm,
      value2: filter.setting.val2ServerForm,
    };
  }

  getFilterGroupServerVerion(name: string, isGlobal: boolean){
    return {
      details: this.activeFilters
        .filter(filter=> filter.setting.isComplete)
        .map((filter) => this.filterToServerVersion(filter)),
      id: undefined,
      isGlobal: isGlobal,
      name: name,
    }
  }

  @action.bound
  async saveActiveFiltersAsNewFilterGroup(name: string, isGlobal: boolean) {
    const api = getApi(this.ctx);
    const filterGrouId = await api.saveFilter({
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      PanelId: getDataView(this.ctx).modelId,
      Filter: this.getFilterGroupServerVerion(name, isGlobal),
      IsDefault: false,
    });
    const filterGroup = {
      filters: this.activeFilters,
      id: filterGrouId,
      isGlobal: isGlobal,
      name: name
    }
    this.filterGroups.push(filterGroup);
  }

  @action.bound
  async deleteFilterGroup() {
    if (!this.selectedFilterGroupId) {
      return;
    }
    const api = getApi(this.ctx);
    await api.deleteFilter({filterId: this.selectedFilterGroupId});

    const index = this.filterGroups.findIndex((group) => group.id === this.selectedFilterGroupId);
    if (index > -1) {
      this.filterGroups.splice(index, 1);
    }
    this.filterConfiguration.clearFilters();
    this.selectedFilterGroupId = undefined;
  }

  @action.bound
  async resetDefaultFilterGroup() {
    if(!this._defaultFilter){
      return;
    }
    const api = getApi(this.ctx);
    await api.resetDefaultFilter({
      SessionFormIdentifier: getSessionId(this.ctx),
      PanelInstanceId: getDataView(this.ctx).modelInstanceId,
    });
    this._defaultFilter = undefined;
  }

  @action.bound
  cancelSelectedFilter() {
    this.filterConfiguration.clearFilters();
    this.selectedFilterGroupId = undefined;
  }

  @action.bound
  async setSelectedFilterGroupAsDefault() {
    const api = getApi(this.ctx);
    await api.setDefaultFilter({
      SessionFormIdentifier: getSessionId(this.ctx),
      PanelInstanceId: getDataView(this.ctx).modelInstanceId,
      DataStructureEntityId:  getDataStructureEntityId(this.ctx),
      PanelId: getDataView(this.ctx).modelId,
      Filter: this.getFilterGroupServerVerion("DEFAULT", false),
      IsDefault: true
    });
  }

  parent?: any;
}