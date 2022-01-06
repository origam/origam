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
import React from "react";
import SSplitter from "gui/Workbench/ScreenArea/CustomSplitter.module.scss";
import { findBoxes, findUIChildren, findUIRoot } from "xmlInterpreters/screenXml";
import { Box } from "gui/Components/ScreenElements/Box";
import { DataView } from "gui/Components/ScreenElements/DataView";
import { Label } from "gui/Components/ScreenElements/Label";
import { VBox } from "gui/Components/ScreenElements/VBox";
import { WorkflowFinishedPanel } from "gui/Components/WorkflowFinishedPanel/WorkflowFinishedPanel";
import actions from "model/actions-ui-tree";
import { HBox } from "gui/Components/ScreenElements/HBox";
import { IDataView } from "model/entities/types/IDataView";
import { getDataViewById } from "model/selectors/DataView/getDataViewById";
import { serverValueToPanelSizeRatio } from "model/actions-ui/Splitter/splitterPositionToServerValue";
import { pluginLibrary } from "plugins/tools/PluginLibrary";
import { getSessionId } from "model/selectors/getSessionId";
import { IPanelData } from "gui/Components/Splitter/IPanelData";
import { StandaloneDetailNavigator } from "gui/connections/MobileComponents/Navigation/DetailNavigator";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { INavigationNode, NavigationNode } from "gui/connections/MobileComponents/Navigation/NavigationNode";
import { TabNavigator } from "gui/connections/MobileComponents/Navigation/TabNavigator";
import { getDataViewLabel } from "model/selectors/DataView/getDataViewLabel";

@observer
export class FormScreenBuilder extends React.Component<{
  title: string;
  xmlWindowObject: any;
}> {
  static contextType = MobXProviderContext;

  get formScreen(): IFormScreen {
    return this.context.formScreen.formScreen;
  }

  buildScreen() {
    const self = this;
    const dataViewMap = new Map<string, IDataView>();
    const uiRoot = findUIRoot(this.props.xmlWindowObject);

    function getDataView(xso: any) {
      const dataView = getDataViewById(self.formScreen, xso.attributes.Id);
      if (dataView) {
        dataViewMap.set(xso.attributes.Id, dataView);
      }
      return dataView;
    }

    function getMasterNavigationNodeName(xmlNode: any, xmlParentNode?: any){
      return !xmlNode.attributes.Name && (xmlNode === uiRoot || xmlParentNode === uiRoot)
        ? self.props.title
        : xmlNode.attributes.Name;
    }

    function mobileRecursiveBuilder(xso: any, currentNavigationNode: INavigationNode | undefined): any {
      const isRootLevelNavigationNode = !currentNavigationNode;
      if (xso.attributes.Type === "VSplit" ||
          xso.attributes.Type === "HSplit" ||
          xso.attributes.Type === "VBox"||
          xso.attributes.Type === "HBox") {

        let masterNavigationNode = new NavigationNode();
        let detailNavigationNode = new NavigationNode();
        masterNavigationNode.addChild(detailNavigationNode);

        const [masterXmlNode, detailXmlNode] = findUIChildren(xso);
        const masterReactElement = mobileRecursiveBuilder(masterXmlNode, masterNavigationNode);
        if(!detailXmlNode){
          return masterReactElement;
        }
        const detailReactElement = mobileRecursiveBuilder(detailXmlNode, detailNavigationNode);

        if(!masterReactElement){
          throw new Error ("Master element cannot be null");
        }
        assignMasterNavigationNodeProperties(masterNavigationNode, masterReactElement, masterXmlNode, xso);
        if(detailReactElement){
          assignDetailNavigationNodeProperties(detailNavigationNode, detailReactElement, detailXmlNode, xso);
        }

        if (isRootLevelNavigationNode) {
          return <StandaloneDetailNavigator node={masterNavigationNode}/>;
        }
        else {
          currentNavigationNode!.merge(masterNavigationNode);
          return masterReactElement;
        }
      }

      if (xso.attributes.Type === "Tab") {
        const boxes = findBoxes(xso);
        const masterNode = new NavigationNode();
        masterNode.id = xso.attributes.Id;
        masterNode.name = getMasterNavigationNodeName(xso);
        masterNode.formScreen = self.formScreen;

        for (const box of boxes) {
          const childXmlNode = findUIChildren(box)[0];
          let tabNode = new NavigationNode();
          masterNode.addChild(tabNode)

          const element = mobileRecursiveBuilder(childXmlNode, tabNode);
          if(element){
            tabNode.element = element;
            tabNode.id = childXmlNode.attributes.Id;
            tabNode.name = childXmlNode.attributes.Name === ""
              ? box.attributes.Name
              : childXmlNode.attributes.Name;
            tabNode.dataView = self.formScreen.getDataViewByModelInstanceId(childXmlNode.attributes.ModelInstanceId)
          }
        }
        if(isRootLevelNavigationNode){
          return <TabNavigator rootNode={masterNode}/>
        }else{
          currentNavigationNode?.merge(masterNode);
          return undefined;
        }
      }
      if(xso.attributes.Type === "TreePanel" || xso.attributes.Type === "Grid"){
        return (
          <DataView
            key={xso.$iid}
            id={xso.attributes.Id}
            modelInstanceId={xso.attributes.ModelInstanceId}
            isHeadless={xso.attributes.IsHeadless === "true"}
          />
        );
      }

      return desktopRecursiveBuilder(xso);
    }

    function desktopRecursiveBuilder(xso: any, parentIsNavigator?: boolean) {
      switch (xso.attributes.Type) {
        case "ScreenLevelPlugin":
        case "SectionLevelPlugin": {
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
      return isMobileLayoutActive(self.formScreen)
        ? mobileRecursiveBuilder(xso, undefined)
        : desktopRecursiveBuilder(xso);
    }

    function assignMasterNavigationNodeProperties(navigationNode: NavigationNode, element: any, xmlNode: any, parentXmlElement: any) {
      navigationNode.element = element;
      navigationNode.id = xmlNode.attributes.Id;
      navigationNode.formScreen = self.formScreen;
      navigationNode.dataView =  self.formScreen.getDataViewByModelInstanceId(element.props?.modelInstanceId);
      navigationNode.name = getMasterNavigationNodeName(xmlNode, parentXmlElement);
    }

    function assignDetailNavigationNodeProperties(navigationNode: NavigationNode, element: any, xmlNode: any, parentXmlElement: any) {
      navigationNode.element = element;
      navigationNode.id = xmlNode.attributes.Id;
      navigationNode.formScreen = self.formScreen;
      navigationNode.dataView =  self.formScreen.getDataViewByModelInstanceId(element.props?.modelInstanceId);
      navigationNode.name = getDataViewLabel(navigationNode.dataView) ?? element.attributes.Name;
    }

    return recursive(uiRoot);
  }


  render() {
    return this.buildScreen();
  }
}
