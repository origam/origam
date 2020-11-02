import { flow } from "mobx";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { handleError } from "model/actions/handleError";
import React, { PropsWithChildren } from "react";

export class ErrorBoundary extends React.Component<{
  onErrorCaught?: (error: any, errorInfo: any) => void;
}> {
  componentDidCatch(error: any, errorInfo: any) {
    console.log("CAUGHT ERROR:", error, errorInfo);
    this.props.onErrorCaught?.(error, errorInfo);
  }

  render() {
    return this.props.children;
  }
}

export class ErrorBoundaryEncapsulated extends React.Component<PropsWithChildren<{ ctx: any }>> {
  handleScreenError(error: any) {
    const self = this;
    flow(function* () {
      try {
        yield* handleError(self.props.ctx)(error);
      } catch (e) {
      } finally {
        yield onScreenTabCloseClick(self.props.ctx)(undefined);
      }
    })();
  }

  render() {
    return (
      <ErrorBoundary onErrorCaught={(error) => this.handleScreenError(error)}>
        {this.props.children}
      </ErrorBoundary>
    );
  }
}
