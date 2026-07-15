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

import S from '@components/modelTree/createWizard/CreateWizard.module.scss';
import { observer } from 'mobx-react-lite';
import React, { useContext, useEffect, useMemo, useState } from 'react';
import { RootStoreContext, T } from '@/main';
import { ICreateWizardResult, IScreenWizardData } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';

interface CreateWorkQueueWizardProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateWizardResult) => void;
}

export const CreateWorkQueueWizard: React.FC<CreateWorkQueueWizardProps> = observer(
  ({ entityId, parentNodeName, onCancel, onCreate }) => {
    const rootStore = useContext(RootStoreContext);
    const run = useMemo(
      () => runInFlowWithHandler(rootStore.errorDialogController),
      [rootStore.errorDialogController],
    );

    const [step, setStep] = useState(0);
    const [entityData, setEntityData] = useState<IScreenWizardData | null>(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [selectedFieldIds, setSelectedFieldIds] = useState<Set<string>>(new Set());
    const [caption, setCaption] = useState('');

    const steps = [
      {
        id: 'fields',
        label: T('Fields', 'create_workqueue_step_fields_label'),
        hint: T('Caption & tracked columns', 'create_workqueue_step_fields_hint'),
      },
      {
        id: 'review',
        label: T('Review', 'wizard_step_review_label'),
        hint: T('Confirm and create', 'wizard_step_review_hint'),
      },
    ];

    useEffect(() => {
      let cancelled = false;
      run({
        generator: function* () {
          try {
            const data = (yield rootStore.architectApi.getScreenWizardData(
              entityId,
            )) as IScreenWizardData;
            if (cancelled) return;
            setEntityData(data);
          } finally {
            if (!cancelled) setLoading(false);
          }
        },
      });
      return () => {
        cancelled = true;
      };
    }, [entityId, run, rootStore.architectApi]);

    useEffect(() => {
      const onKeyDown = (event: KeyboardEvent) => {
        if (event.key === 'Escape') {
          event.stopPropagation();
          onCancel();
        }
      };
      document.addEventListener('keydown', onKeyDown);
      return () => document.removeEventListener('keydown', onKeyDown);
    }, [onCancel]);

    const toggleField = (id: string) => {
      setSelectedFieldIds(prev => {
        const next = new Set(prev);
        if (next.has(id)) next.delete(id);
        else next.add(id);
        return next;
      });
    };

    const selectAll = () => {
      if (!entityData) return;
      setSelectedFieldIds(new Set(entityData.columns.map(column => column.id)));
    };

    const clearAll = () => setSelectedFieldIds(new Set());

    const canAdvance = (step === 0 && selectedFieldIds.size > 0) || step === 1;

    const next = () => setStep(current => Math.min(current + 1, steps.length - 1));
    const back = () => setStep(current => Math.max(current - 1, 0));

    const submit = () => {
      if (submitting) return;
      setSubmitting(true);
      run({
        generator: function* () {
          try {
            const result = (yield rootStore.architectApi.createWorkQueueClass({
              entityId,
              selectedFieldIds: Array.from(selectedFieldIds),
            })) as ICreateWizardResult;
            onCreate(result);
          } finally {
            setSubmitting(false);
          }
        },
      });
    };

    const renderStep = () => {
      if (step === 0) {
        const columns = entityData?.columns ?? [];
        return (
          <>
            <h2 className={S.formTitle}>
              {T('Which columns to track?', 'create_workqueue_fields_title')}
            </h2>
            <p className={S.formSubtitle}>
              {T(
                'The WorkQueue Class will be created with name',
                'create_workqueue_fields_subtitle_pre',
              )}{' '}
              <strong>{entityData?.entityName ?? '…'}</strong>
              {T(
                '. Pick the columns that should be exposed on the work queue records.',
                'create_workqueue_fields_subtitle_post',
              )}
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                {T('Caption', 'create_workqueue_caption_label')}
              </label>
              <input
                className={S.input}
                autoFocus
                placeholder={
                  entityData
                    ? T('e.g. {0}', 'wizard_placeholder_example', entityData.entityName)
                    : ''
                }
                value={caption}
                onChange={event => setCaption(event.target.value)}
              />
            </div>

            <div
              style={{
                display: 'flex',
                gap: 8,
                marginBottom: 12,
                marginTop: 8,
                fontSize: 12,
              }}
            >
              <button
                type="button"
                className={S.btn}
                style={{ height: 28, padding: '0 12px', fontSize: 12 }}
                onClick={selectAll}
              >
                {T('Select all', 'wizard_select_all')}
              </button>
              <button
                type="button"
                className={S.btn}
                style={{ height: 28, padding: '0 12px', fontSize: 12 }}
                onClick={clearAll}
              >
                {T('Clear', 'wizard_clear')}
              </button>
              <div
                style={{
                  alignSelf: 'center',
                  color: 'var(--background6)',
                  marginLeft: 'auto',
                }}
              >
                {T(
                  '{0} of {1} selected',
                  'wizard_fields_selected_count',
                  selectedFieldIds.size,
                  columns.length,
                )}
              </div>
            </div>

            <div
              style={{
                border: '1px solid var(--background3)',
                borderRadius: 6,
                flex: 1,
                minHeight: 0,
                overflowY: 'auto',
                background: 'var(--background1)',
              }}
            >
              {columns.map(column => {
                const checked = selectedFieldIds.has(column.id);
                return (
                  <label
                    key={column.id}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 10,
                      padding: '8px 12px',
                      borderBottom: '1px solid var(--background3)',
                      cursor: 'pointer',
                      fontSize: 13,
                      color: 'var(--background8)',
                    }}
                  >
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={() => toggleField(column.id)}
                      style={{ accentColor: 'var(--brand)' }}
                    />
                    {column.name}
                  </label>
                );
              })}
            </div>
          </>
        );
      }

      if (!entityData) return null;
      const selected = (entityData.columns ?? []).filter(column => selectedFieldIds.has(column.id));
      return (
        <>
          <h2 className={S.formTitle}>{T('Ready to create', 'wizard_ready_title')}</h2>
          <p className={S.formSubtitle}>
            {T(
              'Review what will be added. You can adjust the WorkQueue Class afterward in its editor.',
              'create_workqueue_review_subtitle',
            )}
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>W</div>
              <div>
                <div className={S.reviewCardTitle}>{entityData.entityName}</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  {T('WorkQueue Class', 'create_workqueue_review_type')}
                </div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>{T('Entity', 'create_workqueue_review_entity')}</div>
              <div>{entityData.entityName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Tracked fields', 'create_workqueue_review_tracked_fields')}
              </div>
              <div>{selected.length}</div>
            </div>
          </div>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>F</div>
              <div>
                <div className={S.reviewCardTitle}>
                  {T('Selected fields', 'create_workqueue_review_selected_fields_title')}
                </div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  {T('Tracked on the queue', 'create_workqueue_review_selected_fields_hint')}
                </div>
              </div>
            </div>
            <div style={{ fontSize: 13, color: 'var(--background8)', lineHeight: 1.7 }}>
              {selected.map(column => column.name).join(', ') || '—'}
            </div>
          </div>
        </>
      );
    };

    return (
      <div className={S.drawer} role="dialog" aria-modal="true">
        <div className={S.header}>
          <div className={S.headerIcon}>W</div>
          <div className={S.headerText}>
            <div className={S.headerTitle}>
              {T('Create WorkQueue Class', 'create_workqueue_header_title')}
            </div>
            <div className={S.headerSubtitle}>
              {T('from {0}', 'create_workqueue_header_subtitle', parentNodeName)}
            </div>
          </div>
          <button className={S.closeBtn} onClick={onCancel} aria-label={T('Close', 'wizard_close')}>
            ✕
          </button>
        </div>

        <div className={S.body}>
          <div className={S.stepperCol}>
            {steps.map((stepInfo, index) => (
              <div
                key={stepInfo.id}
                className={`${S.stepperItem} ${index === step ? S.active : ''} ${
                  index < step ? S.done : ''
                }`}
                onClick={() => index < step && setStep(index)}
              >
                <div className={S.stepBullet}>{index < step ? '✓' : index + 1}</div>
                <div className={S.stepText}>
                  <div className={S.stepLabel}>{stepInfo.label}</div>
                  <div className={S.stepHint}>{stepInfo.hint}</div>
                </div>
              </div>
            ))}
          </div>

          <div className={S.formCol}>
            <div className={S.formContent}>{renderStep()}</div>

            <div className={S.footer}>
              <div className={S.footerHint}>
                {T('Step {0} of {1}', 'wizard_step_counter', step + 1, steps.length)}
              </div>
              <div className={S.footerBtns}>
                <button className={S.btn} onClick={onCancel}>
                  {T('Cancel', 'wizard_btn_cancel')}
                </button>
                {step > 0 && (
                  <button className={S.btn} onClick={back} disabled={submitting}>
                    {T('Back', 'wizard_btn_back')}
                  </button>
                )}
                {step < steps.length - 1 ? (
                  <button
                    className={`${S.btn} ${S.btnPrimary}`}
                    onClick={next}
                    disabled={!canAdvance || loading}
                  >
                    {T('Next →', 'wizard_btn_next')}
                  </button>
                ) : (
                  <button
                    className={`${S.btn} ${S.btnPrimary}`}
                    onClick={submit}
                    disabled={submitting || loading}
                  >
                    {submitting
                      ? T('Creating…', 'wizard_btn_creating')
                      : T('Create WorkQueue', 'create_workqueue_btn_create')}
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  },
);
