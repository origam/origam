import { getApi } from "model/selectors/getApi";
import { getMenuItemId } from "../selectors/getMenuItemId";
import { getDataStructureEntityId } from "../selectors/DataView/getDataStructureEntityId";
import { getColumnNamesToLoad } from "../selectors/DataView/getColumnNamesToLoad";
import { getDataView } from "model/selectors/DataView/getDataView";


export interface IDataCacheContent {
  [key: string]: any[][];
}

export interface IDataEndpoint {
  getRows(
    ordering: Array<[string, string]>,
    filter: Array<[string, string, string]>
  ): Promise<any[][]>;
}

export class DataReorderingCache implements IDataEndpoint {
  public originalContent: IDataCacheContent = {};
  public reorderedContent: IDataCacheContent = {};
  public isPreloaded: boolean = false;

  async getRows(
    ordering: Array<[string, string]>,
    filter: Array<[string, string, string]>
  ) {
    const entity = getDataView(this).entity;
    if (this.reorderedContent.hasOwnProperty(entity)) {
      return this.reorderedContent[entity];
    }
    if (this.originalContent.hasOwnProperty(entity)) {
      return this.originalContent[entity];
    }
    if (this.isPreloaded) {
      throw new Error(`Entity ${entity} has not been loaded.`);
    }

    const api = getApi(this);
    const apiResult = await api.getEntities({
      MenuId: getMenuItemId(this),
      DataStructureEntityId: getDataStructureEntityId(this),
      Ordering: ordering,
      ColumnNames: getColumnNamesToLoad(this),
      Filter: JSON.stringify(filter)
    });
    this.originalContent[entity] = apiResult;
    return apiResult as any[][];
  }
}
