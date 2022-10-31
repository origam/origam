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

  const node1 = resultNode.node.children[0];
  expect(node1.name).toBe("Accepted Offers");
  expect(node1.children.length).toBe(1);
  expect(node1.children[0].id).toBe("AsPanel3_6");

  const node2 = resultNode.node.children[1];
  expect(node2.name).toBe("Payments");
  expect(node2.children.length).toBe(0);

  const node3 = resultNode.node.children[2];
  expect(node3.name).toBe("Consumables");
  expect(node3.children.length).toBe(1);
  expect(node3.children[0].id).toBe("AsPanel6_13");

  const node4 = resultNode.node.children[3];
  expect(node4.name).toBe("Accessories");
  expect(node4.children.length).toBe(1);
  expect(node4.children[0].id).toBe("AsPanel2_18");

  const node5 = resultNode.node.children[4];
  expect(node5.name).toBe("Services");
  expect(node5.children.length).toBe(1);
  expect(node5.children[0].id).toBe("AsPanel3_22");

  const node6 = resultNode.node.children[5];
  expect(node6.name).toBe("Extras");
  expect(node6.children.length).toBe(1);
  expect(node6.children[0].id).toBe("AsPanel15_26");

  const node7 = resultNode.node.children[6];
  expect(node7.name).toBe("Lessor Notes");
  expect(node7.children.length).toBe(0);
});

test('Should parse screen with split panel inside of tab panel', () => {

  const uiRoot = getUiRoot("testFiles/screen_with_split_panel_in_tab_panel.xml");

  const resultNode = mobileRecursiveBuilder({
    uiRoot: uiRoot,
    formScreen: mockFormScreen as any,
    title: "Test Title",
    desktopRecursiveBuilder: (formScreen: IFormScreen, xso: any) => {return {name: "MockReactNode"}} ,
    componentFactory: new MockComponentFactory(),
  });

  expect(resultNode.node.children.length).toBe(10);

  const node1 = resultNode.node.children[0];
  expect(node1.name).toBe("TestTab1");
  expect(node1.children.length).toBe(0);

  const node2 = resultNode.node.children[1];
  expect(node2.name).toBe("TestTab2");
  expect(node2.children.length).toBe(0);

  const node3 = resultNode.node.children[2];
  expect(node3.name).toBe("TestTab3");
  expect(node3.children.length).toBe(0);

  const node4 = resultNode.node.children[3];
  expect(node4.name).toBe("TestTab4");
  expect(node4.children.length).toBe(0);

  const node5 = resultNode.node.children[4];
  expect(node5.name).toBe("TestTab5");
  expect(node5.children.length).toBe(0);

  const node6 = resultNode.node.children[5];
  expect(node6.name).toBe("TestTab6");
  expect(node6.children.length).toBe(0);

  const node7 = resultNode.node.children[6];
  expect(node7.name).toBe("TestTab7");
  expect(node7.children.length).toBe(1);
  expect(node7.children[0].name).toBe("Detail1");

  const node8 = resultNode.node.children[7];
  expect(node8.name).toBe("TestTab8");
  expect(node8.children.length).toBe(0);

  const node9 = resultNode.node.children[8];
  expect(node9.name).toBe("TestTab9");
  expect(node9.children.length).toBe(0);

  const node10 = resultNode.node.children[9];
  expect(node10.name).toBe("TestTab10");
  expect(node10.children.length).toBe(0);
});

test('Should parse screen with split panel inside of split panel', () => {

  const uiRoot = getUiRoot("testFiles/screen_with_split_panel_in_split_panel.xml");

  const resultNode = mobileRecursiveBuilder({
    uiRoot: uiRoot,
    formScreen: mockFormScreen as any,
    title: "Test Title",
    desktopRecursiveBuilder: (formScreen: IFormScreen, xso: any) => {return {name: "MockReactNode"}} ,
    componentFactory: new MockComponentFactory(),
  });

  expect(resultNode.node.children.length).toBe(1);

  const node1 = resultNode.node;
  expect(node1.id).toBe("AsPanel1_1");
  expect(node1.children.length).toBe(1);

  const node2 = node1.children[0];
  expect(node2.name).toBe("Detail2");
  expect(node2.children.length).toBe(0);
});

test('Should parse screen with nested split panels and binding that alter the split panel node relationships', () => {

  const uiRoot = getUiRoot("testFiles/screen_with_nested_split_panels_and_bindings.xml");

  const resultNode = mobileRecursiveBuilder({
    uiRoot: uiRoot,
    formScreen: mockFormScreen as any,
    title: "Test Title",
    desktopRecursiveBuilder: (formScreen: IFormScreen, xso: any) => {return {name: "MockReactNode"}} ,
    componentFactory: new MockComponentFactory(),
  });

  expect(resultNode.node.children.length).toBe(1);
  //
  // const node1 = resultNode.node;
  // expect(node1.id).toBe("AsPanel1_1");
  // expect(node1.children.length).toBe(1);
  //
  // const node2 = node1.children[0];
  // expect(node2.name).toBe("Detail2");
  // expect(node2.children.length).toBe(0);
});