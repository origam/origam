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
import {
  ICreateWizardResult,
  IDropDownValue,
  IIdName,
  IRelationshipWizardData,
} from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';
import { FilterableSelect } from '@editors/propertyEditor/FilterableSelect';

interface CreateRelationshipWizardProps {
  entityId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateWizardResult) => void;
}

export interface RelationshipModel {
  relatedEntityId: string;
  isParentChild: boolean;
  relationName: string;
  baseFieldId: string;
  relatedFieldId: string;
  keyName: string;
}

export const CreateRelationshipWizard: React.FC<CreateRelationshipWizardProps> = observer(
  ({ entityId, parentNodeName, onCancel, onCreate }) => {
    const rootStore = useContext(RootStoreContext);
    const run = useMemo(
      () => runInFlowWithHandler(rootStore.errorDialogController),
      [rootStore.errorDialogController],
    );

    const wizardRef = useRef<HTMLDivElement>(null);
    const lastFocusedRef = useRef<HTMLElement | null>(null);
    const keyNameEditedRef = useRef(false);
    const [step, setStep] = useState(0);
    const [entityData, setEntityData] = useState<IRelationshipWizardData | null>(null);
    const [relatedFields, setRelatedFields] = useState<IIdName[]>([]);
    const [loading, setLoading] = useState(true);
    const [loadingRelatedFields, setLoadingRelatedFields] = useState(false);
    const [submitting, setSubmitting] = useState(false);
    const [model, setModel] = useState<RelationshipModel>({
      relatedEntityId: '',
      isParentChild: false,
      relationName: '',
      baseFieldId: '',
      relatedFieldId: '',
      keyName: '',
    });

    const steps = [
      {
        id: 'relation',
        label: T('Relation', 'create_relationship_step_relation_label'),
        hint: T('Entity, name & key', 'create_relationship_step_relation_hint'),
      },
      {
        id: 'review',
        label: T('Review', 'wizard_step_review_label'),
        hint: T('Confirm and create', 'wizard_step_review_hint'),
      },
    ];

    const update = (patch: Partial<RelationshipModel>) => setModel(prev => ({ ...prev, ...patch }));

    const findName = (list: IIdName[] | undefined, id: string) =>
      (list ?? []).find(item => item.id === id)?.name ?? '';

    const onRelatedEntityChange = (newRelatedEntityId: string) => {
      const relatedEntityName = findName(entityData?.entities, newRelatedEntityId);
      setModel(prev => ({
        ...prev,
        relatedEntityId: newRelatedEntityId,
        relatedFieldId: '',
        keyName: keyNameEditedRef.current
          ? prev.keyName
          : relatedEntityName
            ? `${relatedEntityName}_RelationKey`
            : '',
      }));
      setRelatedFields([]);
      if (!newRelatedEntityId) {
        return;
      }
      setLoadingRelatedFields(true);
      run({
        generator: function* () {
          try {
            const fields = (yield rootStore.architectApi.getEntityFields(
              newRelatedEntityId,
            )) as IIdName[];
            setRelatedFields(fields);
          } finally {
            setLoadingRelatedFields(false);
          }
        },
      });
    };

    const onRelationNameChange = (newName: string) => {
      update({ relationName: newName });
    };

    const onKeyNameChange = (newName: string) => {
      keyNameEditedRef.current = true;
      update({ keyName: newName });
    };

    useEffect(() => {
      let cancelled = false;
      run({
        generator: function* () {
          try {
            const data = (yield rootStore.architectApi.getRelationshipWizardData(
              entityId,
            )) as IRelationshipWizardData;
            if (cancelled) return;
            setEntityData(data);
            setModel(prev => ({
              ...prev,
              relationName: prev.relationName || data.baseEntityName,
            }));
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

    const keyGroupDisabled = loading || !model.relatedEntityId;

    const isValid =
      !!model.relatedEntityId &&
      model.relationName.trim().length > 0 &&
      !!model.baseFieldId &&
      !!model.relatedFieldId &&
      model.keyName.trim().length > 0;

    const canAdvance = (step === 0 && isValid) || step === 1;
    const next = () => setStep(current => Math.min(current + 1, steps.length - 1));
    const back = () => setStep(current => Math.max(current - 1, 0));

    const submit = () => {
      if (submitting || !isValid) return;
      setSubmitting(true);
      run({
        generator: function* () {
          try {
            const result = (yield rootStore.architectApi.createRelationship({
              baseEntityId: entityId,
              relatedEntityId: model.relatedEntityId,
              relationName: model.relationName.trim(),
              isParentChild: model.isParentChild,
              keyName: model.keyName.trim(),
              baseFieldId: model.baseFieldId,
              relatedFieldId: model.relatedFieldId,
            })) as ICreateWizardResult;
            onCreate(result);
          } finally {
            setSubmitting(false);
          }
        },
      });
    };

    const entityOptions = useMemo<IDropDownValue[]>(
      () => (entityData?.entities ?? []).map(entity => ({ value: entity.id, name: entity.name })),
      [entityData?.entities],
    );
    const baseFieldOptions = useMemo<IDropDownValue[]>(
      () =>
        (entityData?.baseEntityColumns ?? []).map(column => ({
          value: column.id,
          name: column.name,
        })),
      [entityData?.baseEntityColumns],
    );
    const relatedFieldOptions = useMemo<IDropDownValue[]>(
      () => relatedFields.map(field => ({ value: field.id, name: field.name })),
      [relatedFields],
    );

    const relatedEntityName = findName(entityData?.entities, model.relatedEntityId);

    const renderForm = () => (
      <>
        <h2 className={S.formTitle}>
          {T('What are we relating to?', 'create_relationship_relation_title')}
        </h2>
        <p className={S.formSubtitle}>
          {T(
            'A relationship connects this entity to another one. Choose the target entity, then map the key fields.',
            'create_relationship_relation_subtitle',
          )}
        </p>

        <div className={S.field}>
          <label className={S.fieldLabel}>
            {T('Relation Name', 'create_relationship_relation_name_label')}{' '}
            <span className={S.required}>*</span>
          </label>
          <input
            className={S.input}
            placeholder={T('e.g. Counter', 'create_relationship_relation_name_placeholder')}
            value={model.relationName}
            onChange={event => onRelationNameChange(event.target.value)}
          />
        </div>

        <div className={S.field}>
          <label
            className={S.fieldLabel}
            style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer' }}
          >
            <input
              type="checkbox"
              checked={model.isParentChild}
              onChange={event => update({ isParentChild: event.target.checked })}
              style={{ accentColor: 'var(--brand)' }}
            />
            {T('Is Parent-Child', 'create_relationship_is_parent_child_label')}
          </label>
        </div>

        <div className={S.field}>
          <label className={S.fieldLabel}>
            {T('Table', 'create_relationship_table_label')} <span className={S.required}>*</span>
            <span className={S.fieldHint}>
              {T('the related entity', 'create_relationship_table_hint')}
            </span>
          </label>
          <FilterableSelect
            className={S.filterableSelect}
            options={entityOptions}
            selectedValue={model.relatedEntityId}
            disabled={loading}
            autoFocus={!loading}
            onChange={value => onRelatedEntityChange(value ?? '')}
          />
        </div>

        <fieldset className={`${S.group} ${keyGroupDisabled ? S.groupDisabled : ''}`}>
          <legend className={S.groupTitle}>
            {T('Key', 'create_relationship_key_group_label')}
          </legend>

          <div className={S.field}>
            <label className={S.fieldLabel}>
              {T('Key Name', 'create_relationship_key_name_label')}{' '}
              <span className={S.required}>*</span>
            </label>
            <input
              className={S.input}
              placeholder={T(
                'e.g. Counter_RelationKey',
                'create_relationship_key_name_placeholder',
              )}
              value={model.keyName}
              disabled={keyGroupDisabled}
              onChange={event => onKeyNameChange(event.target.value)}
            />
          </div>

          <div className={S.field}>
            <label className={S.fieldLabel}>
              {T('Base Entity Field', 'create_relationship_base_field_label')}{' '}
              <span className={S.required}>*</span>
            </label>
            <FilterableSelect
              className={S.filterableSelect}
              options={baseFieldOptions}
              selectedValue={model.baseFieldId}
              disabled={keyGroupDisabled}
              onChange={value => update({ baseFieldId: value ?? '' })}
            />
          </div>

          <div className={S.field}>
            <label className={S.fieldLabel}>
              {T('Related Entity Field', 'create_relationship_related_field_label')}{' '}
              <span className={S.required}>*</span>
            </label>
            <FilterableSelect
              className={S.filterableSelect}
              options={relatedFieldOptions}
              selectedValue={model.relatedFieldId}
              disabled={keyGroupDisabled || loadingRelatedFields}
              onChange={value => update({ relatedFieldId: value ?? '' })}
            />
          </div>
        </fieldset>
      </>
    );

    const renderReview = () => {
      if (!entityData) return null;
      return (
        <>
          <h2 className={S.formTitle}>{T('Ready to create', 'wizard_ready_title')}</h2>
          <p className={S.formSubtitle}>
            {T(
              'This Wizard will create a Field With a Relationship Entity with these parameters:',
              'create_relationship_review_subtitle',
            )}
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>{T('Table', 'create_relationship_table_label')}</div>
              <div>{entityData.baseEntityName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Parent Child', 'create_relationship_review_parent_child')}
              </div>
              <div>
                {model.isParentChild
                  ? T('True', 'create_relationship_review_true')
                  : T('False', 'create_relationship_review_false')}
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Related Entity', 'create_relationship_related_entity_label')}
              </div>
              <div>{relatedEntityName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Lookup Key Name', 'create_relationship_review_lookup_key_name')}
              </div>
              <div>{model.keyName}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Base Entity Field', 'create_relationship_base_field_label')}
              </div>
              <div>{findName(entityData.baseEntityColumns, model.baseFieldId)}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>
                {T('Related Entity Field', 'create_relationship_related_field_label')}
              </div>
              <div>{findName(relatedFields, model.relatedFieldId)}</div>
            </div>
          </div>
        </>
      );
    };

    return (
      <div className={S.drawer} role="dialog" aria-modal="true" ref={wizardRef}>
        <div className={S.header}>
          <div className={S.headerIcon}>R</div>
          <div className={S.headerText}>
            <div className={S.headerTitle}>
              {T('Create Relationship With Key', 'create_relationship_header_title')}
            </div>
            <div className={S.headerSubtitle}>
              {T('in {0}', 'create_relationship_header_subtitle', parentNodeName)}
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
                    disabled={submitting || loading || !isValid}
                  >
                    {submitting
                      ? T('Creating…', 'wizard_btn_creating')
                      : T('Create Relationship', 'create_relationship_btn_create')}
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
