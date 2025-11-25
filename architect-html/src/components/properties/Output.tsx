import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { RootStoreContext } from '@/main.tsx';

const Output = observer(() => {
  const rootStore = useContext(RootStoreContext);

  return <div>{rootStore.output}</div>;
});

export default Output;
