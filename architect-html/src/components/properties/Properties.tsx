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

import { RootStoreContext } from '@/main';
import S from '@components/properties/Properties.module.scss';
import { PropertyEditor } from '@editors/propertyEditor/PropertyEditor';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';

export const Properties: React.FC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const propertiesState = rootStore.propertiesState;
  return (
    <div>
      <div className={S.editedItemLabel}>{propertiesState.editedItemName}</div>
      <PropertyEditor propertyManager={propertiesState} properties={propertiesState.properties} />
    </div>
  );
});
