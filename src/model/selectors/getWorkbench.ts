import { CWorkbench, IWorkbench } from '../types/IWorkbench';

export function getWorkbench(ctx: any): IWorkbench {
  let cn = ctx;
  while(true) {
    if(cn.$type === CWorkbench) {
      return cn
    }
    cn = cn.parent;
  }
}