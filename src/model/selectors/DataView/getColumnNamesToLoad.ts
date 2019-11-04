import { getProperties } from "./getProperties";
import { getDataSourceFields } from "../DataSources/getDataSourceFields";

export function getColumnNamesToLoad(ctx: any): string[] {
  return getDataSourceFields(ctx)
    .map(field => field.name)
    .filter(name => !name.startsWith("__"));
}
