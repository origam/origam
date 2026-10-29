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

import { observer } from 'mobx-react-lite';
import cn from 'classnames';
import { XsltEditorState } from '@editors/gridEditor/XsltEditorState.ts';
import { OrigamDataType } from '@api/IArchitectApi.ts';
import CodeEditor from '@editors/codeEditor/CodeEditor';
import S from '@editors/xsltEditor/ParametersEditor.module.scss';
import { T } from '@/main';

const ORIGAM_DATA_TYPES: string[] = Object.values(OrigamDataType);

interface ParametersEditorProps {
  editorState: XsltEditorState;
}

export const ParametersEditor = observer(({ editorState }: ParametersEditorProps) => {
  const parameters = editorState.parameters;
  if (!parameters) {
    return null;
  }
  if (parameters.length === 0) {
    return (
      <div className={S.emptyMessage} data-test-id="xslt-parameters-empty">
        {T(
          'No input parameters. Declare them in the XSL, e.g. <xsl:param name="date" AS:DataType="Date"/>.',
          'xsl_editor_parameters_empty',
        )}
      </div>
    );
  }

  const selectedParameter = editorState.selectedParameter;
  return (
    <div className={S.root}>
      <div className={S.parameterList}>
        {parameters.map(parameter => (
          <div
            key={parameter}
            className={cn(S.parameterItem, { [S.selected]: parameter === selectedParameter })}
            onClick={() => editorState.selectParameter(parameter)}
            data-test-id={`xslt-parameter-${parameter}`}
          >
            {parameter}
          </div>
        ))}
      </div>
      {selectedParameter && (
        <ParameterDetail
          key={selectedParameter}
          name={selectedParameter}
          editorState={editorState}
        />
      )}
    </div>
  );
});

const ParameterDetail = observer(
  ({ name, editorState }: { name: string; editorState: XsltEditorState }) => {
    const type = editorState.parameterTypes.get(name) ?? OrigamDataType.String;
    const value = editorState.parameterValues.get(name) ?? '';

    return (
      <div className={S.parameterDetail}>
        <div className={S.typeRow}>
          <div>{T('Type', 'xsl_editor_parameter_type')}</div>
          <select
            value={type}
            onChange={e => editorState.setParameterType(name, e.target.value as OrigamDataType)}
            data-test-id="xslt-parameter-type"
          >
            {ORIGAM_DATA_TYPES.map(type => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        </div>
        <div className={S.valueEditor} data-test-id="xslt-parameter-value">
          <CodeEditor
            defaultLanguage="xml"
            value={value}
            onChange={text => editorState.setParameterValue(name, text ?? '')}
          />
        </div>
      </div>
    );
  },
);
