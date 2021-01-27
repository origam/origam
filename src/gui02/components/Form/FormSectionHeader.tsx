import React from "react";
import S from "./FormSectionHeader.module.scss";

export const FormSectionHeader: React.FC<{foreGroundColor: string | undefined}> = props => (
    <h1 className={S.root} style={{color: props.foreGroundColor}}>{props.children}</h1>
);