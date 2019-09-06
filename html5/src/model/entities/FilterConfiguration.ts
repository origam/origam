import _ from "lodash";
import { action, comparer, flow, observable, reaction, toJS } from "mobx";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { getDataTable } from "../selectors/DataView/getDataTable";
import { IDataTable } from "./types/IDataTable";
import {
  IFilterConfiguration,
  IFilterTerm
} from "./types/IFilterConfiguration";

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
    if (this.isFilterControlsDisplayed) {
      // TODO: Wait for data loaded?
    }
  }

  get filteringFunction(): (dataTable: IDataTable) => (row: any[]) => boolean {
    return (dataTable: IDataTable) => (row: any[]) => {
      for (let term of this.filtering) {
        const prop = dataTable.getPropertyById(term.propertyId)!;
        switch (prop.column) {
          case "Text": {
            const txt1 = dataTable.getCellText(row, prop);
            if (txt1 === undefined) return true;
            if (term.setting.dataType === "string") {
              const t1 = txt1.toLocaleLowerCase();

              switch (term.setting.type) {
                case "contains": {
                  const t2 = term.setting.val1.toLocaleLowerCase();
                  return t1.includes(t2);
                }
                case "ends": {
                  const t2 = term.setting.val1.toLocaleLowerCase();
                  return t1.endsWith(t2);
                }
                case "eq": {
                  const t2 = term.setting.val1.toLocaleLowerCase();
                  return t1 === t2;
                }
                case "ncontains": {
                  const t2 = term.setting.val1.toLocaleLowerCase();
                  return !t1.includes(t2);
                }
                case "nends": {
                  const t2 = term.setting.val1.toLocaleLowerCase();
                  return !t1.endsWith(t2);
                }
                case "neq": {
                  const t2 = term.setting.val1.toLocaleLowerCase();
                  return t1 !== t2;
                }
                case "nnull": {
                  return t1 !== null;
                }
                case "nstarts": {
                  const t2 = term.setting.val1.toLocaleLowerCase();
                  return !t1.startsWith(t2);
                }
                case "null": {
                  return t1 === null;
                }
                case "starts": {
                  const t2 = term.setting.val1.toLocaleLowerCase();
                  return t1.startsWith(t2);
                }
              }
            }
            break;
          }
          case "Date": {
            const txt1 = dataTable.getCellValue(row, prop)
            if (txt1 === undefined) return true;
            const t1 = txt1
            if (term.setting.dataType === "date") {
              switch (term.setting.type) {
                case "between": {
                  const t0 = term.setting.val1;
                  const t2 = term.setting.val2;
                  return t0 < t1 && t1 < t2;
                }
                case "eq":
                  return t1 === term.setting.val1;
                case "gt":
                  return t1 > term.setting.val1;
                case "gte":
                  return t1 >= term.setting.val1;
                case "lt":
                  return t1 < term.setting.val1;
                case "lte":
                  return t1 <= term.setting.val1;
                case "nbetween": {
                  const t0 = term.setting.val1;
                  const t2 = term.setting.val2;
                  return !(t0 < t1 && t1 < t2);
                }
                case "neq":
                  return t1 !== term.setting.val1;
                case "nnull":
                  return t1 !== null;
                case "null":
                  return t1 === null;
              }
            }
          }
          case "Number": {
            const txt1 = dataTable.getCellValue(row, prop)
            if (txt1 === undefined) return true;
            const t1 = prop.column === "Number" ? parseFloat(txt1) : txt1;
            if (term.setting.dataType === "number") {
              switch (term.setting.type) {
                case "between": {
                  const t0 = parseFloat(term.setting.val1);
                  const t2 = parseFloat(term.setting.val2);
                  return t0 < t1 && t1 < t2;
                }
                case "eq":
                  return t1 === parseFloat(term.setting.val1);
                case "gt":
                  return t1 > parseFloat(term.setting.val1);
                case "gte":
                  return t1 >= parseFloat(term.setting.val1);
                case "lt":
                  return t1 < parseFloat(term.setting.val1);
                case "lte":
                  return t1 <= parseFloat(term.setting.val1);
                case "nbetween": {
                  const t0 = parseFloat(term.setting.val1);
                  const t2 = parseFloat(term.setting.val2);
                  return !(t0 < t1 && t1 < t2);
                }
                case "neq":
                  return t1 !== parseFloat(term.setting.val1);
                case "nnull":
                  return t1 !== null;
                case "null":
                  return t1 === null;
              }
            }
          }
          case "ComboBox": {
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

  @action.bound applyNewFilteringImm = flow(function*(
    this: FilterConfiguration
  ) {
    console.log("New filtering:", toJS(this.filtering));
    const dataView = getDataView(this);
    const dataTable = getDataTable(dataView);
    if (dataView.isReorderedOnClient) {
      if (this.filtering.length > 0) {
        const comboProps = this.filtering
          .map(term => getDataViewPropertyById(this, term.propertyId)!)
          .filter(prop => prop.column === "ComboBox");

        yield Promise.all(
          comboProps.map(prop =>
            prop.lookup!.resolveList(
              new Set(dataTable.getAllValuesOfProp(prop))
            )
          )
        );
        dataTable.setFilteringFn(this.filteringFunction);
      } else {
        dataTable.setFilteringFn(undefined);
      }
    }
  });

  applyNewFiltering = _.throttle(this.applyNewFilteringImm, 200);

  @action.bound stop() {
    this.disposers.forEach(dis => dis());
  }

  parent?: any;
}
