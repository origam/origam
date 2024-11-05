import S
  from "src/components/screenSectionEditor2/ToolBox.module.scss";
import {
  DraggableLabel
} from "src/components/screenSectionEditor2/designComponents/DraggableLabel.tsx";

export const ToolBox = () => {
  return (
    <div className={S.root}>
      <DraggableLabel  text={"Label1"}/>
    </div>
  );
}

