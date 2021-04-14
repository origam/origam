import {flow} from "mobx";
import {handleError} from "../../actions/handleError";
import {getApi} from "../../selectors/getApi";

export function onResetColumnConfigClick(ctx: any) {
    return flow(function* onMainMenuItemClick(args: {
        item: any;
    }) {
        try {
            if(!args?.item?.attributes?.id){
                return;
            }
            const api = getApi(ctx);
            yield api.resetObjectConfiguration({instanceId : args.item.attributes.id});
        } catch (e) {
            yield* handleError(ctx)(e);
            throw e;
        }
    });
}