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

import { IFormScreen } from "model/entities/types/IFormScreen";
import { INavigationNode, NavigationNode } from "gui/connections/MobileComponents/Navigation/NavigationNode";
import { getDataViewById } from "model/selectors/DataView/getDataViewById";
import { getDataViewLabel } from "model/selectors/DataView/getDataViewLabel";
import { IComponentFactory } from "gui/Workbench/ScreenArea/FormScreenBuilder/IComponentFactory";
import { findBoxes, findUIChildren } from "xmlInterpreters/xmlUtils";

interface IParsingResult {
  navigatorElement: any | undefined,
  nodes: (INavigationNode | undefined)[]
}

export function mobileRecursiveBuilder(args:{
  formScreen: IFormScreen;
  title: string;
  uiRoot: any;
  desktopRecursiveBuilder: (formScreen: IFormScreen, xso: any) => any;
  componentFactory: IComponentFactory;
}) {

  function run(xso: any, currentNavigationNode: INavigationNode | undefined): IParsingResult {
    const isRootLevelNavigationNode = !currentNavigationNode;
    if (xso.attributes.Type === "VSplit" ||
      xso.attributes.Type === "HSplit" ||
      xso.attributes.Type === "VBox" ||
      xso.attributes.Type === "HBox") {

      let masterNavigationNode = new NavigationNode();
      let detailNavigationNode = new NavigationNode();

      const [masterXmlNode, detailXmlNode] = findUIChildren(xso);
      const parsingResult = run(masterXmlNode, masterNavigationNode);
      const masterReactElement = parsingResult.navigatorElement;
      if (!detailXmlNode) {
        if (masterReactElement) {
          return parsingResult;
        } else {
          return {
            navigatorElement: args.componentFactory.getDetailNavigator(masterNavigationNode),
            nodes:  [masterNavigationNode]
          };
        }
      }
      const detailResult = run(detailXmlNode, detailNavigationNode);
      const detailReactElement = detailResult.navigatorElement;
      const detailNodes = detailResult.nodes;

      if (!masterReactElement) {
        // Happens if the top element is a splitPanel with tabControls in both master and detail slots.
        return {
          navigatorElement: wrapInNavigator(masterNavigationNode, detailNavigationNode,
            args.formScreen, masterXmlNode,  xso),
          nodes:  [masterNavigationNode]
        };
      }
      masterNavigationNode.addChildren(detailNodes);
      assignMasterNavigationNodeProperties(masterNavigationNode, masterReactElement, masterXmlNode, xso);

      if (detailReactElement) {
        assignDetailNavigationNodeProperties(detailNavigationNode, detailReactElement, detailXmlNode);
      }

      if (isRootLevelNavigationNode) {
        return {
          navigatorElement: args.componentFactory.getDetailNavigator(masterNavigationNode),
          nodes:  [masterNavigationNode]
        };
      } else {
        currentNavigationNode!.merge(masterNavigationNode);
        return {
          navigatorElement: masterReactElement,
          nodes:  [masterNavigationNode]
        };
      }
    }

    if (xso.attributes.Type === "Tab") {
      const tabNodes = findBoxes(xso).map(box => {
        const childXmlNode = findUIChildren(box)[0];
        const tabNode = new NavigationNode();
        const {navigatorElement} = run(childXmlNode, tabNode);
        if (navigatorElement) {
          tabNode.element = navigatorElement;
          tabNode.id = childXmlNode.attributes.Id;
          tabNode.name = box.attributes.Name
            ? box.attributes.Name
            : childXmlNode.attributes.Name;
          tabNode.dataView = getDataViewById(args.formScreen, navigatorElement.props.id);
        }
        return tabNode;
      });

      const masterNode = new NavigationNode();
      masterNode.id = xso.attributes.Id;
      masterNode.name = getMasterNavigationNodeName(xso);
      masterNode.formScreen = args.formScreen;
      tabNodes.forEach(tabNode => masterNode.addChild(tabNode));
      if (isRootLevelNavigationNode) {
        return {
          navigatorElement: args.componentFactory.getDetailNavigator(masterNode),
          nodes: tabNodes
        };
      } else {
        mergeNodesToParentView(currentNavigationNode, masterNode, tabNodes);
        return {
          navigatorElement: undefined,
          nodes: tabNodes
        };
      }
    }
    if (xso.attributes.Type === "TreePanel" || xso.attributes.Type === "Grid") {
      const dataView = args.componentFactory.getDataView(
        {
          key: xso.$iid,
          id:xso.attributes.Id,
          modelInstanceId: xso.attributes.ModelInstanceId,
          isHeadless: xso.attributes.IsHeadless === "true"
        });
      return {
        navigatorElement: dataView,
        nodes: [currentNavigationNode]
      };
    }
    return {
      navigatorElement: args.desktopRecursiveBuilder(args.formScreen, xso),
      nodes: [currentNavigationNode]
    };
  }

  function wrapInNavigator(childNode1: NavigationNode, childNode2: NavigationNode,
      formScreen: IFormScreen, masterXmlNode: any,  parentXmlElement: any){
    const navigationNode = new NavigationNode();
    navigationNode.id = parentXmlElement.attributes.Id;
    navigationNode.addChild(childNode1);
    navigationNode.addChild(childNode2);
    navigationNode.name = getMasterNavigationNodeName(masterXmlNode, parentXmlElement);
    navigationNode.formScreen = formScreen
    return args.componentFactory.getDetailNavigator(navigationNode);
  }

  function getMasterNavigationNodeName(xmlNode: any, xmlParentNode?: any) {
    return !xmlNode.attributes.Name && (xmlNode === args.uiRoot || xmlParentNode === args.uiRoot)
      ? args.title
      : xmlNode.attributes.Name;
  }

  function assignMasterNavigationNodeProperties(navigationNode: NavigationNode, element: any, xmlNode: any, parentXmlElement: any) {
    navigationNode.element = element;
    navigationNode.id = xmlNode.attributes.Id;
    navigationNode.formScreen = args.formScreen;
    navigationNode.dataView = args.formScreen.getDataViewByModelInstanceId(element.props?.modelInstanceId);
    navigationNode.name = getMasterNavigationNodeName(xmlNode, parentXmlElement);
  }

  function assignDetailNavigationNodeProperties(navigationNode: NavigationNode, reactElement: any, xmlNode: any) {
    navigationNode.element = reactElement;
    navigationNode.id = xmlNode.attributes.Id;
    navigationNode.formScreen = args.formScreen;
    navigationNode.dataView = args.formScreen.getDataViewByModelInstanceId(reactElement.props?.modelInstanceId);
    navigationNode.name = navigationNode.dataView ? getDataViewLabel(navigationNode.dataView) : xmlNode.attributes.Name;
  }

  function mergeNodesToParentView(currentNavigationNode: INavigationNode, masterNode: INavigationNode, tabNodes: INavigationNode[]) {
    const parent = currentNavigationNode.parent;
    if (parent) {
      parent.removeChild(currentNavigationNode);
      tabNodes.forEach(tabNode => parent.addChild(tabNode));
    } else {
      // tabNodes.forEach(tabNode => currentNavigationNode.addChild(tabNode));
      currentNavigationNode.merge(masterNode);
    }
  }

  return run(args.uiRoot, undefined).navigatorElement;
}
