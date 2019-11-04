import React, { useEffect } from "react";
import { useLocalStore } from "mobx-react-lite";
import { IAccordionContext, AccordionContext } from "./AccordionTypes";

export const Accordion: React.FC<{
  className: string;
  subscribeActivator?: (
    setActiveSectionId: (id: string | undefined) => void
  ) => () => void;
}> = props => {
  const state = useLocalStore<IAccordionContext>(() => ({
    activeSectionId: undefined,
    setActiveSectionId(id: string | undefined) {
      this.activeSectionId = id;
    }
  }));
  useEffect(() => {
    if (props.subscribeActivator) {
      return props.subscribeActivator(state.setActiveSectionId);
    }
  }, []);
  return (
    <AccordionContext.Provider value={state}>
      <div className={props.className}>{props.children}</div>
    </AccordionContext.Provider>
  );
};
