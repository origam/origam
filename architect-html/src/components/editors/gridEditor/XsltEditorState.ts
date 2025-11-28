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
  IArchitectApi,
  IParameterData,
  IParametersResult,
  ITransformResult,
  IUpdatePropertiesResult,
  IValidationResult,
  OrigamDataType,
  ShemaItemInfo,
} from '@api/IArchitectApi.ts';
import { GridEditorState } from '@editors/gridEditor/GridEditorState.ts';
import { observable } from 'mobx';
import { ITabViewState } from '@components/tabView/ITabViewState.ts';
import { IEditorNode } from '@components/editorTabView/EditorTabViewState.ts';
import { EditorProperty } from '@editors/gridEditor/EditorProperty.ts';
import { IEditorState } from '@components/editorTabView/IEditorState.ts';
import { IPropertyManager } from '@editors/propertyEditor/IPropertyManager.tsx';

export class XsltEditorState implements ITabViewState, IEditorState, IPropertyManager {
  @observable public accessor activeTabIndex = 0;
  @observable public accessor parameters: string[] = [];
  @observable public accessor xmlResult = '';
  @observable public accessor inputXml = '<ROOT>\n</ROOT>';
  @observable public accessor parameterValues = new Map<string, string>();
  @observable public accessor parameterTypes = new Map<string, OrigamDataType>();
  @observable.shallow public accessor datastructures: (ShemaItemInfo | undefined)[] = [];
  @observable.shallow public accessor ruleSets: (ShemaItemInfo | undefined)[] = [];
  @observable public accessor sourceDataStructureId: string | undefined;
  @observable public accessor ruleSetId: string | undefined;
  private readonly gridEditorState: GridEditorState;
  public readonly nameProperty: EditorProperty | undefined;

  // This editor can be used with multiple Origam SchemaItems. If the item is XslRule,
  // there will be a property named targetDataStructurePropertyName which contains the
  // target datastructure id and if the datastructure id changes, the property
  // has to be updated as well.
  // If the edited schema is something else (e.g. XslTransformation)
  // The targetDataStructurePropertyName property will not be present, and we have
  // to store the id value somewhere else - targetDataStructureId.
  public readonly targetDataStructurePropertyName = 'Structure';
  public readonly targetStructureProperty: EditorProperty | undefined;
  @observable public accessor targetDataStructureId: string | undefined;
  private readonly transformPropertyName: string;

  get xsl(): string {
    return (
      this.gridEditorState.properties.find(x => x.name === this.transformPropertyName)?.value ?? ''
    );
  }

  constructor(
    public editorId: string,
    private editorNode: IEditorNode,
    properties: EditorProperty[] | undefined,
    isDirty: boolean,
    private architectApi: IArchitectApi,
  ) {
    this.gridEditorState = new GridEditorState(
      editorId,
      editorNode,
      properties,
      isDirty,
      architectApi,
    );
    this.nameProperty = properties?.find(prop => prop.name === 'Name');
    this.targetStructureProperty = properties?.find(prop => prop.name === 'Structure');
    if (this.targetStructureProperty) {
      this.targetDataStructureId = this.targetStructureProperty.value;
    }

    if (properties?.find(x => x.name === 'TextStore')) {
      this.transformPropertyName = 'TextStore';
    } else {
      this.transformPropertyName = 'Xsl';
    }
  }

  *onPropertyUpdated(
    property: EditorProperty,
    value: any,
  ): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    yield* this.gridEditorState.onPropertyUpdated(property, value);

    if (this.targetStructureProperty && property.name === this.targetDataStructurePropertyName) {
      this.targetDataStructureId = value;
    }
  }

  *save(): Generator<Promise<any>, void, any> {
    yield* this.gridEditorState.save();
  }

  get isDirty() {
    return this.gridEditorState.isDirty;
  }

  get isActive() {
    return this.gridEditorState.isActive;
  }
  set isActive(value: boolean) {
    this.gridEditorState.isActive = value;
  }
  get label() {
    return this.gridEditorState.label;
  }

  *validate(): Generator<Promise<IValidationResult>, IValidationResult, IValidationResult> {
    return yield this.architectApi.validateTransformation({
      schemaItemId: this.editorNode.origamId,
      sourceDataStructureId: this.sourceDataStructureId ? this.sourceDataStructureId : undefined,
      targetDataStructureId: this.targetDataStructureId ? this.targetDataStructureId : undefined,
      ruleSetId: this.ruleSetId,
    });
  }
  *transform(): Generator<Promise<ITransformResult>, ITransformResult, ITransformResult> {
    return yield this.architectApi.runTransformation({
      schemaItemId: this.editorNode.origamId,
      sourceDataStructureId: this.sourceDataStructureId ? this.sourceDataStructureId : undefined,
      targetDataStructureId: this.targetDataStructureId ? this.targetDataStructureId : undefined,
      ruleSetId: this.ruleSetId,
      inputXml: this.inputXml,
      parameters: this.parameters.map(name => {
        return {
          name,
          type: this.parameterTypes.get(name) ?? OrigamDataType.String,
          value: this.parameterValues.get(name) ?? '',
        };
      }),
    });
  }
  *getXsltParameters(): Generator<
    Promise<IParametersResult>,
    IParametersResult,
    IParametersResult
  > {
    return yield this.architectApi.getXsltParameters(this.editorNode.origamId);
  }

  setParameters(parameters: IParameterData[]) {
    this.parameters = parameters.map(x => x.name);
  }

  *loadSettings(): Generator<Promise<ShemaItemInfo[]>, void, ShemaItemInfo[]> {
    this.datastructures = [undefined, ...(yield this.architectApi.getXsltSettings())];
  }

  *updateRuleSets(): Generator<Promise<ShemaItemInfo[]>, void, ShemaItemInfo[]> {
    if (this.targetDataStructureId) {
      this.ruleSets = [
        undefined,
        ...(yield this.architectApi.getRuleSets(this.targetDataStructureId)),
      ];
    } else {
      this.ruleSets = [];
    }
  }

  *onTransformChange(value: string | undefined) {
    const textProperty = this.gridEditorState.properties.find(
      x => x.name === this.transformPropertyName,
    )!;
    yield* this.gridEditorState.onPropertyUpdated(textProperty, value);
  }
}
