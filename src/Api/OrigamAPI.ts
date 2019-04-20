import xmlJs from "xml-js";
import axios from "axios";
import { IApi } from "./IApi";
import _ from "lodash";

export class OrigamAPI implements IApi {
  constructor() {
    this.urlPrefix = "/internalApi";
  }

  urlPrefix: string;
  accessToken = "";

  setAccessToken(token: string) {
    this.accessToken = token;
  }

  resetAccessToken(): void {
    this.accessToken = "";
  }

  get httpAuthHeader() {
    return {
      Authorization: `Bearer ${this.accessToken}`
    };
  }

  async login(credentials: { UserName: string; Password: string }) {
    return (await axios.post(`${this.urlPrefix}/Account/Login`, credentials))
      .data;
  }

  async logout() {
    return;
  }

  async getMenu() {
    return xmlJs.xml2js(
      (await axios.get(`${this.urlPrefix}/MetaData/GetMenu`, {
        headers: this.httpAuthHeader
      })).data,
      { addParent: true, alwaysChildren: true }
    );
  }

  async getScreen(id: string) {
    return xmlJs.xml2js(
      (await axios.get(`${this.urlPrefix}/MetaData/GetScreeSection`, {
        params: { id },
        headers: this.httpAuthHeader
      })).data,
      { addParent: true, alwaysChildren: true }
    );
  }

  async getEntities(query: {
    MenuId: string;
    DataStructureEntityId: string;
    Ordering: Array<[string, string]>;
    ColumnNames: string[];
    Filter: string;
    RowLimit?: number;
    MasterRowId?: string;
  }) {
    const response = await axios.post(
      `${this.urlPrefix}/Data/EntitiesGet`,
      query,
      {
        headers: this.httpAuthHeader
      }
    );
    if (_.isString(response.data)) {
      return [];
    } else {
      return response.data;
    }
  }

  async getLookupLabels(query: {
    LookupId: string;
    MenuId: string;
    LabelIds: string[];
  }) {
    return (await axios.post(`${this.urlPrefix}/Data/GetLookupLabels`, query, {
      headers: this.httpAuthHeader
    })).data;
  }
}
