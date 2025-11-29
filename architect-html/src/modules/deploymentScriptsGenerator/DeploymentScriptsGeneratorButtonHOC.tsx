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

import { runInFlowWithHandler } from '@/errorHandling/runInFlowWithHandler';
import { RootStoreContext, T } from '@/main';
import Button from '@components/Button/Button';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { VscSymbolMisc } from 'react-icons/vsc';

const DeploymentScriptsGeneratorButtonHOC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const packagesState = rootStore.packagesState;

  const handleOnClick = () => {
    run({ generator: rootStore.editorTabViewState.openDeploymentScriptsGeneratorEditor() });
  };

  if (!packagesState.activePackageId) {
    return null;
  }

  return (
    <Button
      type="secondary"
      title={T('Deployment Scripts Generator', 'deployment_scripts_generator_button_open_label')}
      prefix={<VscSymbolMisc />}
      onClick={handleOnClick}
    />
  );
});

export default DeploymentScriptsGeneratorButtonHOC;
