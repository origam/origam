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
import React, { useContext, useEffect, useState } from 'react';
import { RootStoreContext, T } from '@/main';
import { ICreateWizardResult } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';

interface CreateMenuItemWizardProps {
  formId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateWizardResult) => void;
}

export const CreateMenuItemWizard: React.FC<CreateMenuItemWizardProps> = observer(
  ({ formId, parentNodeName, onCancel, onCreate }) => {
    const rootStore = useContext(RootStoreContext);
    const run = runInFlowWithHandler(rootStore.errorDialogController);

    const [step, setStep] = useState(0);
    const [submitting, setSubmitting] = useState(false);
    const [caption, setCaption] = useState('');
    const [role, setRole] = useState(parentNodeName);

    const steps = [
      {
        id: 'basics',
        label: T('Basics', 'create_menu_item_step_basics_label'),
        hint: T('Caption & role', 'create_menu_item_step_basics_hint'),
      },
      {
        id: 'review',
        label: T('Review', 'wizard_step_review_label'),
        hint: T('Confirm and create', 'wizard_step_review_hint'),
      },
    ];

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

    const canAdvance = (step === 0 && caption.trim().length > 0) || step === 1;
    const next = () => setStep(current => Math.min(current + 1, steps.length - 1));
    const back = () => setStep(current => Math.max(current - 1, 0));

    const submit = () => {
      if (submitting) return;
      setSubmitting(true);
      run({
        generator: function* () {
          try {
            const result = (yield rootStore.architectApi.createMenuItem({
              formId,
              caption: caption.trim(),
              role: role.trim() || '*',
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
              {T('Configure menu entry', 'create_menu_item_basics_title')}
            </h2>
            <p className={S.formSubtitle}>
              {T(
                'A Form Reference menu item will be added under the Main Menu, pointing to this screen.',
                'create_menu_item_basics_subtitle',
              )}
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                {T('Caption', 'create_menu_item_caption_label')}{' '}
                <span className={S.required}>*</span>
              </label>
              <input
                className={S.input}
                autoFocus
                placeholder={T('e.g. {0}', 'wizard_placeholder_example', parentNodeName)}
                value={caption}
                onChange={event => setCaption(event.target.value)}
              />
            </div>

            <div className={S.field}>
              <label className={S.fieldLabel}>{T('Role', 'create_menu_item_role_label')}</label>
              <input
                className={S.input}
                value={role}
                onChange={event => setRole(event.target.value)}
              />
            </div>

            <div className={S.preview}>
              <div className={S.previewTitle}>
                {T('What will be created', 'wizard_what_created')}
              </div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>
                  {T('Menu Item', 'wizard_artifact_menu_item')}
                </span>
                <span>{caption || '<caption>'}</span>
              </div>
            </div>
          </>
        );
      }

      return (
        <>
          <h2 className={S.formTitle}>{T('Ready to create', 'wizard_ready_title')}</h2>
          <p className={S.formSubtitle}>
            {T(
              'Review what will be added under the Main Menu.',
              'create_menu_item_review_subtitle',
            )}
          </p>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>M</div>
              <div>
                <div className={S.reviewCardTitle}>
                  {caption || T('Untitled', 'wizard_untitled')}
                </div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  {T('Form Reference Menu Item', 'create_menu_item_review_type')}
                </div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>{T('Role', 'create_menu_item_role_label')}</div>
              <div>{role.trim() || '*'}</div>
            </div>
          </div>
        </>
      );
    };

    return (
      <div className={S.drawer} role="dialog" aria-modal="true">
        <div className={S.header}>
          <div className={S.headerIcon}>M</div>
          <div className={S.headerText}>
            <div className={S.headerTitle}>
              {T('Create Menu Item', 'create_menu_item_header_title')}
            </div>
            <div className={S.headerSubtitle}>
              {T('for {0}', 'create_menu_item_header_subtitle', parentNodeName)}
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
                    disabled={!canAdvance}
                  >
                    {T('Next →', 'wizard_btn_next')}
                  </button>
                ) : (
                  <button
                    className={`${S.btn} ${S.btnPrimary}`}
                    onClick={submit}
                    disabled={submitting}
                  >
                    {submitting
                      ? T('Creating…', 'wizard_btn_creating')
                      : T('Create Menu Item', 'create_menu_item_btn_create')}
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
