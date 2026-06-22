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
import { ICreateWizardResult, IDropDownValue, ILookupWizardEntityData } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { FilterableSelect } from '@editors/propertyEditor/FilterableSelect';

interface CreateLookupWizardProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateWizardResult) => void;
}

export interface LookupModel {
  name: string;
  displayFieldId: string;
  idFilterId: string;
  listFilterId: string;
}

export const CreateLookupWizard: React.FC<CreateLookupWizardProps> = observer(
  ({ entityId, parentNodeName, onCancel, onCreate }) => {
    const rootStore = useContext(RootStoreContext);
    const run = useMemo(
      () => runInFlowWithHandler(rootStore.errorDialogController),
      [rootStore.errorDialogController],
    );

    const wizardRef = useRef<HTMLDivElement>(null);
    const nameManuallyEditedRef = useRef(false);
    const [step, setStep] = useState(0);
    const [entityData, setEntityData] = useState<ILookupWizardEntityData | null>(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [model, setModel] = useState<LookupModel>({
      name: '',
      displayFieldId: '',
      idFilterId: '',
      listFilterId: '',
    });

    const steps = [
      {
        id: 'source',
        label: T('Source', 'create_lookup_step_source_label'),
        hint: T('Display field & filters', 'create_lookup_step_source_hint'),
      },
      {
        id: 'basics',
        label: T('Basics', 'create_lookup_step_basics_label'),
        hint: T('Name your lookup', 'create_lookup_step_basics_hint'),
      },
      {
        id: 'review',
        label: T('Review', 'wizard_step_review_label'),
        hint: T('Confirm and create', 'wizard_step_review_hint'),
      },
    ];

    const update = (patch: Partial<LookupModel>) => setModel(prev => ({ ...prev, ...patch }));

    const buildAutoName = (
      entityName: string,
      displayColumnName: string | undefined,
      idFilterName: string | undefined,
    ) => {
      let name = entityName;
      if (displayColumnName) {
        name += `_${displayColumnName}`;
      }
      if (idFilterName) {
        name += `_${idFilterName}`;
      }
      return name;
    };

    const onDisplayFieldChange = (newDisplayFieldId: string) => {
      if (nameManuallyEditedRef.current || !entityData) {
        update({ displayFieldId: newDisplayFieldId });
        return;
      }
      const column = entityData.columns?.find(column => column.id === newDisplayFieldId);
      const idFilter = entityData.filters?.find(filter => filter.id === model.idFilterId);
      setModel(prev => ({
        ...prev,
        displayFieldId: newDisplayFieldId,
        name: buildAutoName(entityData.entityName, column?.name, idFilter?.name),
      }));
    };

    const onIdFilterChange = (newIdFilterId: string) => {
      if (nameManuallyEditedRef.current || !entityData) {
        update({ idFilterId: newIdFilterId });
        return;
      }
      const column = entityData.columns?.find(column => column.id === model.displayFieldId);
      const idFilter = entityData.filters?.find(filter => filter.id === newIdFilterId);
      setModel(prev => ({
        ...prev,
        idFilterId: newIdFilterId,
        name: buildAutoName(entityData.entityName, column?.name, idFilter?.name),
      }));
    };

    const onNameChange = (newName: string) => {
      nameManuallyEditedRef.current = true;
      update({ name: newName });
    };

    useEffect(() => {
      let cancelled = false;
      run({
        generator: function* () {
          try {
            const data = (yield rootStore.architectApi.getLookupWizardEntityData(
              entityId,
            )) as ILookupWizardEntityData;
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
          return;
        }
        if (event.key === 'Tab' && wizardRef.current) {
          const focusableNodes = Array.from(
            wizardRef.current.querySelectorAll<HTMLElement>(
              'a[href], button, input, select, textarea, [tabindex]',
            ),
          ).filter(node => {
            if (node.hasAttribute('disabled')) return false;
            if (node.getAttribute('tabindex') === '-1') return false;
            if (node.offsetParent === null && node !== document.activeElement) return false;
            return true;
          });
          if (focusableNodes.length === 0) return;
          event.preventDefault();
          event.stopPropagation();
          const activeElement = document.activeElement as HTMLElement | null;
          const activeIndex = activeElement ? focusableNodes.indexOf(activeElement) : -1;
          const direction = event.shiftKey ? -1 : 1;
          const nextIndex =
            activeIndex === -1
              ? event.shiftKey
                ? focusableNodes.length - 1
                : 0
              : (activeIndex + direction + focusableNodes.length) % focusableNodes.length;
          focusableNodes[nextIndex].focus();
        }
      };
      document.addEventListener('keydown', onKeyDown);
      return () => document.removeEventListener('keydown', onKeyDown);
    }, [onCancel]);

    const canAdvance =
      (step === 0 && !!model.displayFieldId && !!model.idFilterId) ||
      (step === 1 && model.name.trim().length > 0) ||
      step === 2;

    const next = () => setStep(current => Math.min(current + 1, steps.length - 1));
    const back = () => setStep(current => Math.max(current - 1, 0));

    const submit = () => {
      if (submitting) return;
      setSubmitting(true);
      run({
        generator: function* () {
          try {
            const result = (yield rootStore.architectApi.createLookup({
              entityId,
              name: model.name.trim(),
              displayFieldId: model.displayFieldId,
              idFilterId: model.idFilterId,
              listFilterId: model.listFilterId || null,
            })) as ICreateWizardResult;
            onCreate(result);
          } finally {
            setSubmitting(false);
          }
        },
      });
    };

    const findName = (list: { id: string; name: string }[] | undefined, id: string) =>
      (list ?? []).find(item => item.id === id)?.name ?? '';

    const columnOptions = useMemo<IDropDownValue[]>(
      () => (entityData?.columns ?? []).map(column => ({ value: column.id, name: column.name })),
      [entityData?.columns],
    );
    const idFilterOptions = useMemo<IDropDownValue[]>(
      () => (entityData?.filters ?? []).map(filter => ({ value: filter.id, name: filter.name })),
      [entityData?.filters],
    );
    const listFilterOptions = idFilterOptions;

    const renderStep = () => {
      if (step === 1) {
        return (
          <>
            <h2 className={S.formTitle}>
              {T("Let's name your lookup", 'create_lookup_basics_title')}
            </h2>
            <p className={S.formSubtitle}>
              {T(
                'A lookup defines how a foreign key value is resolved into a human-readable label across the application.',
                'create_lookup_basics_subtitle',
              )}
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                {T('Name', 'create_lookup_name_label')} <span className={S.required}>*</span>
              </label>
              <input
                className={S.input}
                autoFocus
                placeholder={T('e.g. BusinessPartner_Name_GetId', 'create_lookup_name_placeholder')}
                value={model.name}
                onChange={event => onNameChange(event.target.value)}
              />
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
                <span className={S.previewBadge}>{T('Lookup', 'wizard_artifact_lookup')}</span>
                <span>{model.name || '<name>'}</span>
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>
                  {T('Data Structure', 'wizard_artifact_data_structure')}
                </span>
                <span>Lookup{model.name || '<name>'}</span>
              </div>
            </div>
          </>
        );
      }

      if (step === 0) {
        return (
          <>
            <h2 className={S.formTitle}>
              {T('Where does the data come from?', 'create_lookup_source_title')}
            </h2>
            <p className={S.formSubtitle}>
              {T(
                'Choose which column shows as the display value, and the filters used to fetch records by id or build the dropdown list.',
                'create_lookup_source_subtitle',
              )}
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                {T('Display Field', 'create_lookup_display_field_label')}{' '}
                <span className={S.required}>*</span>
              </label>
              <FilterableSelect
                className={S.filterableSelect}
                options={columnOptions}
                selectedValue={model.displayFieldId}
                disabled={loading}
                autoFocus={!loading}
                onChange={value => onDisplayFieldChange(value ?? '')}
              />
            </div>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                {T('List Filter', 'create_lookup_list_filter_label')}
              </label>
              <FilterableSelect
                className={S.filterableSelect}
                options={listFilterOptions}
                selectedValue={model.listFilterId}
                disabled={loading}
                onChange={value => update({ listFilterId: value ?? '' })}
              />
            </div>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                {T('Id Filter', 'create_lookup_id_filter_label')}{' '}
                <span className={S.required}>*</span>
              </label>
              <FilterableSelect
                className={S.filterableSelect}
                options={idFilterOptions}
                selectedValue={model.idFilterId}
                disabled={loading}
                onChange={value => onIdFilterChange(value ?? '')}
              />
            </div>

            <div className={S.preview}>
              <div className={S.previewTitle}>
                {T('What will be created', 'wizard_what_created')}
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>{T('Lookup', 'wizard_artifact_lookup')}</span>
                <span>{model.name}</span>
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>
                  {T('Data Structure', 'wizard_artifact_data_structure')}
                </span>
                <span>Lookup{model.name}</span>
              </div>
            </div>
          </>
        );
      }

      if (!entityData) return null;
      return (
        <>
          <h2 className={S.formTitle}>{T('Ready to create', 'wizard_ready_title')}</h2>
          <p className={S.formSubtitle}>
            {T(
              'Review what will be added to the model. You can edit anything afterward.',
              'create_lookup_review_subtitle',
            )}
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>L</div>
              <div>
                <div className={S.reviewCardTitle}>
                  {model.name || T('Untitled', 'wizard_untitled')}
                </div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  {T('Data Service Lookup', 'create_lookup_review_type')}
                </div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>{T('Entity', 'create_lookup_review_entity')}</div>
              <div>{entityData.entityName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>{T('Id Column', 'create_lookup_review_id_column')}</div>
              <div>{entityData.primaryKeyName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Display Field', 'create_lookup_display_field_label')}
              </div>
              <div>{findName(entityData.columns, model.displayFieldId)}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('List Filter', 'create_lookup_list_filter_label')}
              </div>
              <div>
                {findName(entityData.filters, model.listFilterId) ||
                  T('none', 'create_lookup_review_none')}
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>{T('Id Filter', 'create_lookup_id_filter_label')}</div>
              <div>{findName(entityData.filters, model.idFilterId)}</div>
            </div>
          </div>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>D</div>
              <div>
                <div className={S.reviewCardTitle}>Lookup{model.name}</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  {T('Data Structure (auto-generated)', 'create_lookup_review_ds_type')}
                </div>
              </div>
            </div>
          </div>
        </>
      );
    };

    return (
      <div className={S.drawer} role="dialog" aria-modal="true" ref={wizardRef}>
        <div className={S.header}>
          <div className={S.headerIcon}>L</div>
          <div className={S.headerText}>
            <div className={S.headerTitle}>{T('Create Lookup', 'create_lookup_header_title')}</div>
            <div className={S.headerSubtitle}>
              {T('in {0}', 'create_lookup_header_subtitle', parentNodeName)}
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
                      : T('Create Lookup', 'create_lookup_btn_create')}
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
