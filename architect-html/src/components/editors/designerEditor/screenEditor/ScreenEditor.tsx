import React, { createContext } from 'react';
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

export const DesignerStateContext = createContext<ScreenEditorState | null>(null);

export const ScreenEditor: React.FC<{
  designerState: ScreenEditorState
}> = ({designerState}) => {
  return (
    <DesignerStateContext.Provider value={designerState}>
    <div className={S.componentDesigner}>
      <ScreenToolbox designerState={designerState}/>
      <DesignSurface designerState={designerState}/>
    </div>
    </DesignerStateContext.Provider>
  );
};

export default ScreenEditor;