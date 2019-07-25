import React, { useContext, useLayoutEffect } from 'react';
import { observer, useLocalStore } from 'mobx-react-lite';
import { AccordionContext } from './AccordionTypes';

export const AccordionBody: React.FC<{
  id: string;
  initialActive?: boolean;
  className: string;
}> = observer(props => {
  const accordionContext = useContext(AccordionContext);
  // const visibilityContext = useContext(VisibilityContext);
  const fwdVisibilityContext = useLocalStore(() => ({
    get isVisible() {
      return (
        // (visibilityContext || visibilityContext === undefined) &&
        accordionContext.activeSectionId === props.id
      );
    }
  }));
  useLayoutEffect(() => {
    if (props.initialActive) {
      accordionContext.setActiveSectionId(props.id);
    }
  }, []);
  return (
    /*<VisibilityContext.Provider value={fwdVisibilityContext}>*/
      <div
        className={
          props.className + (fwdVisibilityContext.isVisible ? "" : " hidden")
        }
      >
        {props.children}
      </div>
    /*</VisibilityContext.Provider>*/
  );
});