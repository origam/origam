import { TypeSymbol } from "dic/Container";
import { IDataTable } from "model/entities/types/IDataTable";
import { IProperty } from "model/entities/types/IProperty";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";

export class DataViewData {
  constructor(
    private dataTable: () => IDataTable,
    private propertyById: (id: string) => IProperty | undefined
  ) {}
  getCellValue(rowId: string, propertyId: string) {
    const dataTable = this.dataTable();
    const property = this.propertyById(propertyId);
    const row = dataTable.getRowById(rowId);
    //dataTable.resolveCellText(property, value)
    if (property && row) {
      return dataTable.getCellValue(row, property);
    } else return null;
  }

  getCellText(propertyId: string, value: any) {
    const property = this.propertyById(propertyId);
    if (property && value) {
      return this.dataTable().resolveCellText(property, value);
    } else return null;
  }

  getIsCellTextLoading(propertyId: string, value: any): boolean {
    const property = this.propertyById(propertyId);
    if (property && value) {
      return this.dataTable().isCellTextResolving(property, value);
    } else return false;
  }

  setNewValue(rowId: string, propertyId: string, value: any) {
    const dataTable = this.dataTable();
    const row = dataTable.getRowById(rowId);
    const property = this.propertyById(propertyId);
    if (property && row) {
      onFieldChange(property)(undefined, row, property, value);
    }
  }
}
export const IDataViewData = TypeSymbol<DataViewData>("IDataViewData");
