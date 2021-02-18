import {getProperty} from "model/selectors/DataView/getProperty";
import { getApi } from "model/selectors/getApi";

export default {
  isLinkToForm(ctx: any) {
    return getProperty(ctx).isLink;
  },

  async getLinkMenuId(ctx: any, value: any) {
    const property = getProperty(ctx);
    if(property.linkDependsOnValue){
      const api = getApi(ctx);
      return await api.getMenuId({
        LookupId: property.lookupId!,
        ReferenceId: value
      });
    }
    return property.linkToMenuId;
  }
};
