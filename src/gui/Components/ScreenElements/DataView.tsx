import React from "react";
import S from "./DataView.module.css";
import { Toolbar } from "./DataViewToolbar";
import { observer, inject, Provider } from "mobx-react";
import { getDataViewById } from "../../../model/selectors/DataView/getDataViewById";
import { IDataView } from "../../../model/entities/types/IDataView";
import { FormBuilder } from "../../Workbench/ScreenArea/FormView/FormBuilder";
import { IPanelViewType } from "../../../model/entities/types/IPanelViewType";
import { TableView } from "../../Workbench/ScreenArea/TableView/TableView";

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
    return (
      <Provider dataView={this.props.dataView}>
        <div className={S.dataView} style={this.getDataViewStyle()}>
          {!this.props.isHeadless && <Toolbar />}
          <div
            style={{
              // width: "100%",
              // height: "100%",
              flexGrow: 1,
              display:
                this.props.dataView!.activePanelView !== IPanelViewType.Table
                  ? "none"
                  : "flex"
            }}
          >
            <TableView />
          </div>
          <div
            style={{
              // width: "100%",
              // height: "100%",
              flexGrow: 1,
              display:
                this.props.dataView!.activePanelView !== IPanelViewType.Form
                  ? "none"
                  : "flex"
            }}
          >
            <FormBuilder />
          </div>
          {this.props.dataView!.isWorking && (
            <div className={S.dataViewOverlay}>
              <div className={S.dataViewLoadingLabel}>loading</div>
            </div>
          )}
        </div>
      </Provider>
    );
  }
}
