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

import React, {
  forwardRef,
  useImperativeHandle,
  useMemo,
  useRef,
  useState,
  useEffect,
} from "react";
import cx from "classnames";

export function MessageBarRaw(
  props: {
    messages: React.ReactNode;
    onUserScrolledToTail?(isTailed: boolean): void;
    isTrackingLatestMessages?: boolean;
  },
  ref: any
) {
  const [isScrollToEnd, setIsScrollToEnd] = useState(false);

  let isUserScroll = true;
  let hIsUserScrollTimeout: any;

  const scrollToEnd = useMemo(
    () => () => {
      if (refContentContainer.current) {
        //console.log(refContentContainer.current!.scrollHeight, refContentContainer.current!.clientHeight);
        const elm = refContentContainer.current!;
        elm.scrollTop = elm.scrollHeight - elm.clientHeight;
        // TODO: Examine following disabled rules whether they actually do not inform about a bug.
        // eslint-disable-next-line react-hooks/exhaustive-deps
        isUserScroll = false;
        clearTimeout(hIsUserScrollTimeout);
        // eslint-disable-next-line react-hooks/exhaustive-deps
        hIsUserScrollTimeout = setTimeout(() => (isUserScroll = true), 200);
      }
    },
    []
  );

  useImperativeHandle(
    ref,
    () => ({
      scrollToEnd() {
        scrollToEnd();
      },
    }),
    [scrollToEnd]
  );

  const refContentContainer = useRef<HTMLDivElement>(null);

  const handleScrollToEndClick = useMemo(
    () => () => {
      scrollToEnd();
      props.onUserScrolledToTail?.(true);
    },
    [scrollToEnd, props]
  );

  const checkScrollToEnd = useMemo(
    () => () => {
      if (refContentContainer.current) {
        const elm = refContentContainer.current!;
        //console.log(elm.scrollHeight - elm.clientHeight - elm.scrollTop);
        if (elm.scrollHeight - elm.clientHeight - elm.scrollTop > 10) {
          //console.log(props.isTrackingLatestMessages);
          if (props.isTrackingLatestMessages) {
            scrollToEnd();
          } else {
            if (!isScrollToEnd && isUserScroll) setIsScrollToEnd(true);
          }
        } else {
          if (isScrollToEnd && isUserScroll) setIsScrollToEnd(false);
        }
      }
    },
    [props.isTrackingLatestMessages, isScrollToEnd, isUserScroll, scrollToEnd]
  );

  useEffect(() => {
    const hTimer = setInterval(() => {
      checkScrollToEnd();
    }, 100);
    return () => clearInterval(hTimer);
  }, [props.isTrackingLatestMessages, checkScrollToEnd]);

  const handleMessageBarScroll = useMemo(
    () => (event: any) => {
      if (refContentContainer.current) {
        const elm = refContentContainer.current!;
        if (elm.scrollHeight - elm.clientHeight - elm.scrollTop > 10) {
          setIsScrollToEnd(true);
          if (isUserScroll) props.onUserScrolledToTail?.(false);
        } else {
          setIsScrollToEnd(false);
          if (isUserScroll) props.onUserScrolledToTail?.(true);
        }
      }
    },
    [isUserScroll, props]
  );

  return (
    <div className="messageBarContainer">
      <div
        className="messageBar"
        ref={refContentContainer}
        onScroll={handleMessageBarScroll}
      >
        <div className="messageBar__inner">{props.messages}</div>
      </div>
      <div
        className={cx("messageBarContainer__scrollToEndControl", {
          "messageBarContainer__scrollToEndControl--isHidden": !isScrollToEnd,
        })}
      >
        <button className="scrollToEndButton" onClick={handleScrollToEndClick}>
          <i className="fas fa-arrow-down" />
        </button>
      </div>
    </div>
  );
}

export const MessageBar = forwardRef(MessageBarRaw);
