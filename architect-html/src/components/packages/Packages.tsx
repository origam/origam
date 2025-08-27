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

import { RootStoreContext } from '@/main.tsx';
import { PackageItem } from '@components/packages/PackageItem.tsx';
import S from '@components/packages/Packages.module.scss';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler.ts';
import { observer } from 'mobx-react-lite';
import React, { useContext, useEffect } from 'react';

export const Packages: React.FC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const packagesState = rootStore.packagesState;

  useEffect(() => {
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: packagesState.loadPackages.bind(packagesState),
    });
  }, [packagesState, rootStore.errorDialogController]);

  return (
    <div className={S.root}>
      {packagesState.packages.map(x => (
        <PackageItem key={x.id} isSelected={packagesState.activePackageId === x.id} package={x} />
      ))}
    </div>
  );
});
