import { observer } from 'mobx-react-lite';
import { XsltEditorState } from '@editors/gridEditor/XsltEditorState.ts';
import PropertyEditor from '@editors/propertyEditor/PropertyEditor.tsx';
import S from '@editors/xsltEditor/Settings.module.scss';
import { RootStoreContext, T } from '@/main';
import { action } from 'mobx';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler.ts';
import { useContext } from 'react';

export const Settings = observer(({ editorState }: { editorState: XsltEditorState }) => {
  const rootStore = useContext(RootStoreContext);
  const { TransformFieldName } = editorState;

  function onSourceStructureChange(e: any) {
    action((editorState.sourceDataStructureId = e.target.value ? e.target.value : undefined));
  }

  function onTargetStructureChange(e: any) {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        editorState.targetDataStructureId = e.target.value ? e.target.value : undefined;
        yield* editorState.updateRuleSets();
      },
    });
  }

  return (
    <>
      <PropertyEditor
        compact={true}
        propertyManager={editorState}
        properties={editorState.properties.filter(
          x => x.name !== TransformFieldName && x.name !== 'Package',
        )}
      />
      <div className={S.inputs}>
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
