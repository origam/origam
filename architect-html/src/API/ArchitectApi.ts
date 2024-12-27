import axios, { AxiosInstance } from "axios";
import {
  IApiTreeNode,
  IArchitectApi,
  IApiEditorData,
  IMenuItemInfo,
  ApiControl,
  IModelChange,
  IPropertyChange,
  ISectionEditorModel, IUpdatePropertiesResult, IPackagesInfo
} from "src/API/IArchitectApi.ts";

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

  async getPackages(): Promise<IPackagesInfo> {
    return (await this.axiosInstance.get("/Package/GetAll")).data
  }

  async getTopModelNodes(): Promise<IApiTreeNode[]> {
    return (await this.axiosInstance.get(`/Model/GetTopNodes`)).data;
  }

  async getNodeChildren(node: IApiTreeNode): Promise<IApiTreeNode[]> {
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

  async openEditor(schemaItemId: string): Promise<IApiEditorData> {
    return (await (this.axiosInstance.post("/Editor/OpenEditor",
      {schemaItemId: schemaItemId}))).data;
  }

  async closeEditor(schemaItemId: string) {
    await (this.axiosInstance.post("/Editor/CloseEditor",
      {schemaItemId: schemaItemId}));
  }

  async persistChanges(schemaItemId: string): Promise<void> {
    await this.axiosInstance.post(`/Editor/PersistChanges`, {
      schemaItemId
    });
  }

  async updateProperties(schemaItemId: string, changes: IPropertyChange[]): Promise<IUpdatePropertiesResult> {
    return (await this.axiosInstance.post(`/PropertyEditor/Update`, {
      schemaItemId,
      changes
    })).data;
  }

  async deleteSchemaItem(schemaItemId: string) {
    await this.axiosInstance.post("/Model/DeleteSchemaItem",
      {schemaItemId: schemaItemId}
    )
  }

  async getMenuItems(node: IApiTreeNode): Promise<IMenuItemInfo[]> {
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

  async getOpenEditors(): Promise<IApiEditorData[]> {
    return (await this.axiosInstance.get(
      `/Editor/GetOpenEditors`)).data;
  }

  async createNode(node: IApiTreeNode, typeName: string): Promise<IApiEditorData> {
    return (await this.axiosInstance.post("/Editor/CreateNode",
      {
        nodeId: node.origamId,
        newTypeName: typeName
      }
    )).data;
  }

  async updateSectionEditor(args: {
    schemaItemId: string | undefined,
    name: string,
    selectedDataSourceId: string,
    modelChanges: IModelChange[]
  }): Promise<ISectionEditorModel> {
    return (await this.axiosInstance.post(`/SectionEditor/Update`, args)).data;
  }

  async createSectionEditorItem(
    args: {
      editorSchemaItemId: string,
      parentControlSetItemId: string,
      componentType: string,
      fieldName?: string,
      top: number,
      left: number
    }
  ): Promise<ApiControl> {
    return (await this.axiosInstance.post("/SectionEditor/CreateItem",
      args
    )).data;
  }

  async deleteSectionEditorItem(
    args: {
      schemaItemId: string,
      editorSchemaItemId: string,
    }
  ): Promise<ISectionEditorModel> {
    return (await this.axiosInstance.post("/SectionEditor/DeleteItem",
      args
    )).data;
  }
}

export function simpleErrorHandler(error: any) {
  console.error(error);
}


