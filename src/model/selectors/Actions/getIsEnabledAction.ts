import {getAction} from "./getAction";

export function getIsEnabledAction(ctx: any) {
  console.log(ctx, getAction(ctx).isEnabled);
  if(ctx.id === "79f35d80-72d9-4366-9d8c-4fa5cf12efa4") {
    //debugger
  }
  return getAction(ctx).isEnabled;
}
