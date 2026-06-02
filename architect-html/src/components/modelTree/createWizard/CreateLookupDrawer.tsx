/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.
This file is part of ORIGAM (http://www.origam.org).
*/

import S from './CreateLookupDrawer.module.scss';
import { observer } from 'mobx-react-lite';
import React, { useState } from 'react';

interface CreateLookupDrawerProps {
  parentNodeName: string;
  onCancel: () => void;
  onCreate: (model: LookupModel) => void;
}

export interface LookupModel {
  name: string;
  dataType: string;
  dataStructure: string;
  valueMember: string;
  displayMember: string;
  autoCreateScreen: boolean;
}

const STEPS = [
  { label: 'Basics', hint: 'Name & data type' },
  { label: 'Source', hint: 'Data structure & members' },
  { label: 'Review', hint: 'Confirm and create' },
];

const DATA_TYPES = ['String', 'Integer', 'UniqueIdentifier', 'Boolean', 'Date', 'Currency'];

const MOCK_STRUCTURES = [
  'BusinessPartner_LookupList',
  'Country_LookupList',
  'Currency_LookupList',
  'User_LookupList',
];

export const CreateLookupDrawer: React.FC<CreateLookupDrawerProps> = observer(
  ({ parentNodeName, onCancel, onCreate }) => {
    const [step, setStep] = useState(0);
    const [model, setModel] = useState<LookupModel>({
      name: '',
      dataType: 'UniqueIdentifier',
      dataStructure: '',
      valueMember: 'Id',
      displayMember: 'Name',
      autoCreateScreen: true,
    });

    const update = (patch: Partial<LookupModel>) => setModel(m => ({ ...m, ...patch }));

    const canAdvance =
      (step === 0 && model.name.trim().length > 0) ||
      (step === 1 && model.dataStructure.length > 0) ||
      step === 2;

    const next = () => setStep(s => Math.min(s + 1, STEPS.length - 1));
    const back = () => setStep(s => Math.max(s - 1, 0));

    const renderStep = () => {
      if (step === 0) {
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
                <span className={S.fieldHint}>— used everywhere this lookup is referenced</span>
              </label>
              <input
                className={S.input}
                autoFocus
                placeholder="e.g. BusinessPartner"
                value={model.name}
                onChange={e => update({ name: e.target.value })}
              />
            </div>

            <div className={S.fieldRow}>
              <div className={S.field}>
                <label className={S.fieldLabel}>Value data type</label>
                <select
                  className={S.select}
                  value={model.dataType}
                  onChange={e => update({ dataType: e.target.value })}
                >
                  {DATA_TYPES.map(t => (
                    <option key={t}>{t}</option>
                  ))}
                </select>
              </div>
              <div className={S.field}>
                <label className={S.fieldLabel}>Created in</label>
                <input className={S.input} value={parentNodeName} disabled />
              </div>
            </div>

            <div className={S.preview}>
              <div className={S.previewTitle}>What will be created</div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Lookup</span>
                <span>{model.name || '<name>'}</span>
              </div>
            </div>
          </>
        );
      }

      if (step === 1) {
        return (
          <>
            <h2 className={S.formTitle}>Where does the data come from?</h2>
            <p className={S.formSubtitle}>
              Pick a data structure to read from, and choose which columns are the value and the
              display.
            </p>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                Data structure <span className={S.required}>*</span>
              </label>
              <select
                className={S.select}
                value={model.dataStructure}
                onChange={e => update({ dataStructure: e.target.value })}
              >
                <option value="">— select —</option>
                {MOCK_STRUCTURES.map(s => (
                  <option key={s}>{s}</option>
                ))}
              </select>
            </div>

            <div className={S.fieldRow}>
              <div className={S.field}>
                <label className={S.fieldLabel}>Value member</label>
                <input
                  className={S.input}
                  value={model.valueMember}
                  onChange={e => update({ valueMember: e.target.value })}
                />
              </div>
              <div className={S.field}>
                <label className={S.fieldLabel}>Display member</label>
                <input
                  className={S.input}
                  value={model.displayMember}
                  onChange={e => update({ displayMember: e.target.value })}
                />
              </div>
            </div>

            <div className={S.field}>
              <label className={S.fieldLabel}>
                <input
                  type="checkbox"
                  checked={model.autoCreateScreen}
                  onChange={e => update({ autoCreateScreen: e.target.checked })}
                  style={{ marginRight: 6 }}
                />
                Also create a default Screen Section for this lookup
              </label>
            </div>

            <div className={S.preview}>
              <div className={S.previewTitle}>What will be created</div>
              <div className={S.previewItem}>
                <span className={S.previewBadge}>Lookup</span>
                <span>{model.name}</span>
              </div>
              {model.dataStructure && (
                <div className={S.previewItem}>
                  <span className={S.previewBadge}>Binds to</span>
                  <span>
                    {model.dataStructure} · {model.valueMember} → {model.displayMember}
                  </span>
                </div>
              )}
              {model.autoCreateScreen && (
                <div className={S.previewItem}>
                  <span className={S.previewBadge}>Screen Section</span>
                  <span>{model.name}_Section</span>
                </div>
              )}
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
                <div style={{ fontSize: 12, color: 'var(--background6)' }}>Data Lookup</div>
              </div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Data type</div>
              <div>{model.dataType}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Data structure</div>
              <div>{model.dataStructure}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Value member</div>
              <div>{model.valueMember}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Display member</div>
              <div>{model.displayMember}</div>
            </div>
            <div className={S.reviewKv}>
              <div className={S.reviewKey}>Parent</div>
              <div>{parentNodeName}</div>
            </div>
          </div>

          {model.autoCreateScreen && (
            <div className={S.reviewCard}>
              <div className={S.reviewCardHeader}>
                <div className={S.reviewCardIcon} style={{ background: '#7c3aed' }}>
                  S
                </div>
                <div>
                  <div className={S.reviewCardTitle}>{model.name}_Section</div>
                  <div style={{ fontSize: 12, color: 'var(--background6)' }}>
                    Screen Section (auto-generated)
                  </div>
                </div>
              </div>
            </div>
          )}
        </>
      );
    };

    return (
      <>
        <div className={S.drawer} role="dialog" aria-modal="true">
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
                    <button className={S.btn} onClick={back}>
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
                      onClick={() => onCreate(model)}
                    >
                      Create Lookup
                    </button>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      </>
    );
  },
);
