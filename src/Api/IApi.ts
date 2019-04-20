import { AxiosPromise } from "axios";

export interface IApi {
  setAccessToken(token: string | undefined): void;
  resetAccessToken(): void;
  httpAuthHeader: { Authorization: string };
  login(credentials: { UserName: string; Password: string }): Promise<string>;
  logout(): Promise<void>;
  getMenu(): Promise<any>;
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
  }): Promise<{[key: string]: string}>;
}
