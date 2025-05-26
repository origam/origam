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

import React, { useContext } from "react";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import { observer } from "mobx-react-lite";
import { RootStoreContext } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { Item, Menu, TriggerEvent, useContextMenu } from "react-contexify";
import S from "src/components/editorTabView/EditorTabView.module.scss";
import { action } from "mobx";

export const TabHeader: React.FC<{
  editor: IEditorState
}> = observer((props) => {
  const rootStore = useContext(RootStoreContext);
  const state = rootStore.editorTabViewState;
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const menuId = "TabMenu_" + props.editor.schemaItemId

  const {show, hideAll} = useContextMenu({
    id: menuId,
  });

  function onClose(editor: IEditorState) {
    run({generator: state.closeEditor(editor.schemaItemId)});
  }

  async function handleContextMenu(event: TriggerEvent) {
    show({event, props: {}});
  }

  function getLabel(editor: IEditorState) {
    if (!editor.isDirty) {
      return editor.label;
    }
    if (!editor.label) {
      return "*";
    }
    return editor.label + " *";
  }

  function closeAllTabsExcept(ignoreId: string | null) {
    run({
      generator: function* () {
        const editors = state.editors.map(x => x.state);
        for (const editor of editors) {
          if (ignoreId && editor.schemaItemId === ignoreId) {
            continue;
          }
          yield* state.closeEditor(editor.schemaItemId)()
        }
      }
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
      key={props.editor.label} className={S.labelContainer}
      onClick={() => action(() => state.setActiveEditor(props.editor.schemaItemId))()}
    >
      <div
        className={props.editor.isActive ? S.activeTab : ""}
        onContextMenu={handleContextMenu}
      >
        {getLabel(props.editor)}
      </div>
      <div
        className={S.closeSymbol}
        onClick={() => onClose(props.editor)}
      >X
      </div>
      <Menu
        id={menuId}
        onVisibilityChange={onMenuVisibilityChange}
      >
        <Item
          id="closeAll"
          onClick={() => closeAllTabsExcept(null)}
        >
          Close All
        </Item>
        <Item
          id="closeAllButThis"
          onClick={() => closeAllTabsExcept(props.editor.schemaItemId)}
        >
          Close All But This
        </Item>
      </Menu>
    </div>
  )
})