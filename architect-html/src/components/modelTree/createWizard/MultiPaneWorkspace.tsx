/* Multi-pane Workspace prototype — opens as a "new tab" filling the workspace */

import S from './MultiPaneWorkspace.module.scss';
import { observer } from 'mobx-react-lite';
import React, { useState } from 'react';

interface MultiPaneWorkspaceProps {
  parentNodeName: string;
  onCancel: () => void;
  onCreate: () => void;
}

type OutlineKey = 'lookup' | 'screen-section' | 'menu-item';

export const MultiPaneWorkspace: React.FC<MultiPaneWorkspaceProps> = observer(
  ({ parentNodeName, onCancel, onCreate }) => {
    const [name, setName] = useState('BusinessPartner');
    const [dataType, setDataType] = useState('UniqueIdentifier');
    const [structure, setStructure] = useState('BusinessPartner_LookupList');
    const [valueMember, setValueMember] = useState('Id');
    const [displayMember, setDisplayMember] = useState('Name');
    const [selected, setSelected] = useState<OutlineKey>('lookup');

    return (
      <div className={S.workspace} onClick={e => e.stopPropagation()}>
        {/* Tab bar — mimics editor tabs */}
        <div className={S.tabBar}>
          <div className={S.tab}>
            <span className={S.tabIcon}>L</span>
            <span className={S.tabDirty} title="Unsaved">●</span>
            <span>New Lookup: {name}</span>
            <button className={S.tabClose} onClick={onCancel}>✕</button>
          </div>
          <div className={S.tabBarSpacer} />
          <button className={S.tabBarBtn} title="Split view">⊟</button>
          <button className={S.tabBarBtn} title="Settings">⚙</button>
        </div>

        {/* Toolbar with crumbs + status + actions */}
        <div className={S.toolbar}>
          <div className={S.crumbs}>
            <span className={S.crumb}>Root</span>
            <span className={S.crumbSep}>/</span>
            <span className={S.crumb}>{parentNodeName}</span>
            <span className={S.crumbSep}>/</span>
            <span className={`${S.crumb} ${S.current}`}>{name}</span>
            <span className={S.statusBadge}>Draft</span>
          </div>
          <button className={S.toolbarBtn} onClick={onCancel}>
            Discard
          </button>
          <button className={`${S.toolbarBtn} ${S.toolbarBtnPrimary}`} onClick={onCreate}>
            Save all (3)
          </button>
        </div>

        {/* Three panes */}
        <div className={S.panes}>
          {/* Outline pane: structure of what's being created */}
          <div className={S.outlinePane}>
            <div className={S.outlineLabel}>Will be created</div>
            <div
              className={`${S.outlineItem} ${selected === 'lookup' ? S.active : ''}`}
              onClick={() => setSelected('lookup')}
            >
              <span className={`${S.outlineDot} ${S.new}`} />
              <span>{name || 'Untitled'}</span>
              <span style={{ marginLeft: 'auto', fontSize: 10, opacity: 0.6 }}>Lookup</span>
            </div>
            <div
              className={`${S.outlineItem} ${S.nested} ${selected === 'screen-section' ? S.active : ''}`}
              onClick={() => setSelected('screen-section')}
            >
              <span className={`${S.outlineDot} ${S.new}`} />
              <span>{name}_Section</span>
            </div>
            <div
              className={`${S.outlineItem} ${S.nested} ${selected === 'menu-item' ? S.active : ''}`}
              onClick={() => setSelected('menu-item')}
            >
              <span className={`${S.outlineDot} ${S.new}`} />
              <span>{name} (menu)</span>
            </div>

            <button className={S.outlineAddBtn}>+ Add related object</button>

            <div className={S.outlineLabel} style={{ marginTop: 24 }}>
              Will be modified
            </div>
            <div className={S.outlineItem}>
              <span className={`${S.outlineDot} ${S.modified}`} />
              <span style={{ fontSize: 12 }}>Root Menu</span>
            </div>
          </div>

          {/* Main form pane — context depends on outline selection */}
          <div className={S.formPane}>
            {selected === 'lookup' && (
              <>
                <h1 className={S.formTitle}>
                  <span className={S.formIcon}>L</span>
                  {name || 'Untitled lookup'}
                </h1>
                <p className={S.formSubtitle}>Data Lookup · child of {parentNodeName}</p>

                <div className={S.formGroup}>
                  <div className={S.formGroupLabel}>Identification</div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Name</label>
                    <input
                      className={S.input}
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
                    </select>
                  </div>
                </div>

                <div className={S.formGroup}>
                  <div className={S.formGroupLabel}>Data binding</div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Data structure</label>
                    <select
                      className={S.select}
                      value={structure}
                      onChange={e => setStructure(e.target.value)}
                    >
                      <option>BusinessPartner_LookupList</option>
                      <option>Country_LookupList</option>
                      <option>User_LookupList</option>
                    </select>
                  </div>
                  <div className={S.fieldRow}>
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
                  </div>
                </div>
              </>
            )}

            {selected === 'screen-section' && (
              <>
                <h1 className={S.formTitle}>
                  <span className={S.formIcon} style={{ background: 'linear-gradient(135deg, #7c3aed, #ec4899)' }}>
                    S
                  </span>
                  {name}_Section
                </h1>
                <p className={S.formSubtitle}>Screen Section · auto-generated companion</p>
                <div className={S.formGroup}>
                  <div className={S.formGroupLabel}>Layout</div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Section name</label>
                    <input className={S.input} value={`${name}_Section`} disabled />
                  </div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Layout type</label>
                    <select className={S.select} defaultValue="Form">
                      <option>Form</option>
                      <option>Grid</option>
                      <option>Tree</option>
                    </select>
                  </div>
                </div>
              </>
            )}

            {selected === 'menu-item' && (
              <>
                <h1 className={S.formTitle}>
                  <span className={S.formIcon} style={{ background: 'linear-gradient(135deg, #f59e0b, #ef4444)' }}>
                    M
                  </span>
                  {name} (menu)
                </h1>
                <p className={S.formSubtitle}>Menu Item · entry point in main navigation</p>
                <div className={S.formGroup}>
                  <div className={S.formGroupLabel}>Placement</div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Parent menu</label>
                    <select className={S.select} defaultValue="Root Menu">
                      <option>Root Menu</option>
                      <option>Reference Data</option>
                    </select>
                  </div>
                  <div className={S.field}>
                    <label className={S.fieldLabel}>Caption</label>
                    <input className={S.input} defaultValue={name} />
                  </div>
                </div>
              </>
            )}
          </div>

          {/* Right pane: details / preview / XML */}
          <div className={S.previewPane}>
            <div className={S.previewLabel}>Summary</div>
            <div className={S.previewCard}>
              <div className={S.previewCardHeader}>
                <span className={S.formIcon} style={{ width: 22, height: 22, fontSize: 11 }}>
                  L
                </span>
                <span className={S.previewCardTitle}>{name}</span>
              </div>
              <div className={S.kv}>
                <span className={S.kvKey}>Type</span>
                <span className={S.kvVal}>Data Lookup</span>
              </div>
              <div className={S.kv}>
                <span className={S.kvKey}>Value type</span>
                <span className={S.kvVal}>{dataType}</span>
              </div>
              <div className={S.kv}>
                <span className={S.kvKey}>Source</span>
                <span className={S.kvVal}>{structure}</span>
              </div>
              <div className={S.kv}>
                <span className={S.kvKey}>Members</span>
                <span className={S.kvVal}>
                  {valueMember} → {displayMember}
                </span>
              </div>
            </div>

            <div className={S.previewLabel}>XML preview</div>
            <div className={S.xmlBlock}>
              <span className={S.xmlTag}>&lt;dataLookup</span>{'\n'}
              {'  '}<span className={S.xmlAttr}>name</span>=<span className={S.xmlVal}>"{name}"</span>{'\n'}
              {'  '}<span className={S.xmlAttr}>valueType</span>=<span className={S.xmlVal}>"{dataType}"</span>{'\n'}
              {'  '}<span className={S.xmlAttr}>valueMember</span>=<span className={S.xmlVal}>"{valueMember}"</span>{'\n'}
              {'  '}<span className={S.xmlAttr}>displayMember</span>=<span className={S.xmlVal}>"{displayMember}"</span>{'\n'}
              <span className={S.xmlTag}>/&gt;</span>
            </div>
          </div>
        </div>
      </div>
    );
  },
);
