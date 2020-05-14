import React from "react";
import S from "./WorkflowFinishedPanel.module.scss";

export const WorkflowFinishedPanel: React.FC<{
  isCloseButton: boolean;
  isRepeatButton: boolean;
  onCloseClick?(event: any): void;
  onRepeatClick?(event: any): void;
  message: string;
}> = (props) => (
  <div className={S.root}>
    {props.isRepeatButton && <button onClick={props.onRepeatClick}>Repeat</button>}
    {props.isCloseButton && <button onClick={props.onCloseClick}>Close</button>}
    {/*<iframe className={S.message} srcDoc={} />*/}
    <div className={S.message} dangerouslySetInnerHTML={{ __html: `${props.message}` }} />
  </div>
);
