export const sIdle = "sIdle";
export const onChangeMasterRow = "onChangeMasterRow";
export const onLoadGetData = "onLoadGetData";

export const sChangeMasterRow = "sChangeMasterRow";
export const onChangeMasterRowDone = "onChangeMasterRowDone";

export const sLoadChildren = "sLoadChildren";
export const onLoadChildrenDone = "onLoadChildrenDone";

export const sLoadGetData = "sLoadGetData";
export const onLoadGetDataDone = "onLoadGetDataDone";

export const DataViewLifecycleDef = {
  initial: sIdle,
  states: {
    [sIdle]: {
      on: {
        [onChangeMasterRow]: sChangeMasterRow,
        [onLoadGetData]: sLoadGetData
      }
    },
    [sChangeMasterRow]: {
      invoke: { src: "changeMasterRow" },
      on: {
        [onChangeMasterRowDone]: {
          target: sIdle,
          actions: "navigateChildren"
        }
      }
    },
    [sLoadGetData]: {
      invoke: {src: 'loadGetData'},
      on: {
        [onLoadGetDataDone]: sIdle
      }
    }
  }
};
