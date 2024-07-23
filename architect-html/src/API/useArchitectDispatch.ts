import { useContext } from 'react';
import { useDispatch } from 'react-redux';
import { AppDispatch } from 'src/stores/store.ts';
import { ArchitectApiContext } from 'src/API/ArchitectApiContext';

export const useArchitectDispatch = () => {
  const dispatch = useDispatch<AppDispatch>();
  const architectApi = useContext(ArchitectApiContext);

  if (!architectApi) {
    throw new Error('ArchitectApi context is not available');
  }

  return (action: any) => {
    if (typeof action === 'function') {
      return action(dispatch, architectApi);
    }
    return dispatch(action);
  };
};