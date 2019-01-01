import * as React from "react";
import { observer, inject } from "mobx-react";
import { GridViewType } from "src/Grid/types";

@observer
export class GridForm extends React.Component<any> {
  public render() {
    /*const { gridPaneBacking } = this.props;
    const isActiveView =
      gridPaneBacking.gridInteractionSelectors.activeView ===
      GridViewType.Form;*/

    const isActiveView = true;
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
