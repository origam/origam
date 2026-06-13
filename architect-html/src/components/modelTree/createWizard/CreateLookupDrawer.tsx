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
import { ICreateActionResult, IDropDownValue, ILookupWizardEntityData } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { FilterableSelect } from '@editors/propertyEditor/FilterableSelect';

interface CreateLookupDrawerProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateActionResult) => void;
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
    const run = useMemo(
      () => runInFlowWithHandler(rootStore.errorDialogController),
      [rootStore.errorDialogController],
    );

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
        if (event.key === 'Tab' && drawerRef.current) {
          const focusableNodes = Array.from(
            drawerRef.current.querySelectorAll<HTMLElement>(
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

    const next = () => setStep(current => Math.min(current + 1, STEPS.length - 1));
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
            })) as ICreateActionResult;
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
            <h2 className={S.formTitle}>Let&apos;s name your lookup</h2>
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
                onChange={event => onNameChange(event.target.value)}
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
              Choose which column shows as the display value, and the filters used to fetch records
              by id or build the dropdown list.
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

      if (!entityData) return null;
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
