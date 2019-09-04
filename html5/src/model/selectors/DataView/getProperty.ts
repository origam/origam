import { IProperty, isIProperty } from "../../entities/types/IProperty";

export function getProperty(ctx: any): IProperty {
  let cn = ctx;
  while (true) {
    if (isIProperty(cn)) {
      return cn;
    }
    cn = cn.parent;
  }
}
