import xmlJs from "xml-js";
import axios from "axios";

import _ from "lodash";
import { IApi } from "./types/IApi";
import { userManager } from "oauth";
import { IAggregation } from "./types/IAggregation";

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

  createCanceller(): () => void {
    const tokenSource = axios.CancelToken.source();
    const token = tokenSource.token;
    const canceller = () => {
      tokenSource.cancel();
    };
    canceller.token = token;
    return canceller as any;
  }

  _getCancelToken(canceller: any) {
    return canceller.any;
  }

  async login(credentials: { UserName: string; Password: string }) {
    return (await axios.post(`${this.urlPrefix}/User/Login`, credentials)).data;
  }

  async logout() {
    return await axios.post(
      `${this.urlPrefix}/User/Logout`,
      {},
      {
        headers: this.httpAuthHeader
      }
    );
  }
  /*
  async getMenu() {
    return xmlJs.xml2js(
      (await axios.get(`${this.urlPrefix}/UI/GetMenu`, {
        headers: this.httpAuthHeader
      })).data,
      { addParent: true, alwaysChildren: true }
    );
  }
*/

  async getScreen(id: string) {
    return xmlJs.xml2js(
      (
        await axios.get(`${this.urlPrefix}/UI/GetUI`, {
          params: { id },
          headers: this.httpAuthHeader
        })
      ).data,
      { addParent: true, alwaysChildren: true }
    );
  }

  async initUI(data: {
    Type: string;
    FormSessionId: string | undefined;
    IsNewSession: boolean;
    RegisterSession: boolean;
    DataRequested: boolean;
    ObjectId: string;
    Caption: string;
    Parameters: { [key: string]: any };
  }) {
    const result = (
      await axios.post(`${this.urlPrefix}/UIService/InitUI`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
    return {
      ...result,
      formDefinition: xmlJs.xml2js(result.formDefinition, {
        addParent: true,
        alwaysChildren: true
      })
    };
  }

  async destroyUI(data: { FormSessionId: string }) {
    return (
      await axios.get(`${this.urlPrefix}/UIService/DestroyUI/${data.FormSessionId}`, {
        headers: this.httpAuthHeader
      })
    ).data;
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
    const response = await axios.post(`${this.urlPrefix}/Data/GetRows`, query, {
      headers: this.httpAuthHeader
    });
    if (_.isString(response.data)) {
      return [];
    } else {
      return response.data;
    }
  }

  async getLookupLabels(query: {
    LookupId: string;
    MenuId: string | undefined;
    LabelIds: string[];
  }) {
    return (
      await axios.post(`${this.urlPrefix}/UIService/GetLookupLabels`, query, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async getLookupLabelsEx(
    query: {
      LookupId: string;
      MenuId: string | undefined;
      LabelIds: string[];
    }[]
  ) {
    return (
      await axios.post(`${this.urlPrefix}/UIService/GetLookupLabelsEx`, query, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async newEntity(data: { DataStructureEntityId: string; MenuId: string }) {
    return (
      await axios.post(`${this.urlPrefix}/Data/NewEmptyRow`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async putEntity(data: {
    DataStructureEntityId: string;
    RowId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }) {
    return (
      await axios.put(`${this.urlPrefix}/Data/Row`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async postEntity(data: {
    DataStructureEntityId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }) {
    return (
      await axios.post(`${this.urlPrefix}/Data/Row`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async deleteEntity(data: {
    DataStructureEntityId: string;
    RowIdToDelete: string;
    MenuId: string;
  }) {
    return (
      await axios.request({
        url: `${this.urlPrefix}/Data/Row`,
        method: "DELETE",
        data,
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async createEntity() {
    // TODO
  }

  async createSession(data: {
    MenuId: string;
    Parameters: { [key: string]: any };
    InitializeStructure: boolean;
  }) {
    return (
      await axios.post(`${this.urlPrefix}/Session/CreateSession`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async deleteSession() { }

  async saveSession(sessionFormIdentifier: string) {
    return (
      await axios.get(`${this.urlPrefix}/UIService/SaveData/${sessionFormIdentifier}`, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async saveSessionQuery(sessionFormIdentifier: string) {
    return (
      await axios.get(`${this.urlPrefix}/UIService/SaveDataQuery/${sessionFormIdentifier}`, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async refreshSession(sessionFormIdentifier: string) {
    return (
      await axios.get(`${this.urlPrefix}/UIService/RefreshData/${sessionFormIdentifier}`, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async sessionChangeMasterRecord(data: {
    SessionFormIdentifier: string;
    Entity: string;
    RowId: string;
  }) {
    return (
      await axios.post(`${this.urlPrefix}/Session/ChangeMasterRecord`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async sessionDeleteEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    RowId: string;
  }) {
    return (
      await axios.post(`${this.urlPrefix}/Session/DeleteRow`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async sessionCreateEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Values: { [key: string]: any };
    Parameters: { [key: string]: any };
    RequestingGridId: string;
  }) {
    return (
      await axios.post(`${this.urlPrefix}/Session/CreateRow`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async sessionGetEntity(data: {
    sessionFormIdentifier: string;
    childEntity: string;
    parentRecordId: string;
    rootRecordId: string;
  }) {
    return (
      await axios.get(`${this.urlPrefix}/Session/Rows`, {
        params: data,
        headers: this.httpAuthHeader
      })
    ).data;
  }

  // TODO: Remove this method.
  async sessionUpdateEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
    Property: string;
    NewValue: any;
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/Session/UpdateRow`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async getLookupList(data: {
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
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/GetLookupList`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async initPortal(): Promise<any> {
    const { data } = await axios.get(`${this.urlPrefix}/UIService/InitPortal`, {
      headers: this.httpAuthHeader
    });
    return {
      ...data,
      menu: xmlJs.xml2js(data.menu, { addParent: true, alwaysChildren: true })
    };
  }

  async setMasterRecord(
    data: {
      SessionFormIdentifier: string;
      Entity: string;
      RowId: string;
    },
    canceller?: any
  ) {
    return (
      await axios.post(`${this.urlPrefix}/UIService/MasterRecord`, data, {
        headers: this.httpAuthHeader,
        cancelToken: canceller && canceller.token
      })
    ).data;
  }

  async updateObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
    Values: { [key: string]: any };
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/UpdateObject`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async createObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Values: { [key: string]: any };
    Parameters: { [key: string]: any };
    RequestingGridId: string;
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/CreateObject`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async deleteObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/DeleteObject`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async executeActionQuery(data: {
    SessionFormIdentifier: string;
    Entity: string;
    ActionType: string;
    ActionId: string;
    ParameterMappings: { [key: string]: any };
    SelectedItems: string[];
    InputParameters: { [key: string]: any };
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/ExecuteActionQuery`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async executeAction(data: {
    SessionFormIdentifier: string;
    Entity: string;
    ActionType: string;
    ActionId: string;
    ParameterMappings: { [key: string]: any };
    SelectedItems: string[];
    InputParameters: { [key: string]: any };
    RequestingGrid: string;
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/ExecuteAction`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async getGroups(data: {
    MenuId: string;
    DataStructureEntityId: string;
    Filter: string | undefined;
    Ordering: string[] | undefined;
    RowLimit: number; GroupBy: string;
    MasterRowId: string | undefined;
    GroupByLookupId: string | undefined;
    SessionFormIdentifier: string | undefined;
    AggregatedColumn: IAggregation[] | undefined;
  }): Promise<any[]> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/GetGroups`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }
  async getAggregations(data: {
    MenuId: string;
    DataStructureEntityId: string;
    Filter: string | undefined;
    AggregatedColumn: IAggregation[];
    SessionFormIdentifier: string | undefined;
    MasterRowId: string | undefined;
  }): Promise<any[]> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/GetAggregations`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async getRows(data: {
    MenuId: string;
    SessionFormIdentifier: string;
    DataStructureEntityId: string;
    Filter: string;
    Ordering: string[][];
    RowLimit: number;
    ColumnNames: string[];
    MasterRowId: string | undefined;
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/GetRows`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async getData(data: {
    SessionFormIdentifier: string;
    ChildEntity: string;
    ParentRecordId: string;
    RootRecordId: string;
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/GetData`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async getRowStates(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Ids: string[];
  }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/RowStates`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async getWorkQueueList(): Promise<any> {
    return (
      await axios.get(`${this.urlPrefix}/UIService/WorkQueueList`, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async saveObjectConfiguration(data: {
    instanceId: string;
    columnSettings: Array<{
      propertyId: string;
      isHidden: boolean;
      width: number;
    }>;
    defaultView: string;
  }): Promise<any> {
    await axios.post(
      `${this.urlPrefix}/UIService/SaveObjectConfig`,
      {
        ObjectinstanceId: data.instanceId,
        Section: "columnWidths",
        SettingsData: data.columnSettings
          .map(
            setting =>
              `<column 
                  property="${setting.propertyId}" 
                  isHidden="${setting.isHidden ? "true" : "false"}" 
                  width="${setting.width}" 
                  aggregationType="0" 
                />`
          )
          .join("")
      },
      {
        headers: this.httpAuthHeader
      }
    );
    await axios.post(
      `${this.urlPrefix}/UIService/SaveObjectConfig`,
      {
        ObjectinstanceId: data.instanceId,
        Section: "defaultView",
        SettingsData: `<view id="${data.defaultView}" />`
      },
      {
        headers: this.httpAuthHeader
      }
    );
  }

  async saveSplitPanelConfiguration(data: { InstanceId: string; Position: number }): Promise<any> {
    return (
      await axios.post(`${this.urlPrefix}/UIService/SaveSplitPanelConfig`, data, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async workflowAbort(data: { sessionFormIdentifier: string }): Promise<any> {
    return (
      await axios.get(`${this.urlPrefix}/UIService/WorkflowAbort/${data.sessionFormIdentifier}`, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async workflowRepeat(data: { sessionFormIdentifier: string }): Promise<any> {
    return (
      await axios.get(`${this.urlPrefix}/UIService/WorkflowRepeat/${data.sessionFormIdentifier}`, {
        headers: this.httpAuthHeader
      })
    ).data;
  }

  async getRecordInfo(data: { MenuId: string; DataStructureEntityId: string; RowId: string }) {
    return (
      await axios.post(`${this.urlPrefix}/UIService/GetRecordTooltip`, data, {
        headers: this.httpAuthHeader
      })
    ).data.tooltip;
  }

  async getRecordAudit(data: { MenuId: string; DataStructureEntityId: string; RowId: string }) {
    return (
      await axios.post(`${this.urlPrefix}/UIService/GetAudit`, data, {
        headers: this.httpAuthHeader
      })
    ).data.data.map((row: any[]) => ({
      id: row[IAuditLogColumnIndices.Id],
      dateTime: row[IAuditLogColumnIndices.DateTime],
      userName: row[IAuditLogColumnIndices.UserName],
      fieldName: row[IAuditLogColumnIndices.FieldName],
      oldValue: row[IAuditLogColumnIndices.OldValue],
      newValue: row[IAuditLogColumnIndices.NewValue],
      actionType: row[IAuditLogColumnIndices.ActionType]
    }));
  }

  async getReport(data: { reportUrl: string }): Promise<any> {
    return (
      await axios.get(`${data.reportUrl}`, {
        headers: this.httpAuthHeader
      })
    ).data;
  }
}

export enum IAuditLogColumnIndices {
  Id = 0,
  DateTime = 1,
  UserName = 2,
  FieldName = 3,
  OldValue = 4,
  NewValue = 5,
  ActionType = 6
}
