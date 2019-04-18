import { IDataSource, IDataSourceField } from "./types";
import { observable } from "mobx";


export class DataSource implements IDataSource {
  constructor(P: {
    id: string;
    fields: IDataSourceField[];
    dataStructureEntityId: string;
  }) {
    this.id = P.id;
    this.fields = P.fields;
    this.dataStructureEntityId = P.dataStructureEntityId;
  }

  id: string;
  dataStructureEntityId: string;
  @observable fields: IDataSourceField[] = [];

  fieldById(id: string): IDataSourceField | undefined {
    return this.fields.find(item => item.id === id);
  }

  reorderedIds(ids: string[]): string[] {
    const fields = ids
      .map(id => this.fieldById(id))
      .filter(field => field !== undefined) as IDataSourceField[];
    fields.sort((a, b) => a.idx - b.idx);
    return fields.map(field => field.id);
  }
}
