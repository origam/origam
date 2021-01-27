import React from "react";
import S from "./FormLabel.module.scss";

export const FormLabel: React.FC<{
  title: string;
  top: number;
  left: number;
  width: number;
  height: number;
  foregroundColor: string | undefined;
}> = props => (
  <div
    className={S.root}
    style={{
      top: props.top,
      left: props.left,
      width: props.width,
      height: props.height,
      color: props.foregroundColor
    }}
  >
    {props.title}
  </div>
);
