import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getChatrooms } from "model/selectors/Chatrooms/getChatrooms";
import { getSearcher } from "model/selectors/getSearcher";

export function onRefreshChatrooms(ctx: any) {
  return function *onRefreshChatrooms(){
    try {
      const chatRooms = getChatrooms(ctx);
      yield* chatRooms.getChatroomsList();
      getSearcher(ctx).indexChats(chatRooms.items);
    } catch (e) {
      yield* handleError(ctx)(e);
      console.log("Error during getChatroomsList call ignored.");
    };
  }
}
