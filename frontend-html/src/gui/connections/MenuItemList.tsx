/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

import React from "react";
import { MainMenuUL } from "gui/Components/MainMenu/MainMenuUL";
import { MainMenuLI } from "gui/Components/MainMenu/MainMenuLI";
import { getMainMenu } from "model/selectors/MainMenu/getMainMenu";
import { CMainMenuFolderItem, CMainMenuCommandItem } from "./CMainMenu";
import { observer } from "mobx-react";

export const MenuItemList = observer((props: { ctx: any; }) => {
  const mainMenu = getMainMenu(props.ctx);
  return mainMenu ? listFromNode(mainMenu.menuUI, 1, true) : null;
});

function itemForNode(node: any, level: number, isOpen: boolean) {
  switch (node.name) {
    case "Submenu":
      return (
        <MainMenuLI key={node.$iid}>
          <CMainMenuFolderItem node={node} level={level} isOpen={isOpen} />
        </MainMenuLI>
      );
    case "Command":
      return (
        <MainMenuLI key={node.$iid}>
          <CMainMenuCommandItem node={node} level={level} isOpen={isOpen} />
        </MainMenuLI>
      );
    default:
      return <></>;
  }
}

export function listFromNode(node: any, level: number, isOpen: boolean) {
  return (
    <MainMenuUL>
      {node.elements
        .filter((childNode: any) => childNode.attributes.isHidden !== "true")
        .map((node: any) => itemForNode(node, level, isOpen))}
    </MainMenuUL>
  );
}
