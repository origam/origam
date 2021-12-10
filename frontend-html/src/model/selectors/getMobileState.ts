import { IMobileState } from "model/entities/types/IMobileState";
import { getApplication } from "model/selectors/getApplication";

export function getMobileState(ctx: any): IMobileState {
  const mobileState = getApplication(ctx).mobileState;
  if (!mobileState) {
    throw new Error("No mobileState in Application.");
  }
  return mobileState;
}