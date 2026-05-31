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

import ActionPanel from '@/components/ActionPanel/ActionPanel';
import Button from '@/components/Button/Button';
import { runInFlowWithHandler } from '@/errorHandling/runInFlowWithHandler';
import { RootStoreContext, T } from '@/main';
import {
  DeploymentActivityStatus,
  IActivityStatus,
  IDeploymentVersionStatus,
  IPackageStatus,
} from '@api/IArchitectApi';
import S from '@editors/DeploymentStatusModule/DeploymentStatusModule.module.scss';
import DeploymentStatusModuleState from '@editors/DeploymentStatusModule/DeploymentStatusModuleState';
import { observer } from 'mobx-react-lite';
import { useContext } from 'react';
import { VscChevronDown, VscChevronRight, VscRefresh } from 'react-icons/vsc';

const DeploymentStatusModule = observer(
  ({ editorState }: { editorState: DeploymentStatusModuleState }) => {
    const rootStore = useContext(RootStoreContext);
    const run = runInFlowWithHandler(rootStore.errorDialogController);
    const packages = editorState.response.packages;

    const openActivity = (activityId: string) => {
      run({
        generator: rootStore.editorTabViewState.openEditorBySchemaItemId(activityId),
      });
    };

    return (
      <div className={S.root}>
        <div className={S.editorBox}>
          <ActionPanel
            title={T(
              'Deployment Status ({0} packages)',
              'editor_DeploymentStatus_ActionPanelTitle',
              packages.length,
            )}
            buttons={
              <div>
                <Button
                  title={<VscRefresh />}
                  type="secondary"
                  onClick={() => run({ action: () => editorState.reload() })}
                />
              </div>
            }
          />
          <div className={S.scrollArea}>
            {packages.length === 0 && (
              <div className={S.empty}>
                {T('No packages to show.', 'editor_DeploymentStatus_NoPackages')}
              </div>
            )}
            {packages.map(pkg => (
              <PackageBlock
                key={pkg.packageId}
                pkg={pkg}
                isExpanded={editorState.isExpanded(pkg.packageId)}
                onToggle={() => editorState.toggleExpanded(pkg.packageId)}
                onActivityClick={openActivity}
              />
            ))}
          </div>
        </div>
      </div>
    );
  },
);

const PackageBlock = observer(
  ({
    pkg,
    isExpanded,
    onToggle,
    onActivityClick,
  }: {
    pkg: IPackageStatus;
    isExpanded: boolean;
    onToggle: () => void;
    onActivityClick: (activityId: string) => void;
  }) => {
    const currentVersion = pkg.versions.find(v => v.isCurrentVersion) ?? null;
    const totalActivities = pkg.versions.reduce(
      (sum, v) => sum + v.activities.length,
      0,
    );
    const pendingActivities = pkg.versions
      .filter(v => v.status === 'Pending')
      .reduce((sum, v) => sum + v.activities.length, 0);
    const aggregateStatus: DeploymentActivityStatus =
      pendingActivities > 0 ? 'Pending' : 'Done';
    return (
      <div className={S.packageBlock}>
        <div
          className={S.packageHeader}
          onClick={onToggle}
          role="button"
          tabIndex={0}
          onKeyDown={e => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              onToggle();
            }
          }}
        >
          <span className={S.chevron}>
            {isExpanded ? <VscChevronDown /> : <VscChevronRight />}
          </span>
          <span className={S.packageName}>{pkg.packageName}</span>
          <span className={S.versionInfo}>
            {T('Model:', 'editor_DeploymentStatus_Label_Model')}{' '}
            <strong>{pkg.packageModelVersion}</strong>
          </span>
          <span className={S.versionInfo}>
            {T('Deployed:', 'editor_DeploymentStatus_Label_Deployed')}{' '}
            <strong>{pkg.deployedVersion}</strong>
          </span>
          <span className={S.summary}>
            <span className={S.activityCount}>
              {T(
                '{0} activities',
                'editor_DeploymentStatus_ActivityCount',
                totalActivities,
              )}
            </span>
            {pendingActivities > 0 && (
              <span className={S.pendingCount}>
                {T(
                  '({0} pending)',
                  'editor_DeploymentStatus_PendingSuffix',
                  pendingActivities,
                )}
              </span>
            )}
            {!currentVersion && (
              <span className={S.activityCount}>
                {T(
                  'No current version',
                  'editor_DeploymentStatus_NoCurrentVersion',
                )}
              </span>
            )}
            <StatusBadge status={aggregateStatus} />
          </span>
        </div>
        {isExpanded && (
          <table className={S.statusTable}>
            <colgroup>
              <col className={S.colName} />
              <col className={S.colType} />
              <col className={S.colOrder} />
              <col className={S.colStatus} />
            </colgroup>
            <thead>
              <tr>
                <th>
                  {T('Version / Activity', 'editor_DeploymentStatus_Column_Item')}
                </th>
                <th>{T('Type', 'editor_DeploymentStatus_Column_Type')}</th>
                <th>{T('Order', 'editor_DeploymentStatus_Column_Order')}</th>
                <th>{T('Status', 'editor_DeploymentStatus_Column_Status')}</th>
              </tr>
            </thead>
            <tbody>
              {pkg.versions.length === 0 ? (
                <tr>
                  <td colSpan={4} className={S.empty}>
                    {T(
                      'No deployment versions defined.',
                      'editor_DeploymentStatus_NoVersions',
                    )}
                  </td>
                </tr>
              ) : (
                pkg.versions.map(version => (
                  <ActivityRows
                    key={version.id}
                    version={version}
                    onActivityClick={onActivityClick}
                  />
                ))
              )}
            </tbody>
          </table>
        )}
      </div>
    );
  },
);

const ActivityRows = observer(
  ({
    version,
    onActivityClick,
  }: {
    version: IDeploymentVersionStatus;
    onActivityClick: (activityId: string) => void;
  }) => {
    return (
      <>
        <tr className={S.versionRow}>
          <td>
            {version.name}
            {version.isCurrentVersion && (
              <span className={S.currentBadge}>
                {T('Current', 'editor_DeploymentStatus_CurrentBadge')}
              </span>
            )}
          </td>
          <td>{T('Version', 'editor_DeploymentStatus_VersionRowType')}</td>
          <td />
          <td>
            <StatusBadge status={version.status} />
          </td>
        </tr>
        {version.activities.map(activity => (
          <ActivityRow
            key={activity.id}
            activity={activity}
            onClick={() => onActivityClick(activity.id)}
          />
        ))}
      </>
    );
  },
);

const ActivityRow = ({
  activity,
  onClick,
}: {
  activity: IActivityStatus;
  onClick: () => void;
}) => {
  return (
    <tr
      className={`${S.activityRow} ${S.clickable}`}
      onClick={onClick}
      role="button"
      tabIndex={0}
      onKeyDown={e => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          onClick();
        }
      }}
    >
      <td className={S.indented}>{activity.name}</td>
      <td>{activity.activityType}</td>
      <td>{activity.activityOrder}</td>
      <td>
        <StatusBadge status={activity.status} />
      </td>
    </tr>
  );
};

const StatusBadge = ({ status }: { status: DeploymentActivityStatus }) => {
  const cls = status === 'Done' ? S.statusDone : S.statusPending;
  const label =
    status === 'Done'
      ? T('Done', 'editor_DeploymentStatus_Status_Done')
      : T('Pending', 'editor_DeploymentStatus_Status_Pending');
  return <span className={`${S.statusBadge} ${cls}`}>{label}</span>;
};

export default DeploymentStatusModule;
