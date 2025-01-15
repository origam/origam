import React from "react";
import {
  Component
} from "src/components/editors/designerEditor/common/designerComponents/Component.tsx";
import { observer } from "mobx-react-lite";
import SD
  from "src/components/editors/designerEditor/common/DesignerSurface.module.scss";
import S from "src/components/editors/designerEditor/common/SectionItem.module.scss";

export const SectionItem: React.FC<{
  components: Component[];
}> = observer((props) => {
  return (
    <div className={SD.designSurface + S.sectionRoot}>
      {props.components.map((component) => (
        <>
          <div
            key={component.id + "_label"}
            className={SD.componentLabel}
            style={{
              ...component.getLabelStyle(),
              zIndex: component.zIndex
            }
            }
          >
            {component.data.identifier}
          </div>
          <div
            key={component.id + "_component"}
            className={SD.designComponent}
            style={{
              left: `${component.absoluteLeft}px`,
              top: `${component.absoluteTop}px`,
              width: `${component.width}px`,
              height: `${component.height}px`,
              zIndex: component.zIndex
            }}
          >

            {component.designerRepresentation}

          </div>
        </>
      ))}
    </div>
  );
});