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
using System.IO;
using System.Linq;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using Origam.Windows.Editor.CodeCompletion;

namespace Origam.Windows.Editor;

public class XmlCodeCompletionBinding
{
    XmlSchemaCompletionCollection schemas;

    public XmlCodeCompletionBinding(XmlSchemaCompletionCollection schemas)
    {
        this.schemas = schemas;
    }

    char[] ignoredChars = new[] { '\\', '/', '"', '\'', '=', '>', '!', '?' };
    public string DefaultSchema { get; set; }

    public CodeCompletionKeyPressResult HandleKeyPress(TextEditor editor, char ch)
    {
        return CodeCompletionKeyPressResult.None;
    }

    XmlCompletionItemCollection GetCompletionItems(
        TextEditor editor,
        XmlSchemaCompletion defaultSchema
    )
    {
        int offset = editor.TextArea.Caret.Offset;
        string textUpToCursor = editor.Document.GetText(offset: 0, length: offset);
        XmlCompletionItemCollection items = new XmlCompletionItemCollection();
        if (XmlParser.IsInsideAttributeValue(xml: textUpToCursor, index: offset))
        {
            items = schemas.GetNamespaceCompletion(textUpToCursor: textUpToCursor);
            if (items.Count == 0)
            {
                items = schemas.GetAttributeValueCompletion(
                    text: textUpToCursor,
                    offset: editor.TextArea.Caret.Offset,
                    defaultSchema: defaultSchema
                );
            }
        }
        else
        {
            items = schemas.GetAttributeCompletion(
                textUpToCursor: textUpToCursor,
                defaultSchema: defaultSchema
            );
            if (items.Count == 0)
            {
                items = schemas.GetElementCompletion(
                    textUpToCursor: textUpToCursor,
                    defaultSchema: defaultSchema
                );
            }
        }
        return items;
    }

    void SetCompletionWindowWidth(
        CompletionWindow completionWindow,
        XmlCompletionItemCollection completionItems
    )
    {
        XmlCompletionItem firstListItem = completionItems[index: 0];
        if (firstListItem.DataType == XmlCompletionItemType.NamespaceUri)
        {
            completionWindow.Width = double.NaN;
        }
    }

    public bool CtrlSpace(OrigamTextEditor editor, char ch)
    {
        int elementStartIndex = XmlParser.GetActiveElementStartIndex(
            xml: editor.Document.Text,
            index: editor.TextArea.Caret.Offset
        );
        if (elementStartIndex <= -1)
        {
            return false;
        }

        if (
            ElementStartsWith(
                text: "<!",
                elementStartIndex: elementStartIndex,
                document: editor.Document
            )
        )
        {
            return false;
        }

        if (
            ElementStartsWith(
                text: "<?",
                elementStartIndex: elementStartIndex,
                document: editor.Document
            )
        )
        {
            return false;
        }

        XmlSchemaCompletion defaultSchema = null;
        if (DefaultSchema != null)
        {
            TextReader tr = new StringReader(s: DefaultSchema);
            defaultSchema = new XmlSchemaCompletion(reader: tr);
        }
        XmlCompletionItemCollection completionItems = GetCompletionItems(
            editor: editor,
            defaultSchema: defaultSchema
        );
        if (completionItems.HasItems)
        {
            completionItems.Sort();
            string identifier = XmlParser.GetXmlIdentifierBeforeIndex(
                document: editor.Document,
                index: editor.TextArea.Caret.Offset
            );
            completionItems.PreselectionLength = identifier.Length;
            CompletionWindow completionWindow = new CompletionWindow(textArea: editor.TextArea);
            if (!(char.IsWhiteSpace(c: ch) || ch == '<'))
            {
                completionWindow.StartOffset--;
            }
            foreach (var item in completionItems)
            {
                completionWindow.CompletionList.CompletionData.Add(item: item);
            }
            completionWindow.Show();
            if (completionWindow != null)
            {
                SetCompletionWindowWidth(
                    completionWindow: completionWindow,
                    completionItems: completionItems
                );
            }
            return true;
        }
        return false;
    }

    bool ElementStartsWith(string text, int elementStartIndex, ITextSource document)
    {
        int textLength = Math.Min(val1: text.Length, val2: document.TextLength - elementStartIndex);
        return document
            .GetText(offset: elementStartIndex, length: textLength)
            .Equals(value: text, comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    public bool HandleKeyPressed(OrigamTextEditor editor, char ch)
    {
        //if (char.IsWhiteSpace(ch) || editor.SelectionLength > 0)
        //    return false;
        if (ignoredChars.Contains(value: ch))
        {
            return false;
        }

        if (
            XmlParser
                .GetXmlIdentifierBeforeIndex(
                    document: editor.Document,
                    index: editor.TextArea.Caret.Offset
                )
                .Length > 1
        )
        {
            return false;
        }

        return CtrlSpace(editor: editor, ch: ch);
    }
}
