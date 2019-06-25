import React from "react";
import { DefaultScreen } from "../DefaultScreen";
import { IFormScreen } from "../../../../Screens/FormScreen/types";
import { observer, Provider } from "mobx-react";
import { IUIScreenTreeNode } from "../../../view/types";
import { HSplit } from "../HSplit";
import { VSplit } from "../VSplit";
import { Label } from "../Label";
import { VBox } from "../VBox";
import { TabbedPanel } from "../TabbedPanel";
import { Box } from "../Box";
import { DataView } from "../DataView";
import { FormToolbar } from "./FormToolbar";

@observer
export class FormScreen extends React.Component<{ formScreen: IFormScreen }> {
  getScreen() {
    if (!this.props.formScreen.uiStructure) {
      return null;
    }
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
          return <DataView key={keyGen++} {...node.props} />;
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
            <Box key={keyGen++} {...node.props}>
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

    return this.props.formScreen.uiStructure.map((child: IUIScreenTreeNode) =>
      recursive(child)
    );
  }

  render() {
    return (
      <Provider formScreen={this.props.formScreen}>
        <DefaultScreen
          title={this.props.formScreen.title}
          isLoading={this.props.formScreen.isLoading}
          isVisible={this.props.formScreen.isVisible}
          isSessioned={this.props.formScreen.isSessioned}
        >
          {this.props.formScreen.isVisible &&
            this.props.formScreen.isSessioned &&
            this.props.formScreen.isDirty && <FormToolbar />}
          {this.getScreen()}
        </DefaultScreen>
      </Provider>
    );
  }
}
