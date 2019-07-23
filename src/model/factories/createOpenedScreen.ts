import { IOpenedScreen } from "../entities/types/IOpenedScreen";
import { OpenedScreen } from "../entities/OpenedScreen";
import { IFormScreen } from '../entities/types/IFormScreen';

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
