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
  IParametersResult,
  IValidationResult,
  IParameterData,
  OrigamDataType,
  ITransformResult,
} from '@api/IArchitectApi.ts';
import { GridEditorState } from '@editors/gridEditor/GridEditorState.ts';
import { action, observable } from 'mobx';

export class XsltEditorState extends GridEditorState {
  @observable private accessor _parameterTypes: string[] = [];
  @observable private accessor _selectedParameterType: string | undefined;
  @observable public accessor parameters: ParameterData[] = [];
  @observable public accessor xmlResult = '';
  @observable public accessor inputXml = '<ROOT>\n</ROOT>';
  // @observable public accessor parameterValues = new Map<string, string>();

  get selectedParameterType(): string | undefined {
    return this._selectedParameterType;
  }

  @action
  set selectedParameterType(value: string | undefined) {
    this._selectedParameterType = value;
  }

  get parameterTypes(): string[] {
    return this._parameterTypes;
  }

  @action
  set parameterTypes(value: string[]) {
    this._selectedParameterType = value[0];
    this._parameterTypes = value;
  }
  *validate(): Generator<Promise<IValidationResult>, IValidationResult, IValidationResult> {
    return yield this.architectApi.validateTransformation(this.editorNode.origamId);
  }
  *transform(): Generator<Promise<ITransformResult>, ITransformResult, ITransformResult> {
    return yield this.architectApi.runTransformation(
      this.editorNode.origamId,
      this.inputXml,
      this.parameters,
    );
  }
  *getXsltParameters(): Generator<
    Promise<IParametersResult>,
    IParametersResult,
    IParametersResult
  > {
    return yield this.architectApi.getXsltParameters(this.editorNode.origamId);
  }

  setParameters(parameters: IParameterData[]) {
    this.parameters = parameters.map(x => new ParameterData(x));
  }
}

export class ParameterData implements IParameterData {
  name: string;
  @observable public accessor type: OrigamDataType;
  @observable public accessor value: string = '';

  constructor(parameterFromServer: IParameterData) {
    this.name = parameterFromServer.name;
    this.type = parameterFromServer.type;
  }
}
