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
  IParameterData,
  IParametersResult,
  ITransformResult,
  IValidationResult,
  OrigamDataType,
} from '@api/IArchitectApi.ts';
import { GridEditorState } from '@editors/gridEditor/GridEditorState.ts';
import { observable } from 'mobx';
import { ITabViewState } from '@components/tabView/ITabViewState.ts';

export class XsltEditorState extends GridEditorState implements ITabViewState {
  @observable public accessor activeTabIndex = 0;
  @observable public accessor parameters: string[] = [];
  @observable public accessor xmlResult = '';
  @observable public accessor inputXml = '<ROOT>\n</ROOT>';
  @observable public accessor parameterValues = new Map<string, string>();
  @observable public accessor parameterTypes = new Map<string, OrigamDataType>();

  *validate(): Generator<Promise<IValidationResult>, IValidationResult, IValidationResult> {
    return yield this.architectApi.validateTransformation(this.editorNode.origamId);
  }
  *transform(): Generator<Promise<ITransformResult>, ITransformResult, ITransformResult> {
    return yield this.architectApi.runTransformation(
      this.editorNode.origamId,
      this.inputXml,
      this.parameters.map(name => {
        return {
          name,
          type: this.parameterTypes.get(name) ?? OrigamDataType.String,
          value: this.parameterValues.get(name) ?? '',
        };
      }),
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
    this.parameters = parameters.map(x => x.name);
  }
}
