import { AxiosPromise } from "axios";

export interface IApi {
  accessToken: string;
  
  setAccessToken(token: string | undefined): void;

  resetAccessToken(): void;

  httpAuthHeader: { Authorization: string };

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
    MenuId: string;
    LabelIds: string[];
  }): Promise<{ [key: string]: string }>;

  newEntity(data: {
    DataStructureEntityId: string;
    MenuId: string;
  }): Promise<any>

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

  saveSession(data: { SessionId: string }): Promise<any>;

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

  getLookupListEx(data: {
    DataStructureEntityId: string;
    ColumnNames: string[];
    Property: string;
    Id: string;
    MenuId: string;
    LookupId: string;
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
  }): Promise<any>;

  updateObject(data: {
    SessionFormIdentifier: string;
    Entity: string;
    Id: string;
    Values: {[key: string]: any}
  }): Promise<any>;
}
