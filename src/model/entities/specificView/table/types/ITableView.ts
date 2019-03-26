import { IProperty } from "../../../data/types/IProperty";
import { IRecord } from "../../../data/types/IRecord";
import { IRecordId } from "../../../values/types/IRecordId";
import { IPropertyId } from "../../../values/types/IPropertyId";
import { IViewType } from "src/model/entities/specificViews/types/IViewType";
import { ICursor } from "src/model/entities/cursor/types/ICursor";
import { IProperties } from "src/model/entities/data/types/IProperties";
import { IDataTable } from "src/model/entities/data/types/IDataTable";

export interface ITableView {
  type: IViewType.TableView;
  id: string;
  cursor: ICursor;
  dataTable: IDataTable;
  properties: IProperties;
  reorderedProperties: IProperties;

  updateVisibleRegion(
    rowIdxStart: number,
    rowIdxEnd: number,
    columnIdxStart: number,
    columnIdxEnd: number
  ): void;

  selectCell(
    rowId: IRecordId | undefined,
    columnId: IPropertyId | undefined
  ): void;
}
