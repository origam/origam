import { TypeSymbol } from "dic/Container";
import { PubSub } from "utils/events";

export class ScreenEvents {
  focusField = new PubSub<{propertyId: string}>();
}
export const IScreenEvents = TypeSymbol<ScreenEvents>("IScreenEvents");