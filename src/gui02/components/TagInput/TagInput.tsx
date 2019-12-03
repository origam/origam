import React from "react";
import S from "./TagInput.module.scss";

export const TagInput: React.FC = props => (
  <div className={S.root}>{props.children}</div>
);

export const TagInputItem: React.FC = props => (
  <div className={S.item}>{props.children}</div>
);

export const TagInputPlus: React.FC = props => (
  <div className={S.plus}>
    <i className="fas fa-plus" />
  </div>
);

export const TagInputDeleteBtn: React.FC = props => (
  <div className={S.closeBtn}>
    <i className="fas fa-times" />
  </div>
);

export const TagInputEdit: React.FC<{
  value?: any;
  domRef?: any;
  onKeyDown?(event: any): void;
  onFocus?(event: any): void;
}> = props => (
  <input
    ref={props.domRef}
    className={S.input}
    value={props.value}
    onKeyDown={props.onKeyDown}
    onFocus={props.onFocus}
  />
);

export const TagInputEditFake: React.FC<{
  domRef?: any;
  onKeyDown?(event: any): void;
}> = props => (
  <input
    className={S.fakeInput}
    onKeyDown={props.onKeyDown}
    ref={props.domRef}
  />
);
