import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { RootStoreContext, T } from '@/main.tsx';
import S from '@components/search/SearchInput.module.scss';

const SearchInput = observer(() => {
  const rootStore = useContext(RootStoreContext);

  return (
    <div className={S.inputContainer}>
      <input className={S.input} placeholder={T('Search', 'search_placeholder')} />
    </div>
  );
});

export default SearchInput;
