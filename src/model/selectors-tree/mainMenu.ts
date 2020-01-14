import { getMainMenu } from "model/selectors/MainMenu/getMainMenu"

export default {
  getItemById(ctx: any, id: string) {
    // TODO: What if MainMenu is not yet loaded?
    return getMainMenu(ctx)!.getItemById(id);
  }
}