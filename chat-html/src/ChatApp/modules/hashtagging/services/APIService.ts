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

import { HashtagRootStore } from "../stores/RootStore";
import xmlJs from "xml-js";

export class PubSub {
  reg = new Map<number, any>();
  idGen = 0;

  subs(handler: (...args: any) => void) {
    const myId = this.idGen++;
    this.reg.set(myId, handler);
    return () => this.reg.delete(myId);
  }

  trig(...args: any) {
    for (let h of this.reg.values()) h();
  }
}

export function delay(ms: number, chCancel?: PubSub) {
  return new Promise<void>((resolve) => {
    const hTimer = setTimeout(() => {
      hCancel?.();
      resolve();
    }, ms);
    const hCancel = chCancel?.subs(() => {
      clearTimeout(hTimer);
      hCancel?.();
    });
  });
}

export function capitalize(sin: string) {
  return sin.charAt(0).toUpperCase() + sin.slice(1);
}

export class APIService {
  constructor(public rootStore: HashtagRootStore) {}

  get api() {
    return this.rootStore.httpApi;
  }

  async getCategories(
    searchTerm: string,
    offset: number,
    limit: number,
    chCancel?: PubSub
  ): Promise<any> {
    /* console.log(await this.api.getHashtagCategories());
    console.log("getCategories", searchTerm);
    await delay(250, chCancel);
    const filt = categories.filter((item) => {
      if (!searchTerm) return true;
      return (item[1] || "")
        .toLocaleLowerCase()
        .includes((searchTerm || "").toLocaleLowerCase());
    });
    return filt.slice(offset, offset + limit);*/
    const categories = await this.api.getHashtagAvailableCategories();
    function transformCombo(combo: any) {
      const control = combo?.elements?.[0];
      const controlColumns = control?.elements?.[0]?.elements;
      if (control && controlColumns) {
        const identifierIndex = parseInt(control.attributes.IdentifierIndex);
        const tableConfig = {
          identifierIndex,
          columns: controlColumns.map((ccol: any) => {
            return {
              type: ccol.attributes.Column,
              formatterPattern: ccol.attributes.FormatterPattern,
              label: ccol.attributes.Name,
              name: ccol.attributes.Id,
            };
          }),
          dataSourceFields: [
            { name: "$Id", dataIndex: identifierIndex },
            ...controlColumns.map((ccol: any) => {
              return {
                name: ccol.attributes.Id,
                dataIndex: parseInt(ccol.attributes.Index),
              };
            }),
          ],
        };
        return tableConfig;
      }
    }
    return categories.map((item: any) => {
      return [
        item.deepLinkName,
        item.deepLinkLabel,
        transformCombo(xmlJs.xml2js(item.objectComboboxMetadata)),
      ];
    });
  }

  async getObjects(
    categoryId: string,
    searchTerm: string,
    pageNumber: number,
    pageSize: number,
    chCancel?: PubSub
  ): Promise<any> {
    return await this.api.getHashtagAvailableObjects(
      categoryId,
      pageNumber,
      pageSize,
      searchTerm || undefined,
      chCancel
    );
  }

  async getHashtagLabels(categoryId: string, labelIds: string[]) {
    return await this.api.getHashtagLabels(categoryId, labelIds);
  }
}
