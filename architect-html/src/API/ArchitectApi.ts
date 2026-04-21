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
  IAddToModelRequest,
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
  ISearchResult,
  IScreenEditorItem,
  IScreenEditorModel,
  ISectionEditorModel,
  ITransformationInput,
  ITransformResult,
  IUpdatePropertiesResult,
  IValidationResult,
  ShemaItemInfo,
} from '@api/IArchitectApi';
import { HttpClient } from '@api/httpClient';

export class ArchitectApi implements IArchitectApi {
  errorHandler: (error: any) => void;
  http: HttpClient;

  constructor(errorHandler?: (error: any) => void) {
    this.errorHandler = errorHandler ?? simpleErrorHandler;
    this.http = new HttpClient(error => this.errorHandler(error));
  }

  async setActivePackage(packageId: string): Promise<void> {
    await this.http.post('/Package/SetActive', { id: packageId });
  }

  async getPackages(): Promise<IPackagesInfo> {
    return (await this.http.get('/Package/GetAll')).data;
  }

  async getTopModelNodes(): Promise<IApiTreeNode[]> {
    return (await this.http.get(`/Model/GetTopNodes`)).data;
  }

  async getNodeChildren(node: IApiTreeNode): Promise<IApiTreeNode[]> {
    return (
      await this.http.get(`/Model/GetChildren`, {
        params: {
          id: node.origamId,
          nodeText: node.nodeText,
          isNonPersistentItem: node.isNonPersistentItem,
        },
      })
    ).data;
  }

  async searchText(text: string): Promise<ISearchResult[]> {
    return (
      await this.http.get('/Search/Text', {
        params: {
          text,
        },
      })
    ).data;
  }

  async searchReferences(schemaItemId: string): Promise<ISearchResult[]> {
    return (
      await this.http.get('/Search/References', {
        params: {
          schemaItemId,
        },
      })
    ).data;
  }

  async searchDependencies(schemaItemId: string): Promise<ISearchResult[]> {
    return (
      await this.http.get('/Search/Dependencies', {
        params: {
          schemaItemId,
        },
      })
    ).data;
  }

  async openEditor(schemaItemId: string): Promise<IApiEditorData> {
    return (await this.http.post('/Editor/OpenEditor', { schemaItemId: schemaItemId })).data;
  }

  async closeEditor(editorId: string) {
    await this.http.post('/Editor/CloseEditor', { editorId: editorId });
  }

  async openDocumentationEditor(schemaItemId: string): Promise<IApiEditorData> {
    return (await this.http.post('/Documentation/OpenEditor', { schemaItemId: schemaItemId })).data;
  }

  async updateDocumentationProperties(
    schemaItemId: string,
    changes: IPropertyChange[],
  ): Promise<IUpdatePropertiesResult> {
    return (
      await this.http.post(`/Documentation/Update`, {
        schemaItemId,
        changes,
      })
    ).data;
  }

  async persistDocumentationChanges(schemaItemId: string): Promise<void> {
    await this.http.post(`/Documentation/PersistChanges`, {
      schemaItemId,
    });
  }

  async validateTransformation(input: ITransformationInput): Promise<IValidationResult> {
    return (await this.http.post(`/Xslt/Validate`, input)).data;
  }

  async runTransformation(input: ITransformationInput): Promise<ITransformResult> {
    return (await this.http.post(`/Xslt/Transform`, input)).data;
  }

  async getXsltParameters(schemaItemId: string): Promise<IParametersResult> {
    return (await this.http.get(`/Xslt/Parameters`, { params: { schemaItemId } })).data;
  }

  async getXsltSettings(): Promise<ShemaItemInfo[]> {
    return (await this.http.get(`/Xslt/Settings`)).data;
  }

  async getRuleSets(dataStructureId: string): Promise<ShemaItemInfo[]> {
    return (await this.http.get(`/Xslt/RuleSets`, { params: { dataStructureId } })).data;
  }

  async persistChanges(schemaItemId: string): Promise<void> {
    await this.http.post(`/Editor/PersistChanges`, {
      schemaItemId,
    });
  }

  async persistSectionEditorChanges(schemaItemId: string): Promise<void> {
    await this.http.post(`/SectionEditor/Save`, {
      schemaItemId,
    });
  }

  async updateProperties(
    schemaItemId: string,
    changes: IPropertyChange[],
  ): Promise<IUpdatePropertiesResult> {
    return (
      await this.http.post(`/PropertyEditor/Update`, {
        schemaItemId,
        changes,
      })
    ).data;
  }

  async deleteSchemaItem(schemaItemId: string) {
    await this.http.post('/Model/DeleteSchemaItem', { schemaItemId: schemaItemId });
  }

  async getMenuItems(node: IApiTreeNode): Promise<IMenuItemInfo[]> {
    return (
      await this.http.get(`/Model/GetMenuItems`, {
        params: {
          id: node.origamId,
          nodeText: node.nodeText,
          isNonPersistentItem: node.isNonPersistentItem,
        },
      })
    ).data;
  }

  async getOpenEditors(): Promise<IApiEditorData[]> {
    return (await this.http.get(`/Editor/GetOpenEditors`)).data;
  }

  async createNode(node: IApiTreeNode, typeName: string): Promise<IApiEditorData> {
    return (
      await this.http.post('/Editor/CreateNode', {
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
    return (await this.http.post(`/SectionEditor/Update`, args)).data;
  }

  async createSectionEditorItem(args: {
    editorSchemaItemId: string;
    parentControlSetItemId: string;
    componentType: string;
    fieldName?: string;
    top: number;
    left: number;
  }): Promise<IApiControl> {
    return (await this.http.post('/SectionEditor/CreateItem', args)).data;
  }

  async deleteSectionEditorItem(args: {
    schemaItemIds: string[];
    editorSchemaItemId: string;
  }): Promise<ISectionEditorModel> {
    return (await this.http.post('/SectionEditor/Delete', args)).data;
  }

  async updateScreenEditor(args: {
    schemaItemId: string | undefined;
    name: string;
    selectedDataSourceId: string;
    modelChanges: IModelChange[];
  }): Promise<IScreenEditorModel> {
    return (await this.http.post(`/ScreenEditor/Update`, args)).data;
  }

  async createScreenEditorItem(args: {
    editorSchemaItemId: string;
    parentControlSetItemId: string;
    controlItemId: string;
    top: number;
    left: number;
  }): Promise<IScreenEditorItem> {
    return (await this.http.post('/ScreenEditor/CreateItem', args)).data;
  }

  async deleteScreenEditorItem(args: {
    schemaItemIds: string[];
    editorSchemaItemId: string;
  }): Promise<IScreenEditorModel> {
    return (await this.http.post('/ScreenEditor/Delete', args)).data;
  }

  async loadSections(
    editorSchemaItemId: string,
    sectionIds: string[],
  ): Promise<Record<string, IApiControl>> {
    return (
      await this.http.get(`/ScreenEditor/GetSections`, {
        params: {
          sectionIds: sectionIds,
          editorSchemaItemId: editorSchemaItemId,
        },
      })
    ).data;
  }

  async setVersionCurrent(schemaItemId: string): Promise<void> {
    await this.http.post('/DeploymentScripts/SetVersionCurrent', {
      schemaItemId: schemaItemId,
    });
  }

  async runUpdateScriptActivity(schemaItemId: string): Promise<void> {
    await this.http.post('/DeploymentScripts/Run', { schemaItemId: schemaItemId });
  }

  async fetchDeploymentScriptsList(platform: string | null): Promise<IDatabaseResultResponse> {
    return (
      await this.http.get('/DeploymentScriptsGenerator/List', {
        params: {
          platform,
        },
      })
    ).data;
  }

  async addToDeployment(request: IAddToDeploymentRequest): Promise<void> {
    await this.http.post('/DeploymentScriptsGenerator/AddToDeployment', request);
  }

  async addToModel(request: IAddToModelRequest): Promise<void> {
    await this.http.post('/DeploymentScriptsGenerator/AddToModel', request);
  }
}

export function simpleErrorHandler(error: any) {
  console.error(error);
}
