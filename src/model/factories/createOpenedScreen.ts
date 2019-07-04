import { IOpenedScreen } from "../types/IOpenedScreen";
import { OpenedScreen } from "../OpenedScreen";

export function createOpenedScreen(
  menuItemId: string,
  order: number,
  title: string
): IOpenedScreen {
  return new OpenedScreen({
    menuItemId,
    order,
    title
  });
}
