import { Properties } from "../entities/data/Properties";
import { IPropertyParam } from "../types/ModelParam";
import { Property } from "../entities/data/Property";

export function buildProperty(param: IPropertyParam, recordDataIndex: number) {
  const property = new Property({
    ...param,
    lookup: undefined,
    recordDataIndex
  });
  return property;
}

export function buildProperties(items: IPropertyParam[]) {
  const properties = new Properties(
    items.map((param, idx) => buildProperty(param, idx))
  );
  return properties;
}
