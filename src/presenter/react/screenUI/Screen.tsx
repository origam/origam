import { action, observable } from "mobx";
import { observer, Provider } from "mobx-react";
import * as React from "react";
import { IScreen, IUIScreenTreeNode } from "../../view/types";
import { Box } from "./Box";
import { DataView } from "./DataView";
import { HSplit } from "./HSplit";
import { Label } from "./Label";
import { TabbedPanel } from "./TabbedPanel";
import { VBox } from "./VBox";
import { VSplit } from "./VSplit";

@observer
export class Screen extends React.Component<{ controller: IScreen }> {
  @observable isFullScreen = false;

  @action.bound handleFullScreenBtnClick() {
    this.isFullScreen = !this.isFullScreen;
  }
  
  getScreen() {
    console.log(this.props.controller.uiStructure);
    let keyGen = 0;
    function recursive(node: IUIScreenTreeNode) {
      // console.log(node);
      switch (node.type) {
        case "HSplit":
          return (
            <HSplit key={keyGen++} {...node.props}>
              {node.children.map((child: IUIScreenTreeNode) =>
                recursive(child)
              )}
            </HSplit>
          );
        case "VSplit":
          return (
            <VSplit key={keyGen++} {...node.props}>
              {node.children.map((child: IUIScreenTreeNode) =>
                recursive(child)
              )}
            </VSplit>
          );
        case "Label":
          return <Label key={keyGen++} {...node.props} />;
        case "VBox":
          return (
            <VBox key={keyGen++} {...node.props}>
              {node.children.map((child: IUIScreenTreeNode) =>
                recursive(child)
              )}
            </VBox>
          );
        case "Grid":
          return (
            <DataView key={keyGen++} {...node.props} />
          );
        case "Tab":
          return (
            <TabbedPanel key={keyGen++} {...node.props}>
              {node.children.map((child: IUIScreenTreeNode) =>
                recursive(child)
              )}
            </TabbedPanel>
          );
        case "Box":
          return (
            <Box key={keyGen++} {...node.props} >
              {node.children.map((child: IUIScreenTreeNode) =>
                recursive(child)
              )}
            </Box>
          );
        case "TreePanel":
          return null;
        // TODO...
        default:
          // const n: never = node;
          console.log(node);
          throw new Error("Unknown node " + (node as any).type);
      }
    }

    return this.props.controller.uiStructure.map((child: IUIScreenTreeNode) =>
      recursive(child)
    );
  }

  render() {
    return (
      <Provider dataViewsMap={this.props.controller.dataViewsMap}>
        <div className={"screen" + (this.isFullScreen ? " fullscreen" : "")}>
          <div className="screen-header">
            <div className="screen-icon">
              {!this.props.controller.isLoading ? (
                <i className="fas fa-file-alt" />
              ) : (
                <i className="fas fa-sync-alt fa-spin" />
              )}
            </div>
            {this.props.controller.screenTitle}
            <div className="pusher" />
            <button
              className={
                "screen-fullscreen" + (this.isFullScreen ? " active" : "")
              }
              onClick={this.handleFullScreenBtnClick}
            >
              <i className="fas fa-external-link-alt" />
            </button>
          </div>
          <div className="screen-body">{this.getScreen()}</div>
        </div>
      </Provider>
    );
  }
}
