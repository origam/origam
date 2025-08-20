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
import { useContext } from 'react';
import { runInFlowWithHandler } from 'src/errorHandling/runInFlowWithHandler.ts';
import { RootStoreContext, T } from 'src/main.tsx';
import S from './SaveButton.module.scss';

export const SaveButton = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const progressBarState = rootStore.progressBarState;
  const editorTabViewState = rootStore.editorTabViewState;
  const activeEditor = editorTabViewState.activeEditorState;
  if (!activeEditor) {
    return null;
  }
  const handleSave = () => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        progressBarState.isWorking = true;
        try {
          yield* activeEditor.save();
        } finally {
          progressBarState.isWorking = false;
        }
      },
    });
  };

  return (
    <button
      className={S.root}
      onClick={handleSave}
      disabled={!activeEditor.isDirty}
      style={{
        backgroundColor: activeEditor.isDirty ? 'var(--warning2)' : 'var(--background1)',
        borderColor: activeEditor.isDirty ? 'var(--warning2)' : 'var(--foreground1)',
        color: activeEditor.isDirty ? '#fff' : 'var(--foreground1)',
      }}
    >
      {T('Save', 'save_button_label')}
    </button>
  );
});
