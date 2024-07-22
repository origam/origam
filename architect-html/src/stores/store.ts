import { configureStore } from '@reduxjs/toolkit';
import gridEditorSlice from "src/components/gridEditor/GridEditorSlice.ts";

export default configureStore({
  reducer: {
    editor: gridEditorSlice,
  },
});
