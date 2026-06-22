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
import React, { useContext, useEffect, useMemo, useRef, useState } from 'react';
import { RootStoreContext, T } from '@/main';
import { ICreateWizardResult, IScreenWizardData } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';

interface CreateScreenWizardProps {
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

export const CreateScreenWizard: React.FC<CreateScreenWizardProps> = observer(
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

    const steps = [
      {
        id: 'basics',
        label: T('Basics', 'create_screen_step_basics_label'),
        hint: T('Name your screen', 'create_screen_step_basics_hint'),
      },
      {
        id: 'fields',
        label: T('Fields', 'create_screen_step_fields_label'),
        hint: T('Caption & visible columns', 'create_screen_step_fields_hint'),
      },
      {
        id: 'review',
        label: T('Review', 'wizard_step_review_label'),
        hint: T('Confirm and create', 'wizard_step_review_hint'),
      },
    ];

    const wizardRef = useRef<HTMLDivElement | null>(null);
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
        const drawer = wizardRef.current;
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
        window.alert(
          T(
            'A DataStructure named "{0}" already exists.',
            'create_screen_datastructure_exists',
            trimmedName,
          ),
        );
        return;
      }
      setStep(current => Math.min(current + 1, steps.length - 1));
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
            <h2 className={S.formTitle}>
              {T("Let's name your screen", 'create_screen_basics_title')}
            </h2>
            <p className={S.formSubtitle}>
              {T(
                'A Screen ties a DataStructure to a Screen Section (Panel) and a Form. The name is used for all three artifacts.',
                'create_screen_basics_subtitle',
              )}
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                {T('Name', 'create_screen_name_label')} <span className={S.required}>*</span>
              </label>
              <input
                className={S.input}
                autoFocus
                placeholder={
                  entityData
                    ? T('e.g. {0}', 'wizard_placeholder_example', entityData.entityName)
                    : ''
                }
                value={model.name}
                onChange={event => {
                  nameManuallyEditedRef.current = true;
                  setModel(prev => ({ ...prev, name: event.target.value }));
                }}
              />
              {nameExists && (
                <div style={{ fontSize: 12, color: 'var(--error1)', marginTop: 2 }}>
                  {T(
                    'A DataStructure named "{0}" already exists.',
                    'create_screen_datastructure_exists',
                    trimmedName,
                  )}
                </div>
              )}
            </div>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                {T('Created from entity', 'wizard_created_from_entity')}
              </label>
              <input className={S.input} value={entityData?.entityName ?? ''} disabled />
            </div>

            <div className={S.preview}>
              <div className={S.previewTitle}>
                {T('What will be created', 'wizard_what_created')}
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>
                  {T('Data Structure', 'wizard_artifact_data_structure')}
                </span>
                <span>{model.name || '<name>'}</span>
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>
                  {T('Screen Section', 'wizard_artifact_screen_section')}
                </span>
                <span>{model.name || '<name>'}</span>
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>{T('Screen', 'wizard_artifact_screen')}</span>
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
            <h2 className={S.formTitle}>
              {T('Which columns should appear?', 'create_screen_fields_title')}
            </h2>
            <p className={S.formSubtitle}>
              {T(
                'Pick the entity fields that will be placed on the Screen Section. You can add or remove fields later in the section editor.',
                'create_screen_fields_subtitle',
              )}
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                {T('Caption', 'create_screen_caption_label')}
                <span className={S.fieldHint}>
                  {T('— shown as the Screen Section title', 'create_screen_caption_hint')}
                </span>
              </label>
              <input
                className={S.input}
                placeholder={T('e.g. {0}', 'wizard_placeholder_example', entityData.entityName)}
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
                  model.selectedFieldIds.size,
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
          <h2 className={S.formTitle}>{T('Ready to create', 'wizard_ready_title')}</h2>
          <p className={S.formSubtitle}>
            {T(
              'Review what will be added to the model. You can edit everything afterward.',
              'create_screen_review_subtitle',
            )}
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>S</div>
              <div>
                <div className={S.reviewCardTitle}>
                  {model.name || T('Untitled', 'wizard_untitled')}
                </div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  {T('Screen + Section + DataStructure', 'create_screen_review_type')}
                </div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>{T('Entity', 'create_screen_review_entity')}</div>
              <div>{entityData.entityName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>{T('Caption', 'create_screen_caption_label')}</div>
              <div>{model.caption.trim() || T('(none)', 'create_screen_review_caption_none')}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>{T('Fields', 'create_screen_review_fields')}</div>
              <div>{T('{0} selected', 'create_screen_review_fields_count', selected.length)}</div>
            </div>
          </div>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>F</div>
              <div>
                <div className={S.reviewCardTitle}>
                  {T('Selected fields', 'create_screen_review_selected_fields_title')}
                </div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  {T(
                    'Placed on the Screen Section in order',
                    'create_screen_review_selected_fields_hint',
                  )}
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
      <div className={S.drawer} role="dialog" aria-modal="true" ref={wizardRef}>
        <div className={S.header}>
          <div className={S.headerIcon}>S</div>
          <div className={S.headerText}>
            <div className={S.headerTitle}>{T('Create Screen', 'create_screen_header_title')}</div>
            <div className={S.headerSubtitle}>
              {T('from {0}', 'create_screen_header_subtitle', parentNodeName)}
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
            <div className={S.formContent} ref={formContentRef}>
              {renderStep()}
            </div>

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
                      : T('Create Screen', 'create_screen_btn_create')}
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
