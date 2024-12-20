import React from 'react';
import S
  from 'src/components/editors/screenSectionEditor/ScreenSectionEditor.module.scss';
import {
  ScreenSectionEditorState
} from "src/components/editors/screenSectionEditor/ScreenSectionEditorState.tsx";
import {
  Toolbox
} from "src/components/editors/screenSectionEditor/Toolbox.tsx";
import {
  DesignSurface
} from "src/components/editors/screenSectionEditor/DesignSurface.tsx";

export const ScreenSectionEditor: React.FC<{
  designerState: ScreenSectionEditorState
}> = ({designerState}) => {
  return (
    <div className={S.componentDesigner}>
      <Toolbox designerState={designerState}/>
      <DesignSurface
        designerState={designerState}/>
    </div>
  );
};

export default ScreenSectionEditor;