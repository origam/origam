import * as React from "react";
import { observer, Provider } from "mobx-react";

import { Root } from "./Root";
import { HSplit } from "./HSplit";
import { VSplit } from "./VSplit";
import { Label } from "./Label";
import { VBox } from "./VBox";
import { DataView } from "./DataView";
import { TabbedPanel } from "./TabbedPanel";
import { IUIScreenTreeNode, IPanelDef } from "src/presenter/types/IUIScreenBlueprints";
import { IScreen } from "src/presenter/types/IScreenPresenter";

@observer
export class Screen extends React.Component<{ controller: IScreen }> {
  buildScreen() {
    let keyGen = 0;
    function recursive(node: IUIScreenTreeNode) {
      switch (node.type) {
        case "Root":
          return (
            <Root>
              {node.children.map((child: IUIScreenTreeNode) =>
                recursive(child)
              )}
            </Root>
          );
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
        case "DataView":
          return (
            <DataView key={keyGen++} {...node.props}>
              {node.children.map((child: IUIScreenTreeNode) =>
                recursive(child)
              )}
            </DataView>
          );
        case "TabbedPanel":
          const props = {
            ...node.props,
            panels: node.props.panels.map((panel: IPanelDef) => ({
              ...panel,
              content: panel.content.map((child: IUIScreenTreeNode) =>
                recursive(child)
              )
            }))
          };
          return <TabbedPanel key={keyGen++} {...props} />;
        case "TreePanel":
          return null;
        case "Box":
          return null;
        case "FormRoot":
          return null;
        // TODO...
        default:
          const n: never = node;
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
      <Provider
        tabPanelsMap={this.props.controller.tabPanelsMap}
        dataViewsMap={this.props.controller.dataViewsMap}
      >
        <div
          className={
            "screen" + (this.props.controller.isFullScreen ? " fullscreen" : "")
          }
        >
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
                "screen-fullscreen" +
                (this.props.controller.isFullScreen ? " active" : "")
              }
            >
              <i className="fas fa-external-link-alt" />
            </button>
          </div>
          <div className="screen-body">
            {this.buildScreen() /*this.props.children*/}
          </div>
        </div>
      </Provider>
    );
  }
}
