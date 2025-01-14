import React, { useContext, useEffect } from 'react';
import S
  from 'src/components/editors/designerEditor/screenSectionEditor/ScreenSectionEditor.module.scss';
import {
  DesignSurface
} from "src/components/editors/designerEditor/common/DesignSurface.tsx";
import {
  ScreenEditorState
} from "src/components/editors/designerEditor/screenEditor/ScreenEditorState.tsx";
import {
  ScreenToolbox
} from "src/components/editors/designerEditor/screenEditor/ScreenToolbox.tsx";
import { RootStoreContext } from "src/main.tsx";
import {
  runInFlowWithHandler
} from "src/errorHandling/runInFlowWithHandler.ts";

export const ScreenEditor: React.FC<{
  designerState: ScreenEditorState
}> = ({designerState}) => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);

  useEffect(()=> {
    run({generator: designerState.loadSections()});
  }, [...designerState.surface.components]);

  return (
    <div className={S.componentDesigner}>
      <ScreenToolbox designerState={designerState}/>
      <DesignSurface designerState={designerState}/>
    </div>
  );
};

export default ScreenEditor;