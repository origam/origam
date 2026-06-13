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

import S from '@components/modelTree/createWizard/CreateLookupDrawer.module.scss';
import { observer } from 'mobx-react-lite';
import React, { useContext, useEffect, useMemo, useState } from 'react';
import { RootStoreContext } from '@/main';
import { ICreateActionResult, IScreenWizardData } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';

interface CreateWorkQueueDrawerProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateActionResult) => void;
}

const STEPS = [
  { label: 'Fields', hint: 'Caption & tracked columns' },
  { label: 'Review', hint: 'Confirm and create' },
];

export const CreateWorkQueueDrawer: React.FC<CreateWorkQueueDrawerProps> = observer(
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

    const next = () => setStep(current => Math.min(current + 1, STEPS.length - 1));
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
            })) as ICreateActionResult;
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
            <h2 className={S.formTitle}>Which columns to track?</h2>
            <p className={S.formSubtitle}>
              The WorkQueue Class will be created with name{' '}
              <strong>{entityData?.entityName ?? '…'}</strong>. Pick the columns that should be
              exposed on the work queue records.
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>Caption</label>
              <input
                className={S.input}
                autoFocus
                placeholder={entityData ? `e.g. ${entityData.entityName}` : ''}
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
                Select all
              </button>
              <button
                type="button"
                className={S.btn}
                style={{ height: 28, padding: '0 12px', fontSize: 12 }}
                onClick={clearAll}
              >
                Clear
              </button>
              <div
                style={{
                  alignSelf: 'center',
                  color: 'var(--background6)',
                  marginLeft: 'auto',
                }}
              >
                {selectedFieldIds.size} of {columns.length} selected
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
          <h2 className={S.formTitle}>Ready to create</h2>
          <p className={S.formSubtitle}>
            Review what will be added. You can adjust the WorkQueue Class afterward in its editor.
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>W</div>
              <div>
                <div className={S.reviewCardTitle}>{entityData.entityName}</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>WorkQueue Class</div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Entity</div>
              <div>{entityData.entityName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Tracked fields</div>
              <div>{selected.length}</div>
            </div>
          </div>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>F</div>
              <div>
                <div className={S.reviewCardTitle}>Selected fields</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  Tracked on the queue
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
            <div className={S.headerTitle}>Create WorkQueue Class</div>
            <div className={S.headerSubtitle}>from {parentNodeName}</div>
          </div>
          <button className={S.closeBtn} onClick={onCancel} aria-label="Close">
            ✕
          </button>
        </div>

        <div className={S.body}>
          <div className={S.stepperCol}>
            {STEPS.map((stepInfo, index) => (
              <div
                key={stepInfo.label}
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
                Step {step + 1} of {STEPS.length}
              </div>
              <div className={S.footerBtns}>
                <button className={S.btn} onClick={onCancel}>
                  Cancel
                </button>
                {step > 0 && (
                  <button className={S.btn} onClick={back} disabled={submitting}>
                    Back
                  </button>
                )}
                {step < STEPS.length - 1 ? (
                  <button
                    className={`${S.btn} ${S.btnPrimary}`}
                    onClick={next}
                    disabled={!canAdvance || loading}
                  >
                    Next →
                  </button>
                ) : (
                  <button
                    className={`${S.btn} ${S.btnPrimary}`}
                    onClick={submit}
                    disabled={submitting || loading}
                  >
                    {submitting ? 'Creating…' : 'Create WorkQueue'}
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
