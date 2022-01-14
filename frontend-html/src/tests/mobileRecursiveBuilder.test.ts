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

import { mobileRecursiveBuilder } from "gui/Workbench/ScreenArea/FormScreenBuilder/mobileRecursiveBuilder";
import xmlJs from "xml-js";
import { IFormScreen } from "model/entities/types/IFormScreen";
import { INavigationNode } from "gui/connections/MobileComponents/Navigation/NavigationNode";
import { IComponentFactory } from "gui/Workbench/ScreenArea/FormScreenBuilder/IComponentFactory";
import { findUIRoot } from "xmlInterpreters/xmlUtils";


const fs = require('fs');
const path = require('path');

function readFile(relPath: string) {
  return fs.readFileSync(path.join(__dirname, relPath));
}

class MockFormScreen{
  getDataViewByModelInstanceId(id: string){
    return undefined;
  }

  parent: any;
}

class MockComponentFactory implements IComponentFactory{
  getDataView(args: { key: string; id: string; modelInstanceId: string; isHeadless: boolean }): React.ReactNode {
    return {name: "MockDataViewComponent", props: {id: undefined}};
  }

  getDetailNavigator(masterNavigationNode: INavigationNode): React.ReactNode {
    return {
      name: "MockDetailNavigatorComponent",
      node: masterNavigationNode,
      props: {id: undefined}};
  }

  getTabNavigator(masterNode: INavigationNode): React.ReactNode {
    return {
      name: "MockTabNavigatorComponent",
      node: masterNode,
      props: {id: undefined}};
  }
}

Array.prototype.remove = function (item) {
  const index = this.indexOf(item);
  if (index > -1) {
    this.splice(index, 1);
  }
  return this;
}

const mockOpenScreen = {
  $type_IOpenedScreen: 1,
  content:{
    formScreen:{
      dataViews: []
    }
  }
};
const mockFormScreen = new MockFormScreen();
mockFormScreen.parent = mockOpenScreen;

function getUiRoot(relativeTestFilePath: string){
  const xmlString = readFile(relativeTestFilePath);
  const xmlWindowObject = xmlJs.xml2js(xmlString, {
    addParent: true,
    alwaysChildren: true,
  });
  return findUIRoot(xmlWindowObject);
}

test('Should parse screen with tab panel inside of split panel', () => {

  const uiRoot = getUiRoot("testFiles/screen_with_tab_panel_in_split_panel.xml");

  const resultNode = mobileRecursiveBuilder({
    uiRoot: uiRoot,
    formScreen: mockFormScreen as any,
    title: "Test Title",
    desktopRecursiveBuilder: (formScreen: IFormScreen, xso: any) => {return {name: "MockReactNode"}} ,
    componentFactory: new MockComponentFactory(),
  });

  expect(resultNode.node.children.length).toBe(7);

  const acceptedOffersNode = resultNode.node.children[0];
  expect(acceptedOffersNode.name).toBe("Accepted Offers");
  expect(acceptedOffersNode.children.length).toBe(1);
  expect(acceptedOffersNode.children[0].id).toBe("AsPanel3_6");

  const paymentsNode = resultNode.node.children[1];
  expect(paymentsNode.name).toBe("Payments");
  expect(paymentsNode.children.length).toBe(0);

  const consumablesNode = resultNode.node.children[2];
  expect(consumablesNode.name).toBe("Consumables");
  expect(consumablesNode.children.length).toBe(1);
  expect(consumablesNode.children[0].id).toBe("SplitPanel7_12");

  const accessoriesNode = resultNode.node.children[3];
  expect(accessoriesNode.name).toBe("Accessories");
  expect(accessoriesNode.children.length).toBe(1);
  expect(accessoriesNode.children[0].id).toBe("AsPanel2_18");

  const servicesNode = resultNode.node.children[4];
  expect(servicesNode.name).toBe("Services");
  expect(servicesNode.children.length).toBe(1);
  expect(servicesNode.children[0].id).toBe("AsPanel3_22");

  const extrasNode = resultNode.node.children[5];
  expect(extrasNode.name).toBe("Extras");
  expect(extrasNode.children.length).toBe(1);
  expect(extrasNode.children[0].id).toBe("AsPanel15_26");

  const lessorNotesNode = resultNode.node.children[6];
  expect(lessorNotesNode.name).toBe("Lessor Notes");
  expect(lessorNotesNode.children.length).toBe(0);
});