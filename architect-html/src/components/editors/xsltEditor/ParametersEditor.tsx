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
import { XsltEditorState } from '@editors/gridEditor/XsltEditorState.ts';
import { OrigamDataType } from '@api/IArchitectApi.ts';
import S from '@editors/xsltEditor/ParameterEditor.module.scss';

const ORIGAM_DATA_TYPES: string[] = Object.values(OrigamDataType);

interface ParametersEditorProps {
  editorState: XsltEditorState;
}

export const ParametersEditor = observer(({ editorState }: ParametersEditorProps) => {
  return (
    <div className={S.parameterList}>
      {editorState.parameters.map(parameter => (
        <Parameter key={parameter} name={parameter} editorState={editorState} />
      ))}
    </div>
  );
});

const Parameter = observer(
  ({ name, editorState }: { name: string; editorState: XsltEditorState }) => {
    const type = editorState.parameterTypes.get(name) ?? OrigamDataType.String;
    const value = editorState.parameterValues.get(name) ?? '';
    function onTypeChange(newType: string) {
      editorState.parameterTypes.set(name, newType as OrigamDataType);
    }

    function onValueChange(newValue: string) {
      editorState.parameterValues.set(name, newValue);
    }

    return (
      <div className={S.parameter}>
        <div>{name}</div>
        <select value={type} onChange={e => onTypeChange(e.target.value)}>
          {ORIGAM_DATA_TYPES.map(type => (
            <option key={type} value={type}>
              {type}
            </option>
          ))}
        </select>
        <input onChange={e => onValueChange(e.target.value)} value={value} />
      </div>
    );
  },
);
