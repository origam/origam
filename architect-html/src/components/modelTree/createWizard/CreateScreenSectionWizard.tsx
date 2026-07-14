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
import { ICreateWizardResult, IScreenSectionWizardData } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';

interface CreateScreenSectionWizardProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateWizardResult) => void;
}

export const CreateScreenSectionWizard: React.FC<CreateScreenSectionWizardProps> = observer(
  ({ entityId, parentNodeName, onCancel, onCreate }) => {
    const rootStore = useContext(RootStoreContext);
    const run = useMemo(
      () => runInFlowWithHandler(rootStore.errorDialogController),
      [rootStore.errorDialogController],
    );

    const [step, setStep] = useState(0);
    const [entityData, setEntityData] = useState<IScreenSectionWizardData | null>(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [name, setName] = useState('');
    const [caption, setCaption] = useState('');
    const [selectedFieldIds, setSelectedFieldIds] = useState<Set<string>>(new Set());

    const steps = [
      {
        id: 'basics',
        label: T('Basics', 'create_screen_section_step_basics_label'),
        hint: T('Name & caption', 'create_screen_section_step_basics_hint'),
      },
      {
        id: 'fields',
        label: T('Fields', 'create_screen_section_step_fields_label'),
        hint: T('Columns to show', 'create_screen_section_step_fields_hint'),
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
            const data = (yield rootStore.architectApi.getScreenSectionWizardData(
              entityId,
            )) as IScreenSectionWizardData;
            if (cancelled) return;
            setEntityData(data);
            const existing = new Set(data.existingScreenSectionNames.map(n => n.toLowerCase()));
            let candidate = data.entityName;
            let suffix = 2;
            while (existing.has(candidate.toLowerCase())) {
              candidate = `${data.entityName}${suffix}`;
              suffix += 1;
            }
            setName(candidate);
            setCaption(data.entityCaption || data.entityName);
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

    const existingNamesLower = useMemo(
      () =>
        new Set((entityData?.existingScreenSectionNames ?? []).map(entry => entry.toLowerCase())),
      [entityData],
    );

    const trimmedName = name.trim();
    const nameEmpty = trimmedName.length === 0;
    const nameDuplicate =
      trimmedName.length > 0 && existingNamesLower.has(trimmedName.toLowerCase());
    const nameError = nameEmpty
      ? T('Name is required.', 'create_screen_section_name_required_error')
      : nameDuplicate
        ? T(
            'A Screen Section with this name already exists.',
            'create_screen_section_name_exists_error',
          )
        : '';

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

    const canAdvance =
      (step === 0 && !nameError) || (step === 1 && selectedFieldIds.size > 0) || step === 2;

    const next = () => setStep(current => Math.min(current + 1, steps.length - 1));
    const back = () => setStep(current => Math.max(current - 1, 0));

    const submit = () => {
      if (submitting || nameError || selectedFieldIds.size === 0) return;
      setSubmitting(true);
      run({
        generator: function* () {
          try {
            const result = (yield rootStore.architectApi.createScreenSection({
              entityId,
              name: trimmedName,
              caption: caption.trim(),
              selectedFieldIds: Array.from(selectedFieldIds),
            })) as ICreateWizardResult;
            onCreate(result);
          } finally {
            setSubmitting(false);
          }
        },
      });
    };

    const renderBasics = () => (
      <>
        <h2 className={S.formTitle}>
          {T('Name the Screen Section', 'create_screen_section_basics_title')}
        </h2>
        <p className={S.formSubtitle}>
          {T(
            'A Screen Section (PanelControlSet) will be created on this entity. No Data Structure is created — only the section itself.',
            'create_screen_section_basics_subtitle',
          )}
        </p>

        <div className={S.field}>
          <label className={S.fieldLabel}>
            {T('Name', 'create_screen_section_name_label')} <span className={S.required}>*</span>
            <span className={S.fieldHint}>
              {T('shown in the model tree; must be unique', 'create_screen_section_name_hint')}
            </span>
          </label>
          <input
            className={S.input}
            autoFocus
            value={name}
            onChange={event => setName(event.target.value)}
          />
          {nameError && <span className={S.fieldError}>{nameError}</span>}
        </div>

        <div className={S.field}>
          <label className={S.fieldLabel}>
            {T('Caption', 'create_screen_section_caption_label')}
            <span className={S.fieldHint}>
              {T(
                "PanelTitle shown at runtime; defaults to the entity's caption",
                'create_screen_section_caption_hint',
              )}
            </span>
          </label>
          <input
            className={S.input}
            value={caption}
            onChange={event => setCaption(event.target.value)}
          />
        </div>
      </>
    );

    const renderFields = () => {
      const columns = entityData?.columns ?? [];
      return (
        <>
          <h2 className={S.formTitle}>
            {T('Which fields to show?', 'create_screen_section_fields_title')}
          </h2>
          <p className={S.formSubtitle}>
            {T(
              'Pick the columns that should appear as controls on the section.',
              'create_screen_section_fields_subtitle',
            )}
          </p>

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
                  {column.isPrimaryKey && (
                    <span style={{ color: 'var(--background6)', fontSize: 11 }}>
                      {T('(primary key)', 'wizard_field_primary_key')}
                    </span>
                  )}
                </label>
              );
            })}
          </div>
        </>
      );
    };

    const renderReview = () => {
      if (!entityData) return null;
      const selected = entityData.columns.filter(column => selectedFieldIds.has(column.id));
      return (
        <>
          <h2 className={S.formTitle}>{T('Ready to create', 'wizard_ready_title')}</h2>
          <p className={S.formSubtitle}>
            {T(
              'Only a Screen Section will be created. No Data Structure or Screen is generated by this wizard.',
              'create_screen_section_review_subtitle',
            )}
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>S</div>
              <div>
                <div className={S.reviewCardTitle}>{trimmedName}</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  {T('Screen Section', 'create_screen_section_review_type')}
                </div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Entity', 'create_screen_section_review_entity')}
              </div>
              <div>{entityData.entityName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Caption (PanelTitle)', 'create_screen_section_review_caption')}
              </div>
              <div>{caption.trim() || '—'}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Fields', 'create_screen_section_review_fields')}
              </div>
              <div>{selected.length}</div>
            </div>
          </div>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>F</div>
              <div>
                <div className={S.reviewCardTitle}>
                  {T('Selected fields', 'create_screen_section_review_selected_fields_title')}
                </div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  {T(
                    'Rendered as default controls',
                    'create_screen_section_review_selected_fields_hint',
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

    const renderStep = () => {
      if (step === 0) return renderBasics();
      if (step === 1) return renderFields();
      return renderReview();
    };

    return (
      <div className={S.drawer} role="dialog" aria-modal="true">
        <div className={S.header}>
          <div className={S.headerIcon}>S</div>
          <div className={S.headerText}>
            <div className={S.headerTitle}>
              {T('Create Screen Section', 'create_screen_section_header_title')}
            </div>
            <div className={S.headerSubtitle}>
              {T('from {0}', 'create_screen_section_header_subtitle', parentNodeName)}
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
                    disabled={submitting || loading || !!nameError || selectedFieldIds.size === 0}
                  >
                    {submitting
                      ? T('Creating…', 'wizard_btn_creating')
                      : T('Create Screen Section', 'create_screen_section_btn_create')}
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
