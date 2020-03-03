import { getOpenedScreens } from "model/selectors/getOpenedScreens";
import { stopAutoreload } from "../FormScreen/stopAutoreload";

export function stopAllFormsAutorefresh(ctx: any) {
  return function*() {
    for (let openedScreen of getOpenedScreens(ctx).items) {
      if (
        openedScreen.content &&
        !openedScreen.content.isLoading &&
        openedScreen.content.formScreen
      ) {
        yield* stopAutoreload(openedScreen.content.formScreen)();
      }
    }
  };
}
