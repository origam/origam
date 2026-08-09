/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

import { AiAgentState } from '@/ai/AiAgentState';
import S from '@/ai/AiAgentPanel.module.scss';
import { chatImageUrl } from '@/ai/ChatHistoryApi';
import { Markdown } from '@/ai/Markdown';
import { RootStoreContext, T } from '@/main';
import { observer } from 'mobx-react-lite';
import { useContext, useEffect, useRef, useState } from 'react';

export const AiAgentPanel = observer(function AiAgentPanel() {
  const rootStore = useContext(RootStoreContext);
  const [state] = useState(() => new AiAgentState(rootStore));
  const [isToolsOpen, setIsToolsOpen] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const toolsRef = useRef<HTMLDivElement>(null);

  const activeThread = state.activeThread;
  const messages = state.messages;
  const lastMessage = messages[messages.length - 1];
  const isWaitingForFirstToken = state.isRunning && lastMessage?.role !== 'assistant';

  useEffect(() => {
    void state.loadSections();
    void state.loadHistory();
  }, [state]);

  useEffect(() => {
    function reloadHistoryWhenVisible() {
      if (document.visibilityState === 'visible') {
        void state.loadHistory();
      }
    }
    document.addEventListener('visibilitychange', reloadHistoryWhenVisible);
    window.addEventListener('focus', reloadHistoryWhenVisible);
    return () => {
      document.removeEventListener('visibilitychange', reloadHistoryWhenVisible);
      window.removeEventListener('focus', reloadHistoryWhenVisible);
    };
  }, [state]);

  useEffect(() => {
    if (!isToolsOpen) {
      return;
    }
    function handleClickOutside(event: MouseEvent) {
      if (toolsRef.current && !toolsRef.current.contains(event.target as Node)) {
        setIsToolsOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [isToolsOpen]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages.length, lastMessage?.text.length, state.isRunning]);

  useEffect(() => {
    const textarea = textareaRef.current;
    if (!textarea) {
      return;
    }
    textarea.style.height = 'auto';
    textarea.style.height = Math.min(textarea.scrollHeight, 180) + 'px';
  }, [state.draft]);

  function handleKeyDown(event: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      void state.send();
    }
  }

  function handlePaste(event: React.ClipboardEvent<HTMLTextAreaElement>) {
    const pastedFiles = Array.from(event.clipboardData.files);
    if (pastedFiles.some(file => file.type.startsWith('image/'))) {
      event.preventDefault();
      void state.addImageFiles(pastedFiles);
    }
  }

  return (
    <div className={S.panel}>
      <div className={S.header}>
        <div className={S.threadBar}>
          <select
            className={S.threadSelect}
            value={activeThread.id}
            onChange={event => state.selectThread(event.target.value)}
          >
            {state.threads.map(thread => (
              <option key={thread.id} value={thread.id}>
                {thread.title}
              </option>
            ))}
          </select>
          <button
            className={S.iconButton}
            title={T('New chat', 'ai_chat_new')}
            onClick={() => state.createNewThread()}
          >
            +
          </button>
          <button
            className={S.iconButton}
            title={T('Delete chat', 'ai_chat_delete')}
            onClick={() => state.deleteActiveThread()}
          >
            ✕
          </button>
          <div className={S.toolsWrapper} ref={toolsRef}>
            <button
              className={S.iconButton}
              title={T('Tools', 'ai_chat_tools')}
              onClick={() => setIsToolsOpen(open => !open)}
            >
              🧩
              {state.enabledSections.length > 0 && (
                <span className={S.toolsCount}>{state.enabledSections.length}</span>
              )}
            </button>
            {isToolsOpen && (
              <div className={S.toolsPopover}>
                <div className={S.toolsPopoverHeader}>
                  <div className={S.toolsTitle}>{T('API sections', 'ai_chat_tools_title')}</div>
                  <div className={S.toolsSubtitle}>
                    {T(
                      'Turn Swagger sections on or off. The assistant can only call tools from sections that are on.',
                      'ai_chat_tools_subtitle',
                    )}
                  </div>
                </div>
                {!state.sectionsAvailable && (
                  <div className={S.toolsUnavailable}>
                    {T(
                      'Sections unavailable. Is the Architect server running with Swagger?',
                      'ai_chat_tools_unavailable',
                    )}
                  </div>
                )}
                <div className={S.toolsList}>
                  {state.sections.map(section => {
                    const isEnabled = state.enabledSections.includes(section.name);
                    return (
                      <div key={section.name} className={S.toolsItem}>
                        <div className={S.toolsItemInfo}>
                          <div className={S.toolsItemName}>
                            {section.name}
                            {section.hasDestructive && (
                              <span
                                className={S.toolsCaution}
                                title={T(
                                  'Contains destructive operations (delete, deploy). Off by default.',
                                  'ai_chat_tools_caution',
                                )}
                              >
                                {T('caution', 'ai_chat_tools_caution_badge')}
                              </span>
                            )}
                          </div>
                          <div className={S.toolsItemMeta}>
                            {T(
                              '{0} functions',
                              'ai_chat_tools_functions',
                              section.functionCount.toString(),
                            )}
                          </div>
                        </div>
                        <button
                          type="button"
                          role="switch"
                          aria-checked={isEnabled}
                          className={isEnabled ? `${S.switch} ${S.switchOn}` : S.switch}
                          onClick={() => state.toggleSection(section.name)}
                        >
                          <span className={S.switchKnob} />
                        </button>
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        </div>
        {activeThread.tokensUsed > 0 && (
          <div className={S.tokenBar}>
            {T('Tokens used: {0}', 'ai_chat_tokens', activeThread.tokensUsed.toLocaleString())}
          </div>
        )}
      </div>

      <div className={S.messages}>
        {messages.length === 0 && !state.errorText && (
          <div className={S.empty}>
            {T('No messages yet. Try: "What time is it?"', 'ai_chat_empty')}
          </div>
        )}

        {messages.map(message => (
          <div
            key={message.id}
            className={
              message.role === 'user' ? `${S.row} ${S.rowUser}` : `${S.row} ${S.rowAssistant}`
            }
          >
            <div
              className={
                message.role === 'user'
                  ? `${S.bubble} ${S.bubbleUser}`
                  : `${S.bubble} ${S.bubbleAssistant}`
              }
            >
              <div className={S.role}>
                {message.role === 'user'
                  ? T('You', 'ai_chat_role_user')
                  : T('Assistant', 'ai_chat_role_assistant')}
              </div>
              {message.images && message.images.length > 0 && (
                <div className={S.messageImages}>
                  {message.images.map(image => (
                    <img
                      key={image.id}
                      className={S.messageImage}
                      src={image.dataUrl ?? chatImageUrl(activeThread.id, image.id)}
                      alt={T('Attached image', 'ai_chat_attached_image')}
                    />
                  ))}
                </div>
              )}
              {message.role === 'assistant'
                ? message.text.length > 0 && <Markdown text={message.text} />
                : message.text}
              {message.affectedNodes && message.affectedNodes.length > 0 && (
                <div className={S.affectedNodes}>
                  {T('Created / changed:', 'ai_chat_affected_nodes')}
                  <div>
                    {message.affectedNodes.map(node => {
                      const isDeleted = node.action === 'deleted';
                      const openEditorLabel = rootStore.editorTabViewState.editorsContainers.find(
                        editor => editor.state.origamId === node.origamId,
                      )?.state.label;
                      const chipLabel =
                        node.label ||
                        rootStore.modelTreeState.findNodeById(node.origamId)?.nodeText ||
                        openEditorLabel ||
                        node.itemTypeName ||
                        T('item', 'ai_chat_affected_fallback');
                      return (
                        <button
                          key={node.origamId}
                          type="button"
                          className={
                            isDeleted
                              ? `${S.affectedChip} ${S.affectedChipDeleted}`
                              : S.affectedChip
                          }
                          title={node.itemTypeName ?? undefined}
                          disabled={isDeleted}
                          onClick={() => state.openAffectedNode(node)}
                        >
                          {chipLabel}
                        </button>
                      );
                    })}
                  </div>
                </div>
              )}
              {message.calledFunctions && message.calledFunctions.length > 0 && (
                <div className={S.calledFunctions}>
                  {T('Called functions:', 'ai_chat_called_functions')}
                  <div>
                    {message.calledFunctions.map((functionName, functionIndex) => (
                      <span key={`${functionName}-${functionIndex}`} className={S.pill}>
                        {functionName}
                      </span>
                    ))}
                  </div>
                </div>
              )}
              {message.role === 'assistant' && !!message.totalTokens && (
                <div className={S.tokenNote}>
                  {T('{0} tokens', 'ai_chat_msg_tokens', message.totalTokens.toLocaleString())}
                </div>
              )}
            </div>
          </div>
        ))}

        {isWaitingForFirstToken && (
          <div className={`${S.row} ${S.rowAssistant}`}>
            <div className={`${S.bubble} ${S.bubbleAssistant}`}>
              {T('Thinking…', 'ai_chat_thinking')}
            </div>
          </div>
        )}

        {state.errorText && <div className={S.error}>{state.errorText}</div>}

        <div ref={messagesEndRef} />
      </div>

      <div className={S.composer}>
        {state.attachedImages.length > 0 && (
          <div className={S.thumbnails}>
            {state.attachedImages.map(image => (
              <div key={image.id} className={S.thumbnail}>
                <img
                  className={S.thumbnailImage}
                  src={image.dataUrl}
                  alt={T('Attached image', 'ai_chat_attached_image')}
                />
                <button
                  className={S.thumbnailRemove}
                  title={T('Remove image', 'ai_chat_remove_image')}
                  onClick={() => state.removeAttachedImage(image.id)}
                >
                  ✕
                </button>
              </div>
            ))}
          </div>
        )}
        <div className={S.composerRow}>
          <button
            className={S.attachButton}
            title={T('Attach image', 'ai_chat_attach')}
            onClick={() => fileInputRef.current?.click()}
            disabled={state.isRunning}
          >
            📎
          </button>
          <textarea
            ref={textareaRef}
            className={S.textarea}
            value={state.draft}
            onChange={event => state.setDraft(event.target.value)}
            onKeyDown={handleKeyDown}
            onPaste={handlePaste}
            placeholder={T(
              'Message…  (Enter to send, Shift+Enter for newline)',
              'ai_chat_placeholder',
            )}
            disabled={state.isRunning}
          />
          {state.isRunning ? (
            <button className={S.sendButton} onClick={() => state.stop()}>
              {T('Stop', 'ai_chat_stop')}
            </button>
          ) : (
            <button
              className={S.sendButton}
              onClick={() => void state.send()}
              disabled={!state.canSend}
            >
              {T('Send', 'ai_chat_send')}
            </button>
          )}
        </div>
        <input
          ref={fileInputRef}
          className={S.hiddenInput}
          type="file"
          accept="image/*"
          multiple
          onChange={event => {
            void state.addImageFiles(event.target.files);
            event.target.value = '';
          }}
        />
      </div>
    </div>
  );
});
