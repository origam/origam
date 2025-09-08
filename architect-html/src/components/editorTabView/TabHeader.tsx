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

import { RootStoreContext, T } from '@/main.tsx';
import { IEditorState } from '@components/editorTabView/IEditorState.ts';
import S from '@components/editorTabView/TabHeader.module.scss';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler.ts';
import { action } from 'mobx';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { Item, Menu, TriggerEvent, useContextMenu } from 'react-contexify';
import { VscClose, VscCloseAll } from 'react-icons/vsc';

export const TabHeader = observer(({ editor }: { editor: IEditorState }) => {
  const rootStore = useContext(RootStoreContext);
  const state = rootStore.editorTabViewState;
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const menuId = 'TabMenu_' + editor.editorId;

  const { show, hideAll } = useContextMenu({
    id: menuId,
  });

  function onClose(editor: IEditorState) {
    run({ generator: state.closeEditor(editor.editorId) });
  }

  async function handleContextMenu(event: TriggerEvent) {
    show({ event, props: {} });
  }

  function closeAllTabsExcept(ignoreId: string | null) {
    run({
      generator: function* () {
        const editors = state.editors.map(x => x.state);
        for (const editor of editors) {
          if (ignoreId && editor.editorId === ignoreId) {
            continue;
          }
          yield* state.closeEditor(editor.editorId)();
        }
      },
    });
  }

  function onMenuVisibilityChange(isVisible: boolean) {
    if (isVisible) {
      document.addEventListener('wheel', hideAll);
    } else {
      document.removeEventListener('wheel', hideAll);
    }
  }

  return (
    <div
      key={editor.label}
      className={S.root + ' ' + (editor.isActive ? S.activeTab : '')}
      onClick={() => action(() => state.setActiveEditor(editor.editorId))()}
    >
      <div className={S.title} onContextMenu={handleContextMenu}>
        <span className={S.label}>{editor.label}</span>
        {editor.isDirty && <span className={S.asterisk}>*</span>}
      </div>

      <div className={S.close} onClick={() => onClose(editor)}>
        <VscClose />
      </div>

      <Menu id={menuId} onVisibilityChange={onMenuVisibilityChange}>
        <Item className={S.contextMenuButton} onClick={() => closeAllTabsExcept(null)}>
          <VscCloseAll />
          <span>{T('Close All', 'tab_header_close_all')}</span>
        </Item>
        <Item className={S.contextMenuButton} onClick={() => closeAllTabsExcept(editor.editorId)}>
          <VscCloseAll />
          <span>{T('Close All But This', 'tab_header_close_all_but_this')}</span>
        </Item>
      </Menu>
    </div>
  );
});
