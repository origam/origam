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
    return (await axios.post(`${this.urlPrefix}/User/Login`, credentials))
      .data;
  }

  async logout() {
    return;
  }

  async getMenu() {
    return xmlJs.xml2js(
      (await axios.get(`${this.urlPrefix}/UI/GetMenu`, {
        headers: this.httpAuthHeader
      })).data,
      { addParent: true, alwaysChildren: true }
    );
  }

  async getScreen(id: string) {
    return xmlJs.xml2js(
      (await axios.get(`${this.urlPrefix}/UI/GetUI`, {
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
      `${this.urlPrefix}/Data/GetRows`,
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

  async newEntity(data: { DataStructureEntityId: string; MenuId: string }) {
    return (await axios.post(`${this.urlPrefix}/Data/NewEmptyRow`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async putEntity(data: {
    DataStructureEntityId: string;
    RowId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }) {
    return (await axios.put(`${this.urlPrefix}/Data/Row`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async postEntity(data: {
    DataStructureEntityId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }) {
    return (await axios.post(`${this.urlPrefix}/Data/Row`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async deleteEntity(data: {
    DataStructureEntityId: string;
    RowIdToDelete: string;
    MenuId: string;
  }) {
    return (await axios.request({
      url: `${this.urlPrefix}/Data/Row`,
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
    return (await axios.post(`${this.urlPrefix}/Session/CreateSession`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async deleteSession() {}

  async saveSession(data: { SessionId: string }) {
    return (await axios.post(`${this.urlPrefix}/Session/SaveData`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async reloadSession() {
    // TODO
  }

  async sessionChangeMasterRecord(data: {
    SessionFormIdentifier: string;
    Entity: string;
    RowId: string;
  }) {
    return (await axios.post(
      `${this.urlPrefix}/Session/ChangeMasterRecord`,
      data,
      { headers: this.httpAuthHeader }
    )).data;
  }

  async sessionDeleteEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    RowId: string;
  }) {
    return (await axios.post(`${this.urlPrefix}/Session/DeleteRow`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async sessionCreateEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Values: { [key: string]: any };
    Parameters: { [key: string]: any };
    RequestingGridId: string;
  }) {
    return (await axios.post(`${this.urlPrefix}/Session/CreateRow`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async sessionGetEntity(data: {
    sessionFormIdentifier: string;
    childEntity: string;
    parentRecordId: string;
    rootRecordId: string;
  }) {
    return (await axios.get(`${this.urlPrefix}/Session/Rows`, {
      params: data,
      headers: this.httpAuthHeader
    })).data;
  }

  async sessionUpdateEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
    Property: string;
    NewValue: any;
  }): Promise<any> {
    return (await axios.post(`${this.urlPrefix}/Session/UpdateRow`, data, {
      headers: this.httpAuthHeader
    })).data;
  }

  async getLookupListEx(data: {
    DataStructureEntityId: string;
    ColumnNames: string[];
    Property: string;
    Id: string;
    LookupId: string;
    ShowUniqueValues: boolean;
    SearchText: string;
    PageSize: number;
    PageNumber: number;
    MenuId: string;
  }): Promise<any> {
    return (await axios.post(`${this.urlPrefix}/Data/GetLookupListEx`, data, {
      headers: this.httpAuthHeader
    })).data;
  }
}
