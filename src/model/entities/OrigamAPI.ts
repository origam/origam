import xmlJs from "xml-js";
import axios, { AxiosInstance } from "axios";

import _ from "lodash";
import {IApi, IUpdateData, IUIGridFilterCoreConfiguration, IEntityExportField, IExcelFile} from "./types/IApi";
import { IAggregationInfo } from "./types/IAggregationInfo";
import { IOrdering } from "./types/IOrderingConfiguration";
import { IColumnSettings } from "./types/IColumnSettings";
import { compareByGroupingIndex } from "./ColumnSettings";
import { TypeSymbol } from "dic/Container";

export enum IAuditLogColumnIndices {
  Id = 0,
  DateTime = 1,
  UserName = 2,
  FieldName = 3,
  OldValue = 4,
  NewValue = 5,
  ActionType = 6,
}

export class OrigamAPI implements IApi {
  constructor(errorHandler: (error: any) => void) {
    this.urlPrefix = "/internalApi";
    this.chatroomsUrlPrefix = "/chatrooms";
    this.axiosInstance = this.createAxiosInstance();
    this.errorHandler = errorHandler;
  }

  private createAxiosInstance() {
    const axiosInstance = axios.create({
      baseURL: this.urlPrefix,
      headers: this.httpAuthHeader,
    });

    axiosInstance.interceptors.response.use(
      (response) => response,
      (error) => {
        if (!axios.isCancel(error)) {
          this.errorHandler(error);
        }
        throw error;
      }
    );
    return axiosInstance;
  }

  errorHandler: (error: any) => void;
  axiosInstance: AxiosInstance;
  urlPrefix: string;
  chatroomsUrlPrefix: string;
  accessToken = "";

  setAccessToken(token: string) {
    this.accessToken = token;
    this.axiosInstance = this.createAxiosInstance();
  }

  resetAccessToken(): void {
    this.accessToken = "";
  }

  get httpAuthHeader() {
    return {
      Authorization: `Bearer ${this.accessToken}`,
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
    return (await this.axiosInstance.post("/User/Login", credentials)).data;
  }

  async logout() {
    return await this.axiosInstance.post("/User/Logout", {});
  }

  async getScreen(id: string) {
    return xmlJs.xml2js(
      (
        await this.axiosInstance.get("/UI/GetUI", {
          params: { id },
          headers: this.httpAuthHeader,
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
    AdditionalRequestParameters: object | undefined;
  }) {
    let requestData;
    if (data.AdditionalRequestParameters) {
      const additionalRequestParameters = data.AdditionalRequestParameters;
      delete data["AdditionalRequestParameters"];
      requestData = { ...data, ...additionalRequestParameters };
    } else {
      requestData = data;
    }
    const result = (await this.axiosInstance.post("/UIService/InitUI", requestData)).data;
    return {
      ...result,
      formDefinition: xmlJs.xml2js(result.formDefinition, {
        addParent: true,
        alwaysChildren: true,
      }),
    };
  }

  async destroyUI(data: { FormSessionId: string }) {
    return (await this.axiosInstance.get(`/UIService/DestroyUI/${data.FormSessionId}`)).data;
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
    const response = (await this.axiosInstance.post("/Data/GetRows", query)).data;

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
    return (await this.axiosInstance.post("/UIService/GetLookupLabels", query)).data;
  }

  async getLookupLabelsEx(
    query: {
      LookupId: string;
      MenuId: string | undefined;
      LabelIds: string[];
    }[]
  ) {
    return (await this.axiosInstance.post("/UIService/GetLookupLabelsEx", query)).data;
  }

  async newEntity(data: { DataStructureEntityId: string; MenuId: string }) {
    return (await this.axiosInstance.post("/Data/NewEmptyRow", data)).data;
  }

  async putEntity(data: {
    DataStructureEntityId: string;
    RowId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }) {
    return (await this.axiosInstance.put("/Data/Row", data)).data;
  }

  async postEntity(data: {
    DataStructureEntityId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }) {
    return (await this.axiosInstance.post("/Data/Row", data)).data;
  }

  async deleteEntity(data: {
    DataStructureEntityId: string;
    RowIdToDelete: string;
    MenuId: string;
  }) {
    return (
      await this.axiosInstance.request({
        url: `${this.urlPrefix}/Data/Row`,
        method: "DELETE",
        data,
        headers: this.httpAuthHeader,
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
    return (await this.axiosInstance.post("/Session/CreateSession", data)).data;
  }

  async deleteSession() {}

  async saveSession(sessionFormIdentifier: string) {
    return (await this.axiosInstance.get(`/UIService/SaveData/${sessionFormIdentifier}`)).data;
  }

  async saveSessionQuery(sessionFormIdentifier: string) {
    return (await this.axiosInstance.get(`/UIService/SaveDataQuery/${sessionFormIdentifier}`)).data;
  }

  async refreshSession(sessionFormIdentifier: string) {
    return (await this.axiosInstance.get(`/UIService/RefreshData/${sessionFormIdentifier}`)).data;
  }

  async sessionChangeMasterRecord(data: {
    SessionFormIdentifier: string;
    Entity: string;
    RowId: string;
  }) {
    return (await this.axiosInstance.post("/Session/ChangeMasterRecord", data)).data;
  }

  async sessionDeleteEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    RowId: string;
  }) {
    return (await this.axiosInstance.post("/Session/DeleteRow", data)).data;
  }

  async sessionCreateEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Values: { [key: string]: any };
    Parameters: { [key: string]: any };
    RequestingGridId: string;
  }) {
    return (await this.axiosInstance.post(`/Session/CreateRow`, data)).data;
  }

  async sessionGetEntity(data: {
    sessionFormIdentifier: string;
    childEntity: string;
    parentRecordId: string;
    rootRecordId: string;
  }) {
    return (
      await this.axiosInstance.get("/Session/Rows", {
        params: data,
        headers: this.httpAuthHeader,
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
    return (await this.axiosInstance.post("/Session/UpdateRow", data)).data;
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
    return (await this.axiosInstance.post("/UIService/GetLookupList", data)).data;
  }

  async getLookupCacheDependencies(data: { LookupIds: string[] }): Promise<any> {
    return (await this.axiosInstance.post("/UIService/GetLookupCacheDependencies", data)).data;
  }

  async getNotificationBoxContent(): Promise<any> {
    return (await this.axiosInstance.get("/UIService/GetNotificationBoxContent")).data;
  }

  async defaultCulture(): Promise<any> {
    return (await this.axiosInstance.get("/UIService/DefaultCulture")).data;
  }

  async initPortal(): Promise<any> {
    const data = (await this.axiosInstance.get("/UIService/InitPortal")).data;

    return {
      ...data,
      menu: xmlJs.xml2js(data.menu, { addParent: true, alwaysChildren: true }),
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
      (
        await this.axiosInstance.post("/UIService/MasterRecord", data, {
          headers: this.httpAuthHeader,
          cancelToken: canceller && canceller.token,
        })
      )?.data ?? []
    );
  }

  async restoreData(data: { SessionFormIdentifier: string; ObjectId: string }) {
    return (await this.axiosInstance.post("/UIService/RestoreData", data)).data;
  }

  async updateObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    UpdateData: IUpdateData[];
  }): Promise<any> {
    return (await this.axiosInstance.post("/UIService/UpdateObject", data)).data;
  }

  async createObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Values: { [key: string]: any };
    Parameters: { [key: string]: any };
    RequestingGridId: string;
  }): Promise<any> {
    return (await this.axiosInstance.post("/UIService/CreateObject", data)).data;
  }

  async copyObject(data: {
    Entity: string;
    SessionFormIdentifier: string;
    ForcedValues: {};
    RequestingGridId: string;
    OriginalId: string;
    Entities: string[];
  }): Promise<any> {
    return (await this.axiosInstance.post("/UIService/CopyObject", data)).data;
  }

  async deleteObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
  }): Promise<any> {
    return (await this.axiosInstance.post("/UIService/DeleteObject", data)).data;
  }

  async deleteObjectInOrderedList(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
    OrderProperty: string;
    UpdatedOrderValues: {};
  }): Promise<any> {
    return (await this.axiosInstance.post("/UIService/DeleteObjectInOrderedList", data)).data;
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
    return (await this.axiosInstance.post("/UIService/ExecuteActionQuery", data)).data;
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
    return (await this.axiosInstance.post(`/UIService/ExecuteAction`, data)).data;
  }

  async getGroups(data: {
    MenuId: string;
    DataStructureEntityId: string;
    Filter: string | undefined;
    Ordering: IOrdering[];
    RowLimit: number;
    GroupBy: string;
    MasterRowId: string | undefined;
    GroupByLookupId: string | undefined;
    SessionFormIdentifier: string | undefined;
    AggregatedColumns: IAggregationInfo[] | undefined;
  }): Promise<any[]> {
    return (await this.axiosInstance.post(`/UIService/GetGroups`, data)).data;
  }

  async getAggregations(data: {
    MenuId: string;
    DataStructureEntityId: string;
    Filter: string | undefined;
    AggregatedColumns: IAggregationInfo[];
    SessionFormIdentifier: string | undefined;
    MasterRowId: string | undefined;
  }): Promise<any[]> {
    return (await this.axiosInstance.post(`/UIService/GetAggregations`, data)).data;
  }

  async getRows(data: {
    MenuId: string;
    SessionFormIdentifier: string;
    DataStructureEntityId: string;
    Filter: string;
    Ordering: IOrdering[];
    RowLimit: number;
    RowOffset: number;
    ColumnNames: string[];
    MasterRowId: string | undefined;
    FilterLookups?: { [key: string]: string };
  }): Promise<any> {
    return (await this.axiosInstance.post(`/UIService/GetRows`, data)).data;
  }

  async getData(data: {
    SessionFormIdentifier: string;
    ChildEntity: string;
    ParentRecordId: string;
    RootRecordId: string;
  }): Promise<any> {
    return (await this.axiosInstance.post(`/UIService/GetData`, data)).data;
  }

  getReportFromMenu(data: { menuId: string }): Promise<any>;

  async getReportFromMenu(data: { menuId: string }): Promise<string> {
    return (await this.axiosInstance.get(`/UIService/ReportFromMenu/${data.menuId}`)).data;
  }

  async getRowStates(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Ids: string[];
  }): Promise<any> {
    return (await this.axiosInstance.post(`/UIService/RowStates`, data)).data;
  }

  async saveFavorites(data: {
    ConfigXml: string;
  }): Promise<any> {
    return (await this.axiosInstance.post(`/UIService/SaveFavorites`, data)).data;
  }

  async getWorkQueueList(): Promise<any> {
    return (await this.axiosInstance.get(`/UIService/WorkQueueList`)).data;
  }

  async getChatroomList(): Promise<any> {
    return (await axios.get(`${this.chatroomsUrlPrefix}/Chat`, { headers: this.httpAuthHeader }))
      .data;
  }

  async saveObjectConfiguration(data: {
    instanceId: string;
    columnSettings: IColumnSettings[];
    defaultView: string;
  }): Promise<any> {
    const columnFields = data.columnSettings
      .filter((settings) => settings.groupingIndex !== undefined)
      .sort(compareByGroupingIndex)
      .map(
        (setting) =>
          `<column 
          groupingField="${setting.propertyId}" 
        />`
      );
    const columnsProps = data.columnSettings.map(
      (setting) =>
        `<column 
            property="${setting.propertyId}" 
            isHidden="${setting.isHidden ? "true" : "false"}" 
            width="${setting.width}" 
            aggregationType="${setting.aggregationTypeNumber}" 
          />`
    );

    await this.axiosInstance.post(`/UIService/SaveObjectConfig`, {
      ObjectinstanceId: data.instanceId,
      Section: "columnWidths",
      SettingsData: columnsProps.concat(columnFields).join(""),
    });
    await this.axiosInstance.post(`/UIService/SaveObjectConfig`, {
      ObjectinstanceId: data.instanceId,
      Section: "defaultView",
      SettingsData: `<view id="${data.defaultView}" />`,
    });
  }

  async saveSplitPanelConfiguration(data: { InstanceId: string; Position: number }): Promise<any> {
    return (await this.axiosInstance.post(`/UIService/SaveSplitPanelConfig`, data)).data;
  }

  async workflowAbort(data: { sessionFormIdentifier: string }): Promise<any> {
    const result = (
      await this.axiosInstance.get(`/UIService/WorkflowAbort/${data.sessionFormIdentifier}`)
    ).data;

    return {
      ...result,
      formDefinition: xmlJs.xml2js(result.formDefinition, {
        addParent: true,
        alwaysChildren: true,
      }),
    };
  }

  async workflowRepeat(data: { sessionFormIdentifier: string }): Promise<any> {
    const result = (
      await this.axiosInstance.get(`/UIService/WorkflowRepeat/${data.sessionFormIdentifier}`)
    ).data;

    return {
      ...result,
      formDefinition: xmlJs.xml2js(result.formDefinition, {
        addParent: true,
        alwaysChildren: true,
      }),
    };
  }

  async workflowNext(data: {
    sessionFormIdentifier: string;
    CachedFormIds: string[];
  }): Promise<any> {
    const result = (
      await this.axiosInstance.post(`/UIService/WorkflowNext`, {
        SessionFormIdentifier: data.sessionFormIdentifier,
        CachedFormIds: data.CachedFormIds,
      })
    ).data;

    return {
      ...result,
      formDefinition: xmlJs.xml2js(result.formDefinition, {
        addParent: true,
        alwaysChildren: true,
      }),
    };
  }

  async workflowNextQuery(data: { sessionFormIdentifier: string }): Promise<any> {
    return (
      await this.axiosInstance.get(`/UIService/WorkflowNextQuery/${data.sessionFormIdentifier}`)
    ).data;
  }

  async getRecordInfo(data: { MenuId: string; DataStructureEntityId: string; RowId: string }) {
    return (await this.axiosInstance.post(`/UIService/GetRecordTooltip`, data)).data.tooltip;
  }

  async getRecordAudit(data: { MenuId: string; DataStructureEntityId: string; RowId: string }) {
    return (await this.axiosInstance.post(`/UIService/GetAudit`, data)).data.data.map(
      (row: any[]) => ({
        id: row[IAuditLogColumnIndices.Id],
        dateTime: row[IAuditLogColumnIndices.DateTime],
        userName: row[IAuditLogColumnIndices.UserName],
        fieldName: row[IAuditLogColumnIndices.FieldName],
        oldValue: row[IAuditLogColumnIndices.OldValue],
        newValue: row[IAuditLogColumnIndices.NewValue],
        actionType: row[IAuditLogColumnIndices.ActionType],
      })
    );
  }

  async getReport(data: { reportUrl: string }): Promise<any> {
    return (await this.axiosInstance.get(data.reportUrl)).data;
  }

  async getDownloadToken(data: {
    SessionFormIdentifier: string;
    MenuId: string;
    DataStructureEntityId: string;
    RowId: string;
    Property: string;
    FileName: string;
    Entity: string;
    parameters: any;
  }): Promise<any> {
    return (
      await this.axiosInstance.post(`/Blob/DownloadToken`, {
        SessionFormIdentifier: data.SessionFormIdentifier,
        MenuId: data.MenuId,
        DataStructureEntityId: data.DataStructureEntityId,
        Entity: data.Entity,
        RowId: data.RowId,
        Property: data.Property,
        IsPreview: false,
        Parameters: data.parameters,
      })
    ).data;
  }

  async getBlob(data: { downloadToken: string }) {
    window.open(`${this.urlPrefix}/Blob/${data.downloadToken}`);
    /*return (
await axios.get(`${this.urlPrefix}/Blob/${data.downloadToken}`, {
 headers: this.httpAuthHeader,
})
).data;*/
  }

  async getUploadToken(data: {
    SessionFormIdentifier: string;
    MenuId: string;
    DataStructureEntityId: string;
    Entity: string;
    RowId: string;
    Property: string;
    FileName: string;
    DateCreated: string;
    DateLastModified: string;
    parameters: any;
  }): Promise<any> {
    return (
      await this.axiosInstance.post(`/Blob/UploadToken`, {
        SessionFormIdentifier: data.SessionFormIdentifier,
        MenuId: data.MenuId,
        DataStructureEntityId: data.DataStructureEntityId,
        Entity: data.Entity,
        RowId: data.RowId,
        Property: data.Property,
        IsPreview: false,
        Parameters: data.parameters,
        SubmitImmediately: "true",
      })
    ).data;
  }

  async putBlob(
    data: { uploadToken: string; fileName: string; file: any },
    onUploadProgress?: (event: any) => void
  ): Promise<any> {
    return (
      await this.axiosInstance.post(`/Blob/${data.uploadToken}/${data.fileName}`, data.file, {
        headers: { ...this.httpAuthHeader, "content-type": "application/octet-stream" },
        onUploadProgress,
      })
    ).data;
  }

  async pendingChanges(data: { sessionFormIdentifier: string }): Promise<any[]> {
    return (await this.axiosInstance.get(`/UIService/PendingChanges/${data.sessionFormIdentifier}`))
      .data;
  }

  async changes(data: {
    SessionFormIdentifier: string;
    Entity: string;
    RowId: string;
  }): Promise<any[]> {
    return (
      await this.axiosInstance.post(`/UIService/Changes`, {
        SessionFormIdentifier: data.SessionFormIdentifier,
        Entity: data.Entity,
        RowId: data.RowId,
      })
    ).data;
  }

  async saveFilter(data: {
    DataStructureEntityId: string;
    PanelId: string;
    Filter: IUIGridFilterCoreConfiguration
    IsDefault: boolean;
  }): Promise<string> {
    return (
      await this.axiosInstance.post(`/UIService/SaveFilter`, data)
    ).data;
  }


  async deleteFilter(data: { filterId: string }): Promise<any[]> {
    return (
      await this.axiosInstance.post(`/UIService/DeleteFilter`, data)
    ).data;
  }

  async resetDefaultFilter(data: { SessionFormIdentifier: string; PanelInstanceId: string }): Promise<any[]> {
    return (
      await this.axiosInstance.post(`/UIService/ResetDefaultFilter`, data)
    ).data;
  }

  async setDefaultFilter(data: {
    SessionFormIdentifier: string;
    PanelInstanceId: string;
    DataStructureEntityId: string;
    PanelId: string;
    Filter: IUIGridFilterCoreConfiguration;
    IsDefault: boolean;
  }): Promise<any[]> {
    return (
      await this.axiosInstance.post(`/UIService/SetDefaultFilter`, data)
    ).data;
  }

  async search(searchTerm: string) {
    return (await this.axiosInstance.get(`/Search/${searchTerm}`)).data;
  }

  async getMenuId(data: { LookupId: string; ReferenceId: string}): Promise<string>  {
    return (await this.axiosInstance.post(`/UIService/GetMenuId`, data)).data;
  }

  async getExcelFileUrl(data: {
    Entity: string;
    Fields: IEntityExportField[];
    RowIds: string[];
    SessionFormIdentifier: string;}): Promise<string>
  {
     return (await this.axiosInstance.post(`/ExcelExport/GetFileUrl`, data)).data;
  }
}

export const IOrigamAPI = TypeSymbol<OrigamAPI>("IOrigamAPI");
