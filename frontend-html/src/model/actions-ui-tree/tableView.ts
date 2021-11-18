import {flow} from "mobx";

export default {
  onApplyFilterSetting(ctx: any) {
    return flow(function* onApplyFilterSetting(setting: any) {});
  },

  onColumnHeaderClick(ctx: any) {
    return flow(function* onColumnHeaderClick(event: any) {});
  },

  onColumnOrderChangeFinished(ctx: any) {
    return flow(function* onColumnOrderChangeFinished(event: any) {});
  },

  onColumnWidthChangeFinished(ctx: any) {
    return flow(function* onColumnWidthChangeFinished() {});
  }
};
