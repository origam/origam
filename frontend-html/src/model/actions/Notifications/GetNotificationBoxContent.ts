import { getNotifications } from "model/selectors/Chatrooms/getNotifications";
import { handleError } from "model/actions/handleError";

export function getNotificationBoxContent(ctx: any) {
  return function* getNotificationBoxContent() {
    try {
      yield* getNotifications(ctx).getNotificationBoxContent();
    } catch (e) {
      yield* handleError(ctx)(e);
      console.log("Error during getNotificationBoxContent call ignored.");
    }
  };
}
