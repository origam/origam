import { AxiosPromise } from "axios";

export interface IApi {
  accessToken: string;

  setAccessToken(token: string | undefined): void;

  resetAccessToken(): void;

  httpAuthHeader: { Authorization: string };

  createCanceller(): () => void;

  login(credentials: { UserName: string; Password: string }): Promise<string>;

  logout(): Promise<any>;

  // getMenu(): Promise<any>;

  getScreen(id: string): Promise<any>;

  getEntities(query: {
    MenuId: string;
    DataStructureEntityId: string;
    Ordering: Array<[string, string]>;
    ColumnNames: string[];
    Filter: string;
    RowLimit?: number;
    MasterRowId?: string;
  }): Promise<any>;

  getLookupLabels(query: {
    LookupId: string;
    MenuId: string | undefined;
    LabelIds: string[];
  }): Promise<{ [key: string]: string }>;

  getLookupLabelsEx(
    query: {
      LookupId: string;
      MenuId: string | undefined;
      LabelIds: string[];
    }[]
  ): Promise<{ [key: string]: { [key: string]: string } }>;

  newEntity(data: {
    DataStructureEntityId: string;
    MenuId: string;
  }): Promise<any>;

  putEntity(data: {
    DataStructureEntityId: string;
    RowId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }): Promise<any>;

  postEntity(data: {
    DataStructureEntityId: string;
    NewValues: { [key: string]: any };
    MenuId: string;
  }): Promise<any>;

  deleteEntity(data: {
    DataStructureEntityId: string;
    RowIdToDelete: string;
    MenuId: string;
  }): Promise<any>;

  createSession(data: {
    MenuId: string;
    Parameters: { [key: string]: any };
    InitializeStructure: boolean;
  }): Promise<any>;

  saveSession(sessionFormIdentifier: string): Promise<any>;
  saveSessionQuery(sessionFormIdentifier: string): Promise<any>;
  refreshSession(sessionFormIdentifier: string): Promise<any>;

  sessionChangeMasterRecord(data: {
    SessionFormIdentifier: string;
    Entity: string;
    RowId: string;
  }): Promise<any>;

  sessionGetEntity(data: {
    sessionFormIdentifier: string;
    childEntity: string;
    parentRecordId: string;
    rootRecordId: string;
  }): Promise<any>;

  sessionUpdateEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
    Property: string;
    NewValue: any;
  }): Promise<any>;

  sessionCreateEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Values: { [key: string]: any };
    Parameters: { [key: string]: any };
    RequestingGridId: string;
  }): Promise<any>;

  sessionDeleteEntity(data: {
    SessionFormIdentifier: string;
    Entity: string;
    RowId: string;
  }): Promise<any>;

  getLookupList(data: {
    SessionFormIdentifier?: string;
    Entity?: string;
    DataStructureEntityId?: string;
    ColumnNames: string[];
    Property: string;
    Id: string;
    MenuId: string;
    LookupId: string;
    Parameters?: { [key: string]: any };
    ShowUniqueValues: boolean;
    SearchText: string;
    PageSize: number;
    PageNumber: number;
  }): Promise<any>;

  initPortal(): Promise<any>;
  initUI(data: {
    Type: string;
    FormSessionId: string | undefined;
    IsNewSession: boolean;
    RegisterSession: boolean;
    DataRequested: boolean;
    ObjectId: string;
    Parameters: { [key: string]: any } | undefined;
  }): Promise<any>;
  destroyUI(data: { FormSessionId: string }): Promise<any>;

  setMasterRecord(
    data: {
      SessionFormIdentifier: string;
      Entity: string;
      RowId: string;
    },
    canceller?: any
  ): Promise<any>;

  updateObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
    Values: { [key: string]: any };
  }): Promise<any>;

  createObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Values: { [key: string]: any };
    Parameters: { [key: string]: any };
    RequestingGridId: string;
  }): Promise<any>;

  deleteObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
  }): Promise<any>;

  executeActionQuery(data: {
    SessionFormIdentifier: string;
    Entity: string;
    ActionType: string;
    ActionId: string;
    ParameterMappings: { [key: string]: any };
    SelectedItems: string[];
    InputParameters: { [key: string]: any };
  }): Promise<any>;

  executeAction(data: {
    SessionFormIdentifier: string;
    Entity: string;
    ActionType: string;
    ActionId: string;
    ParameterMappings: { [key: string]: any };
    SelectedItems: string[];
    InputParameters: { [key: string]: any };
    RequestingGrid: string;
  }): Promise<any>;

  getRows(data: {
    MenuId: string;
    DataStructureEntityId: string;
    Filter: string;
    Ordering: string[][];
    RowLimit: number;
    ColumnNames: string[];
    MasterRowId: string | undefined;
  }): Promise<any>;

  getData(data: {
    SessionFormIdentifier: string;
    ChildEntity: string;
    ParentRecordId: string;
    RootRecordId: string;
  }): Promise<any>;

  getRowStates(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Ids: string[];
  }): Promise<any>;

  getWorkQueueList(): Promise<any>;

  saveObjectConfiguration(data: {
    instanceId: string;
    columnSettings: Array<{
      propertyId: string;
      width: number;
      isHidden: boolean;
    }>;
    defaultView: string;
  }): Promise<any>;

  saveSplitPanelConfiguration(data: {
    InstanceId: string;
    Position: number;
  }): Promise<any>;

  workflowAbort(data: { sessionFormIdentifier: string }): Promise<any>;

  workflowRepeat(data: { sessionFormIdentifier: string }): Promise<any>;

  getRecordInfo(data: {
    MenuId: string;
    DataStructureEntityId: string;
    RowId: string;
  }): Promise<any>;

  getRecordAudit(data: {
    MenuId: string;
    DataStructureEntityId: string;
    RowId: string;
  }): Promise<
    Array<{
      id: string;
      dateTime: string;
      userName: string;
      fieldName: string;
      oldValue: string | null;
      newValue: string | null;
      actionType: number;
    }>
  >;
}
