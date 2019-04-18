import { IDataSourceField } from './types';

export class DataSourceField implements IDataSourceField {
  constructor(P: {id: string, idx: number}) {
    this.id = P.id;
    this.idx = P.idx;
  }

  id: string;  
  idx: number;
}