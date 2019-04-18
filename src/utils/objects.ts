import { ML } from "./types";
import _ from "lodash";

export function unpack<T>(obj: ML<T>): T {
  return _.isFunction(obj) ? obj() : obj;
}
