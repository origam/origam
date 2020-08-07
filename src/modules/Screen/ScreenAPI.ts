import { IOrigamAPI } from "model/entities/OrigamAPI";
import { TypeSymbol } from "dic/Container";
import { IApi } from "model/entities/types/IApi";

export class ScreenAPI {
  constructor(
    private getSessionId: () => string | undefined,
    private getMenuItemId: () => string,
    private api: () => IApi
  ) {}

  *getLookupList(args: {
    DataStructureEntityId?: string;
    Entity?: string;
    ColumnNames: string[];
    Property: string;
    Id: string;
    LookupId: string;
    Parameters?: { [key: string]: any };
    ShowUniqueValues: boolean;
    SearchText: string;
    PageSize: number;
    PageNumber: number;
  }) {
    return yield this.api().getLookupList({
      ...args,
      SessionFormIdentifier: this.getSessionId(),
      MenuId: this.getMenuItemId(),
    });
  }
}
export const IScreenAPI = TypeSymbol<ScreenAPI>("IScreenAPI");

/*
DataStructureEntityId?: string;
FormSessionIdentifier?: string;
Entity?: string;
ColumnNames: string[];
Property: string;
Id: string;
LookupId: string;
Parameters?: { [key: string]: any };
ShowUniqueValues: boolean;
SearchText: string;
PageSize: number;
PageNumber: number;
MenuId: string;
*/
