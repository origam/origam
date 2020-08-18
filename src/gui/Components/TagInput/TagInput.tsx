import React, { useContext } from "react";
import S from "./TagInput.module.css";
import { CtxDropdownEditor } from "../../../modules/Editors/DropdownEditor/DropdownEditor";

export const TagInput: React.FC<{ className?: string }> = (props) => {
  return (
    <div className={S.tagInputContainer + (props.className ? ` ${props.className}` : "")}>
      {props.children}
    </div>
  );
};

export const TagInputAdd: React.FC<{
  domRef?: any;
  className?: string;
  onClick: (event: any) => void;
  onMouseDown?: (event: any) => void;
}> = (props) => {
  const beh = useContext(CtxDropdownEditor).behavior;

  function onClick(event: any) {
    beh.handleInputBtnClick(event);
    props.onClick(event);
  }

  return (
    <div
      className={S.tagInputAdd + (props.className ? ` ${props.className}` : "")}
      onClick={(event) => onClick(event)}
      onMouseDown={props.onMouseDown}
      ref={props.domRef}
    >
      <i className="fas fa-plus" />
    </div>
  );
};

export const TagInputItemDelete: React.FC<{
  onClick?: (event: any) => void;
  className?: string;
}> = (props) => {
  return (
    <div
      className={S.tagInputItemDelete + (props.className ? ` ${props.className}` : "")}
      onClick={props.onClick}
    >
      <i className="fas fa-times" />
    </div>
  );
};

export const TagInputItem: React.FC<{ className?: string }> = (props) => {
  return (
    <div className={S.tagInputItem + (props.className ? ` ${props.className}` : "")}>
      {props.children}
    </div>
  );
};
