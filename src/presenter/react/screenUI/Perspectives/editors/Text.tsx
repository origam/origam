import * as React from "react";

export class TextEditor extends React.Component<{
  value: string;
  isReadOnly: boolean;
  isInvalid: boolean;
  isFocused: boolean;
  onChange?(event: any, value: string): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
}> {
 
  componentDidMount() {
    if(this.props.isFocused) {
      this.makeFocused();
    }
  }

  componentDidUpdate(prevProps: {isFocused: boolean}) {
    if(!prevProps.isFocused && this.props.isFocused) {
      this.makeFocused();
    }
  }

  makeFocused() {
    this.elmInput && this.elmInput.focus();
    setTimeout(() => {
      this.elmInput && this.elmInput.select();
    }, 10);
  }

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  }
  
  render() {
    return (
      <div className="editor-container">
        <input
          className="editor"
          type="text"
          value={this.props.value}
          readOnly={this.props.isReadOnly}
          ref={this.refInput}
          onChange={(event: any) =>
            this.props.onChange &&
            this.props.onChange(event, event.target.value)
          }
          onKeyDown={this.props.onKeyDown}
          onClick={this.props.onClick}
        />
        {this.props.isInvalid && (
          <div className="notification">
            <i className="fas fa-exclamation-circle red" />
          </div>
        )}
      </div>
    );
  }
}
