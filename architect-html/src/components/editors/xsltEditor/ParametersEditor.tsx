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
