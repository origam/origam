import React from "react";
import S from "./DataViewLoading.module.css";

export class DataViewLoading extends React.Component {
  render() {
    return (
      <div className={S.container}>
        <div className={S.label}>Loading</div>
      </div>
    );
  }
}
