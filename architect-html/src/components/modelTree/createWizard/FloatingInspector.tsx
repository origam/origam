/* Inspector Editor — full-page "create = edit" prototype */

import S from './FloatingInspector.module.scss';
import { observer } from 'mobx-react-lite';
import React, { useState } from 'react';

interface FloatingInspectorProps {
  parentNodeName: string;
  onCancel: () => void;
  onCreate: () => void;
}

export const FloatingInspector: React.FC<FloatingInspectorProps> = observer(
  ({ parentNodeName, onCancel, onCreate }) => {
    const [name, setName] = useState('NewLookup1');
    const [dataType, setDataType] = useState('UniqueIdentifier');
    const [valueMember, setValueMember] = useState('Id');
    const [displayMember, setDisplayMember] = useState('Name');
    const [structure, setStructure] = useState('');
    const [filter, setFilter] = useState('');
    const [isCached, setIsCached] = useState(false);
    const [autoSort, setAutoSort] = useState(true);

    const [openIdent, setOpenIdent] = useState(true);
    const [openBinding, setOpenBinding] = useState(true);
    const [openBehavior, setOpenBehavior] = useState(true);

    const canSave = name.trim().length > 0 && structure.length > 0;

    return (
      <div className={S.page}>
        {/* Top action bar */}
        <div className={S.actionBar}>
          <div className={S.titleIcon}>L</div>
          <div className={S.titleBlock}>
            <div className={S.titleRow}>
              <span className={S.title}>{name || 'Untitled Lookup'}</span>
              <span className={S.draftBadge}>Draft</span>
            </div>
            <div className={S.subtitle}>Lookup · in {parentNodeName}</div>
          </div>
          <div className={S.actions}>
            <button className={`${S.btn} ${S.btnGhost}`} onClick={onCancel}>
              Discard
            </button>
            <button
              className={`${S.btn} ${S.btnPrimary}`}
              disabled={!canSave}
              onClick={onCreate}
            >
              Save
            </button>
          </div>
          <button className={S.closeBtn} onClick={onCancel} aria-label="Close">
            ✕
          </button>
        </div>

        {/* Scrollable centered content */}
        <div className={S.body}>
          <div className={S.content}>
            <div className={S.section}>
              <div className={S.sectionHeader} onClick={() => setOpenIdent(o => !o)}>
                <span className={S.sectionChevron}>{openIdent ? '▼' : '▶'}</span>
                <span className={S.sectionTitle}>Identification</span>
              </div>
              {openIdent && (
                <div className={S.sectionBody}>
                  <div className={S.field}>
                    <label className={`${S.fieldLabel} ${S.fieldLabelRequired}`}>Name</label>
                    <input
                      className={S.input}
                      autoFocus
                      value={name}
                      onChange={e => setName(e.target.value)}
                    />
                  </div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Data type</label>
                    <select
                      className={S.select}
                      value={dataType}
                      onChange={e => setDataType(e.target.value)}
                    >
                      <option>String</option>
                      <option>Integer</option>
                      <option>UniqueIdentifier</option>
                      <option>Boolean</option>
                      <option>Date</option>
                    </select>
                  </div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Package</label>
                    <input className={S.input} value={parentNodeName} disabled />
                  </div>
                </div>
              )}
            </div>

            <div className={S.section}>
              <div className={S.sectionHeader} onClick={() => setOpenBinding(o => !o)}>
                <span className={S.sectionChevron}>{openBinding ? '▼' : '▶'}</span>
                <span className={S.sectionTitle}>Data binding</span>
              </div>
              {openBinding && (
                <div className={S.sectionBody}>
                  <div className={S.field}>
                    <label className={`${S.fieldLabel} ${S.fieldLabelRequired}`}>
                      Data structure
                    </label>
                    <select
                      className={S.select}
                      value={structure}
                      onChange={e => setStructure(e.target.value)}
                    >
                      <option value="">— pick —</option>
                      <option>BusinessPartner_LookupList</option>
                      <option>Country_LookupList</option>
                      <option>User_LookupList</option>
                    </select>
                  </div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Value member</label>
                    <input
                      className={S.input}
                      value={valueMember}
                      onChange={e => setValueMember(e.target.value)}
                    />
                  </div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Display member</label>
                    <input
                      className={S.input}
                      value={displayMember}
                      onChange={e => setDisplayMember(e.target.value)}
                    />
                  </div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Filter</label>
                    <input
                      className={S.input}
                      placeholder="Optional WHERE clause"
                      value={filter}
                      onChange={e => setFilter(e.target.value)}
                    />
                  </div>
                </div>
              )}
            </div>

            <div className={S.section}>
              <div className={S.sectionHeader} onClick={() => setOpenBehavior(o => !o)}>
                <span className={S.sectionChevron}>{openBehavior ? '▼' : '▶'}</span>
                <span className={S.sectionTitle}>Behavior</span>
              </div>
              {openBehavior && (
                <div className={S.sectionBody}>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Cache results</label>
                    <input
                      className={S.checkbox}
                      type="checkbox"
                      checked={isCached}
                      onChange={e => setIsCached(e.target.checked)}
                    />
                  </div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Auto-sort</label>
                    <input
                      className={S.checkbox}
                      type="checkbox"
                      checked={autoSort}
                      onChange={e => setAutoSort(e.target.checked)}
                    />
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    );
  },
);
