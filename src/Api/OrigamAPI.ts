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

  async putEntity(data: {
    DataStructureEntityId: string;
    RowId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }) {
    return (await axios.put(`${this.urlPrefix}/Data/Entities`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async postEntity(data: {
    DataStructureEntityId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }) {
    return (await axios.post(`${this.urlPrefix}/Data/Entities`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async deleteEntity(data: {
    DataStructureEntityId: string;
    RowIdToDelete: string;
    MenuId: string;
  }) {
    return (await axios.request({
      url: `${this.urlPrefix}/Data/Entities`,
      method: "DELETE",
      data,
      headers: this.httpAuthHeader
    })).data;
  }

  async createEntity() {
    // TODO
  }

  async createSession(data: {
    MenuId: string;
    Parameters: { [key: string]: any };
    InitializeStructure: boolean;
  }) {
    return (await axios.post(`${this.urlPrefix}/Sessions/New`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async deleteSession() {}

  async saveSession() {}

  async reloadSession() {
    // TODO
  }

  async deleteSessionEntity() {}

  async createSessionEntity() {}

  async updateSessionEntity() {}

  async getSessionEntity(data: {
    sessionFormIdentifier: string;
    childEntity: string;
    parentRecordId: string;
    rootRecordId: string;
  }) {
    return (await axios.get(`${this.urlPrefix}/Sessions/EntityData`, {
      params: data,
      headers: this.httpAuthHeader
    })).data;
  }
}
