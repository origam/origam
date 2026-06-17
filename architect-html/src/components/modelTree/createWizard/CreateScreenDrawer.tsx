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
import React, { useContext, useEffect, useMemo, useRef, useState } from 'react';
import { RootStoreContext } from '@/main';
import { ICreateWizardResult, IScreenWizardData } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';

interface CreateScreenDrawerProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateWizardResult) => void;
}

interface ScreenModel {
  name: string;
  caption: string;
  selectedFieldIds: Set<string>;
}

const STEPS = [
  { label: 'Basics', hint: 'Name your screen' },
  { label: 'Fields', hint: 'Caption & visible columns' },
  { label: 'Review', hint: 'Confirm and create' },
];

export const CreateScreenDrawer: React.FC<CreateScreenDrawerProps> = observer(
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
    const [model, setModel] = useState<ScreenModel>({
      name: '',
      caption: '',
      selectedFieldIds: new Set<string>(),
    });

    const drawerRef = useRef<HTMLDivElement | null>(null);
    const formContentRef = useRef<HTMLDivElement | null>(null);
    const nameManuallyEditedRef = useRef(false);

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
            if (!nameManuallyEditedRef.current) {
              setModel(prev => ({ ...prev, name: data.entityName }));
            }
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

    useEffect(() => {
      const trapFocus = (event: KeyboardEvent) => {
        if (event.key !== 'Tab') return;
        const drawer = drawerRef.current;
        if (!drawer) return;
        const focusableNodes = Array.from(
          drawer.querySelectorAll<HTMLElement>(
            'input:not([disabled]), button:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])',
          ),
        ).filter(node => node.offsetParent !== null);
        if (focusableNodes.length === 0) return;
        const firstNode = focusableNodes[0];
        const lastNode = focusableNodes[focusableNodes.length - 1];
        const activeElement = document.activeElement as HTMLElement | null;
        if (event.shiftKey) {
          if (activeElement === firstNode || !drawer.contains(activeElement)) {
            event.preventDefault();
            lastNode.focus();
          }
        } else {
          if (activeElement === lastNode || !drawer.contains(activeElement)) {
            event.preventDefault();
            firstNode.focus();
          }
        }
      };
      document.addEventListener('keydown', trapFocus);
      return () => document.removeEventListener('keydown', trapFocus);
    }, []);

    useEffect(() => {
      if (loading) return;
      const first = formContentRef.current?.querySelector<HTMLElement>(
        'input:not([disabled]), textarea:not([disabled]), select:not([disabled]), button:not([disabled])',
      );
      first?.focus();
    }, [step, loading]);

    const toggleField = (id: string) => {
      setModel(prev => {
        const nextSelectedFieldIds = new Set(prev.selectedFieldIds);
        if (nextSelectedFieldIds.has(id)) nextSelectedFieldIds.delete(id);
        else nextSelectedFieldIds.add(id);
        return { ...prev, selectedFieldIds: nextSelectedFieldIds };
      });
    };

    const selectAll = () => {
      if (!entityData) return;
      setModel(prev => ({
        ...prev,
        selectedFieldIds: new Set(
          entityData.columns.filter(column => !column.isPrimaryKey).map(column => column.id),
        ),
      }));
    };

    const clearAll = () => {
      setModel(prev => ({ ...prev, selectedFieldIds: new Set() }));
    };

    const trimmedName = model.name.trim();
    const nameExists =
      trimmedName.length > 0 &&
      (entityData?.existingDataStructureNames ?? []).some(
        existingName => existingName.toLowerCase() === trimmedName.toLowerCase(),
      );

    const canAdvance =
      (step === 0 && trimmedName.length > 0) ||
      (step === 1 && model.selectedFieldIds.size > 0) ||
      step === 2;

    const next = () => {
      if (step === 0 && nameExists) {
        window.alert(`A DataStructure named "${trimmedName}" already exists.`);
        return;
      }
      setStep(current => Math.min(current + 1, STEPS.length - 1));
    };
    const back = () => setStep(current => Math.max(current - 1, 0));

    const submit = () => {
      if (submitting) return;
      setSubmitting(true);
      run({
        generator: function* () {
          try {
            const result = (yield rootStore.architectApi.createScreen({
              entityId,
              name: model.name.trim(),
              caption: model.caption.trim(),
              selectedFieldIds: Array.from(model.selectedFieldIds),
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
        return (
          <>
            <h2 className={S.formTitle}>Let&apos;s name your screen</h2>
            <p className={S.formSubtitle}>
              A Screen ties a DataStructure to a Screen Section (Panel) and a Form. The name is used
              for all three artifacts.
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                Name <span className={S.required}>*</span>
              </label>
              <input
                className={S.input}
                autoFocus
                placeholder={entityData ? `e.g. ${entityData.entityName}` : ''}
                value={model.name}
                onChange={event => {
                  nameManuallyEditedRef.current = true;
                  setModel(prev => ({ ...prev, name: event.target.value }));
                }}
              />
              {nameExists && (
                <div style={{ fontSize: 12, color: 'var(--error1)', marginTop: 2 }}>
                  A DataStructure named &quot;{trimmedName}&quot; already exists.
                </div>
              )}
            </div>

            <div className={S.field}>
              <label className={S.fieldLabel}>Created from entity</label>
              <input className={S.input} value={entityData?.entityName ?? ''} disabled />
            </div>

            <div className={S.preview}>
              <div className={S.previewTitle}>What will be created</div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Data Structure</span>
                <span>{model.name || '<name>'}</span>
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Screen Section</span>
                <span>{model.name || '<name>'}</span>
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Screen</span>
                <span>{model.name || '<name>'}</span>
              </div>
            </div>
          </>
        );
      }

      if (step === 1) {
        if (!entityData) return null;
        const columns = (entityData.columns ?? []).filter(column => !column.isPrimaryKey);
        return (
          <>
            <h2 className={S.formTitle}>Which columns should appear?</h2>
            <p className={S.formSubtitle}>
              Pick the entity fields that will be placed on the Screen Section. You can add or
              remove fields later in the section editor.
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                Caption
                <span className={S.fieldHint}>— shown as the Screen Section title</span>
              </label>
              <input
                className={S.input}
                placeholder={`e.g. ${entityData.entityName}`}
                value={model.caption}
                onChange={event => setModel(prev => ({ ...prev, caption: event.target.value }))}
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
                {model.selectedFieldIds.size} of {columns.length} selected
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
                const checked = model.selectedFieldIds.has(column.id);
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
      const selected = (entityData.columns ?? []).filter(column =>
        model.selectedFieldIds.has(column.id),
      );
      return (
        <>
          <h2 className={S.formTitle}>Ready to create</h2>
          <p className={S.formSubtitle}>
            Review what will be added to the model. You can edit everything afterward.
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>S</div>
              <div>
                <div className={S.reviewCardTitle}>{model.name || 'Untitled'}</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  Screen + Section + DataStructure
                </div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Entity</div>
              <div>{entityData.entityName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Caption</div>
              <div>{model.caption.trim() || '(none)'}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Fields</div>
              <div>{selected.length} selected</div>
            </div>
          </div>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>F</div>
              <div>
                <div className={S.reviewCardTitle}>Selected fields</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  Placed on the Screen Section in order
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
      <div className={S.drawer} role="dialog" aria-modal="true" ref={drawerRef}>
        <div className={S.header}>
          <div className={S.headerIcon}>S</div>
          <div className={S.headerText}>
            <div className={S.headerTitle}>Create Screen</div>
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
            <div className={S.formContent} ref={formContentRef}>
              {renderStep()}
            </div>

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
                    {submitting ? 'Creating…' : 'Create Screen'}
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
