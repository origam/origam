#region license
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
#endregion

using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using Origam.UI;
using UserControl = System.Windows.Forms.UserControl;

namespace Origam.Windows.Editor;

public partial class XmlEditor : UserControl
{
    XmlFormattingStrategy _formattingStrategy = new XmlFormattingStrategy();
    XmlSchemaCompletionCollection _schemas = new XmlSchemaCompletionCollection();
    XmlCodeCompletionBinding _codeCompletion;
    DataSet _data = new DataSet("ROOT");
    XmlFoldingStrategy _foldingStrategy;
    FoldingManager _foldingManager;
    public delegate void ChangedEventHandler(object sender, EventArgs e);
    public AbstractMargin LeftMargin
    {
        set
        {
            editor.ShowLineNumbers = false;
            editor.TextArea.LeftMargins.Add(value);
        }
    }
    public IBackgroundRenderer BackgroundRenderer
    {
        set => editor.TextArea.TextView.BackgroundRenderers.Add(value);
    }

    public XmlEditor()
    {
        _codeCompletion = new XmlCodeCompletionBinding(_schemas);
        InitializeComponent();
        editor.FontFamily = new System.Windows.Media.FontFamily("Courier New");
        editor.FontSize = 16;
        TextEditorOptions options = new TextEditorOptions();
        options.ConvertTabsToSpaces = false;
        options.HighlightCurrentLine = true;
        options.ShowEndOfLine = true;
        options.ShowSpaces = true;
        options.ShowTabs = true;
        options.ShowColumnRuler = true;
        options.ColumnRulerPosition = 80;
        editor.Options = options;
        editor.ShowLineNumbers = true;
        editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
        var valueBrush = new SimpleHighlightingBrush(
            System.Windows.Media.Color.FromRgb(18, 53, 182)
        );
        var elementBrush = new SimpleHighlightingBrush(
            System.Windows.Media.Color.FromRgb(
                OrigamColorScheme.TabActiveStartColor.R,
                OrigamColorScheme.TabActiveStartColor.G,
                OrigamColorScheme.TabActiveStartColor.B
            )
        );
        //            System.Windows.Media.Color.FromRgb(161, 19, 0));
        var attributeBrush = new SimpleHighlightingBrush(
            System.Windows.Media.Color.FromRgb(255, 73, 61)
        );
        var docBrush = new SimpleHighlightingBrush(System.Windows.Media.Color.FromRgb(0, 154, 41));
        var declarationBrush = new SimpleHighlightingBrush(
            System.Windows.Media.Color.FromRgb(100, 100, 100)
        );
        editor.TextArea.TextView.LinkTextForegroundBrush = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(161, 112, 0)
        );
        foreach (var color in editor.SyntaxHighlighting.NamedHighlightingColors)
        {
            switch (color.Name)
            {
                case "XmlTag":
                    color.Foreground = elementBrush;
                    break;
                case "AttributeName":
                    color.Foreground = attributeBrush;
                    break;
                case "AttributeValue":
                    color.Foreground = valueBrush;
                    break;
                case "CData":
                case "DocType":
                case "Entity":
                case "XmlDeclaration":
                    color.Foreground = declarationBrush;
                    break;
            }
        }
        SearchPanel.Install(editor);

        _foldingManager = FoldingManager.Install(editor.TextArea);
        _foldingStrategy = new XmlFoldingStrategy();
        _foldingStrategy.ShowAttributesWhenFolded = true;
        this.editor.TextArea.IndentationStrategy =
            new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
        this.editor.TextArea.TextEntered += TextArea_TextEntered;
        AddInternalSchema("xslt.xsd");
        editor.TextArea.Document.TextChanged += Document_TextChanged;
    }

    public event ChangedEventHandler ContentChanged;

    // Invoke the Changed event; called whenever list changes
    protected virtual void OnContentChanged(EventArgs e)
    {
        if (ContentChanged != null)
        {
            ContentChanged(this, e);
        }
    }

    private void Document_TextChanged(object sender, System.EventArgs e)
    {
        foldingTimer.Enabled = false;
        foldingTimer.Enabled = true;
        OnContentChanged(EventArgs.Empty);
    }

    public string ResultSchema
    {
        get { return _codeCompletion.DefaultSchema; }
        set { _codeCompletion.DefaultSchema = value; }
    }
    public new string Text
    {
        get { return editor.Document.Text; }
        set
        {
            editor.Document.Text = value ?? "";
            editor.Document.UndoStack.ClearAll();
        }
    }
    public bool IsReadOnly
    {
        get { return editor.IsReadOnly; }
        set { editor.IsReadOnly = value; }
    }

    public void DisableScrolling()
    {
        editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        editor.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }

    private void AddInternalSchema(string schemaName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Origam.Windows.Editor." + schemaName;
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            _schemas.Add(new XmlSchemaCompletion(reader));
        }
    }

    private void TextArea_TextEntered(
        object sender,
        System.Windows.Input.TextCompositionEventArgs e
    )
    {
        if (e.Text.Length > 0 && !e.Handled)
        {
            char c = e.Text[0];
            // When entering a newline, AvalonEdit might use either "\r\n" or "\n", depending on
            // what was passed to TextArea.PerformTextInput. We'll normalize this to '\n'
            // so that formatting strategies don't have to handle both cases.
            if (c == '\r')
                c = '\n';
            _formattingStrategy.FormatLine(this.editor, c);
            if (c == '\n')
            {
                // Immediately parse on enter.
                // This ensures we have up-to-date CC info about the method boundary when a user
                // types near the end of a method.
                //SD.ParserService.ParseAsync(this.FileName, this.Document.CreateSnapshot()).FireAndForget();
            }
            else
            {
                if (e.Text.Length == 1)
                {
                    _codeCompletion.HandleKeyPressed(editor, c);
                }
            }
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.H | Keys.Control))
        {
            FindReplaceForm frm = new FindReplaceForm();
            frm.Editor = editor;
            frm.Left = this.Width - frm.Width;
            frm.Top = this.PointToScreen(new System.Drawing.Point(0, 0)).Y;
            frm.Show(this.FindForm());
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void foldingTimer_Tick(object sender, System.EventArgs e)
    {
        _foldingStrategy.UpdateFoldings(_foldingManager, editor.Document);
        foldingTimer.Enabled = false;
    }
}
