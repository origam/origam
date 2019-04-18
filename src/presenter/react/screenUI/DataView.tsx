import * as React from "react";
import { inject, observer } from "mobx-react";
import { parseNumber } from "../../../utils/xml";
import { TableView } from "./Perspectives/TableView/TableView";
import { ITableView } from "../../view/Perspectives/TableView/types";
import { FormView } from "./Perspectives/FormView/FormView";
import { IFormView } from "../../view/Perspectives/FormView/types";
import {
  IToolbar,
  IToolbarButtonState,
  IViewTypeBtn
} from "../../view/Perspectives/types";
import { IViewType } from "../../../DataView/types/IViewType";
import { DataView as DataViewModel } from "../../../DataView/DataView";
import { FormView as FormViewModel } from "../../../DataView/FormView/FormView";
import { IFormScreen } from "../../../Screens/FormScreen/types";
import { isFormView } from "../../../DataView/FormView/types";
import { isTableView } from "../../../DataView/TableView/ITableView";

/*@inject(
  (
    { dataViewsMap }: { dataViewsMap: Map<string, IDataView> },
    { id }: { id: string }
  ) => {
    return {
      dataView: dataViewsMap.get(id)
    };
  }
)*/

@inject(({ formScreen }: { formScreen: IFormScreen }, { Id }) => {
  const dataView = formScreen.dataViewMap.get(Id);
  return {
    dataView
  };
})
@observer
export class DataView extends React.Component<{
  Id: string;
  Height?: string;
  dataView?: DataViewModel;
}> {
  // tableView: ITableView

  getView() {
    const dataView = this.props.dataView!;
    if (dataView && dataView.availViews.activeView) {
      if (isFormView(dataView.availViews.activeView)) {
        return <FormView controller={dataView.availViews.activeView} />;
      }
      if (isTableView(dataView.availViews.activeView)) {
        return <TableView controller={dataView.availViews.activeView} />;
      }
      return this.props.Id;
    } else {
      return this.props.Id;
    }
    /*
  
    if (!this.props.dataView || !this.props.dataView.activeView) {
      return this.props.id;
    } else {
      return this.props.id;
      switch (this.props.dataView.activeView.type) {
        case IViewType.FormView:
          return <FormView controller={this.props.dataView.activeView} />;
        case IViewType.TableView:
          return <TableView controller={this.props.dataView.activeView} />;
        default:
          const n: never = this.props.dataView.activeView;
          console.log(this.props.dataView.activeView);
          throw new Error("Unknown view.");*/
  }

  getDataViewStyle() {
    if (this.props.Height) {
      return {
        flexGrow: 0,
        height: parseNumber(this.props.Height)
      };
    } else {
      return {
        flexGrow: 1
      };
    }
  }

  render() {
    return (
      <div className="data-view" style={this.getDataViewStyle()}>
        {this.getView()}
      </div>
    );
  }
}
