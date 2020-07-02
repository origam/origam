import {getMainMenu} from "./getMainMenu";

export function getMainMenuExists(ctx: any) {
  return getMainMenu(ctx) !== undefined;
}