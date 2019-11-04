import React from "react";
import S from "./FormScreenLoading.module.css";

export class FormScreenLoading extends React.Component {
  render() {
    return (
      <div className={S.container}>
        <div className={S.label}>Loading</div>
      </div>
    );
  }
}
