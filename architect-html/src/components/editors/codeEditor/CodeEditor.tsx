/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

import Editor, { EditorProps } from '@monaco-editor/react';
import { observer } from 'mobx-react-lite';
import { useCallback, useContext, useEffect, useRef } from 'react';
import S from 'src/components/editors/codeEditor/CodeEditor.module.scss';
// @ts-expect-error types for monaco-vim are missing
import * as monacoVim from 'monaco-vim';
import { RootStoreContext } from 'src/main';

export default observer(function CodeEditor({
  value,
  onChange,
  defaultLanguage = 'xml',
}: {
  value: string;
  onChange: (value: string | undefined) => void;
  defaultLanguage: string;
}) {
  const editorRef = useRef<any>(null);
  const vimStatusBarRef = useRef<HTMLDivElement | null>(null);
  const vimModeRef = useRef<any>(null);

  const rootStore = useContext(RootStoreContext);
  const uiState = rootStore.uiState;

  const initVim = useCallback(() => {
    if (
      uiState.settings.isVimEnabled &&
      editorRef.current &&
      vimStatusBarRef.current &&
      !vimModeRef.current
    ) {
      vimModeRef.current = monacoVim.initVimMode(editorRef.current, vimStatusBarRef.current);
    }
  }, [uiState.settings.isVimEnabled]);

  useEffect(() => {
    return () => {
      if (vimModeRef.current) {
        vimModeRef.current.dispose();
      }
    };
  }, []);

  useEffect(() => {
    if (uiState.settings.isVimEnabled) {
      initVim();
    } else {
      if (vimModeRef.current) {
        vimModeRef.current.dispose();
        vimModeRef.current = null;
      }
    }

    // Force layout recalculation after VIM mode changed
    if (editorRef.current) {
      requestAnimationFrame(() => {
        if (editorRef.current) {
          editorRef.current.layout();
        }
      });
    }
  }, [uiState.settings.isVimEnabled, initVim]);

  const handleEditorDidMount: EditorProps['onMount'] = editor => {
    editorRef.current = editor;
    initVim();
  };

  const handleEditorChange = (value: string | undefined) => {
    onChange(value);
  };

  return (
    <div className={S.root}>
      <div className={S.editorContainer}>
        <Editor
          height="100%"
          defaultLanguage={defaultLanguage}
          value={value}
          onChange={handleEditorChange}
          onMount={handleEditorDidMount}
          options={{
            minimap: { enabled: false },
            lineNumbers: 'on',
            scrollBeyondLastLine: false,
            automaticLayout: true,
          }}
        />
      </div>
      {uiState.settings.isVimEnabled && <div ref={vimStatusBarRef} className={S.vimStatus} />}
    </div>
  );
});
