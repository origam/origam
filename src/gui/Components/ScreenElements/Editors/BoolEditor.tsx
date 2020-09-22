import * as React from "react";
import {observer} from "mobx-react";
import S from './BoolEditor.module.scss';
import cx from 'classnames';
import {IFocusable} from "../../../../model/entities/FocusManager";
import CS from "gui/Components/ScreenElements/Editors/CommonStyle.module.css";
import {Tooltip} from "react-tippy";


@observer
export class BoolEditor extends React.Component<{
  value: boolean;
  isReadOnly: boolean;
  onChange?(event: any, value: boolean): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onBlur?: ()=>void;
  onFocus?: ()=>void;
  isInvalid: boolean;
  invalidMessage?: string;
  id?: string;
  subscribeToFocusManager?: (obj: IFocusable) => (()=>void);
}> {

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };
  unsubscribeFromFocusManager?: () => void;

  componentDidMount() {
    if(this.elmInput && this.props.subscribeToFocusManager){
      this.unsubscribeFromFocusManager = this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  componentWillUnmount() {
    this.unsubscribeFromFocusManager && this.unsubscribeFromFocusManager();
  }
  
  render() {
    return (
      <div className={cx(S.editorContainer)}>
        <input
          id={ this.props.id ? this.props.id : undefined }
          className="editor"
          type="checkbox"
          checked={this.props.value}
          readOnly={this.props.isReadOnly}
          disabled={this.props.isReadOnly}
          onChange={(event: any) => {
            this.props.onChange && !this.props.isReadOnly &&
              this.props.onChange(event, event.target.checked);
          }}
          onKeyDown={this.props.onKeyDown}
          onClick={this.props.onClick}
          onBlur={this.props.onBlur}
          onFocus={this.props.onFocus}
          ref={this.refInput}
        />
        {this.props.isInvalid && (
          <div className={CS.notification}>
            <Tooltip html={this.props.invalidMessage} arrow={true}>
              <i className="fas fa-exclamation-circle red" />
            </Tooltip>
          </div>
        )}
      </div>
    );
  }
}
