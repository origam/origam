import React from 'react';
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

export const ScreenEditor: React.FC<{
  designerState: ScreenEditorState
}> = ({designerState}) => {
  return (
    <div className={S.componentDesigner}>
      <ScreenToolbox designerState={designerState}/>
      <DesignSurface
        designerState={designerState}/>
    </div>
  );
};

export default ScreenEditor;