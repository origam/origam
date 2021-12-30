/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { Splitter } from "gui/Components/Splitter/Splitter";
import { CScreenSectionTabbedView } from "gui/connections/CScreenSectionTabbedView";
import { MobXProviderContext, observer } from "mobx-react";
import { onSplitterPositionChangeFinished } from "model/actions-ui/Splitter/onSplitterPositionChangeFinished";
import { IFormScreen } from "model/entities/types/IFormScreen";
import React, { ReactNode } from "react";
import SSplitter from "gui/Workbench/ScreenArea/CustomSplitter.module.scss";
import { findBoxes, findUIChildren, findUIRoot } from "../../../xmlInterpreters/screenXml";
import { Box } from "../../Components/ScreenElements/Box";
import { DataView } from "../../Components/ScreenElements/DataView";
import { Label } from "../../Components/ScreenElements/Label";
import { VBox } from "../../Components/ScreenElements/VBox";
import { WorkflowFinishedPanel } from "gui/Components/WorkflowFinishedPanel/WorkflowFinishedPanel";
import actions from "model/actions-ui-tree";
import { HBox } from "gui/Components/ScreenElements/HBox";
import { IDataView } from "model/entities/types/IDataView";
import { getDataViewById } from "model/selectors/DataView/getDataViewById";
import { serverValueToPanelSizeRatio } from "../../../model/actions-ui/Splitter/splitterPositionToServerValue";
import { pluginLibrary } from "../../../plugins/tools/PluginLibrary";
import { getSessionId } from "../../../model/selectors/getSessionId";
import { IPanelData } from "gui/Components/Splitter/IPanelData";
import { StandaloneDetailNavigator } from "gui/connections/MobileComponents/Navigation/DetailNavigator";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import {
  INavigationNode,
  TabNavigationNode
} from "gui/connections/MobileComponents/Navigation/NavigationNode";
import { RootNavigationNode, TabNavigator } from "gui/connections/MobileComponents/Navigation/TabNavigator";

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
    const dataViewMap = new Map<string, IDataView>();

    function getDataView(xso: any) {
      const dataView = getDataViewById(self.formScreen, xso.attributes.Id);
      if (dataView) {
        dataViewMap.set(xso.attributes.Id, dataView);
      }
      return dataView;
    }

    let currentNavigationNode: NavigationNode2 | undefined;
    const panelMap: { [key: string]: ReactNode } = {};

    function mobileRecursiveBuilder(xso: any): any {
      const isRootLevelNavigationNode = !currentNavigationNode;
      debugger;
      if (xso.attributes.Type === "VSplit" ||
          xso.attributes.Type === "HSplit" ||
          xso.attributes.Type === "VBox"||
          xso.attributes.Type === "HBox") {

        let masterNode = new NavigationNode2();
        let detailNode = new NavigationNode2();

        masterNode.addChild(detailNode);
        if (isRootLevelNavigationNode) {
          currentNavigationNode = detailNode;
        }

        const [masterXmlNode, detailXmlNode] = findUIChildren(xso);
        const masterElement = mobileRecursiveBuilder(masterXmlNode);
        const detailElement = mobileRecursiveBuilder(detailXmlNode);

        if(!masterElement){
          throw new Error ("Master element cannot be null");
        }
        masterNode.element = masterElement;
        masterNode.id = masterXmlNode.attributes.Id;
        masterNode.name = masterXmlNode.attributes.Name;

        if(detailElement){
          detailNode.element = detailElement;
          detailNode.id = detailXmlNode.attributes.Id;
          detailNode.name = detailXmlNode.attributes.Name;
        }

        if (isRootLevelNavigationNode) {
          return <StandaloneDetailNavigator node={masterNode}/>;
        }
        else {
          currentNavigationNode!.merge(masterNode);
          return undefined;
        }

        //
        // const panels = findUIChildren(xso).map((child, idx) => {
        //   const element = mobileRecursiveBuilder(child, true);
        //   return {
        //     modelInstanceId: (element as any).props.modelInstanceId,
        //     element: element,
        //   }
        // });
        //
        // const masterDataView = panels
        //   .map(panel => self.formScreen.getBindingsByParentId(panel.modelInstanceId))
        //   .find(bindings => bindings.length > 0)
        //   ?.[0].parentDataView;
        //
        // if (masterDataView) {
        //   if (parentIsNavigator) {
        //     return panels.find(panel => panel.modelInstanceId === masterDataView.modelInstanceId)!.element;
        //   }
        //   return <StandaloneDetailNavigator node={new NavigationNode(masterDataView, panelMap)}/>;
        // }
      }

      if (xso.attributes.Type === "Tab") {
        const boxes = findBoxes(xso);
        const masterNode = isRootLevelNavigationNode
          ? new RootNavigationNode(xso.attributes.Id)
          : currentNavigationNode!;

        for (const box of boxes) {
          const childXmlNode = findUIChildren(box)[0];
          let tabNode = new NavigationNode2();
          masterNode.addChild(tabNode)

          currentNavigationNode = tabNode;
          const element = mobileRecursiveBuilder(childXmlNode);
          if(element){
            tabNode.element = element;
            tabNode.id = childXmlNode.attributes.Id;
            tabNode.name = childXmlNode.attributes.Name;
          }
        }
        if(isRootLevelNavigationNode){
          return <TabNavigator rootNode={masterNode}/>
        }else{
          return undefined;
        }

        //
        // const nodes = findBoxes(xso).map(box => {
        //   let panels = findUIChildren(box).map(child  => {
        //     const element = mobileRecursiveBuilder(child);
        //     let modelInstanceId = (element as any).props.modelInstanceId;
        //     return {
        //       element: element,
        //       masterDataView: self.formScreen.getBindingsByParentId(modelInstanceId)?.[0]?.parentDataView
        //     }
        //   });
        //   const masterPanel = panels.find(panel => panel.masterDataView);
        //
        //   return new TabNavigationNode({
        //       name: box.attributes.Name,
        //       id: masterPanel?.masterDataView?.id ?? box.attributes.Id,
        //       dataView: masterPanel?.masterDataView,
        //       ctx: self.formScreen,
        //       element: panels.map(panel => panel.element),
        //       panelMap: panelMap
        //     }
        //   )
        // });
        // return <TabNavigator name={xso.attributes.Id} nodes={nodes}/>
      }

      return desktopRecursiveBuilder(xso);
    }

    function desktopRecursiveBuilder(xso: any, parentIsNavigator?: boolean) {
      if (xso.attributes.Type === "ScreenLevelPlugin" ||
        xso.attributes.Type === "SectionLevelPlugin") {
        let dataView = getDataView(xso);
        let sessionId = getSessionId(self.formScreen);
        return pluginLibrary.getComponent(
          {
            name: xso.attributes.Name,
            modelInstanceId: xso.attributes.ModelInstanceId,
            sessionId: sessionId,
            ctx: dataView
          });
      }

      switch (xso.attributes.Type) {
        case "WorkflowFinishedPanel": {
          return (
            <WorkflowFinishedPanel
              key={xso.$iid}
              isCloseButton={xso.attributes.showWorkflowCloseButton}
              isRepeatButton={xso.attributes.showWorkflowRepeatButton}
              message={xso.attributes.Message}
              onCloseClick={actions.workflow.onCloseClick(self.formScreen)}
              onRepeatClick={actions.workflow.onRepeatClick(self.formScreen)}
            />
          );
        }
        case "VSplit":
        case "HSplit": {
          const serverStoredValue = self.formScreen.getPanelPosition(xso.attributes.ModelInstanceId);
          const panelPositionRatio = serverValueToPanelSizeRatio(serverStoredValue);

          const panels: IPanelData[] = findUIChildren(xso).map((child, idx) => {
            return {
              id: idx,
              positionRatio: idx === 0 ? panelPositionRatio : 1 - panelPositionRatio,
              element: recursive(child),
            }
          });
          return (
            <Splitter
              key={xso.$iid}
              STYLE={SSplitter}
              type={xso.attributes.Type === "HSplit" ? "isHoriz" : "isVert"}
              id={xso.attributes.ModelInstanceId}
              onSizeChangeFinished={(
                panelId1: any,
                panelId2: any,
                panel1SizeRatio: number,
                panel2SizeRatio: number
              ) => {
                if (panelId1 === panels[0].id) {
                  onSplitterPositionChangeFinished(self.formScreen)(
                    xso.attributes.ModelInstanceId,
                    panel1SizeRatio
                  );
                }
                if (panelId2 === panels[0].id) {
                  onSplitterPositionChangeFinished(self.formScreen)(
                    xso.attributes.ModelInstanceId,
                    panel2SizeRatio
                  );
                }
              }}
              panels={panels}
            />
          );
        }
        case "Label":
          return (
            <Label
              key={xso.$iid}
              height={parseInt(xso.attributes.Height, 10)}
              text={xso.attributes.Name}
            />
          );
        case "VBox":
          return (
            <VBox
              key={xso.$iid}
              height={xso.attributes.Height ? parseInt(xso.attributes.Height, 10) : undefined}
            >
              {findUIChildren(xso).map((child) => recursive(child))}
            </VBox>
          );
        case "HBox":
          return (
            <HBox
              key={xso.$iid}
              width={xso.attributes.Width ? parseInt(xso.attributes.Width, 10) : undefined}
            >
              {findUIChildren(xso).map((child) => recursive(child))}
            </HBox>
          );
        case "TreePanel":
        case "Grid":
          const dataView = getDataViewById(self.formScreen, xso.attributes.Id);
          if (dataView) {
            dataViewMap.set(xso.attributes.Id, dataView);
          }
          return (
            <DataView
              key={xso.$iid}
              id={xso.attributes.Id}
              modelInstanceId={xso.attributes.ModelInstanceId}
              height={xso.attributes.Height ? parseInt(xso.attributes.Height, 10) : undefined}
              width={xso.attributes.Width ? parseInt(xso.attributes.Width, 10) : undefined}
              isHeadless={xso.attributes.IsHeadless === "true"}
            />
          );
        case "Tab":
          return (
            <CScreenSectionTabbedView
              key={xso.$iid}
              boxes={findBoxes(xso)}
              nextNode={recursive}
              dataViewMap={dataViewMap}
            />
          );
        case "Box":
          return <Box key={xso.$iid}>{findUIChildren(xso).map((child) => recursive(child))}</Box>;
        default:
          console.error("Unknown node:", xso);  // eslint-disable-line no-console
          return null;
      }
    }

    function recursive(xso: any) {
      const element: any = isMobileLayoutActive(self.formScreen)
        ? mobileRecursiveBuilder(xso)
        : desktopRecursiveBuilder(xso);
      panelMap[xso.attributes.ModelInstanceId] = element;
      return element;
    }

    const uiRoot = findUIRoot(this.props.xmlWindowObject);
    return recursive(uiRoot);
  }

  render() {
    return this.buildScreen();
  }
}

class NavigationNode2 implements INavigationNode {
  _children: NavigationNode2[] = [];

  parent: NavigationNode2 | undefined;
  readonly showDetailLinks = true;

  public id: string = "";
  public name: string = "";
  public element: ReactNode = null as any;

  get children() {
    return this._children;
  }

  get parentChain() {
    let parent = this.parent;
    const chain: INavigationNode[] = [this];
    while (parent) {
      chain.push(parent);
      parent = parent.parent;
    }
    return chain.reverse();
  }

  addChild(node: NavigationNode2) {
    this._children.push(node);
    node.parent = this;
  }

  removeChild(node: NavigationNode2) {
    this._children.remove(node);
    node.parent = undefined;
  }

  merge(other: NavigationNode2) {
    this.element = other.element;
    for (const child of [...other.children]) {
      other.removeChild(child);
      this.addChild(child);
    }
    this.id = other.id;
    this.name = other.name;
  }

  equals(other: INavigationNode): boolean {
    return this.id === other.id;
  }
}
