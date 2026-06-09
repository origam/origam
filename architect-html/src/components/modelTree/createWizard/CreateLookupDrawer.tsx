/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.
This file is part of ORIGAM (http://www.origam.org).
*/

import S from './CreateLookupDrawer.module.scss';
import { observer } from 'mobx-react-lite';
import React, { useContext, useEffect, useMemo, useRef, useState } from 'react';
import { RootStoreContext } from '@/main';
import {
  ICreateLookupResult,
  IDropDownValue,
  ILookupWizardEntityData,
} from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { FilterableSelect } from '@editors/propertyEditor/FilterableSelect';

interface CreateLookupDrawerProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateLookupResult) => void;
}

export interface LookupModel {
  name: string;
  displayFieldId: string;
  idFilterId: string;
  listFilterId: string;
}

const STEPS = [
  { label: 'Source', hint: 'Display field & filters' },
  { label: 'Basics', hint: 'Name your lookup' },
  { label: 'Review', hint: 'Confirm and create' },
];

export const CreateLookupDrawer: React.FC<CreateLookupDrawerProps> = observer(
  ({ entityId, parentNodeName, onCancel, onCreate }) => {
    const rootStore = useContext(RootStoreContext);
    const run = runInFlowWithHandler(rootStore.errorDialogController);

    const drawerRef = useRef<HTMLDivElement>(null);
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

    const update = (patch: Partial<LookupModel>) => setModel(m => ({ ...m, ...patch }));

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
      const column = entityData.columns?.find(c => c.id === newDisplayFieldId);
      const idFilter = entityData.filters?.find(f => f.id === model.idFilterId);
      setModel(m => ({
        ...m,
        displayFieldId: newDisplayFieldId,
        name: buildAutoName(entityData.entityName, column?.name, idFilter?.name),
      }));
    };

    const onIdFilterChange = (newIdFilterId: string) => {
      if (nameManuallyEditedRef.current || !entityData) {
        update({ idFilterId: newIdFilterId });
        return;
      }
      const column = entityData.columns?.find(c => c.id === model.displayFieldId);
      const idFilter = entityData.filters?.find(f => f.id === newIdFilterId);
      setModel(m => ({
        ...m,
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
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [entityId]);

    useEffect(() => {
      const onKeyDown = (e: KeyboardEvent) => {
        if (e.key === 'Escape') {
          e.stopPropagation();
          onCancel();
          return;
        }
        if (e.key === 'Tab' && drawerRef.current) {
          const nodes = Array.from(
            drawerRef.current.querySelectorAll<HTMLElement>(
              'a[href], button, input, select, textarea, [tabindex]',
            ),
          ).filter(el => {
            if (el.hasAttribute('disabled')) return false;
            if (el.getAttribute('tabindex') === '-1') return false;
            if (el.offsetParent === null && el !== document.activeElement) return false;
            return true;
          });
          if (nodes.length === 0) return;
          e.preventDefault();
          e.stopPropagation();
          const active = document.activeElement as HTMLElement | null;
          const idx = active ? nodes.indexOf(active) : -1;
          const dir = e.shiftKey ? -1 : 1;
          const nextIdx = idx === -1
            ? (e.shiftKey ? nodes.length - 1 : 0)
            : (idx + dir + nodes.length) % nodes.length;
          nodes[nextIdx].focus();
        }
      };
      document.addEventListener('keydown', onKeyDown);
      return () => document.removeEventListener('keydown', onKeyDown);
    }, [onCancel]);

    const canAdvance =
      (step === 0 && !!model.displayFieldId && !!model.idFilterId) ||
      (step === 1 && model.name.trim().length > 0) ||
      step === 2;

    const next = () => setStep(s => Math.min(s + 1, STEPS.length - 1));
    const back = () => setStep(s => Math.max(s - 1, 0));

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
            })) as ICreateLookupResult;
            onCreate(result);
          } finally {
            setSubmitting(false);
          }
        },
      });
    };

    const findName = (list: { id: string; name: string }[] | undefined, id: string) =>
      (list ?? []).find(x => x.id === id)?.name ?? '';

    const columnOptions = useMemo<IDropDownValue[]>(
      () => (entityData?.columns ?? []).map(c => ({ value: c.id, name: c.name })),
      [entityData?.columns],
    );
    const idFilterOptions = useMemo<IDropDownValue[]>(
      () => (entityData?.filters ?? []).map(f => ({ value: f.id, name: f.name })),
      [entityData?.filters],
    );
    const listFilterOptions = idFilterOptions;

    const renderStep = () => {
      if (step === 1) {
        return (
          <>
            <h2 className={S.formTitle}>Let's name your lookup</h2>
            <p className={S.formSubtitle}>
              A lookup defines how a foreign key value is resolved into a human-readable label
              across the application.
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                Name <span className={S.required}>*</span>
              </label>
              <input
                className={S.input}
                autoFocus
                placeholder="e.g. BusinessPartner_Name_GetId"
                value={model.name}
                onChange={e => onNameChange(e.target.value)}
              />
            </div>

            <div className={S.field}>
              <label className={S.fieldLabel}>Created from entity</label>
              <input className={S.input} value={entityData?.entityName ?? ''} disabled />
            </div>

            <div className={S.preview}>
              <div className={S.previewTitle}>What will be created</div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Lookup</span>
                <span>{model.name || '<name>'}</span>
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Data Structure</span>
                <span>Lookup{model.name || '<name>'}</span>
              </div>
            </div>
          </>
        );
      }

      if (step === 0) {
        return (
          <>
            <h2 className={S.formTitle}>Where does the data come from?</h2>
            <p className={S.formSubtitle}>
              Choose which column shows as the display value, and the filters used to fetch
              records by id or build the dropdown list.
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                Display Field <span className={S.required}>*</span>
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
              <label className={S.fieldLabel}>List Filter</label>
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
                Id Filter <span className={S.required}>*</span>
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
              <div className={S.previewTitle}>What will be created</div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Lookup</span>
                <span>{model.name}</span>
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Data Structure</span>
                <span>Lookup{model.name}</span>
              </div>
            </div>
          </>
        );
      }

      return (
        <>
          <h2 className={S.formTitle}>Ready to create</h2>
          <p className={S.formSubtitle}>
            Review what will be added to the model. You can edit anything afterward.
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>L</div>
              <div>
                <div className={S.reviewCardTitle}>{model.name || 'Untitled'}</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>Data Service Lookup</div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Entity</div>
              <div>{entityData.entityName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Id Column</div>
              <div>{entityData.primaryKeyName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Display Field</div>
              <div>{findName(entityData.columns, model.displayFieldId)}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>List Filter</div>
              <div>{findName(entityData.filters, model.listFilterId) || 'none'}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Id Filter</div>
              <div>{findName(entityData.filters, model.idFilterId)}</div>
            </div>
          </div>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>D</div>
              <div>
                <div className={S.reviewCardTitle}>Lookup{model.name}</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  Data Structure (auto-generated)
                </div>
              </div>
            </div>
          </div>
        </>
      );
    };

    return (
      <div className={S.drawer} role="dialog" aria-modal="true" ref={drawerRef}>
        <div className={S.header}>
          <div className={S.headerIcon}>L</div>
          <div className={S.headerText}>
            <div className={S.headerTitle}>Create Lookup</div>
            <div className={S.headerSubtitle}>in {parentNodeName}</div>
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
                    {submitting ? 'Creating…' : 'Create Lookup'}
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
