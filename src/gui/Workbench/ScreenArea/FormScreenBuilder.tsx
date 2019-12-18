import { action, computed, observable } from "mobx";
import { observer, Observer, MobXProviderContext } from "mobx-react";
import React from "react";
import SSplitter from "styles/CustomSplitter.module.scss";
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
import { Splitter } from "gui02/components/Splitter/Splitter";
import { CScreenSectionTabbedView } from "gui02/connections/CScreenSectionTabbedView";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { onSplitterPositionChangeFinished } from "model/actions-ui/Splitter/onSplitterPositionChangeFinished";

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
            isActive={this.activePanelId === box.attributes.Id}
            label={box.attributes.Name}
            onClick={() => this.activateTab(box.attributes.Id)}
          />
        ))}
      >
        {boxes.map(box => (
          <Observer>
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
  static contextType = MobXProviderContext;

  get formScreen(): IFormScreen {
    return this.context.formScreen.formScreen;
  }

  buildScreen() {
    const self = this;
    function recursive(xso: any) {
      switch (xso.attributes.Type) {
        case "HSplit": {
          const panels = findUIChildren(xso).map((child, idx) => [
            idx,
            1,
            recursive(child)
          ]);
          return (
            <Splitter
              STYLE={SSplitter}
              type="isHoriz"
              id={xso.attributes.ModelInstanceId}
              sizeOverrideFirstPanel={self.formScreen.getPanelPosition(
                xso.attributes.ModelInstanceId
              )}
              onSizeChangeFinished={(
                panelId1: any,
                panelId2: any,
                panelSize1: number,
                panelSize2: number
              ) => {
                if (panelId1 === panels[0][0]) {
                  onSplitterPositionChangeFinished(self.formScreen)(
                    xso.attributes.ModelInstanceId,
                    panelSize1
                  );
                }
                if (panelId2 === panels[0][0]) {
                  onSplitterPositionChangeFinished(self.formScreen)(
                    xso.attributes.ModelInstanceId,
                    panelSize2
                  );
                }
              }}
              panels={panels as any[]}
            />
          );
        }
        case "VSplit": {
          const panels = findUIChildren(xso).map((child, idx) => [
            idx,
            1,
            recursive(child)
          ]);
          return (
            <Splitter
              STYLE={SSplitter}
              type="isVert"
              id={xso.attributes.ModelInstanceId}
              sizeOverrideFirstPanel={self.formScreen.getPanelPosition(
                xso.attributes.ModelInstanceId
              )}
              onSizeChangeFinished={(
                panelId1: any,
                panelId2: any,
                panelSize1: number,
                panelSize2: number
              ) => {
                if (panelId1 === panels[0][0]) {
                  onSplitterPositionChangeFinished(self.formScreen)(
                    xso.attributes.ModelInstanceId,
                    panelSize1
                  );
                }
                if (panelId2 === panels[0][0]) {
                  onSplitterPositionChangeFinished(self.formScreen)(
                    xso.attributes.ModelInstanceId,
                    panelSize2
                  );
                }
              }}
              panels={panels as any[]}
            />
          );
        }
        case "Label":
          console.log(xso);
          return (
            <Label
              height={parseInt(xso.attributes.Height, 10)}
              text={xso.attributes.Name}
            />
          );
        case "VBox":
          return (
            <VBox
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
              height={
                xso.attributes.Height
                  ? parseInt(xso.attributes.Height, 10)
                  : undefined
              }
              isHeadless={xso.attributes.IsHeadless === "true"}
            />
          );
        case "Tab":
          return (
            <CScreenSectionTabbedView
              boxes={findBoxes(xso)}
              nextNode={recursive}
            />
          );
        case "Box":
          return (
            <Box>{findUIChildren(xso).map(child => recursive(child))}</Box>
          );
        default:
          console.log("Unknown node:", xso);
          return null;
      }
    }

    const uiRoot = findUIRoot(this.props.xmlWindowObject);
    return recursive(uiRoot);
  }

  render() {
    return this.buildScreen();
  }
}
