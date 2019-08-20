import { IApplication } from '../entities/types/IApplication';
import { useContext } from 'react';
import { MobXProviderContext } from 'mobx-react';

export function getApplication(ctx?: any): IApplication {
  let cn = ctx;
  while(cn.parent) {
    cn = cn.parent;
  }
  return cn;
}

