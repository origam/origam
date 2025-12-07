/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import {
  IAddToDeploymentRequest,
  IApiControl,
  IApiEditorData,
  IApiTreeNode,
  IArchitectApi,
  IDatabaseResultResponse,
  IMenuItemInfo,
  IModelChange,
  IPackagesInfo,
  IParametersResult,
  IPropertyChange,
  IScreenEditorItem,
  IScreenEditorModel,
  ISectionEditorModel,
  ITransformationInput,
  ITransformResult,
  IUpdatePropertiesResult,
  IValidationResult,
  ShemaItemInfo,
} from '@api/IArchitectApi';
import axios, { AxiosInstance } from 'axios';

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
      response => {
        return response;
      },
      async error => {
        if (error.response?.data?.constructor?.name === 'Blob') {
          error.response.data = await error.response.data.text();
        }
        if (!axios.isCancel(error)) {
          this.errorHandler(error);
        }
        throw error;
      },
    );
    return axiosInstance;
  }

  async setActivePackage(packageId: string): Promise<void> {
    await this.axiosInstance.post('/Package/SetActive', { id: packageId });
  }

  async getPackages(): Promise<IPackagesInfo> {
    return (await this.axiosInstance.get('/Package/GetAll')).data;
  }

  async getTopModelNodes(): Promise<IApiTreeNode[]> {
    return (await this.axiosInstance.get(`/Model/GetTopNodes`)).data;
  }

  async getNodeChildren(node: IApiTreeNode): Promise<IApiTreeNode[]> {
    return (
      await this.axiosInstance.get(`/Model/GetChildren`, {
        params: {
          id: node.origamId,
          nodeText: node.nodeText,
          isNonPersistentItem: node.isNonPersistentItem,
        },
      })
    ).data;
  }

  async openEditor(schemaItemId: string): Promise<IApiEditorData> {
    return (await this.axiosInstance.post('/Editor/OpenEditor', { schemaItemId: schemaItemId }))
      .data;
  }

  async closeEditor(editorId: string) {
    await this.axiosInstance.post('/Editor/CloseEditor', { editorId: editorId });
  }

  async openDocumentationEditor(schemaItemId: string): Promise<IApiEditorData> {
    return (
      await this.axiosInstance.post('/Documentation/OpenEditor', { schemaItemId: schemaItemId })
    ).data;
  }

  async updateDocumentationProperties(
    schemaItemId: string,
    changes: IPropertyChange[],
  ): Promise<IUpdatePropertiesResult> {
    return (
      await this.axiosInstance.post(`/Documentation/Update`, {
        schemaItemId,
        changes,
      })
    ).data;
  }

  async persistDocumentationChanges(schemaItemId: string): Promise<void> {
    await this.axiosInstance.post(`/Documentation/PersistChanges`, {
      schemaItemId,
    });
  }

  async validateTransformation(input: ITransformationInput): Promise<IValidationResult> {
    return (await this.axiosInstance.post(`/Xslt/Validate`, input)).data;
  }

  async runTransformation(input: ITransformationInput): Promise<ITransformResult> {
    return (await this.axiosInstance.post(`/Xslt/Transform`, input)).data;
  }

  async getXsltParameters(schemaItemId: string): Promise<IParametersResult> {
    return (await this.axiosInstance.get(`/Xslt/Parameters`, { params: { schemaItemId } })).data;
  }

  async getXsltSettings(): Promise<ShemaItemInfo[]> {
    return (await this.axiosInstance.get(`/Xslt/Settings`)).data;
  }

  async getRuleSets(dataStructureId: string): Promise<ShemaItemInfo[]> {
    return (await this.axiosInstance.get(`/Xslt/RuleSets`, { params: { dataStructureId } })).data;
  }

  async persistChanges(schemaItemId: string): Promise<void> {
    await this.axiosInstance.post(`/Editor/PersistChanges`, {
      schemaItemId,
    });
  }

  async persistSectionEditorChanges(schemaItemId: string): Promise<void> {
    await this.axiosInstance.post(`/SectionEditor/Save`, {
      schemaItemId,
    });
  }

  async updateProperties(
    schemaItemId: string,
    changes: IPropertyChange[],
  ): Promise<IUpdatePropertiesResult> {
    return (
      await this.axiosInstance.post(`/PropertyEditor/Update`, {
        schemaItemId,
        changes,
      })
    ).data;
  }

  async deleteSchemaItem(schemaItemId: string) {
    await this.axiosInstance.post('/Model/DeleteSchemaItem', { schemaItemId: schemaItemId });
  }

  async getMenuItems(node: IApiTreeNode): Promise<IMenuItemInfo[]> {
    return (
      await this.axiosInstance.get(`/Model/GetMenuItems`, {
        params: {
          id: node.origamId,
          nodeText: node.nodeText,
          isNonPersistentItem: node.isNonPersistentItem,
        },
      })
    ).data;
  }

  async getOpenEditors(): Promise<IApiEditorData[]> {
    return (await this.axiosInstance.get(`/Editor/GetOpenEditors`)).data;
  }

  async createNode(node: IApiTreeNode, typeName: string): Promise<IApiEditorData> {
    return (
      await this.axiosInstance.post('/Editor/CreateNode', {
        nodeId: node.origamId,
        newTypeName: typeName,
      })
    ).data;
  }

  async updateSectionEditor(args: {
    schemaItemId: string | undefined;
    name: string;
    selectedDataSourceId: string;
    modelChanges: IModelChange[];
  }): Promise<ISectionEditorModel> {
    return (await this.axiosInstance.post(`/SectionEditor/Update`, args)).data;
  }

  async createSectionEditorItem(args: {
    editorSchemaItemId: string;
    parentControlSetItemId: string;
    componentType: string;
    fieldName?: string;
    top: number;
    left: number;
  }): Promise<IApiControl> {
    return (await this.axiosInstance.post('/SectionEditor/CreateItem', args)).data;
  }

  async deleteSectionEditorItem(args: {
    schemaItemIds: string[];
    editorSchemaItemId: string;
  }): Promise<ISectionEditorModel> {
    return (await this.axiosInstance.post('/SectionEditor/Delete', args)).data;
  }

  async updateScreenEditor(args: {
    schemaItemId: string | undefined;
    name: string;
    selectedDataSourceId: string;
    modelChanges: IModelChange[];
  }): Promise<IScreenEditorModel> {
    return (await this.axiosInstance.post(`/ScreenEditor/Update`, args)).data;
  }

  async createScreenEditorItem(args: {
    editorSchemaItemId: string;
    parentControlSetItemId: string;
    controlItemId: string;
    top: number;
    left: number;
  }): Promise<IScreenEditorItem> {
    return (await this.axiosInstance.post('/ScreenEditor/CreateItem', args)).data;
  }

  async deleteScreenEditorItem(args: {
    schemaItemIds: string[];
    editorSchemaItemId: string;
  }): Promise<IScreenEditorModel> {
    return (await this.axiosInstance.post('/ScreenEditor/Delete', args)).data;
  }

  async loadSections(
    editorSchemaItemId: string,
    sectionIds: string[],
  ): Promise<Record<string, IApiControl>> {
    return (
      await this.axiosInstance.get(`/ScreenEditor/GetSections`, {
        params: {
          sectionIds: sectionIds,
          editorSchemaItemId: editorSchemaItemId,
        },
      })
    ).data;
  }

  async setVersionCurrent(schemaItemId: string): Promise<void> {
    await this.axiosInstance.post('/DeploymentScripts/SetVersionCurrent', {
      schemaItemId: schemaItemId,
    });
  }

  async runUpdateScriptActivity(schemaItemId: string): Promise<void> {
    await this.axiosInstance.post('/DeploymentScripts/Run', { schemaItemId: schemaItemId });
  }

  async fetchDeploymentScriptsList(platform: string | null): Promise<IDatabaseResultResponse> {
    return (
      await this.axiosInstance.post('/DeploymentScriptsGenerator/List', {
        platform,
      })
    ).data;
  }

  async addToDeployment(request: IAddToDeploymentRequest): Promise<void> {
    await this.axiosInstance.post('/DeploymentScriptsGenerator/AddToDeployment', request);
  }
}

export function simpleErrorHandler(error: any) {
  console.error(error);
}
