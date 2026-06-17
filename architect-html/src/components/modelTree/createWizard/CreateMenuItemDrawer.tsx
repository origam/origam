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
import React, { useContext, useEffect, useState } from 'react';
import { RootStoreContext } from '@/main';
import { ICreateWizardResult } from '@api/IArchitectApi';
import { runInFlowWithHandler } from '@errors/runInFlowWithHandler';

interface CreateMenuItemDrawerProps {
  formId: string;
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (result: ICreateWizardResult) => void;
}

const STEPS = [
  { label: 'Basics', hint: 'Caption & role' },
  { label: 'Review', hint: 'Confirm and create' },
];

export const CreateMenuItemDrawer: React.FC<CreateMenuItemDrawerProps> = observer(
  ({ formId, parentNodeName, onCancel, onCreate }) => {
    const rootStore = useContext(RootStoreContext);
    const run = runInFlowWithHandler(rootStore.errorDialogController);

    const [step, setStep] = useState(0);
    const [submitting, setSubmitting] = useState(false);
    const [caption, setCaption] = useState('');
    const [role, setRole] = useState(parentNodeName);

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
    const next = () => setStep(current => Math.min(current + 1, STEPS.length - 1));
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
            <h2 className={S.formTitle}>Configure menu entry</h2>
            <p className={S.formSubtitle}>
              A Form Reference menu item will be added under the Main Menu, pointing to this screen.
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                Caption <span className={S.required}>*</span>
              </label>
              <input
                className={S.input}
                autoFocus
                placeholder={`e.g. ${parentNodeName}`}
                value={caption}
                onChange={event => setCaption(event.target.value)}
              />
            </div>

            <div className={S.field}>
              <label className={S.fieldLabel}>Role</label>
              <input
                className={S.input}
                value={role}
                onChange={event => setRole(event.target.value)}
              />
            </div>

            <div className={S.preview}>
              <div className={S.previewTitle}>What will be created</div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Menu Item</span>
                <span>{caption || '<caption>'}</span>
              </div>
            </div>
          </>
        );
      }

      return (
        <>
          <h2 className={S.formTitle}>Ready to create</h2>
          <p className={S.formSubtitle}>Review what will be added under the Main Menu.</p>

          <div className={S.reviewCard}>
            <div className={S.reviewCardHeader}>
              <div className={S.reviewCardIcon}>M</div>
              <div>
                <div className={S.reviewCardTitle}>{caption || 'Untitled'}</div>
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                  Form Reference Menu Item
                </div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Role</div>
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
            <div className={S.headerTitle}>Create Menu Item</div>
            <div className={S.headerSubtitle}>for {parentNodeName}</div>
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
                    disabled={!canAdvance}
                  >
                    Next →
                  </button>
                ) : (
                  <button
                    className={`${S.btn} ${S.btnPrimary}`}
                    onClick={submit}
                    disabled={submitting}
                  >
                    {submitting ? 'Creating…' : 'Create Menu Item'}
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
