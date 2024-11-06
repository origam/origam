import axios, { AxiosInstance } from "axios";
import {
  ApiEditorProperty, ApiTreeNode,
  IArchitectApi, INewEditorData, MenuItemInfo,
  Package, RuleErrors
} from "src/API/IArchitectApi.ts";


import { TreeNode } from "src/components/lazyLoadedTree/TreeNode.ts";


export class ArchitectApi implements IArchitectApi {

  errorHandler: (error: any) => void;
  axiosInstance: AxiosInstance;

  constructor(errorHandler?: (error: any) => void) {
    this.axiosInstance = this.createAxiosInstance();
    this.errorHandler = errorHandler ?? simpleErrorHandler;
  }

  private createAxiosInstance() {
    const axiosInstance = axios.create({});

    axiosInstance.interceptors.response.use(
      (response) => {
        return response;
      },
      async (error) => {
        if (error.response?.data?.constructor?.name === 'Blob') {
          error.response.data = await error.response.data.text();
        }
        if (!axios.isCancel(error)) {
          this.errorHandler(error);
        }
        throw error;
      }
    );
    return axiosInstance;
  }


  async setActivePackage(packageId: string): Promise<void> {
    await this.axiosInstance.post("/Package/SetActive", {id: packageId})
  }

  async getPackages(): Promise<Package[]> {
    return (await this.axiosInstance.get("/Package/GetAll")).data
  }

  async getTopModelNodes(): Promise<TreeNode[]> {
    return (await this.axiosInstance.get(`/Model/GetTopNodes`)).data;
  }

  async getNodeChildren(node: TreeNode): Promise<TreeNode[]> {
    return (await this.axiosInstance.get(
      `/Model/GetChildren`,
      {
        params: {
          id: node.origamId,
          nodeText: node.nodeText,
          isNonPersistentItem: node.isNonPersistentItem
        }
      })).data;
  }

  async getProperties(schemaItemId: string): Promise<ApiEditorProperty[]> {
    return (await (this.axiosInstance.get("/Editor/EditableProperties", {
      params: {schemaItemId: schemaItemId}
    }))).data;
  }

  async persistChanges(schemaItemId: string, changedProperties: ApiEditorProperty[]): Promise<void> {
    const changes = changedProperties
      .filter(x => !x.readOnly)
      .map(x => {
        return {
          name: x.name,
          value: x.value === undefined || x.value === null ? null : x.value.toString(),
        }
      });
    await this.axiosInstance.post(`/Editor/PersistChanges`, {
      schemaItemId,
      changes
    });
  }

  async checkRules(schemaItemId: string, changedProperties: ApiEditorProperty[]): Promise<RuleErrors[]> {
    const changes = changedProperties
      .filter(x => !x.readOnly)
      .map(x => {
        return {
          name: x.name,
          value: x.value === undefined || x.value === null ? null : x.value.toString(),
        }
      });
    return (await this.axiosInstance.post(`/Editor/CheckRules`, {
      schemaItemId,
      changes
    })).data;
  }

  async deleteSchemaItem(schemaItemId: string) {
    await this.axiosInstance.post("/Model/DeleteSchemaItem",
      {schemaItemId: schemaItemId}
    )
  }

  async getMenuItems(node: ApiTreeNode): Promise<MenuItemInfo[]> {
    return (await this.axiosInstance.get(
      `/Model/GetMenuItems`,
      {
        params: {
          id: node.origamId,
          nodeText: node.nodeText,
          isNonPersistentItem: node.isNonPersistentItem
        }
      })).data;
  }

  async createNew(node: ApiTreeNode, typeName: string): Promise<INewEditorData> {
    return (await this.axiosInstance.post("/Editor/CreateNew",
      {
        nodeId: node.origamId,
        newTypeName: typeName
      }
    )).data;
  }
}

export function simpleErrorHandler(error: any) {
  console.error(error);
  alert(
    error?.response?.data ??
    error?.message ??
    error?.code ??
    "Unknown error");
}


