import { About } from "model/entities/AboutInfo";
import { getWorkbench } from "model/selectors/getWorkbench";

export function getAbout(ctx: any): About {
  return getWorkbench(ctx).about;
}