import * as React from "react";
import { observer, inject } from "mobx-react";
import { GridViewType } from "src/Grid/types";
import { IDataViewType } from '../skeleton/types';

@inject("dataViewState")
@observer
export class GridForm extends React.Component<any> {
  public render() {
    /*const { gridPaneBacking } = this.props;
    const isActiveView =
      gridPaneBacking.gridInteractionSelectors.activeView ===
      GridViewType.Form;*/

    // TODO: Types for this component

    const isActiveView = this.props.dataViewState.activeView === IDataViewType.Form;
    return (
      <div
        className="oui-form-root"
        style={{ display: !isActiveView ? "none" : undefined }}
      >
        {this.props.children}
      </div>
    );
  }
}
