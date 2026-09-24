/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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
import S from '@components/modelCheck/ModelCheckButton.module.scss';
import { formatRunTime } from '@components/modelCheck/ModelCheckState';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { VscCheck, VscChecklist, VscLoading } from 'react-icons/vsc';

const ModelCheckButtonHOC = observer(() => {
  const rootStore = useContext(RootStoreContext);
  const run = runInFlowWithHandler(rootStore.errorDialogController);
  const modelCheckState = rootStore.modelCheckState;

  const handleOnClick = () => {
    run({ generator: modelCheckState.run.bind(modelCheckState) });
  };

  if (!rootStore.packagesState.activePackageId) {
    return null;
  }

  const isRunning = modelCheckState.isRunning;
  const lastResult = modelCheckState.lastResult;
  const problemCount = modelCheckState.problemCount;
  const label = isRunning
    ? T('Validating model...', 'modelcheck_button_running')
    : T('Validate model', 'modelcheck_button_label');

  return (
    <Button
      type="secondary"
      title={
        <>
          {label}
          {!isRunning && lastResult?.lastRunAt && (
            <span
              className={problemCount > 0 ? S.badgeError : S.badgeOk}
              title={T(
                'Last check {0}',
                'modelcheck_results_checked_at',
                formatRunTime(lastResult.lastRunAt),
              )}
              data-test-id="topbar-check-model-badge"
            >
              {problemCount > 0 ? problemCount : <VscCheck />}
            </span>
          )}
        </>
      }
      prefix={isRunning ? <VscLoading className={S.spinner} /> : <VscChecklist />}
      isDisabled={isRunning}
      onClick={handleOnClick}
      dataTestId="topbar-check-model"
    />
  );
});

export default ModelCheckButtonHOC;
