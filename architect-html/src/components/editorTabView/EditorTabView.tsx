/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
import { useContext } from "react";
import S from "./EditorTabView.module.scss";
import { observer } from "mobx-react-lite";
import { action, flow } from "mobx";
import { RootStoreContext } from "src/main.tsx";

export const EditorTabView: React.FC= observer(() => {
  const projectState = useContext(RootStoreContext).projectState;
  const editors = projectState.editors.map(x => x.state);
  return (
    <div className={S.root}>
      <div className={S.labels}>
        {editors.map((editor) => (
          <div className={S.labelContainer} >
            <div key={editor.label} onClick={() => action(() => projectState.setActiveEditor(editor.schemaItemId))()}>
              {editor.label}
            </div>
            <div className={S.closeSymbol} onClick={() => flow(projectState.closeEditor(editor.schemaItemId).bind(projectState))() }>X</div>
          </div>
        ))}
      </div>
      <div className={S.content}>
        {projectState.editors.map((editorContainer) => (
          <div key={editorContainer.state.label} className={projectState.activeEditorState?.schemaItemId === editorContainer.state.schemaItemId ? S.visible : S.hidden }>
            {editorContainer.element}
          </div>
        ))}
      </div>
    </div>
  );
});