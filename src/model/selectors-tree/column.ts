import { getProperty } from "model/selectors/DataView/getProperty";

export default {
  isLinkToForm(ctx: any) {
    return getProperty(ctx).isLink;
  },

  getLinkMenuId(ctx: any) {
    return getProperty(ctx).linkToMenuId;
  }
};
