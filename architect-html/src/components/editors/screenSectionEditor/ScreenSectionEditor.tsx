import React from 'react';
import S
  from 'src/components/editors/screenSectionEditor/ScreenSectionEditor.module.scss';
import {
  ScreenSectionEditorState
} from "src/components/editors/screenSectionEditor/ScreenSectionEditorState.tsx";
import {
  DesignSurface
} from "src/components/editors/screenSectionEditor/DesignSurface.tsx";
import {
  SectionToolbox
} from "src/components/editors/screenSectionEditor/SectionToolbox.tsx";

export const ScreenSectionEditor: React.FC<{
  designerState: ScreenSectionEditorState
}> = ({designerState}) => {
  return (
    <div className={S.componentDesigner}>
      <SectionToolbox designerState={designerState}/>
      <DesignSurface
        designerState={designerState}/>
    </div>
  );
};

export default ScreenSectionEditor;