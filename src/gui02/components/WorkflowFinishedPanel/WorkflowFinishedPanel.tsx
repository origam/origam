import React from "react";
import S from "./WorkflowFinishedPanel.module.scss";
import {T} from "utils/translation";

export const WorkflowFinishedPanel: React.FC<{
  isCloseButton: boolean;
  isRepeatButton: boolean;
  onCloseClick?(event: any): void;
  onRepeatClick?(event: any): void;
  message: string;
}> = (props) => (
  <div className={S.root}>
    {props.isRepeatButton && <button onClick={props.onRepeatClick}>{T("Repeat","button_repeat")}</button>}
    {props.isCloseButton && <button onClick={props.onCloseClick}>{T("Close","button_close")}</button>}
    {/*<iframe className={S.message} srcDoc={} />*/}
    <div className={S.message} dangerouslySetInnerHTML={{ __html: `${props.message}` }} />
  </div>
);
