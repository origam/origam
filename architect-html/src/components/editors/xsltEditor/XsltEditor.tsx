/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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
along with ORIGAM. If notL, see <http://www.gnu.org/licenses/>.
*/

import S from "src/components/editors/xsltEditor/XsltEditor.module.scss";
import {
} from "src/components/lazyLoadedTree/LazyLoadedTree.tsx";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "src/stores/store.ts";
import { useEffect, useRef } from "react";
import {
  EditorState, getEditorId,
  updateProperty
} from "src/components/editors/gridEditor/GrirEditorSlice.ts";
import { TabView } from "src/components/tabView/TabView.tsx";
import {
  PropertyEditor
} from "src/components/editors/propertyEditor/PropertyEditor.tsx";
import React from 'react';
import Editor, { EditorProps } from '@monaco-editor/react';
import * as monacoVim from 'monaco-vim';
import {
  useEditorInitialization
} from "src/components/editors/gridEditor/GridEditor.tsx";
import { TreeNode } from "src/components/lazyLoadedTree/LazyLoadedTreeSlice.ts";
import { TabViewState } from "src/components/tabView/TabViewState.ts";

export const XsltEditor = (props: { node: TreeNode }) => {
  const editorId = getEditorId(props.node);
  const dispatch = useDispatch();
  const editorState = useSelector<RootState, EditorState>(state => state.editorStates.editors[editorId]);
  useEditorInitialization(editorState, props.node);

  if (!editorState) {
    return null;
  }

  const handleInputChange = (propertyName: string, value: any) => {
    dispatch(updateProperty({editorId: editorId, propertyName, value}));
  };

  return (
    <div className={S.root}>
      <TabView
        state={new TabViewState()}
        items={[
          {
            label: "XSL",
            node: <CodeEditor
              value={editorState.properties.find(x => x.name === "TextStore")?.value ?? ""}
              onChange={(text) => handleInputChange("TextStore", text)}/>
          },
          {
            label: "Settings",
            node:
              <PropertyEditor
                editorId={editorId}
                properties={editorState.properties
                  .filter(x => x.name !== "TextStore")}/>
          }
        ]}/>
    </div>
  );
}

interface CodeEditorProps {
  value: string;
  onChange: (value: string | undefined) => void;
}

const CodeEditor: React.FC<CodeEditorProps> = ({value, onChange}) => {
  const editorRef = useRef<any>(null);
  const vimStatusBarRef = useRef<HTMLDivElement | null>(null);
  const vimModeRef = useRef<any>(null);

  useEffect(() => {
    return () => {
      if (vimModeRef.current) {
        vimModeRef.current.dispose();
      }
    };
  }, []);

  const initVim = () => {
    if (editorRef.current && vimStatusBarRef.current && !vimModeRef.current) {
      vimModeRef.current = monacoVim.initVimMode(editorRef.current, vimStatusBarRef.current);
    }
  };

  const handleEditorDidMount: EditorProps['onMount'] = (editor) => {
    editorRef.current = editor;
    initVim();
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
          minimap: {enabled: false},
          lineNumbers: 'on',
          scrollBeyondLastLine: false,
          automaticLayout: true,
        }}
      />
      <div ref={vimStatusBarRef} className={S.vimStatus}/>
    </div>
  );
};