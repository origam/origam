import React from "react";

import S from "./MainMenuPanelAccordion.module.css";
import { Accordion } from "gui/Components/Accordion/Accordion";
import { AccordionHandle } from "../Components/Accordion/AccordionHandle";
import { AccordionBody } from "gui/Components/Accordion/AccordionBody";


export const MainMenuPanelAccordion: React.FC = props => (
  <Accordion className={S.accordion} {...props} />
);
export const MainMenuPanelAccordionHandle: React.FC<{ id: string }> = props => (
  <AccordionHandle className={S.accordionHandle} {...props} />
);
export const MainMenuPanelAccordionBody: React.FC<{
  id: string;
  initialActive?: boolean;
}> = props => <AccordionBody className={S.accordionBody} {...props} />;

/*
export const MainMenuPanelAccordion: React.FC = props => (
  <div className={S.accordion}>
    <AccordionHeader>
      <div className={S.accordionHeaderIcon}>
        <i className="far fa-envelope icon" />
      </div>
      Work Queues
    </AccordionHeader>
    <AccordionHeader>
      <div className={S.accordionHeaderIcon}>
        <i className="fas fa-star" />
      </div>
      Favourites
    </AccordionHeader>
    <AccordionHeader>
      <div className={S.accordionHeaderIcon}>
        <i className="fas fa-home" />
      </div>
      Menu
    </AccordionHeader>
    <div className={S.accordionSection + " open"}>Hello</div>
    <AccordionHeader>
      <div className={S.accordionHeaderIcon}>
        <i className="fas fa-info-circle" />
      </div>
      Info
    </AccordionHeader>
    <AccordionHeader>
      <div className={S.accordionHeaderIcon}>
        <i className="fas fa-search" />
      </div>
      Search
    </AccordionHeader>
  </div>
);

export const AccordionHeader: React.FC = props => (
  <div className={S.accordionHeader}>{props.children}</div>
);*/
