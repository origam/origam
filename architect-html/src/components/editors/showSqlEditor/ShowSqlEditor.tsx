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

import ActionPanel from '@/components/ActionPanel/ActionPanel';
import { T } from '@/main';
import Button from '@components/Button/Button.tsx';
import CodeEditor from '@editors/codeEditor/CodeEditor';
import { ShowSqlEditorState } from '@editors/showSqlEditor/ShowSqlEditorState';
import S from '@editors/showSqlEditor/ShowSqlEditor.module.scss';
import { observer } from 'mobx-react-lite';
import { useCallback, useState } from 'react';
import { VscCopy } from 'react-icons/vsc';

const COPIED_FEEDBACK_MS = 800;

const ShowSqlEditor = observer(({ editorState }: { editorState: ShowSqlEditorState }) => {
  const [copied, setCopied] = useState(false);

  const handleCopy = useCallback(async () => {
    try {
      await navigator.clipboard.writeText(editorState.sql);
      setCopied(true);
      window.setTimeout(() => setCopied(false), COPIED_FEEDBACK_MS);
    } catch {
      /* clipboard unavailable */
    }
  }, [editorState.sql]);

  return (
    <div className={S.root}>
      <div className={S.editorBox}>
        <ActionPanel
          title={T('SQL: {0}', 'show_sql_editor_title', editorState.dataStructureName)}
          buttons={
            <Button
              type="secondary"
              title={copied ? T('Copied!', 'show_sql_copied') : T('Copy', 'show_sql_copy')}
              prefix={<VscCopy />}
              onClick={handleCopy}
            />
          }
        />
        <CodeEditor defaultLanguage="sql" value={editorState.sql} readOnly={true} />
      </div>
    </div>
  );
});

export default ShowSqlEditor;
