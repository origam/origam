import { IProperty } from "model/entities/types/IProperty";
import { getDataSourceFieldByName } from "../DataSources/getDataSourceFieldByName";
import { getDataTable } from "./getDataTable";

export function getFieldErrorMessage(ctx: any) {
  return function getFieldErrorMessage(row: any[], property: IProperty) {
    if (row) {
      const dataTable = getDataTable(ctx);
      const dsFieldErrors = getDataSourceFieldByName(ctx, "__Errors");
      const errors = dsFieldErrors
        ? dataTable.getCellValueByDataSourceField(row, dsFieldErrors)
        : null;

      const errMap: Map<number, string> | undefined = errors
        ? new Map(
            Object.entries<string>(
              errors.fieldErrors
            ).map(([dsIndexStr, errMsg]: [string, string]) => [parseInt(dsIndexStr, 10), errMsg])
          )
        : undefined;

      const errMsg = dsFieldErrors && errMap ? errMap.get(property.dataSourceIndex) : undefined;
      return errMsg;
    }
  };
}
