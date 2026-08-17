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
import { ICreateWizardResult, IDataStructureWizardData } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';

interface CreateDataStructureWizardProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateWizardResult) => void;
}

export interface DataStructureModel {
  name: string;
}

export const CreateDataStructureWizard: React.FC<CreateDataStructureWizardProps> = observer(
  ({ entityId, parentNodeName, onCancel, onCreate }) => {
    const rootStore = useContext(RootStoreContext);
    const run = useMemo(
      () => runInFlowWithHandler(rootStore.errorDialogController),
      [rootStore.errorDialogController],
    );

    const wizardRef = useRef<HTMLDivElement>(null);
    const lastFocusedRef = useRef<HTMLElement | null>(null);
    const [step, setStep] = useState(0);
    const [entityData, setEntityData] = useState<IDataStructureWizardData | null>(null);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [model, setModel] = useState<DataStructureModel>({ name: '' });

    const steps = [
      {
        id: 'structure',
        label: T('Structure', 'create_data_structure_step_structure_label'),
        hint: T('Name the structure', 'create_data_structure_step_structure_hint'),
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
            const data = (yield rootStore.architectApi.getDataStructureWizardData(
              entityId,
            )) as IDataStructureWizardData;
            if (cancelled) return;
            setEntityData(data);
            setModel(prev => ({ ...prev, name: prev.name || data.entityName || '' }));
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
      const onFocusIn = (event: FocusEvent) => {
        const target = event.target as HTMLElement | null;
        if (target && wizardRef.current?.contains(target)) {
          lastFocusedRef.current = target;
        }
      };
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
          const firstNode = focusableNodes[0];
          const lastNode = focusableNodes[focusableNodes.length - 1];
          const activeElement = document.activeElement as HTMLElement | null;
          const isInsideWizard = !!activeElement && wizardRef.current.contains(activeElement);
          const direction = event.shiftKey ? -1 : 1;
          if (!isInsideWizard) {
            event.preventDefault();
            const resumeIndex = lastFocusedRef.current
              ? focusableNodes.indexOf(lastFocusedRef.current)
              : -1;
            const targetNode =
              resumeIndex === -1
                ? event.shiftKey
                  ? lastNode
                  : firstNode
                : focusableNodes[
                    (resumeIndex + direction + focusableNodes.length) % focusableNodes.length
                  ];
            targetNode.focus();
          } else if (event.shiftKey && activeElement === firstNode) {
            event.preventDefault();
            lastNode.focus();
          } else if (!event.shiftKey && activeElement === lastNode) {
            event.preventDefault();
            firstNode.focus();
          }
        }
      };
      document.addEventListener('focusin', onFocusIn);
      document.addEventListener('keydown', onKeyDown);
      return () => {
        document.removeEventListener('focusin', onFocusIn);
        document.removeEventListener('keydown', onKeyDown);
      };
    }, [onCancel]);

    const trimmedName = (model.name ?? '').trim();
    const nameExists = useMemo(
      () =>
        (entityData?.existingDataStructureNames ?? []).some(
          existingName => existingName.toLowerCase() === trimmedName.toLowerCase(),
        ),
      [entityData?.existingDataStructureNames, trimmedName],
    );

    const isValid = trimmedName.length > 0 && !nameExists;
    const canAdvance = (step === 0 && isValid) || step === 1;
    const next = () => setStep(current => Math.min(current + 1, steps.length - 1));
    const back = () => setStep(current => Math.max(current - 1, 0));

    const submit = () => {
      if (submitting || !isValid) return;
      setSubmitting(true);
      run({
        generator: function* () {
          try {
            const result = (yield rootStore.architectApi.createDataStructure({
              entityId,
              name: trimmedName,
            })) as ICreateWizardResult;
            onCreate(result);
          } finally {
            setSubmitting(false);
          }
        },
      });
    };

    const renderForm = () => (
      <>
        <h2 className={S.formTitle}>
          {T('Name the data structure', 'create_data_structure_structure_title')}
        </h2>
        <p className={S.formSubtitle}>
          {T(
            'This wizard creates a Data Structure over this entity. Choose a unique name.',
            'create_data_structure_structure_subtitle',
          )}
        </p>

        <div className={S.field}>
          <label className={S.fieldLabel}>
            {T('Name of Structure', 'create_data_structure_name_label')}{' '}
            <span className={S.required}>*</span>
          </label>
          <input
            className={S.input}
            value={model.name}
            disabled={loading}
            autoFocus={!loading}
            onChange={event => setModel({ name: event.target.value })}
          />
          {nameExists && (
            <span className={S.fieldError}>
              {T('Name of Structure already exists.', 'create_data_structure_name_exists_error')}
            </span>
          )}
        </div>
      </>
    );

    const renderReview = () => {
      if (!entityData) return null;
      return (
        <>
          <h2 className={S.formTitle}>{T('Ready to create', 'wizard_ready_title')}</h2>
          <p className={S.formSubtitle}>
            {T(
              'This wizard will create a Data Structure with these parameters:',
              'create_data_structure_review_subtitle',
            )}
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Data Structure', 'create_data_structure_review_name')}
              </div>
              <div>{trimmedName}</div>
            </div>
          </div>
        </>
      );
    };

    return (
      <div className={S.drawer} role="dialog" aria-modal="true" ref={wizardRef}>
        <div className={S.header}>
          <div className={S.headerIcon}>D</div>
          <div className={S.headerText}>
            <div className={S.headerTitle}>
              {T('Create Data Structure', 'create_data_structure_header_title')}
            </div>
            <div className={S.headerSubtitle}>
              {T('in {0}', 'create_data_structure_header_subtitle', parentNodeName)}
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
            <div className={S.formContent}>{step === 0 ? renderForm() : renderReview()}</div>

            <div className={S.footer}>
              <div className={S.footerHint}>
                {T('Step {0} of {1}', 'wizard_step_counter', step + 1, steps.length)}
              </div>
              <div className={S.footerBtns}>
                <button type="button" className={S.btn} onClick={onCancel}>
                  {T('Cancel', 'wizard_btn_cancel')}
                </button>
                {step > 0 && (
                  <button type="button" className={S.btn} onClick={back} disabled={submitting}>
                    {T('Back', 'wizard_btn_back')}
                  </button>
                )}
                {step < steps.length - 1 ? (
                  <button
                    type="button"
                    className={`${S.btn} ${S.btnPrimary}`}
                    onClick={next}
                    disabled={!canAdvance || loading}
                  >
                    {T('Next →', 'wizard_btn_next')}
                  </button>
                ) : (
                  <button
                    type="button"
                    className={`${S.btn} ${S.btnPrimary}`}
                    onClick={submit}
                    disabled={submitting || loading || !isValid}
                  >
                    {submitting
                      ? T('Creating…', 'wizard_btn_creating')
                      : T('Create Data Structure', 'create_data_structure_btn_create')}
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
