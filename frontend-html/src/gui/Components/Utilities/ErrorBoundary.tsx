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

import { flow } from "mobx";
import { onScreenTabCloseClick } from "model/actions-ui/ScreenTabHandleRow/onScreenTabCloseClick";
import { handleError } from "model/actions/handleError";
import React, { PropsWithChildren } from "react";

export class ErrorBoundary extends React.Component<{
  onErrorCaught?: (error: any, errorInfo: any) => void;
}> {
  componentDidCatch(error: any, errorInfo: any) {
    console.log("CAUGHT ERROR:", error, errorInfo); // eslint-disable-line no-console
    this.props.onErrorCaught?.(error, errorInfo);
  }

  render() {
    return this.props.children;
  }
}

export class ErrorBoundaryEncapsulated extends React.Component<PropsWithChildren<{ ctx: any }>> {
  handleScreenError(error: any) {
    const self = this;
    flow(function*() {
      try {
        yield*handleError(self.props.ctx)(error);
      } catch (e) {
      } finally {
        yield onScreenTabCloseClick(self.props.ctx)(undefined, true);
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
