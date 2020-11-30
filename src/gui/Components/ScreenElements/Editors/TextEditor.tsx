import { action } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { Tooltip } from "react-tippy";
import S from "./TextEditor.module.scss";
import { IFocusAble } from "../../../../model/entities/FocusManager";

import { EditorState, convertToRaw, ContentState } from "draft-js";
import { Editor } from "react-draft-wysiwyg";
import draftToHtml from "draftjs-to-html";
import htmlToDraft from "html-to-draftjs";
import { useCallback, useEffect, useState } from "react";

import "react-draft-wysiwyg/dist/react-draft-wysiwyg.css";

@observer
export class TextEditor extends React.Component<{
  value: string | null;
  isMultiline?: boolean;
  isReadOnly: boolean;
  isPassword?: boolean;
  isInvalid: boolean;
  invalidMessage?: string;
  isFocused: boolean;
  backgroundColor?: string;
  foregroundColor?: string;
  maxLength?: number;
  isRichText: boolean;
  customStyle?: any;
  subscribeToFocusManager?: (obj: IFocusAble) => void;
  refocuser?: (cb: () => void) => () => void;
  onChange?(event: any, value: string): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onDoubleClick?(event: any): void;
  onEditorBlur?(event: any): void;
}> {
  disposers: any[] = [];

  componentDidMount() {
    this.props.refocuser && this.disposers.push(this.props.refocuser(this.makeFocusedIfNeeded));
    this.makeFocusedIfNeeded();
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  componentWillUnmount() {
    this.disposers.forEach((d) => d());
  }

  componentDidUpdate(prevProps: { isFocused: boolean }) {
    if (!prevProps.isFocused && this.props.isFocused) {
      this.makeFocusedIfNeeded();
    }
  }

  @action.bound
  makeFocusedIfNeeded() {
    if (this.props.isFocused && this.elmInput) {
      this.elmInput.select();
      this.elmInput.scrollLeft = 0;
    }
  }

  @action.bound
  handleFocus(event: any) {
    if (this.elmInput) {
      this.elmInput.select();
      this.elmInput.scrollLeft = 0;
    }
  }

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
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
          <div className={S.notification}>
            <Tooltip html={this.props.invalidMessage} arrow={true}>
              <i className="fas fa-exclamation-circle red" />
            </Tooltip>
          </div>
        )}
      </div>
    );
  }

  private renderValueTag() {
    const maxLength = this.props.maxLength === 0 ? undefined : this.props.maxLength;
    if (this.props.isRichText) {
      if (this.props.isReadOnly) {
        return (
          <div className={S.editorContainer}>
            <div
              style={this.getStyle()}
              className={S.input}
              dangerouslySetInnerHTML={{ __html: this.props.value ?? "" }}
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
              this.props.onChange?.(undefined, newValue);
            }}
            onBlur={this.props.onEditorBlur}
            onFocus={this.handleFocus}
          />
        );
      }
    }
    if (!this.props.isMultiline) {
      return (
        <input
          style={this.getStyle()}
          className={S.input}
          type={this.props.isPassword ? "password" : "text"}
          autoComplete={this.props.isPassword ? "new-password" : undefined}
          value={this.props.value || ""}
          readOnly={this.props.isReadOnly}
          maxLength={maxLength}
          ref={this.refInput}
          onChange={(event: any) =>
            this.props.onChange && this.props.onChange(event, event.target.value)
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
          className={S.input}
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
          style={this.getStyle()}
          className={S.input}
          value={this.props.value || ""}
          readOnly={this.props.isReadOnly}
          ref={this.refInput}
          maxLength={maxLength}
          onChange={(event: any) =>
            this.props.onChange && this.props.onChange(event, event.target.value)
          }
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

function RichTextEditor(props: {
  value: any;
  onChange?: (newValue: any) => void;
  onBlur?: (event: any) => void;
  onFocus?: (event: any) => void;
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
    [setInternalEditorState, setInternalEditorStateHtml, props.onChange]
  );

  useEffect(() => {
    if (props.value !== internalEditorStateHtml) {
      const blocksFromHtml = htmlToDraft(props.value);
      const { contentBlocks, entityMap } = blocksFromHtml;
      const contentState = ContentState.createFromBlockArray(contentBlocks, entityMap);
      const editorState = EditorState.createWithContent(contentState);
      setInternalEditorStateHtml(props.value);
      setInternalEditorState(editorState);
    }
  }, [props.value, internalEditorStateHtml]);

  return (
    <div style={{ overflow: "auto", width: "100%", height: "100%" }}>
      <div style={{ minWidth: 800, minHeight: 600 }}>
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
