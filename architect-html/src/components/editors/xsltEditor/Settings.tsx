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
import S from '@editors/xsltEditor/Settings.module.scss';
import { RootStoreContext, T } from '@/main';
import { action } from 'mobx';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler.ts';
import { useContext } from 'react';
import SinglePropertyEditor from '@editors/propertyEditor/SinglePropertyEditor.tsx';
import { EditorProperty } from '@editors/gridEditor/EditorProperty.ts';

export const Settings = observer(({ editorState }: { editorState: XsltEditorState }) => {
  const rootStore = useContext(RootStoreContext);

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
