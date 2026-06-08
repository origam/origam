/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.
This file is part of ORIGAM (http://www.origam.org).
*/

import S from './CreatedConfirmationDialog.module.scss';
import { ISearchResult } from '@api/IArchitectApi';
import React, { useEffect, useRef } from 'react';
import { VscPass } from 'react-icons/vsc';

interface CreatedConfirmationDialogProps {
  title: string;
  results: ISearchResult[];
  onShowResult: () => void;
  onClose: () => void;
}

function getInitials(type: string | undefined): string {
  if (!type) return '?';
  const words = type
    .split(/[\s_\-]+/)
    .filter(Boolean)
    .filter(w => !/^(the|a|of|for|with|by)$/i.test(w));
  if (words.length >= 2) {
    return (words[0][0] + words[1][0]).toUpperCase();
  }
  if (words.length === 1) {
    return words[0].slice(0, 2).toUpperCase();
  }
  return type.slice(0, 2).toUpperCase();
}

function getItemName(r: ISearchResult): string {
  // Prefer the last segment of foundIn (the leaf name), fall back to whole.
  if (!r.foundIn) return '';
  const parts = r.foundIn.split(/[\\/]/).filter(Boolean);
  return parts[parts.length - 1] ?? r.foundIn;
}

export const CreatedConfirmationDialog: React.FC<CreatedConfirmationDialogProps> = ({
  title,
  results,
  onShowResult,
  onClose,
}) => {
  const showResultRef = useRef<HTMLButtonElement | null>(null);

  useEffect(() => {
    showResultRef.current?.focus();
  }, []);

  return (
    <div className={S.dialog} role="dialog" aria-modal="true">
      <div className={S.header}>
        <div className={S.icon}>
          <VscPass />
        </div>
        <div className={S.headerText}>
          <div className={S.title}>{title}</div>
          <div className={S.subtitle}>
            {results.length} item{results.length === 1 ? '' : 's'} added to the model
          </div>
        </div>
      </div>

      {results.length > 0 && (
        <div className={S.body}>
          <ul className={S.list}>
            {results.slice(0, 6).map(r => (
              <li key={r.schemaId} className={S.listItem}>
                <div className={S.itemBadge}>{getInitials(r.type)}</div>
                <div className={S.itemName}>{getItemName(r)}</div>
                <div className={S.itemType}>{r.type}</div>
              </li>
            ))}
          </ul>
          {results.length > 6 && (
            <div className={S.more}>+ {results.length - 6} more…</div>
          )}
        </div>
      )}

      <div className={S.footer}>
        <button className={S.btn} onClick={onClose}>
          Close
        </button>
        <button
          ref={showResultRef}
          className={`${S.btn} ${S.btnPrimary}`}
          onClick={onShowResult}
        >
          Show result
        </button>
      </div>
    </div>
  );
};
