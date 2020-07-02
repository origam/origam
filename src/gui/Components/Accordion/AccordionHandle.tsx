import React, {useContext} from "react";
import {observer} from "mobx-react-lite";
import {AccordionContext} from "./AccordionTypes";

export const AccordionHandle: React.FC<{
  id: string;
  className: string;
}> = observer(props => {
  const accordionContext = useContext(AccordionContext);
  const isActive = accordionContext.activeSectionId === props.id;
  return (
    <div
      className={props.className + (isActive ? " active" : "")}
      onClick={() => accordionContext.setActiveSectionId(props.id)}
    >
      {props.children}
    </div>
  );
});
