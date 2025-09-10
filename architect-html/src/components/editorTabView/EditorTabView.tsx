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

import { RootStoreContext } from '@/main';
import S from '@components/editorTabView/EditorTabView.module.scss';
import { TabHeader } from '@components/editorTabView/TabHeader';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { useContext, useEffect, useMemo } from 'react';

export const EditorTabView = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const state = rootStore.editorTabViewState;
  const editors = state.editors.map(x => x.state);
  const initializeOpenEditors = useMemo(() => state.initializeOpenEditors.bind(state), [state]);

  const run = runInFlowWithHandler(rootStore.errorDialogController);

  useEffect(() => {
    run({ generator: initializeOpenEditors });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (editors.length === 0) {
    return null;
  }

  return (
    <div className={S.root}>
      <div className={S.tabs}>
        {editors.map(editor => (
          <TabHeader key={editor.editorId} editor={editor} />
        ))}
      </div>
      <div className={S.content}>
        {state.editors.map(editorContainer => (
          <div
            key={editorContainer.state.editorId}
            className={editorContainer.state.isActive ? S.visible : S.hidden}
          >
            {editorContainer.element}
          </div>
        ))}
      </div>
    </div>
  );
});
