import { TypeSymbol, Func } from "dic/Container";
import { IDataView } from "./DataViewTypes";
import { IOrigamAPI } from "model/entities/OrigamAPI";
import { IScreenAPI, ScreenAPI } from "modules/Screen/ScreenAPI";

export class DataViewAPI {
  constructor(
    public getDataStructureEntityId: () => string,
    public getEntity: () => string,
    public api: () => ScreenAPI
  ) {}

  *getLookupList(args: {
    ColumnNames: string[];
    Property: string;
    Id: string;
    LookupId: string;
    Parameters?: { [key: string]: any };
    ShowUniqueValues: boolean;
    SearchText: string;
    PageSize: number;
    PageNumber: number;
  }): any {
    return yield* this.api().getLookupList({
      ...args,
      DataStructureEntityId: this.getDataStructureEntityId(),
      Entity: this.getEntity(),
    });
  }
}
export const IDataViewAPI = TypeSymbol<DataViewAPI>("IDataViewAPI");
