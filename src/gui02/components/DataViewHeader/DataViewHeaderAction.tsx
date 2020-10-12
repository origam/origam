import React from "react";
import S from "./DataViewHeaderAction.module.scss";
import cx from "classnames";

export const DataViewHeaderAction: React.FC<{
  onClick?(event: any): void;
  className?: string;
  isActive?: boolean;
  isDisabled? : boolean;
  refDom?: any;
}> = function(props){

  function onClick(event: any){
    if(!props.isDisabled && props.onClick){
      props.onClick(event);
    }
  }

  return (
    <div
      className={cx(S.root, props.className, { isActive: props.isActive })}
      onClick={onClick}
      ref={props.refDom}
    >
      {props.children}
    </div>
  );
}
    
