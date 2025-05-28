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
import React, { useContext, useEffect, useMemo } from "react";
import S from "./EditorTabView.module.scss";
import { observer } from "mobx-react-lite";
import { RootStoreContext } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";
import { TabHeader } from "src/components/editorTabView/TabHeader.tsx";

export const EditorTabView: React.FC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const state = rootStore.editorTabViewState;
  const editors = state.editors.map(x => x.state);
  const initializeOpenEditors = useMemo(
    () => state.initializeOpenEditors.bind(state),
    [state]
  );

  const run = runInFlowWithHandler(rootStore.errorDialogController);

  useEffect(() => {
    run({generator: initializeOpenEditors});
  }, [initializeOpenEditors]);

  return (
    <div className={S.root}>
      <div className={S.labels}>
        {editors.map((editor) =>
          <TabHeader editor={editor}/>)
        }
      </div>
      <div className={S.content}>
        {state.editors.map((editorContainer) => (
          <div key={editorContainer.state.schemaItemId}
               className={editorContainer.state.isActive ? S.visible : S.hidden}>
            {editorContainer.element}
          </div>
        ))}
      </div>
    </div>
  );
});


