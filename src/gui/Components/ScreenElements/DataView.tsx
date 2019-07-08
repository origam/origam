import React from "react";
import S from "./DataView.module.css";
import { Toolbar } from "./DataViewToolbar";
import { observer, inject, Provider } from "mobx-react";
import { getDataViewById } from "../../../model/selectors/DataView/getDataViewById";
import { IDataView } from "../../../model/types/IDataView";

@inject(({ formScreen }, { id }) => {
  const dataView = getDataViewById(formScreen, id);
  return {
    dataView
  };
})
@observer
export class DataView extends React.Component<{
  id: string;
  height?: number;
  isHeadless: boolean;
  dataView?: IDataView;
}> {
  getDataViewStyle() {
    if (this.props.height !== undefined) {
      return {
        flexGrow: 0,
        height: this.props.height
      };
    } else {
      return {
        flexGrow: 1,
        width: "100%",
        height: "100%"
      };
    }
  }

  render() {
    console.log(this.props.dataView);
    return (
      <Provider dataView={this.props.dataView}>
        <div className={S.dataView} style={this.getDataViewStyle()}>
          {!this.props.isHeadless && <Toolbar />}
          {this.props.children}
        </div>
      </Provider>
    );
  }
}
