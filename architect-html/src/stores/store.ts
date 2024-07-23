import { configureStore, ThunkAction, Action } from '@reduxjs/toolkit';
import editorsReducer from 'src/components/gridEditor/GrirEditorSlice.ts';
import { IArchitectApi } from 'src/API/IArchitectApi';
import { ArchitectApi } from "src/API/ArchitectApi.ts";

const architectApi = new ArchitectApi();

export const store = configureStore({
  reducer: {
    editorStates: editorsReducer,
  },
  middleware: getDefaultMiddleware =>
    getDefaultMiddleware({
      thunk: {
        extraArgument: { architectApi }
      }
    })
})


export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export type AppThunk<ReturnType = void> = ThunkAction<
  ReturnType,
  RootState,
  { architectApi: IArchitectApi },
  Action<string>
>;