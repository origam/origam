/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { RootStoreContext, T } from '@/main';
import Button from '@components/Button/Button';
import SettingsModal from '@modules/settings/SettingsModal';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { VscSettingsGear } from 'react-icons/vsc';

const SettingsButtonHOC = observer(() => {
  const rootStore = useContext(RootStoreContext);

  const handleOnClick = () => {
    const closeDialog = rootStore.dialogStack.pushDialog(
      'settings_modal',
      <SettingsModal onClose={() => closeDialog()} />,
      { width: 600, height: 400 },
      true,
    );
  };

  return (
    <Button
      type="secondary"
      title={T('Settings', 'settings_button_open_label')}
      prefix={<VscSettingsGear />}
      onClick={handleOnClick}
      isAnimated
    />
  );
});

export default SettingsButtonHOC;
