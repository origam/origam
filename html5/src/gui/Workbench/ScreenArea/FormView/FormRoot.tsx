import React from "react";
import S from './FormRoot.module.css'

export class FormRoot extends React.Component {
  render() {
    return (
      <div className={S.formRoot}>
        {this.props.children}
      </div>
    )
  }
}