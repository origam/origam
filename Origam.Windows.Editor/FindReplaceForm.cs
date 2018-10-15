using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Origam.Windows.Editor
{
    public partial class FindReplaceForm : Form
    {
        public TextEditor Editor { get; set; }

        public FindReplaceForm()
        {
            InitializeComponent();
            this.Shown += FindReplaceForm_Shown;
        }

        private void FindReplaceForm_Shown(object sender, EventArgs e)
        {
            if (!Editor.TextArea.Selection.IsMultiline)
            {
                txtFind.Text = Editor.TextArea.Selection.GetText();
                txtFind.SelectAll();
            }
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            if (!FindNext(txtFind.Text))
                SystemSounds.Beep.Play();
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            Regex regex = GetRegEx(txtFind.Text);
            string input = Editor.Text.Substring(Editor.SelectionStart, Editor.SelectionLength);
            Match match = regex.Match(input);
            bool replaced = false;
            if (match.Success && match.Index == 0 && match.Length == input.Length)
            {
                Editor.Document.Replace(Editor.SelectionStart, Editor.SelectionLength, 
                    txtReplace.Text);
                replaced = true;
            }

            if (!FindNext(txtFind.Text) && !replaced)
                SystemSounds.Beep.Play();
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to Replace All occurences of \"" +
            txtFind.Text + "\" with \"" + txtReplace.Text + "\"?",
                "Replace All", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) 
                ==  DialogResult.OK)
            {
                Regex regex = GetRegEx(txtFind.Text, true);
                int offset = 0;
                Editor.BeginChange();
                foreach (Match match in regex.Matches(Editor.Text))
                {
                    Editor.Document.Replace(offset + match.Index, match.Length, txtReplace.Text);
                    offset += txtReplace.Text.Length - match.Length;
                }
                Editor.EndChange();
            }
        }

        private bool FindNext(string textToFind)
        {
            Regex regex = GetRegEx(textToFind);
            int start = regex.Options.HasFlag(RegexOptions.RightToLeft) ?
            Editor.SelectionStart : Editor.SelectionStart + Editor.SelectionLength;
            Match match = regex.Match(Editor.Text, start);

            if (!match.Success)  // start again from beginning or end
            {
                if (regex.Options.HasFlag(RegexOptions.RightToLeft))
                    match = regex.Match(Editor.Text, Editor.Text.Length);
                else
                    match = regex.Match(Editor.Text, 0);
            }

            if (match.Success)
            {
                Editor.Select(match.Index, match.Length);
                TextLocation loc = Editor.Document.GetLocation(match.Index);
                Editor.ScrollTo(loc.Line, loc.Column);
            }

            return match.Success;
        }

        private Regex GetRegEx(string textToFind, bool leftToRight = false)
        {
            RegexOptions options = RegexOptions.None;
            if (cbSearchUp.Checked && !leftToRight)
                options |= RegexOptions.RightToLeft;
            if (! cbCaseSensitive.Checked)
                options |= RegexOptions.IgnoreCase;

            if (cbRegex.Checked == true)
            {
                return new Regex(textToFind, options);
            }
            else
            {
                string pattern = Regex.Escape(textToFind);
                if (cbWildcards.Checked)
                    pattern = pattern.Replace("\\*", ".*").Replace("\\?", ".");
                if (cbWholeWord.Checked)
                    pattern = "\\b" + pattern + "\\b";
                return new Regex(pattern, options);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
