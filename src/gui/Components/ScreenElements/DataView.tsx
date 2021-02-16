import { CDataViewHeader } from "gui/connections/CDataViewHeader";
import { inject, Observer, observer, Provider } from "mobx-react";
import { getIsDataViewOrFormScreenWorkingDelayed } from "model/selectors/DataView/getIsDataViewOrFormScreenWorking";
import React, { createContext, Fragment, ReactNode } from "react";
import { IDataView } from "../../../model/entities/types/IDataView";
import { getDataViewById } from "../../../model/selectors/DataView/getDataViewById";
import S from "./DataView.module.css";
import { DataViewLoading } from "./DataViewLoading";
import { scopeFor } from "dic/Container";
import { IDataViewBodyUI } from "modules/DataView/DataViewUI";
import { TreeView } from "./TreeView";
import { observable } from "mobx";
import { getIsDataViewOrFormScreenWorking } from "model/selectors/DataView/getIsDataViewOrFormScreenWorking.1";

export interface IDataViewHeaderExtensionItem {
  $iid: number;
  group: string;
  render(): ReactNode;
}

export class DataViewHeaderExtension {
  @observable items = new Map<number, IDataViewHeaderExtensionItem>();

  put(item: IDataViewHeaderExtensionItem) {
    this.items.set(item.$iid, item);
  }

  del(item: { $iid: number }) {
    this.items.delete(item.$iid);
  }

  render(group: string) {
    return (
      <Observer>
        {() => (
          <>
            {Array.from(this.items)
              .filter(([iid, item]) => item.group === group)
              .map(([iid, item]) => (
                <Observer key={iid}>{() => <>{item.render()}</>}</Observer>
              ))}
          </>
        )}
      </Observer>
    );
  }
}

export const CtxDataViewHeaderExtension = createContext<
  DataViewHeaderExtension
>(new DataViewHeaderExtension());

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
  dataViewHeaderExtension = new DataViewHeaderExtension();

  getDataViewStyle() {
    if (this.props.height !== undefined || this.props.width !== undefined) {
      return {
        flexGrow: 0,
        minHeight: this.props.height,
        minWidth: this.props.width,
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

  renderUiBodyWithHeader() {
    const $cont = scopeFor(this.props.dataView);
    const uiBody = $cont && $cont.resolve(IDataViewBodyUI);
    const isWorking = getIsDataViewOrFormScreenWorking(this.props.dataView);
    return (
      <>
        {/* <div style={{position:"relative"}}> */}
        <div className={S.overlayContainer}>
          <CDataViewHeader isVisible={!this.props.isHeadless} />
          {isWorking && <DataViewLoading />}
        </div>
        <div className={S.dataViewContentContainer}>
          {uiBody && uiBody.render()}
        </div>
      </>
    );
  }

  render() {
    // TODO: Move styling to stylesheet
    const isWorkingDelayed = getIsDataViewOrFormScreenWorkingDelayed(
      this.props.dataView
    );

    return (
      <CtxDataViewHeaderExtension.Provider value={this.dataViewHeaderExtension}>
        <Provider dataView={this.props.dataView}>
          <div className={S.dataView} style={this.getDataViewStyle()}>
            {this.props.dataView?.type === "TreePanel" ? (
              <TreeView dataView={this.props.dataView} />
            ) : (
              this.renderUiBodyWithHeader()
            )}

            {isWorkingDelayed && <DataViewLoading />}
          </div>
        </Provider>
      </CtxDataViewHeaderExtension.Provider>
    );
  }
}
