import * as React from "react";
import { inject, observer } from "mobx-react";
import { FormView } from "../FormView/FormView";
import { TableView } from "../TableView/TableView";
import { IDataView, IViewType } from "src/presenter/types/IScreenPresenter";

@inject(
  (
    { dataViewsMap }: { dataViewsMap: Map<string, IDataView> },
    { id }: { id: string }
  ) => {
    return {
      dataView: dataViewsMap.get(id)
    };
  }
)
@observer
export class DataView extends React.Component<{
  id: string;
  height?: number;
  dataView?: IDataView;
}> {
  getView() {
    if (!this.props.dataView || !this.props.dataView.activeView) {
      return this.props.id;
    } else {
      switch (this.props.dataView.activeView.type) {
        case IViewType.FormView:
          return <FormView controller={this.props.dataView.activeView} />;
        case IViewType.TableView:
          return <TableView controller={this.props.dataView.activeView} />;
        default:
          const n: never = this.props.dataView.activeView;
          console.log(this.props.dataView.activeView);
          throw new Error("Unknown view.");
      }
    }
  }

  getDataViewStyle() {
    if (this.props.height) {
      return {
        flexGrow: 0,
        height: this.props.height
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
