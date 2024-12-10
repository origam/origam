import React from 'react';
import S
  from 'src/components/editors/screenSectionEditor/ComponentDesigner.module.scss';
import {
  ComponentDesignerState
} from "src/components/editors/screenSectionEditor/ComponentDesignerState.tsx";
import {
  Toolbox
} from "src/components/editors/screenSectionEditor/Toolbox.tsx";
import {
  DesignSurface
} from "src/components/editors/screenSectionEditor/DesignSurface.tsx";

export const ComponentDesigner: React.FC<{
  designerState: ComponentDesignerState
}> = ({designerState}) => {
  return (
    <div className={S.componentDesigner}>
      <Toolbox designerState={designerState}/>
      <DesignSurface
        designerState={designerState}/>
    </div>
  );
};

export default ComponentDesigner;