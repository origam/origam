import { IApplication } from '../types/IApplication';

export function getApplication(ctx: any): IApplication {
  let cn = ctx;
  while(cn.parent) {
    cn = cn.parent;
  }
  return cn;
}