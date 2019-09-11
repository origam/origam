import { action, computed, observable } from "mobx";
import { observer, Observer } from "mobx-react";
import React from "react";
import {
  findBoxes,
  findUIChildren,
  findUIRoot
} from "../../../xmlInterpreters/screenXml";
import { Box } from "../../Components/ScreenElements/Box";
import { DataView } from "../../Components/ScreenElements/DataView";
import { HSplit, HSplitPanel } from "../../Components/ScreenElements/HSplit";
import { Label } from "../../Components/ScreenElements/Label";
import {
  TabbedPanel,
  TabBody,
  TabHandle
} from "../../Components/ScreenElements/TabbedPanel";
import { VBox } from "../../Components/ScreenElements/VBox";
import { VSplit, VSplitPanel } from "../../Components/ScreenElements/VSplit";

@observer
class TabbedPanelHelper extends React.Component<{
  boxes: any[];
  nextNode: (node: any) => React.ReactNode;
}> {
  @observable activePanelId: string =
    this.props.boxes.length > 0 ? this.props.boxes[0].attributes.Id : "";

  @action.bound activateTab(tabId: string) {
    this.activePanelId = tabId;
  }

  render() {
    const { boxes, nextNode } = this.props;
    return (
      <TabbedPanel
        handles={boxes.map(box => (
          <TabHandle
            key={box.attributes.Id}
            isActive={this.activePanelId === box.attributes.Id}
            label={box.attributes.Name}
            onClick={() => this.activateTab(box.attributes.Id)}
          />
        ))}
      >
        {boxes.map(box => (
          <Observer key={box.attributes.Id}>
            {() => (
              <TabBody
                isActive={computed(
                  () => this.activePanelId === box.attributes.Id
                ).get()}
              >
                {findUIChildren(box).map(child => nextNode(child))}
              </TabBody>
            )}
          </Observer>
        ))}
      </TabbedPanel>
    );
  }
}

@observer
export class FormScreenBuilder extends React.Component<{
  xmlWindowObject: any;
}> {
  buildScreen() {
    const self = this;
    function recursive(xso: any) {
      switch (xso.attributes.Type) {
        case "HSplit":
          return (
            <HSplit handleClassName="screenSplitterHandle" key={self.keyGen++}>
              {findUIChildren(xso).map((child, idx) => (
                <HSplitPanel id={idx} key={idx}>
                  {recursive(child)}
                </HSplitPanel>
              ))}
            </HSplit>
          );
        case "VSplit":
          return (
            <VSplit handleClassName="screenSplitterHandle" key={self.keyGen++}>
              {findUIChildren(xso).map((child, idx) => (
                <VSplitPanel id={idx} key={idx}>
                  {recursive(child)}
                </VSplitPanel>
              ))}
            </VSplit>
          );
        case "Label":
          return (
            <Label
              key={self.keyGen++}
              height={parseInt(xso.attributes.Height, 10)}
              text={xso.attributes.Name}
            />
          );
        case "VBox":
          return (
            <VBox
              key={self.keyGen++}
              height={
                xso.attributes.Height
                  ? parseInt(xso.attributes.Height, 10)
                  : undefined
              }
            >
              {findUIChildren(xso).map(child => recursive(child))}
            </VBox>
          );
        case "Grid":
          return (
            <DataView
              id={xso.attributes.Id}
              key={xso.attributes.Id}
              height={
                xso.attributes.Height
                  ? parseInt(xso.attributes.Height, 10)
                  : undefined
              }
              isHeadless={xso.attributes.IsHeadless === "true"}
            >
              null
            </DataView>
          );
        case "Tab":
          return (
            <TabbedPanelHelper
              key={self.keyGen++}
              boxes={findBoxes(xso)}
              nextNode={recursive}
            />
          );

        case "Box":
          return (
            <Box key={self.keyGen++}>
              {findUIChildren(xso).map(child => recursive(child))}
            </Box>
          );
        default:
          console.log("Unknown node:", xso);
          return null;
      }
    }

    const uiRoot = findUIRoot(this.props.xmlWindowObject);
    return recursive(uiRoot);
  }

  keyGen = 1;

  render() {
    this.keyGen = 1;
    return this.buildScreen();
  }
}
