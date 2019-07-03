import { findStopping } from "./xmlUtils";

export const findMenu = (node: any) =>
  findStopping(node, n => n.name === "Menu")[0];

