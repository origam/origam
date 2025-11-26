import { observer } from 'mobx-react-lite';
import { ParameterData, XsltEditorState } from '@editors/gridEditor/XsltEditorState.ts';
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
        <Parameter key={parameter.name + parameter.type} parameter={parameter} />
      ))}
    </div>
  );
});

const Parameter = observer(({ parameter }: { parameter: ParameterData }) => {
  function onTypeChange(value: string) {
    parameter.type = value as OrigamDataType;
  }

  function onValueChange(value: string) {
    parameter.value = value;
  }

  return (
    <div className={S.parameter}>
      <div>{parameter.name}</div>
      <select value={parameter.type} onChange={e => onTypeChange(e.target.value)}>
        {ORIGAM_DATA_TYPES.map(type => (
          <option key={type} value={type}>
            {type}
          </option>
        ))}
      </select>
      <input onChange={e => onValueChange(e.target.value)} value={parameter.value} />
    </div>
  );
});
