import {getMainMenu} from "./getMainMenu";

const BREAK_RECURSION = Symbol("BREAK_RECURSION");

export function getMainMenuItemById(ctx: any, id: string) {
  const rawMenu = getMainMenu(ctx)!.menuUI;
  let result: any;
  function recursive(node: any) {
    if (node.attributes.id === id) {
      result = node;
      throw BREAK_RECURSION;
    }
    for (let child of node.elements) {
      recursive(child);
    }
  }
  try {
    recursive(rawMenu);
  } catch (e) {
    if (e === BREAK_RECURSION) {
      return result;
    }
    throw e;
  }
}
