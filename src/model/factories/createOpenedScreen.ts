import { IOpenedScreen } from "../types/IOpenedScreen";
import { OpenedScreen } from "../OpenedScreen";
import { IFormScreen } from '../types/IFormScreen';

export function createOpenedScreen(
  menuItemId: string,
  order: number,
  title: string,
  content: IFormScreen
): IOpenedScreen {
  return new OpenedScreen({
    menuItemId,
    order,
    title,
    content
  });
}
