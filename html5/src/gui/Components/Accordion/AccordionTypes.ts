import React from "react";

export interface IAccordionContext {
  activeSectionId: string | undefined;
  setActiveSectionId(id: string | undefined): void;
}

export const AccordionContext = React.createContext<IAccordionContext>({
  activeSectionId: undefined
} as any);
