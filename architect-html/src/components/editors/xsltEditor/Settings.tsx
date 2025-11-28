import { observer } from 'mobx-react-lite';
import { XsltEditorState } from '@editors/gridEditor/XsltEditorState.ts';
import S from '@editors/xsltEditor/Settings.module.scss';
import { RootStoreContext, T } from '@/main';
import { action } from 'mobx';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler.ts';
import { useContext } from 'react';
import SinglePropertyEditor from '@editors/propertyEditor/SinglePropertyEditor.tsx';
import { EditorProperty } from '@editors/gridEditor/EditorProperty.ts';

export const Settings = observer(({ editorState }: { editorState: XsltEditorState }) => {
  const rootStore = useContext(RootStoreContext);
  // const { TransformFieldName } = editorState;

  function onSourceStructureChange(e: any) {
    action((editorState.sourceDataStructureId = e.target.value));
  }

  function onTargetStructureChange(e: any) {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        editorState.targetDataStructureId = e.target.value;
        yield* editorState.updateRuleSets();
      },
    });
  }

  function renderPropertyEditor(
    nameProperty: EditorProperty | undefined,
    label: string | undefined,
  ) {
    if (!nameProperty) {
      return null;
    }
    return (
      <div className={S.inputRow}>
        <div>{label}</div>
        <div className={S.propertyEditorContainer}>
          <SinglePropertyEditor
            property={nameProperty}
            propertyManager={editorState}
            compact={true}
          />
        </div>
      </div>
    );
  }

  return (
    <>
      <div className={S.inputs}>
        {renderPropertyEditor(editorState.nameProperty, editorState.nameProperty?.name)}
        <div className={S.inputRow}>
          <div>{T('Source Structure', 'source_structure_label')}</div>
          <select value={editorState.sourceDataStructureId} onChange={onSourceStructureChange}>
            {editorState.datastructures.map(structure => (
              <option key={structure?.schemaItemId ?? 'None'} value={structure?.schemaItemId}>
                {structure?.name}
              </option>
            ))}
          </select>
        </div>

        {editorState.targetStructureProperty ? (
          renderPropertyEditor(
            editorState.targetStructureProperty,
            T('Target Structure', 'target_structure_label'),
          )
        ) : (
          <div className={S.inputRow}>
            <div>{T('Target Structure', 'target_structure_label')}</div>
            <select value={editorState.targetDataStructureId} onChange={onTargetStructureChange}>
              {editorState.datastructures.map(structure => (
                <option key={structure?.schemaItemId ?? 'None'} value={structure?.schemaItemId}>
                  {structure?.name}
                </option>
              ))}
            </select>
          </div>
        )}

        <div className={S.inputRow}>
          <div>{T('Rule Set', 'rule_set_label')}</div>
          <select
            value={editorState.ruleSetId}
            onChange={e => action((editorState.ruleSetId = e.target.value))}
          >
            {editorState.ruleSets.map(structure => (
              <option key={structure?.schemaItemId ?? 'None'} value={structure?.schemaItemId}>
                {structure?.name}
              </option>
            ))}
          </select>
        </div>
      </div>
    </>
  );
});
