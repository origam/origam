import { getApplication } from "model/selectors/getApplication";
import { MobileState } from "model/entities/MobileState/MobileState";

export function getMobileState(ctx: any): MobileState {
  const mobileState = getApplication(ctx).mobileState;
  if (!mobileState) {
    throw new Error("No mobileState in Application.");
  }
  return mobileState;
}