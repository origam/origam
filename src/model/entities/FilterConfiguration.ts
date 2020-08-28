import _ from "lodash";
import { action, comparer, computed, flow, observable, reaction, toJS } from "mobx";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getDataViewPropertyById } from "model/selectors/DataView/getDataViewPropertyById";
import { getDataTable } from "../selectors/DataView/getDataTable";
import { IFilterConfiguration } from "./types/IFilterConfiguration";
import produce from "immer";
import { getDataSource } from "../selectors/DataSources/getDataSource";
import { IFilter } from "./types/IFilter";

export class FilterConfiguration implements IFilterConfiguration {
  constructor(implicitFilters: IImplicitFilter[]) {
    this.implicitFilters = implicitFilters;
    this.start();
  }

  $type_IFilterConfigurationData: 1 = 1;

  implicitFilters: IImplicitFilter[];
  @observable.ref filters: IFilter[] = [];

  getSettingByPropertyId(propertyId: string): any {
    return this.filters.find((item) => item.propertyId === propertyId);
  }

  @action.bound
  setFilter(term: IFilter): void {
    this.filters = produce(this.filters, (draft: any) => {
      const oldIdx = draft.findIndex((item: any) => item.propertyId === term.propertyId);
      if (oldIdx > -1) {
        draft.splice(oldIdx, 1);
      }
      draft.push(term);
    });
  }

  @action.bound
  clearFilters(): void {
    this.filters = [];
  }

  @observable isFilterControlsDisplayed: boolean = false;

  @action.bound
  onFilterDisplayClick(event: any): void {
    this.isFilterControlsDisplayed = !this.isFilterControlsDisplayed;
    if (this.isFilterControlsDisplayed) {
      // TODO: Wait for data loaded?
    } else {
      this.clearFilters();
    }
  }

  get filteringFunction(): () => (row: any[]) => boolean {
    return () => (row: any[]) => {
      if (!this.isPresentInDetail(row)) {
        return false;
      }
      for (let term of this.implicitFilters) {
        if (!this.implicitFilterPredicate(row, term)) {
          return false;
        }
      }
      for (let term of this.filters) {
        if (!this.userFilterPredicate(row, term)) {
          return false;
        }
      }
      return true;
    };
  }

  userFilterPredicate(row: any[], term: any) {
    const dataTable = getDataTable(this);
    const prop = dataTable.getPropertyById(term.propertyId)!;
    switch (prop.column) {
      case "Text": {
        const txt1 = dataTable.getCellValue(row, prop);
        if (txt1 === undefined) return true;

        switch (term.setting.type) {
          case "contains": {
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            const t2 = term.setting.val1.toLocaleLowerCase();
            return txt1.toLocaleLowerCase().includes(t2);
          }
          case "ends": {
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            const t2 = term.setting.val1.toLocaleLowerCase();
            return txt1.toLocaleLowerCase().endsWith(t2);
          }
          case "eq": {
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            const t2 = term.setting.val1.toLocaleLowerCase();
            return txt1.toLocaleLowerCase() === t2;
          }
          case "ncontains": {
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            const t2 = term.setting.val1.toLocaleLowerCase();
            return !txt1.toLocaleLowerCase().includes(t2);
          }
          case "nends": {
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            const t2 = term.setting.val1.toLocaleLowerCase();
            return !txt1.toLocaleLowerCase().endsWith(t2);
          }
          case "neq": {
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            const t2 = term.setting.val1.toLocaleLowerCase();
            return txt1.toLocaleLowerCase() !== t2;
          }
          case "nnull": {
            return txt1 !== null;
          }
          case "nstarts": {
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            const t2 = term.setting.val1.toLocaleLowerCase();
            return !txt1.toLocaleLowerCase().startsWith(t2);
          }
          case "null": {
            return txt1 === null;
          }
          case "starts": {
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            const t2 = term.setting.val1.toLocaleLowerCase();
            return txt1.toLocaleLowerCase().startsWith(t2);
          }
        }
        break;
      }
      case "Date": {
        const txt1 = dataTable.getCellValue(row, prop);
        if (txt1 === undefined) return false;
        if (term.setting.type === "nnull") return txt1 !== null;
        if (term.setting.type === "null") return txt1 === null;
        if (
            term.setting.val1 === "" ||
            term.setting.val1 === undefined ||
            term.setting.val1 === null
        )
          return true;

        const t1 = term.setting.val1.endsWith("T00:00:00.000")
          ? txt1.substr(0, 10).concat("T00:00:00.000") : txt1;

        switch (term.setting.type) {
          case "between": {
            if (
              term.setting.val2 === "" ||
              term.setting.val2 === undefined ||
              term.setting.val2 === null
            )
              return true;
            if (txt1 === null) return false;
            const t0 = term.setting.val1;
            const t2 = term.setting.val2;
            return t0 < t1 && t1 < t2;
          }
          case "eq":
            if (txt1 === null) return false;
            return t1 === term.setting.val1;
          case "gt":
            if (txt1 === null) return false;
            return t1 > term.setting.val1;
          case "gte":
            if (txt1 === null) return false;
            return t1 >= term.setting.val1;
          case "lt":
            if (txt1 === null) return false;
            return t1 < term.setting.val1;
          case "lte":
            if (txt1 === null) return false;
            return t1 <= term.setting.val1;
          case "nbetween": {
            if (
              term.setting.val2 === "" ||
              term.setting.val2 === undefined ||
              term.setting.val2 === null
            )
              return true;
            if (txt1 === null) return false;
            const t0 = term.setting.val1;
            const t2 = term.setting.val2;
            return !(t0 < t1 && t1 < t2);
          }
          case "neq":
            if (txt1 === null) return false;
            return t1 !== term.setting.val1;
        }
      }
      case "Number": {
        const txt1 = dataTable.getCellValue(row, prop);
        if (txt1 === undefined) return true;
        const t1 = prop.column === "Number" ? parseFloat(txt1) : txt1;

        switch (term.setting.type) {
          case "between": {
            if (
              term.setting.val1 === "" ||
              term.setting.val2 === "" ||
              term.setting.val1 === undefined ||
              term.setting.val2 === undefined
            )
              return true;
            if (txt1 === null) return false;
            const t0 = parseFloat(term.setting.val1);
            const t2 = parseFloat(term.setting.val2);
            return t0 < t1 && t1 < t2;
          }
          case "eq":
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            return t1 === parseFloat(term.setting.val1);
          case "gt":
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            return t1 > parseFloat(term.setting.val1);
          case "gte":
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            return t1 >= parseFloat(term.setting.val1);
          case "lt":
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;
            return t1 < parseFloat(term.setting.val1);
          case "lte":
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            if (txt1 === null) return false;

            return t1 <= parseFloat(term.setting.val1);
          case "nbetween": {
            if (
              term.setting.val1 === "" ||
              term.setting.val2 === "" ||
              term.setting.val1 === undefined ||
              term.setting.val2 === undefined
            )
              return true;
            if (txt1 === null) return false;
            const t0 = parseFloat(term.setting.val1);
            const t2 = parseFloat(term.setting.val2);
            return !(t0 < t1 && t1 < t2);
          }
          case "neq":
            if (txt1 === null) return false;
            if (term.setting.val1 === "" || term.setting.val1 === undefined) return true;
            return t1 !== parseFloat(term.setting.val1);
          case "nnull":
            return t1 !== null;
          case "null":
            return t1 === null;
        }
      }
      case "ComboBox": {
        switch (term.setting.type) {
          case "in":
          case "eq": {
            const txt1 = dataTable.getCellValue(row, prop);
            const val1 = term.setting.val1 || [];
            if (val1.length === 0) return true;
            if (txt1 === null) return false;
            if (val1.findIndex((item: any) => item.value === txt1) > -1) {
              return true;
            }
            return false;
          }
          case "nin":
          case "neq": {
            const txt1 = dataTable.getCellValue(row, prop);
            const val1 = term.setting.val1 || [];
            if (val1.length === 0) return true;
            if (txt1 === null) return false;
            if (val1.findIndex((item: any) => item.value === txt1) > -1) {
              return false;
            }
            return true;
          }
          case "null": {
            const txt1 = dataTable.getCellValue(row, prop);
            return txt1 === null;
          }
          case "nnull": {
            const txt1 = dataTable.getCellValue(row, prop);
            return txt1 !== null;
          }
          case "contains": {
            const txt1 = dataTable.getCellText(row, prop);
            const val2 = term.setting.val2 || "";
            if (val2 === "") return true;
            if (txt1 === null) return false;
            return txt1.toLocaleLowerCase().includes(val2.toLocaleLowerCase());
          }
          case "ncontains": {
            const txt1 = dataTable.getCellText(row, prop);
            const val2 = term.setting.val2 || "";
            if (val2 === "") return true;
            if (txt1 === null) return false;
            return !txt1.toLocaleLowerCase().includes(val2.toLocaleLowerCase());
          }
        }
      }
      case "CheckBox": {
        switch (term.setting.type) {
          case "eq": {
            if (term.setting.val1 === undefined) return true;
            const bool1 = dataTable.getCellValue(row, prop);
            return bool1 === !!term.setting.val1;
          }
        }
        break;
      }
    }
  }

  implicitFilterPredicate(row: any[], implicitFilter: IImplicitFilter) {
    const dataTable = getDataTable(this);
    const dataSource = getDataSource(dataTable);
    const sourceField = dataSource.getFieldByName(implicitFilter.propertyId)!;
    const cellValue = dataTable.getCellValueByDataSourceField(row, sourceField);

    switch (parseInt(implicitFilter.operatorCode)) {
      case 1:
        return implicitFilter.value === String(cellValue);
      case 10:
        return implicitFilter.value !== String(cellValue);
      case 15:
        return cellValue === null;
      case 16:
        return cellValue !== null;
      default:
        throw new Error(`Operator code ${implicitFilter.operatorCode} not implemented.`);
    }
  }

  isPresentInDetail(row: any[]): boolean {
    if (this.dataView.isBindingRoot) return true;
    for (let binding of this.dataView.parentBindings) {
      const selectedRow = binding.parentDataView.selectedRow;
      if (!selectedRow) {
        return false;
      }
      for (let bindingPair of binding.bindingPairs) {
        const parentDsField = binding.parentDataView.dataSource.getFieldByName(
          bindingPair.parentPropertyId
        );
        if (!parentDsField) {
          return false;
        }
        const parentValue = binding.parentDataView.dataTable.getCellValueByDataSourceField(
          selectedRow,
          parentDsField
        );
        const childDsField = binding.childDataView.dataSource.getFieldByName(
          bindingPair.childPropertyId
        );
        if (!childDsField) {
          return false;
        }
        const childValue = binding.childDataView.dataTable.getCellValueByDataSourceField(
          row,
          childDsField
        );
        if (parentValue !== childValue) {
          return false;
        }
      }
    }
    return true;
  }

  @computed get dataView() {
    return getDataView(this);
  }

  disposers: any[] = [];

  @action.bound start() {
    this.disposers.push(
      reaction(
        () => {
          const data: any[] = toJS(this.filters);
          data.forEach((item) => delete item.setting.caption);
          return data;
        },
        () => {
          this.applyNewFiltering();
        },
        { equals: comparer.structural }
      )
    );
  }

  @action.bound applyNewFilteringImm = flow(function* (this: FilterConfiguration) {
    //console.log("New filtering:", toJS(this.filters));
    const dataView = getDataView(this);
    const dataTable = getDataTable(dataView);
    if (dataView.isReorderedOnClient) {
      if (this.filters.length > 0) {
        const comboProps = this.filters
          .map((term) => getDataViewPropertyById(this, term.propertyId)!)
          .filter((prop) => prop.column === "ComboBox");

        yield Promise.all(
          comboProps.map(async (prop) => {
            return prop.lookupEngine?.lookupResolver.resolveList(
              new Set(dataTable.getAllValuesOfProp(prop))
            );
          })
        );
      }
    }
  });

  applyNewFiltering = _.throttle(this.applyNewFilteringImm, 200);

  @action.bound stop() {
    this.disposers.forEach((dis) => dis());
  }

  parent?: any;
}

interface IImplicitFilter {
  propertyId: string;
  operatorCode: string;
  value: any;
}
