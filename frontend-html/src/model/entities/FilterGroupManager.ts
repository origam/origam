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

import { IFilterGroup } from "model/entities/types/IFilterGroup";
import { IFilterConfiguration } from "model/entities/types/IFilterConfiguration";
import { action, observable } from "mobx";
import { IFilter } from "model/entities/types/IFilter";
import { IUIGridFilterFieldConfiguration, } from "model/entities/types/IApi";
import { filterTypeToNumber } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import { getApi } from "model/selectors/getApi";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getSessionId } from "model/selectors/getSessionId";
import { cloneFilterGroup } from "xmlInterpreters/filterXml";
import { getSelectionMember } from "model/selectors/DataView/getSelectionMember";

export class FilterGroupManager {
  ctx: any;
  filterGroups: IFilterGroup[] = [];
  @observable
  private _defaultFilter: IFilterGroup | undefined;
  @observable
  selectedFilterGroup: IFilterGroup | undefined;

  get isSelectedFilterGroupDefault() {
    if (!this.selectedFilterGroup) {
      return false;
    }
    return this.defaultFilter?.id === this.selectedFilterGroup?.id;
  }

  constructor(private filterConfiguration: IFilterConfiguration) {
    this.ctx = filterConfiguration;
    filterConfiguration.registerFilteringOnOffHandler(filteringOn => {
      if (!filteringOn) {
        this.selectedFilterGroup = undefined;
      }
    });
  }

  get filtersHidden() {
    return !this.filterConfiguration.isFilterControlsDisplayed;
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

  get noFilterActive() {
    return !this.selectedFilterGroup?.selectionCheckboxFilter &&
      (
        this.activeFilters.length === 0 ||
        this.activeFilters.every(filter => !filter.setting.isComplete)
      )
  }

  @action.bound
  setFilterGroup(filterGroup: IFilterGroup | undefined) {
    this.selectedFilterGroup = cloneFilterGroup(filterGroup);
    this.filterConfiguration.clearFilters();
    if (this.selectedFilterGroup?.filters) {
      this.filterConfiguration.setFilters(this.selectedFilterGroup.filters);
    }
    this.filterConfiguration.selectionCheckboxFilter = 
      this.selectedFilterGroup?.selectionCheckboxFilter ?? null;
  }

  filterToServerVersion(filter: IFilter): IUIGridFilterFieldConfiguration {
    return {
      operator: filterTypeToNumber(filter.setting.type),
      property: filter.propertyId,
      value1: filter.setting.val1ServerForm,
      value2: filter.setting.val2ServerForm,
    };
  }

  getFilterGroupServerVersion(name: string, isGlobal: boolean) {
    const filters = [...this.activeFilters];
    const {selectionCheckboxFilter} = this.filterConfiguration;
    if(selectionCheckboxFilter !== null) {
      filters.push({
        propertyId: getSelectionMember(this),
        dataType: '',
        setting: {
          type: 'eq',
          val1ServerForm: selectionCheckboxFilter,
          filterValue1: undefined,
          filterValue2: undefined,
          val2ServerForm:  undefined,
          isComplete: true,
          lookupId: undefined
        }
      })
    }
    return {
      details: filters
        .filter(filter => filter.setting.isComplete)
        .map((filter) => this.filterToServerVersion(filter)),
      id: undefined,
      isGlobal: isGlobal,
      name: name,
    }
  }

  @action.bound
  clearFiltersAndClose(event: any) {
    this.filterConfiguration.onFilterDisplayClick(event);
  }

  @action.bound
  async saveActiveFiltersAsNewFilterGroup(name: string, isGlobal: boolean) {
    const api = getApi(this.ctx);
    const filterGroupId = await api.saveFilter({
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      PanelId: getDataView(this.ctx).modelId,
      Filter: this.getFilterGroupServerVersion(name, isGlobal),
      IsDefault: false,
    });
    const filterGroup = {
      filters: this.activeFilters,
      id: filterGroupId,
      isGlobal: isGlobal,
      name: name,
      selectionCheckboxFilter: 
        this.filterConfiguration.selectionCheckboxFilter
    }
    this.filterGroups.push(filterGroup);
  }

  @action.bound
  async deleteFilterGroup() {
    if (!this.selectedFilterGroup) {
      return;
    }
    const api = getApi(this.ctx);
    await api.deleteFilter({filterId: this.selectedFilterGroup.id});

    const index = this.filterGroups.findIndex((group) => group.id === this.selectedFilterGroup?.id);
    if (index > -1) {
      this.filterGroups.splice(index, 1);
    }
    this.filterConfiguration.clearFilters();
    this.selectedFilterGroup = undefined;
  }

  @action.bound
  async resetDefaultFilterGroup() {
    if (!this._defaultFilter) {
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
    this.selectedFilterGroup = undefined;
  }

  @action.bound
  async setSelectedFilterGroupAsDefault() {
    const api = getApi(this.ctx);
    await api.setDefaultFilter({
      SessionFormIdentifier: getSessionId(this.ctx),
      PanelInstanceId: getDataView(this.ctx).modelInstanceId,
      DataStructureEntityId: getDataStructureEntityId(this.ctx),
      PanelId: getDataView(this.ctx).modelId,
      Filter: this.getFilterGroupServerVersion("DEFAULT", false),
      IsDefault: true
    });
    this._defaultFilter = this.selectedFilterGroup;
  }

  parent?: any;
}