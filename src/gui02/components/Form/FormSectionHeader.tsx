import React from "react";
import S from "./FormSectionHeader.module.scss";

export const FormSectionHeader: React.FC = props => (
    <h1 className={S.root}>{props.children}</h1>
);