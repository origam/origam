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

import { RootStoreContext, T } from '@/main';
import Button from '@components/Button/Button';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { VscSave } from 'react-icons/vsc';

const SaveButtonHOC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const progressBarState = rootStore.progressBarState;
  const editorTabViewState = rootStore.editorTabViewState;
  const activeEditor = editorTabViewState.activeEditorState;

  if (!activeEditor) {
    return null;
  }

  const handleSave = () => {
    if (!activeEditor.isDirty) return;

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
    <Button
      type="primary"
      title={T('Save', 'save_button_label')}
      prefix={<VscSave />}
      onClick={handleSave}
      isDisabled={!activeEditor.isDirty}
    />
  );
});

export default SaveButtonHOC;
