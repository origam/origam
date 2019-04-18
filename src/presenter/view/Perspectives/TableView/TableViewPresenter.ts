import { ITableView, ITable } from "./types";
import { IViewType } from "../../../../DataView/types/IViewType";
import { IToolbar } from "../types";
import { ML } from "../../../../utils/types";
import { unpack } from '../../../../utils/objects';

export class TableViewPresenter implements ITableView {
  constructor(public P:{
    toolbar: ML<IToolbar | undefined>;
    table: ML<ITable>;
  }) {}

  type: IViewType.Table = IViewType.Table;

  get toolbar(): IToolbar | undefined {
    return unpack(this.P.toolbar);
  }

  get table(): ITable {
    return unpack(this.P.table);
  }
}
