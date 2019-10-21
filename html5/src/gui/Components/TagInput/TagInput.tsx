import React from "react";
import S from "./TagInput.module.css";

export const TagInput: React.FC = props => {
  return <div className={S.tagInputContainer}>{props.children}</div>;
};

export const TagInputAdd: React.FC<{
  domRef?: any;
  onClick?: (event: any) => void;
  onMouseDown?: (event: any) => void;
}> = props => {
  return (
    <div
      className={S.tagInputAdd}
      onClick={props.onClick}
      onMouseDown={props.onMouseDown}
      ref={props.domRef}
    >
      <i className="fas fa-plus" />
    </div>
  );
};

export const TagInputItemDelete: React.FC<{
  onClick?: (event: any) => void;
}> = props => {
  return (
    <div className={S.tagInputItemDelete} onClick={props.onClick}>
      <i className="fas fa-times" />
    </div>
  );
};

export const TagInputItem: React.FC = props => {
  return <div className={S.tagInputItem}>{props.children}</div>;
};
