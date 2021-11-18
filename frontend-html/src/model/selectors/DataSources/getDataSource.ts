import {isIDataView} from "model/entities/types/IDataView";
import {isIDataSource} from "model/entities/types/IDataSource";

export function getDataSource(ctx: any) {
  let cn = ctx;
  while (true) {
    if (isIDataView(cn)) {
      return cn.dataSource;
    }
    if(isIDataSource(cn)) {
      return cn
    }
    cn = cn.parent;
  }
}