import { IFormScreen } from "model/entities/types/IFormScreen";
import {
  onWorkflowNextClick
} from "model/actions-ui/ScreenHeader/onWorkflowNextClick";

export function* processAutoWorkflowNext(formScreen: IFormScreen | undefined) {
  if (formScreen?.autoWorkflowNext) {
    let canContinue = true;
    for (let i = 0; i < 100; i++) {
      if (!canContinue) {
        return;
      }
      canContinue = yield onWorkflowNextClick(formScreen!)(undefined);
    }
  }
}