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
    throw new Error("Implementation to be fixed.")
    const fields = ids
      .map(id => this.fieldById(id))
      .filter(field => field !== undefined) as IDataSourceField[];
    fields.sort((a, b) => a.idx - b.idx);
    return fields.map(field => field.id);
  }

  /*
    Fields ordered by data source indices are reordered according to given ids.
  */
  reorderedRow(ids: string[], record: any[]): any[] {
    const fields = ids
      .map(id => this.fieldById(id))
      .filter(field => field !== undefined) as IDataSourceField[];
    const newRecord: any[] = fields.map(field => record[field.idx]);
    return newRecord;
  }


}
