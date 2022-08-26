import {IProperty} from "../../entities/types/IProperty";
import {getDataView} from "../../selectors/DataView/getDataView";
import {getSessionId} from "../../selectors/getSessionId";
import {getApi} from "../../selectors/getApi";
import {runInFlowWithHandler} from "../../../utils/runInFlowWithHandler";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";

export async function onTextFieldAutoUpdate(property: IProperty, value: string) {
    const dataView = getDataView(property);
    if (!dataView.selectedRowId) {
        return;
    }
    const sessionId = getSessionId(dataView);
    const valueObj = {} as any;
    valueObj[property.id] = value;

    const updateData = [
        {
            RowId: dataView.selectedRowId,
            Values: valueObj,
        }
    ];
    const formScreenLifecycle = getFormScreenLifecycle(property);
    await formScreenLifecycle.updateRequestAggregator.enqueue({
        SessionFormIdentifier: sessionId,
        Entity: dataView.entity,
        UpdateData: updateData,
    });
}