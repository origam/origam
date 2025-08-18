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

import S from 'src/components/editors/xsltEditor/XsltEditor.module.scss';
import { useContext, useRef } from 'react';
import { TabView } from 'src/components/tabView/TabView.tsx';
import { PropertyEditor } from 'src/components/editors/propertyEditor/PropertyEditor.tsx';
import React from 'react';
import Editor, { EditorProps } from '@monaco-editor/react';
// // @ts-expect-error types for monaco-vim are missing
// import * as monacoVim from 'monaco-vim';
import { TabViewState } from 'src/components/tabView/TabViewState.ts';

import { GridEditorState } from 'src/components/editors/gridEditor/GridEditorState.ts';
import { runInFlowWithHandler } from 'src/errorHandling/runInFlowWithHandler.ts';
import { RootStoreContext, T } from 'src/main.tsx';

export const XsltEditor = (props: { editorState: GridEditorState }) => {
  const rootStore = useContext(RootStoreContext);

  const handleInputChange = (value: any) => {
    const textProperty = props.editorState.properties.find(x => x.name === 'TextStore')!;
    runInFlowWithHandler(rootStore.errorDialogController)({
      generator: function* () {
        yield* props.editorState.onPropertyUpdated(textProperty, value);
      },
    });
  };

  return (
    <div className={S.root}>
      <TabView
        width={400}
        state={new TabViewState()}
        items={[
          {
            label: T('XSL', 'xsl_editor_tab1'),
            node: (
              <CodeEditor
                value={props.editorState.properties.find(x => x.name === 'TextStore')?.value ?? ''}
                onChange={text => handleInputChange(text)}
              />
            ),
          },
          {
            label: T('Settings', 'xsl_editor_tab2'),
            node: (
              <PropertyEditor
                propertyManager={props.editorState}
                properties={props.editorState.properties.filter(x => x.name !== 'TextStore')}
              />
            ),
          },
        ]}
      />
    </div>
  );
};

interface ICodeEditorProps {
  value: string;
  onChange: (value: string | undefined) => void;
}

const CodeEditor: React.FC<ICodeEditorProps> = ({ value, onChange }) => {
  const editorRef = useRef<any>(null);
  const vimStatusBarRef = useRef<HTMLDivElement | null>(null);
  // const vimModeRef = useRef<any>(null);

  // useEffect(() => {
  //   return () => {
  //     if (vimModeRef.current) {
  //       vimModeRef.current.dispose();
  //     }
  //   };
  // }, []);

  // const initVim = () => {
  //   if (editorRef.current && vimStatusBarRef.current && !vimModeRef.current) {
  //     vimModeRef.current = monacoVim.initVimMode(editorRef.current, vimStatusBarRef.current);
  //   }
  // };

  const handleEditorDidMount: EditorProps['onMount'] = editor => {
    editorRef.current = editor;
    // initVim();
  };

  const handleEditorChange = (value: string | undefined) => {
    onChange(value);
  };

  return (
    <div className={S.codeEditor}>
      <Editor
        height="100%"
        defaultLanguage="xml"
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
      <div ref={vimStatusBarRef} className={S.vimStatus} />
    </div>
  );
};
