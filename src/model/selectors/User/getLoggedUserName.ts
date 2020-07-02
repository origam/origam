import {getUserNameFromToken} from "./getUserNameFromToken";
import {getApi} from "../getApi";

export function getLoggedUserName(ctx: any): string | undefined {
  return getUserNameFromToken(getApi(ctx).accessToken);
}
