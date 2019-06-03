export default 0;

export const NS = "FormView";

export const ON_NO_FIELD_CLICK = `${NS}/ON_NO_FIELD_CLICK`;
export const ON_OUTSIDE_FORM_CLICK = `${NS}/ON_OUTSIDE_FORM_CLICK`;
export const ON_FIELD_CLICK = `${NS}/ON_FIELD_CLICK`;
export const ON_PREV_ROW_CLICK = `${NS}/ON_PREV_ROW_CLICK`;
export const ON_NEXT_ROW_CLICK = `${NS}/ON_NEXT_ROW_CLICK`;

export const SELECT_FIRST_CELL = `${NS}/SELECT_FIRST_CELL`;
export const SELECT_NEXT_ROW = `${NS}/SELECT_NEXT_ROW`;
export const SELECT_PREV_ROW = `${NS}/SELECT_PREV_ROW`;
export const SELECT_NEXT_COLUMN = `${NS}/SELECT_NEXT_COLUMN`;
export const SELECT_PREV_COLUMN = `${NS}/SELECT_PREV_COLUMN`;
export const SELECT_FIELD_BY_PROP_ID = `${NS}/SELECT_CELL_BY_PROP_ID`;

export const onOutsideFormClick = () => ({ NS, type: ON_OUTSIDE_FORM_CLICK });

//TODO: Correct typing.
export const onFieldClick = (propId: string) => ({
  NS,
  type: ON_FIELD_CLICK,
  propId
});

export const onNoFieldClick = () => ({ NS, type: ON_NO_FIELD_CLICK });
export const onPrevRowClick = () => ({ NS, type: ON_PREV_ROW_CLICK });
export const onNextRowClick = () => ({ NS, type: ON_NEXT_ROW_CLICK });

export const selectFirstCell = () => ({ NS, type: SELECT_FIRST_CELL });
export const selectNextRow = () => ({ NS, type: SELECT_NEXT_ROW });
export const selectPrevRow = () => ({ NS, type: SELECT_PREV_ROW });
export const selectNextColumn = () => ({ NS, type: SELECT_NEXT_COLUMN });
export const selectPrevColumn = () => ({ NS, type: SELECT_PREV_COLUMN });

export const selectFieldByPropId = (args: { propId: string }) => ({
  NS,
  type: SELECT_FIELD_BY_PROP_ID,
  ...args
});
