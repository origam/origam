import { IDataSources, IDataSource } from "./types";
import { observable } from "mobx";

export class DataSources implements IDataSources {

  constructor(P:{sources: IDataSource[]}) {
    this.sources = P.sources;
  }

  @observable sources: IDataSource[] = [];

  getByEntityName(name: string): IDataSource | undefined {
    return this.sources.find(source => source.id === name);
  }

  getDataSourceEntityIdByEntityName(name: string): string | undefined {
    const source = this.getByEntityName(name);
    const id = source ? source.dataStructureEntityId : undefined;
    return id;
  }

}