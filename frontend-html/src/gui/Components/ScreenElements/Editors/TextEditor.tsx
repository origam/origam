/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import { action } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { useCallback, useEffect, useState } from "react";
import S from "./TextEditor.module.scss";
import { IFocusable } from "../../../../model/entities/FormFocusManager";

import { ContentState, convertToRaw, EditorState } from "draft-js";
import { Editor } from "react-draft-wysiwyg";
import draftToHtml from "draftjs-to-html";
import htmlToDraft from "html-to-draftjs";

import "react-draft-wysiwyg/dist/react-draft-wysiwyg.css";

@observer
export class TextEditor extends React.Component<{
  id?: string;
  value: string | null;
  isMultiline?: boolean;
  isReadOnly: boolean;
  isPassword?: boolean;
  isInvalid: boolean;
  invalidMessage?: string;
  backgroundColor?: string;
  foregroundColor?: string;
  maxLength?: number;
  isRichText: boolean;
  customStyle?: any;
  wrapText: boolean;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onChange?(event: any, value: string): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onDoubleClick?(event: any): void;
  onEditorBlur?(event: any): void;
  onTextOverflowChanged?: (toolTip: string | null | undefined) => void;
}> {
  disposers: any[] = [];
  currentValue = this.props.value;
  lastAutoUpdatedValue = this.props.value;
  updateInterval: NodeJS.Timeout | undefined;

  componentDidMount() {
    this.updateTextOverflowState();
  }

  componentDidUpdate() {
    this.updateTextOverflowState();
  }

  private updateTextOverflowState() {
    if (this.props.isMultiline) {
      return;
    }
    const textOverflow = this.elmInput.offsetWidth < this.elmInput.scrollWidth
    this.props.onTextOverflowChanged?.(textOverflow ? this.props.value : undefined);
  }

  componentWillUnmount() {
    this.disposers.forEach((d) => d());
  }

  @action.bound
  handleFocus(event: any) {
    if (this.elmInput) {
      const isNotMemoField = this.props.maxLength && this.props.maxLength > 0;
      if (isNotMemoField) {
        this.elmInput.select();
      }
      this.elmInput.scrollLeft = 0;
    }
  }

  elmInput: any = null;
  refInput = (elm: any) => {
    this.elmInput = elm;
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
  };

  getStyle() {
    if (this.props.customStyle) {
      return this.props.customStyle;
    } else {
      return {
        color: this.props.foregroundColor,
        backgroundColor: this.props.backgroundColor,
      };
    }
  }

  render() {
    return (
      <div className={S.editorContainer}>
        {this.renderValueTag()}
        {this.props.isInvalid && (
          <div className={S.notification} title={this.props.invalidMessage}>
            <i className="fas fa-exclamation-circle red"/>
          </div>
        )}
      </div>
    );
  }

  getMultilineDivClass() {
    if (this.props.wrapText) {
      return S.input + " " + S.wrapText;
    }
    return S.input + " " + (isMultiLine(this.props.value) ? S.scrollY : S.noScrollY);
  }

  private renderValueTag() {
    const maxLength = this.props.maxLength === 0 ? undefined : this.props.maxLength;
    if (this.props.isRichText) {
      if (this.props.isReadOnly) {
        return (
          <div className={S.editorContainer}>
            <div
              id={this.props.id}
              style={this.getStyle()}
              className={S.input}
              dangerouslySetInnerHTML={{__html: this.props.value ?? ""}}
              onKeyDown={this.props.onKeyDown}
              onClick={this.props.onClick}
              onDoubleClick={this.props.onDoubleClick}
              onBlur={this.props.onEditorBlur}
              onFocus={this.handleFocus}
            />
          </div>
        );
      } else {
        return (
          <RichTextEditor
            value={this.props.value ?? ""}
            onChange={(newValue: any) => {
              this.currentValue = newValue;
              this.props.onChange?.(undefined, newValue);
            }}
            refInput={this.refInput}
            onBlur={this.props.onEditorBlur}
            onFocus={this.handleFocus}
          />
        );
      }
    }
    if (!this.props.isMultiline) {
      return (
        <input
          id={this.props.id}
          style={this.getStyle()}
          className={S.input}
          type={this.props.isPassword ? "password" : "text"}
          autoComplete={this.props.isPassword ? "new-password" : undefined}
          value={this.props.value || ""}
          readOnly={this.props.isReadOnly}
          maxLength={maxLength}
          ref={this.refInput}
          onChange={(event: any) => {
            this.props.onChange && this.props.onChange(event, event.target.value)
            this.updateTextOverflowState();
          }
          }
          onKeyDown={this.props.onKeyDown}
          onClick={this.props.onClick}
          onDoubleClick={this.props.onDoubleClick}
          onBlur={this.props.onEditorBlur}
          onFocus={this.handleFocus}
        />
      );
    }
    if (this.props.isReadOnly) {
      return (
        <div
          id={this.props.id}
          className={this.getMultilineDivClass()}
          onClick={this.props.onClick}
          onDoubleClick={this.props.onDoubleClick}
          onBlur={this.props.onEditorBlur}
          onFocus={this.handleFocus}
        >
          <span style={this.getStyle()} className={S.multiLine}>
            {this.props.value || ""}
          </span>
        </div>
      );
    } else {
      return (
        <textarea
          id={this.props.id}
          style={this.getStyle()}
          className={S.input}
          value={this.props.value || ""}
          readOnly={this.props.isReadOnly}
          ref={this.refInput}
          maxLength={maxLength}
          onChange={(event: any) => {
            this.currentValue = event.target.value;
            this.props.onChange && this.props.onChange(event, event.target.value);
          }}
          onKeyDown={this.props.onKeyDown}
          onDoubleClick={this.props.onDoubleClick}
          onClick={this.props.onClick}
          onBlur={this.props.onEditorBlur}
          onFocus={this.handleFocus}
        />
      );
    }
  }
}

function isMultiLine(text: string | null) {
  if (text === null || text === undefined) {
    return false;
  }
  return text.includes("\n") || text.includes("\r");
}

function RichTextEditor(props: {
  value: any;
  onChange?: (newValue: any) => void;
  onBlur?: (event: any) => void;
  onFocus?: (event: any) => void;
  refInput?: (elm: any) => void;
}) {
  const [internalEditorState, setInternalEditorState] = useState(() => EditorState.createEmpty());
  const [internalEditorStateHtml, setInternalEditorStateHtml] = useState("");


  const onEditorStateChange = useCallback(
    (newEditorState: any) => {
      setInternalEditorState(newEditorState);
      const html = draftToHtml(convertToRaw(newEditorState.getCurrentContent()));
      setInternalEditorStateHtml(html);
      props.onChange?.(html);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [setInternalEditorState, setInternalEditorStateHtml, props.onChange]
  );

  useEffect(() => {
    if (props.refInput) {
      props.refInput(document.querySelector("[role='textbox']"));
    }
  });

  useEffect(() => {
    if (props.value !== internalEditorStateHtml) {
      const blocksFromHtml = htmlToDraft(props.value);
      const {contentBlocks, entityMap} = blocksFromHtml;
      const contentState = ContentState.createFromBlockArray(contentBlocks, entityMap);
      const editorState = EditorState.createWithContent(contentState);
      setInternalEditorStateHtml(props.value);
      setInternalEditorState(editorState);
    }
  }, [props.value, internalEditorStateHtml]);

  return (
    <div style={{overflow: "auto", width: "100%", height: "100%"}}>
      <div style={{minWidth: 800, minHeight: 600}}>
        <Editor
          editorState={internalEditorState}
          wrapperClassName="demo-wrapper"
          editorClassName="demo-editor"
          onEditorStateChange={onEditorStateChange}
          onBlur={props.onBlur}
        />
      </div>
    </div>
  );
}
