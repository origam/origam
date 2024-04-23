// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace Origam.Windows.Editor;

/// <summary>
/// This class currently inserts the closing tags to typed openening tags
/// and does smart indentation for xml files.
/// </summary>
public class XmlFormattingStrategy
{
    public void FormatLine(TextEditor editor, char charTyped)
    {
            //editor.Document.StartUndoableAction();
            try
            {
                if (charTyped == '>')
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    int offset = Math.Min(editor.TextArea.Caret.Offset - 2, editor.Document.TextLength - 1);
                    while (true)
                    {
                        if (offset < 0)
                        {
                            break;
                        }
                        char ch = editor.Document.GetCharAt(offset);
                        if (ch == '<')
                        {
                            string reversedTag = stringBuilder.ToString().Trim();
                            if (!reversedTag.StartsWith("/", StringComparison.Ordinal) && !reversedTag.EndsWith("/", StringComparison.Ordinal))
                            {
                                bool validXml = true;
                                try
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(editor.Document.Text);
                                }
                                catch (XmlException)
                                {
                                    validXml = false;
                                }
                                // only insert the tag, if something is missing
                                if (!validXml)
                                {
                                    StringBuilder tag = new StringBuilder();
                                    for (int i = reversedTag.Length - 1; i >= 0 && !Char.IsWhiteSpace(reversedTag[i]); --i)
                                    {
                                        tag.Append(reversedTag[i]);
                                    }
                                    string tagString = tag.ToString();
                                    if (tagString.Length > 0 && !tagString.StartsWith("!", StringComparison.Ordinal) && !tagString.StartsWith("?", StringComparison.Ordinal))
                                    {
                                        int caretOffset = editor.TextArea.Caret.Offset;
                                        editor.Document.Insert(editor.TextArea.Caret.Offset, "</" + tagString + ">");
                                        editor.TextArea.Caret.Offset = caretOffset;
                                    }
                                }
                            }
                            break;
                        }
                        stringBuilder.Append(ch);
                        --offset;
                    }
                }
            }
            catch (Exception e)
            { // Insanity check
                Debug.Assert(false, e.ToString());
            }
            if (charTyped == '\n')
            {
                IndentLine(editor, editor.Document.GetLineByNumber(editor.TextArea.Caret.Line));
            }
            //editor.Document.EndUndoableAction();
        }

    public void IndentLine(TextEditor editor, DocumentLine line)
    {
            //editor.Document.StartUndoableAction();
            try
            {
                TryIndent(editor, line.LineNumber, line.LineNumber);
            }
            catch (XmlException)
            {
                //LoggingService.Debug(ex.ToString());
            }
            finally
            {
                //editor.Document.EndUndoableAction();
            }
        }

    /// <summary>
    /// This function sets the indentlevel in a range of lines.
    /// </summary>
    public void IndentLines(TextEditor editor, int begin, int end)
    {
            //editor.Document.StartUndoableAction();
            try
            {
                TryIndent(editor, begin, end);
            }
            catch (XmlException)
            {
                //LoggingService.Debug(ex.ToString());
            }
            finally
            {
                //editor.Document.EndUndoableAction();
            }
        }

    public void SurroundSelectionWithComment(TextEditor editor)
    {
            SurroundSelectionWithBlockComment(editor, "<!--", "-->");
        }

    static void TryIndent(TextEditor editor, int begin, int end)
    {
            string currentIndentation = "";
            Stack<string> tagStack = new Stack<string>();
            IDocument document = editor.Document;

            string tab = editor.Options.IndentationString;
            int nextLine = begin; // in #dev coordinates
            bool wasEmptyElement = false;
            XmlNodeType lastType = XmlNodeType.XmlDeclaration;
            using (StringReader stringReader = new StringReader(document.Text))
            {
                XmlTextReader r = new XmlTextReader(stringReader);
                r.XmlResolver = null; // prevent XmlTextReader from loading external DTDs
                while (r.Read())
                {
                    if (wasEmptyElement)
                    {
                        wasEmptyElement = false;
                        if (tagStack.Count == 0)
                            currentIndentation = "";
                        else
                            currentIndentation = tagStack.Pop();
                    }
                    if (r.NodeType == XmlNodeType.EndElement)
                    {
                        if (tagStack.Count == 0)
                            currentIndentation = "";
                        else
                            currentIndentation = tagStack.Pop();
                    }

                    while (r.LineNumber >= nextLine)
                    {
                        if (nextLine > end) break;
                        if (lastType == XmlNodeType.CDATA || lastType == XmlNodeType.Comment)
                        {
                            nextLine++;
                            continue;
                        }
                        // set indentation of 'nextLine'
                        IDocumentLine line = document.GetLineByNumber(nextLine);
                        string lineText = document.GetText(line);

                        string newText;
                        // special case: opening tag has closing bracket on extra line: remove one indentation level
                        if (lineText.Trim() == ">")
                            newText = tagStack.Peek() + lineText.Trim();
                        else
                            newText = currentIndentation + lineText.Trim();

                        document.SmartReplaceLine(line, newText);
                        nextLine++;
                    }
                    if (r.LineNumber > end)
                        break;
                    wasEmptyElement = r.NodeType == XmlNodeType.Element && r.IsEmptyElement;
                    string attribIndent = null;
                    if (r.NodeType == XmlNodeType.Element)
                    {
                        tagStack.Push(currentIndentation);
                        if (r.LineNumber < begin)
                            currentIndentation = DocumentUtilities.GetIndentation(editor.Document, r.LineNumber);
                        if (r.Name.Length < 16)
                            attribIndent = currentIndentation + new string(' ', 2 + r.Name.Length);
                        else
                            attribIndent = currentIndentation + tab;
                        currentIndentation += tab;
                    }
                    lastType = r.NodeType;
                    if (r.NodeType == XmlNodeType.Element && r.HasAttributes)
                    {
                        int startLine = r.LineNumber;
                        r.MoveToAttribute(0); // move to first attribute
                        if (r.LineNumber != startLine)
                            attribIndent = currentIndentation; // change to tab-indentation
                        r.MoveToAttribute(r.AttributeCount - 1);
                        while (r.LineNumber >= nextLine)
                        {
                            if (nextLine > end) break;
                            // set indentation of 'nextLine'
                            IDocumentLine line = document.GetLineByNumber(nextLine);
                            string newText = attribIndent + document.GetText(line).Trim();
                            document.SmartReplaceLine(line, newText);
                            nextLine++;
                        }
                    }
                }
                r.Close();
            }
        }

    /// <summary>
    /// Default implementation for multiline comments.
    /// </summary>
    protected void SurroundSelectionWithBlockComment(TextEditor editor, string blockStart, string blockEnd)
    {
            editor.Document.UndoStack.StartUndoGroup();
            try {
                int startOffset = editor.SelectionStart;
                int endOffset = editor.SelectionStart + editor.SelectionLength;

                if (editor.SelectionLength == 0)
                {
                    IDocumentLine line = editor.Document.GetLineByOffset(editor.SelectionStart);
                    startOffset = line.Offset;
                    endOffset = line.Offset + line.Length;
                }

                BlockCommentRegion region = FindSelectedCommentRegion(editor, blockStart, blockEnd);

                if (region != null)
                {
                    do
                    {
                        editor.Document.Remove(region.EndOffset, region.CommentEnd.Length);
                        editor.Document.Remove(region.StartOffset, region.CommentStart.Length);

                        int selectionStart = region.EndOffset;
                        int selectionLength = editor.SelectionLength - (region.EndOffset - editor.SelectionStart);

                        if (selectionLength > 0)
                        {
                            editor.Select(region.EndOffset, selectionLength);
                            region = FindSelectedCommentRegion(editor, blockStart, blockEnd);
                        }
                        else
                        {
                            region = null;
                        }
                    } while (region != null);

                }
                else
                {
                    editor.Document.Insert(endOffset, blockEnd);
                    editor.Document.Insert(startOffset, blockStart);
                }
            }
            finally
            {
                editor.Document.UndoStack.EndUndoGroup();    
            }
        }
    public static BlockCommentRegion FindSelectedCommentRegion(TextEditor editor, string commentStart, string commentEnd)
    {
            IDocument document = editor.Document;

            if (document.TextLength == 0)
            {
                return null;
            }

            // Find start of comment in selected text.

            int commentEndOffset = -1;
            string selectedText = editor.SelectedText;

            int commentStartOffset = selectedText.IndexOf(commentStart);
            if (commentStartOffset >= 0)
            {
                commentStartOffset += editor.SelectionStart;
            }

            // Find end of comment in selected text.

            if (commentStartOffset >= 0)
            {
                commentEndOffset = selectedText.IndexOf(commentEnd, commentStartOffset + commentStart.Length - editor.SelectionStart);
            }

            // Try to search end of comment in whole selection
            bool startAfterEnd = false;
            int commentEndOffsetWholeText = selectedText.IndexOf(commentEnd);
            if ((commentEndOffsetWholeText >= 0) && (commentEndOffsetWholeText < (commentStartOffset - editor.SelectionStart)))
            {
                // There seems to be an end offset before the start offset in selection
                commentStartOffset = -1;
                startAfterEnd = true;
                commentEndOffset = commentEndOffsetWholeText;
            }

            if (commentEndOffset >= 0)
            {
                commentEndOffset += editor.SelectionStart;
            }

            // Find start of comment before or partially inside the
            // selected text.

            int commentEndBeforeStartOffset = -1;
            if (commentStartOffset == -1)
            {
                int offset = editor.SelectionStart + editor.SelectionLength + commentStart.Length - 1;
                if (offset > document.TextLength)
                {
                    offset = document.TextLength;
                }
                string text = document.GetText(0, offset);
                if (startAfterEnd)
                {
                    commentStartOffset = text.LastIndexOf(commentStart, editor.SelectionStart);
                }
                else
                {
                    commentStartOffset = text.LastIndexOf(commentStart);
                }
                if (commentStartOffset >= 0)
                {
                    // Find end of comment before comment start.
                    commentEndBeforeStartOffset = text.IndexOf(commentEnd, commentStartOffset, editor.SelectionStart - commentStartOffset);
                    if (commentEndBeforeStartOffset > commentStartOffset)
                    {
                        commentStartOffset = -1;
                    }
                }
            }

            // Find end of comment after or partially after the
            // selected text.

            if (commentEndOffset == -1)
            {
                int offset = editor.SelectionStart + 1 - commentEnd.Length;
                if (offset < 0)
                {
                    offset = editor.SelectionStart;
                }
                string text = document.GetText(offset, document.TextLength - offset);
                commentEndOffset = text.IndexOf(commentEnd);
                if (commentEndOffset >= 0)
                {
                    commentEndOffset += offset;
                }
            }

            if (commentStartOffset != -1 && commentEndOffset != -1)
            {
                return new BlockCommentRegion(commentStart, commentEnd, commentStartOffset, commentEndOffset);
            }

            return null;
        }
}

public class BlockCommentRegion
{
    public string CommentStart { get; private set; }
    public string CommentEnd { get; private set; }
    public int StartOffset { get; private set; }
    public int EndOffset { get; private set; }

    /// <summary>
    /// The end offset is the offset where the comment end string starts from.
    /// </summary>
    public BlockCommentRegion(string commentStart, string commentEnd, int startOffset, int endOffset)
    {
            this.CommentStart = commentStart;
            this.CommentEnd = commentEnd;
            this.StartOffset = startOffset;
            this.EndOffset = endOffset;
        }

    public override int GetHashCode()
    {
            int hashCode = 0;
            unchecked
            {
                if (CommentStart != null) hashCode += 1000000007 * CommentStart.GetHashCode();
                if (CommentEnd != null) hashCode += 1000000009 * CommentEnd.GetHashCode();
                hashCode += 1000000021 * StartOffset.GetHashCode();
                hashCode += 1000000033 * EndOffset.GetHashCode();
            }
            return hashCode;
        }

    public override bool Equals(object obj)
    {
            BlockCommentRegion other = obj as BlockCommentRegion;
            if (other == null) return false;
            return this.CommentStart == other.CommentStart &&
                this.CommentEnd == other.CommentEnd &&
                this.StartOffset == other.StartOffset &&
                this.EndOffset == other.EndOffset;
        }
}