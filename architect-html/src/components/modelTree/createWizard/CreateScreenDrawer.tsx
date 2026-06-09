/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.
This file is part of ORIGAM (http://www.origam.org).
*/

import S from './CreateLookupDrawer.module.scss';
import { observer } from 'mobx-react-lite';
import React, { useContext, useEffect, useRef, useState } from 'react';
import { RootStoreContext } from '@/main';
import { ICreateScreenResult, IScreenWizardData } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';

interface CreateScreenDrawerProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateScreenResult) => void;
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
    const run = runInFlowWithHandler(rootStore.errorDialogController);

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
              setModel(m => ({ ...m, name: data.entityName }));
            }
          } finally {
            if (!cancelled) setLoading(false);
          }
        },
      });
      return () => {
        cancelled = true;
      };
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [entityId]);

    useEffect(() => {
      const onKeyDown = (e: KeyboardEvent) => {
        if (e.key === 'Escape') {
          e.stopPropagation();
          onCancel();
        }
      };
      document.addEventListener('keydown', onKeyDown);
      return () => document.removeEventListener('keydown', onKeyDown);
    }, [onCancel]);

    // Focus trap: keep Tab/Shift+Tab cycling within the drawer instead of
    // walking out into the toolbar / model tree behind it.
    useEffect(() => {
      const trap = (e: KeyboardEvent) => {
        if (e.key !== 'Tab') return;
        const drawer = drawerRef.current;
        if (!drawer) return;
        const focusables = Array.from(
          drawer.querySelectorAll<HTMLElement>(
            'input:not([disabled]), button:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])',
          ),
        ).filter(el => el.offsetParent !== null);
        if (focusables.length === 0) return;
        const first = focusables[0];
        const last = focusables[focusables.length - 1];
        const active = document.activeElement as HTMLElement | null;
        if (e.shiftKey) {
          if (active === first || !drawer.contains(active)) {
            e.preventDefault();
            last.focus();
          }
        } else {
          if (active === last || !drawer.contains(active)) {
            e.preventDefault();
            first.focus();
          }
        }
      };
      document.addEventListener('keydown', trap);
      return () => document.removeEventListener('keydown', trap);
    }, []);

    // When a new step renders, drop focus onto the first focusable element in
    // that step so Tab starts from inside the form, not from the Next button.
    useEffect(() => {
      if (loading) return;
      const first = formContentRef.current?.querySelector<HTMLElement>(
        'input:not([disabled]), textarea:not([disabled]), select:not([disabled]), button:not([disabled])',
      );
      first?.focus();
    }, [step, loading]);

    const toggleField = (id: string) => {
      setModel(m => {
        const next = new Set(m.selectedFieldIds);
        if (next.has(id)) next.delete(id);
        else next.add(id);
        return { ...m, selectedFieldIds: next };
      });
    };

    const selectAll = () => {
      if (!entityData) return;
      // Primary-key columns are filtered out of the rendered list entirely,
      // so we only ever select user-facing columns.
      setModel(m => ({
        ...m,
        selectedFieldIds: new Set(
          entityData.columns.filter(c => !c.isPrimaryKey).map(c => c.id),
        ),
      }));
    };

    const clearAll = () => {
      setModel(m => ({ ...m, selectedFieldIds: new Set() }));
    };

    const trimmedName = model.name.trim();
    const nameExists =
      trimmedName.length > 0 &&
      (entityData?.existingDataStructureNames ?? []).some(
        n => n.toLowerCase() === trimmedName.toLowerCase(),
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
      setStep(s => Math.min(s + 1, STEPS.length - 1));
    };
    const back = () => setStep(s => Math.max(s - 1, 0));

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
            })) as ICreateScreenResult;
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
            <h2 className={S.formTitle}>Let's name your screen</h2>
            <p className={S.formSubtitle}>
              A Screen ties a DataStructure to a Screen Section (Panel) and a Form. The name is
              used for all three artifacts.
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
                onChange={e => {
                  nameManuallyEditedRef.current = true;
                  setModel(m => ({ ...m, name: e.target.value }));
                }}
              />
              {nameExists && (
                <div style={{ fontSize: 12, color: 'var(--error1)', marginTop: 2 }}>
                  A DataStructure named "{trimmedName}" already exists.
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
        // Hide primary-key columns — they typically have no Lookup configured,
        // which would make the wizard fail with "Lookup not set for X/Id".
        const columns = (entityData.columns ?? []).filter(c => !c.isPrimaryKey);
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
                onChange={e => setModel(m => ({ ...m, caption: e.target.value }))}
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
              {columns.map(c => {
                const checked = model.selectedFieldIds.has(c.id);
                return (
                  <label
                    key={c.id}
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
                      onChange={() => toggleField(c.id)}
                      style={{ accentColor: 'var(--brand)' }}
                    />
                    {c.name}
                  </label>
                );
              })}
            </div>
          </>
        );
      }

      const selected = (entityData.columns ?? []).filter(c => model.selectedFieldIds.has(c.id));
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
              {selected.map(c => c.name).join(', ') || '—'}
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
            {STEPS.map((s, i) => (
              <div
                key={s.label}
                className={`${S.stepperItem} ${i === step ? S.active : ''} ${
                  i < step ? S.done : ''
                }`}
                onClick={() => i < step && setStep(i)}
              >
                <div className={S.stepBullet}>{i < step ? '✓' : i + 1}</div>
                <div className={S.stepText}>
                  <div className={S.stepLabel}>{s.label}</div>
                  <div className={S.stepHint}>{s.hint}</div>
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
