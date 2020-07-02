import {CDataViewHeader} from "gui02/connections/CDataViewHeader";
import {inject, observer, Provider} from "mobx-react";
import {getIsDataViewOrFormScreenWorking} from "model/selectors/DataView/getIsDataViewOrFormScreenWorking";
import React from "react";
import {IDataView} from "../../../model/entities/types/IDataView";
import {getDataViewById} from "../../../model/selectors/DataView/getDataViewById";
import S from "./DataView.module.css";
import {DataViewLoading} from "./DataViewLoading";
import {scopeFor} from "dic/Container";
import {IDataViewBodyUI} from "modules/DataView/DataViewUI";

@inject(({ formScreen }, { id }) => {
  const dataView = getDataViewById(formScreen, id);
  return {
    dataView,
  };
})
@observer
export class DataView extends React.Component<{
  id: string;
  height?: number;
  width?: number;
  isHeadless: boolean;
  dataView?: IDataView;
}> {
  getDataViewStyle() {
    if (this.props.height !== undefined || this.props.width !== undefined) {
      return {
        flexGrow: 0,
        height: this.props.height,
        width: this.props.width,
      };
    } else {
      return {
        flexGrow: 1,
        flexShrink: 0,
        // width: "100%",
        // height: "100%"
      };
    }
  }

  render() {
    // TODO: Move styling to stylesheet
    const isWorking = getIsDataViewOrFormScreenWorking(this.props.dataView);
    const $cont = scopeFor(this.props.dataView);
    const uiBody = $cont && $cont.resolve(IDataViewBodyUI);
    return (
      <Provider dataView={this.props.dataView}>
        <div className={S.dataView} style={this.getDataViewStyle()}>
          <CDataViewHeader isVisible={!this.props.isHeadless} />

          {uiBody && uiBody.render()}
          {/*

          {this.props.dataView!.activePanelView === IPanelViewType.Form && (

            )}*/}
          {isWorking && <DataViewLoading />}
        </div>
      </Provider>
    );
  }
}
