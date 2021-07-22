/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

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
  }): any {
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
