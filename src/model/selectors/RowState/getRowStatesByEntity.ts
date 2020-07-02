import {getDataSourceByEntity} from "../DataSources/getDataSourceByEntity";

export function getRowStatesByEntity(ctx: any, entity: string) {
  const dataSource = getDataSourceByEntity(ctx, entity);
  const rowStates = dataSource ? dataSource.rowState : undefined;
  return rowStates;
}
