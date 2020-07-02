import {getWorkQueues} from "model/selectors/WorkQueues/getWorkQueues"

export function stopWorkQueues(ctx: any) {
  return function*stopWorkQueues() {
    yield * (getWorkQueues(ctx)!.stopTimer());
  }
}