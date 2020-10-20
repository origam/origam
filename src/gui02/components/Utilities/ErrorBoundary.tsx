import React from "react";

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
