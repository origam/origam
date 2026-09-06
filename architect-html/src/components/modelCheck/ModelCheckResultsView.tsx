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

import { T } from '@/main';
import { IModelFileError, IModelFileErrorSection } from '@api/IArchitectApi';
import { ModelCheckResultsTabState } from '@components/modelCheck/ModelCheckResultsTabState';
import S from '@components/modelCheck/ModelCheckResultsView.module.scss';
import SchemaItemResultsTable from '@components/schemaItemResults/SchemaItemResultsTable';
import { observer } from 'mobx-react-lite';
import { ReactNode, useState } from 'react';
import {
  VscChevronDown,
  VscChevronRight,
  VscError,
  VscFile,
  VscPass,
  VscWarning,
} from 'react-icons/vsc';

const guidPattern = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

function formatRunTime(lastRunAt: string) {
  return new Date(lastRunAt).toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  });
}

function separatorIndex(path: string) {
  return Math.max(path.lastIndexOf('\\'), path.lastIndexOf('/'));
}

function fileNameOf(path: string) {
  const index = separatorIndex(path);
  return index < 0 ? path : path.slice(index + 1);
}

function directoryOf(path: string) {
  const index = separatorIndex(path);
  return index < 0 ? '' : path.slice(0, index + 1);
}

// Ids get their own chip instead of cluttering the sentence.
function withIdChips(text: string, keyPrefix: string): ReactNode[] {
  const parts: ReactNode[] = [];
  let lastEnd = 0;
  for (const match of text.matchAll(guidPattern)) {
    const start = match.index ?? 0;
    if (start > lastEnd) {
      parts.push(text.slice(lastEnd, start));
    }
    parts.push(
      <span key={`${keyPrefix}-id-${start}`} className={S.idChip}>
        {match[0]}
      </span>,
    );
    lastEnd = start + match[0].length;
  }
  if (lastEnd < text.length) {
    parts.push(text.slice(lastEnd));
  }
  return parts;
}

const PathLine = ({ path }: { path: string }) => (
  <div className={S.pathLine} title={path}>
    <VscFile className={S.pathIcon} />
    <span className={S.pathText}>
      <span className={S.pathDirectory}>{directoryOf(path)}</span>
      <span className={S.pathFileName}>{fileNameOf(path)}</span>
    </span>
  </div>
);

// Keep only the file name in the sentence; the full path is shown below.
const FileProblem = ({ error, itemKey }: { error: IModelFileError; itemKey: string }) => {
  const link = error.link ?? '';
  const linkStart = link ? error.text.indexOf(link) : -1;
  const isPathOnly = link !== '' && error.text.trim() === link.trim();

  return (
    <li className={S.problem}>
      <VscWarning className={S.problemIcon} />
      <div className={S.problemBody}>
        {!isPathOnly && (
          <div className={S.problemText}>
            {linkStart < 0 ? (
              withIdChips(error.text, itemKey)
            ) : (
              <>
                {withIdChips(error.text.slice(0, linkStart), `${itemKey}-before`)}
                <span className={S.fileChip} title={link}>
                  {fileNameOf(link)}
                </span>
                {withIdChips(error.text.slice(linkStart + link.length), `${itemKey}-after`)}
              </>
            )}
          </div>
        )}
        {linkStart >= 0 && <PathLine path={link} />}
      </div>
    </li>
  );
};

const FileErrorSection = ({ section }: { section: IModelFileErrorSection }) => {
  const [isExpanded, setExpanded] = useState(true);
  return (
    <div className={S.card} data-test-id="model-check-file-section">
      <button
        type="button"
        className={S.cardHeader}
        onClick={() => setExpanded(expanded => !expanded)}
      >
        {isExpanded ? <VscChevronDown /> : <VscChevronRight />}
        <span className={S.cardTitle}>{section.caption}</span>
        <span className={S.cardCount}>{section.errors.length}</span>
      </button>
      {isExpanded && (
        <ul className={S.problemList}>
          {section.errors.map((fileError, index) => (
            <FileProblem
              key={`${section.caption}-${index}`}
              error={fileError}
              itemKey={`${section.caption}-${index}`}
            />
          ))}
        </ul>
      )}
    </div>
  );
};

const ModelCheckResultsView = observer(
  ({ editorState }: { editorState: ModelCheckResultsTabState }) => {
    const result = editorState.result;
    const ruleErrors = result.ruleErrors ?? [];
    const fileErrorSections = (result.fileErrorSections ?? []).filter(
      section => section.errors.length > 0,
    );
    const fileErrorCount = fileErrorSections.reduce(
      (count, section) => count + section.errors.length,
      0,
    );
    const hasProblems = ruleErrors.length > 0 || fileErrorCount > 0;

    const ruleErrorRows = ruleErrors.map((error, index) => ({
      key: `${error.item.schemaId}-${index}`,
      result: error.item,
      extraCell: <span className={S.message}>{error.message}</span>,
    }));

    return (
      <div className={S.root} data-test-id="model-check-results">
        <div className={S.header}>
          <div className={S.summary} data-test-id="model-check-summary">
            {hasProblems ? (
              <>
                <span className={ruleErrors.length > 0 ? S.pillError : S.pillOk}>
                  {ruleErrors.length > 0 ? <VscError /> : <VscPass />}
                  {T('{0} rule violations', 'modelcheck_summary_rules', ruleErrors.length)}
                </span>
                <span className={fileErrorCount > 0 ? S.pillError : S.pillOk}>
                  {fileErrorCount > 0 ? <VscError /> : <VscPass />}
                  {T('{0} file problems', 'modelcheck_summary_files', fileErrorCount)}
                </span>
              </>
            ) : (
              <span className={S.pillOk}>
                <VscPass />
                {T('No problems found', 'modelcheck_summary_clean')}
              </span>
            )}
          </div>
          {result.lastRunAt && (
            <span className={S.runAt}>
              {T(
                'Last check {0}',
                'modelcheck_results_checked_at',
                formatRunTime(result.lastRunAt),
              )}
            </span>
          )}
        </div>

        <div className={S.content}>
          {!hasProblems && (
            <div className={S.empty} data-test-id="model-check-empty">
              <VscPass className={S.emptyIcon} />
              <span className={S.emptyTitle}>
                {T('The model passed all checks.', 'modelcheck_results_empty_hint')}
              </span>
            </div>
          )}

          {ruleErrors.length > 0 && (
            <div className={S.section}>
              <div className={S.sectionHeader}>
                <VscError className={S.sectionIcon} />
                <span className={S.sectionTitle}>
                  {T('Model rule violations', 'modelcheck_section_rules')}
                </span>
                <span className={S.count}>{ruleErrors.length}</span>
              </div>
              <div className={S.card}>
                <SchemaItemResultsTable
                  rows={ruleErrorRows}
                  extraColumn={{
                    label: T('Message', 'modelcheck_column_message'),
                    position: 'first',
                  }}
                  emptyText={T('No problems found.', 'modelcheck_results_empty')}
                />
              </div>
            </div>
          )}

          {fileErrorSections.length > 0 && (
            <div className={S.section}>
              <div className={S.sectionHeader}>
                <VscError className={S.sectionIcon} />
                <span className={S.sectionTitle}>
                  {T('File problems', 'modelcheck_section_files')}
                </span>
                <span className={S.count}>{fileErrorCount}</span>
              </div>
              {fileErrorSections.map(section => (
                <FileErrorSection key={section.caption} section={section} />
              ))}
            </div>
          )}
        </div>
      </div>
    );
  },
);

export default ModelCheckResultsView;
