import React from "react";
import S from "./DataViewHeaderAction.module.scss";
import cx from "classnames";

export const DataViewHeaderAction: React.FC<{
  onMouseDown?(event: any): void;
  className?: string;
  isActive?: boolean;
  isDisabled? : boolean;
  refDom?: any;
}> = function(props){

  function onMouseDown(event: any){
    if(!props.isDisabled && props.onMouseDown){
      props.onMouseDown(event);
    }
  }

  return (
    <div
      className={cx(S.root, props.className, { isActive: props.isActive }, props.isDisabled ? S.isDisabled : "")}
      onMouseDown={onMouseDown}
      ref={props.refDom}
    >
      {props.children}
    </div>
  );
}
    
