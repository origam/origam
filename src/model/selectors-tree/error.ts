import {IErrorDialogController} from "model/entities/types/IErrorDialog";
import {getApplication} from "model/selectors/getApplication";

export default {
  getDialogController(ctx: any): IErrorDialogController {
    return getApplication(ctx).errorDialogController;
  }
};
