import {IProperty} from "../../entities/types/IProperty";
import {getDataView} from "../../selectors/DataView/getDataView";
import {getSessionId} from "../../selectors/getSessionId";
import {getApi} from "../../selectors/getApi";
import {runInFlowWithHandler} from "../../../utils/runInFlowWithHandler";

export function onTextFieldAutoUpdate(property: IProperty, value: string) {

    const dataView = getDataView(property);
    if (!dataView.selectedRowId) {
        return;
    }
    const sessionId = getSessionId(dataView);
    let api = getApi(dataView);

    const valueObj = {} as any;
    valueObj[property.id] = value;

    const updateData = [
        {
            RowId: dataView.selectedRowId,
            Values: valueObj,
        }
    ];
    runInFlowWithHandler({
        ctx: dataView,
        action: async () => {
            await api.updateObject({
                SessionFormIdentifier: sessionId,
                Entity: dataView.entity,
                UpdateData: updateData,
            });
        }
    });
}