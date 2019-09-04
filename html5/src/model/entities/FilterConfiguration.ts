import {
  IFilterConfiguration,
  IFilterTerm
} from "./types/IFilterConfiguration";
import { observable, action, computed, reaction, toJS, comparer } from "mobx";
import { IDataTable } from "./types/IDataTable";
import { start } from "xstate/lib/actions";
import { getDataTable } from "../selectors/DataView/getDataTable";
import _ from "lodash";

export class FilterConfiguration implements IFilterConfiguration {
  constructor() {
    this.start();
  }

  $type_IFilterConfigurationData: 1 = 1;

  @observable filtering: IFilterTerm[] = [];

  getSettingByPropertyId(propertyId: string): IFilterTerm | undefined {
    return this.filtering.find(item => item.propertyId === propertyId);
  }

  @action.bound
  setFilter(term: IFilterTerm): void {
    const oldIdx = this.filtering.findIndex(
      item => item.propertyId === term.propertyId
    );
    if (oldIdx > -1) {
      this.filtering.splice(oldIdx, 1);
    }
    this.filtering.push(term);
  }

  @action.bound
  clearFilters(): void {
    this.filtering.length = 0;
  }

  @observable isFilterControlsDisplayed: boolean = false;

  @action.bound
  onFilterDisplayClick(event: any): void {
    this.isFilterControlsDisplayed = !this.isFilterControlsDisplayed;
  }

  get filteringFunction(): (
    dataTable: IDataTable
  ) => (row: any[]) => boolean {
    return (dataTable: IDataTable) => (row: any[]) => {
      for (let term of this.filtering) {
        const prop = dataTable.getPropertyById(term.propertyId)!;
        switch (prop.column) {
          case "Text":
          case "Date":
          case "ComboBox": {
            const txt1 = dataTable.getCellText(row, prop);
            if (txt1 === undefined) return true;
            if (term.setting.dataType === "string") {
              switch (term.setting.type) {
                case "contains":
                  return txt1.includes(term.setting.val1);
              }
            }
            break;
          }
          case "CheckBox": {
            const val1 = dataTable.getCellValue(row, prop);
            if (val1 === undefined) return true;
            return true;
            /*if(term.setting.dataType === "boolean") {
            switch (term.setting.type) {
            }
            }*/
            break;
          }
          case "Number": {
            const val1 = dataTable.getCellValue(row, prop);
            dataTable.getCellValue(row, prop);
            break;
          }
        }
      }
      return true;
    };
  }

  disposers: any[] = [];

  @action.bound start() {
    this.disposers.push(
      reaction(
        () => {
          const data = toJS(this.filtering);
          data.forEach(item => delete item.setting.human);
          return data;
        },
        () => {
          this.applyNewFiltering();
        },
        { equals: comparer.structural }
      )
    );
  }

  applyNewFiltering = _.throttle(this.applyNewFilteringImm, 200)

  @action.bound applyNewFilteringImm() {
    console.log("New filtering:", toJS(this.filtering));
    const dataTable = getDataTable(this);
    if (this.filtering.length > 0) {
      dataTable.setFilteringFn(this.filteringFunction);
    } else {
      dataTable.setFilteringFn(undefined);
    }
  }

  @action.bound stop() {
    this.disposers.forEach(dis => dis());
  }

  parent?: any;
}
