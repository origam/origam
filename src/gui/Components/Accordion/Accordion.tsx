import React from "react";
import { useLocalStore } from "mobx-react-lite";
import { IAccordionContext, AccordionContext } from "./AccordionTypes";

export const Accordion: React.FC<{ className: string }> = props => {
  const state = useLocalStore<IAccordionContext>(() => ({
    activeSectionId: undefined,
    setActiveSectionId(id: string | undefined) {
      this.activeSectionId = id;
    }
  }));

  return (
    <AccordionContext.Provider value={state}>
      <div className={props.className}>{props.children}</div>
    </AccordionContext.Provider>
  );
};
